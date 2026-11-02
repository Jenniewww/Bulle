#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Analyze 4 proxemic zones represented by 5 X/Z control points each.

No external Python packages required.

Windows:
    cd D:\\Bulle
    python Tools\\analyze_bulle_csv.py
    python Tools\\analyze_bulle_csv.py --data D:\\Bulle\\Data --out D:\\Bulle\\Reports

Input compatibility:
    Legacy: Index;Nom;X;Y;Z
    V1:     Index;Nom;X;Y;Z;Zone;Point;CenterX;CenterY;CenterZ;...

Area is the *polygonal approximation from 5 points*, not the exact smoothed
Unity mesh/spline area. Units assume 1 Unity unit == 1 meter.
"""
from __future__ import annotations

import argparse
import csv
import math
import sys
import unicodedata
from collections import defaultdict
from pathlib import Path
from statistics import mean
from xml.sax.saxutils import escape

ZONES = ["Intime", "Personnelle", "Sociale", "Publique"]
COLORS = {"Intime": "#e84e52", "Personnelle": "#f3a43b", "Sociale": "#43b877", "Publique": "#8a67d2"}
OUTPUT_COLUMNS = [
    "File", "SessionId", "Phase", "Zone", "Points", "Area_m2", "Equivalent_radius_m",
    "Equivalent_diameter_m", "Width_X_m", "Depth_Z_m", "Polygon_perimeter_m",
    "Compactness_0_1", "Radial_min_m", "Radial_max_m", "Y_spread_m", "Center_source",
]


def normalized(s: str) -> str:
    s = unicodedata.normalize("NFKD", str(s).casefold())
    return "".join(c for c in s if not unicodedata.combining(c))


def parse_zone(s: str) -> str | None:
    s = normalized(s)
    if "intim" in s: return "Intime"
    if "person" in s: return "Personnelle"
    if "social" in s: return "Sociale"
    if "publi" in s: return "Publique"
    return None


def number(s: str) -> float:
    v = float(str(s).strip().replace(",", "."))
    if not math.isfinite(v):
        raise ValueError("Non-finite coordinate")
    return v


def optional_number(s: str | None) -> float | None:
    if s is None or not str(s).strip():
        return None
    return number(s)


def read_points(path: Path):
    with path.open("r", encoding="utf-8-sig", newline="") as fh:
        sample = fh.read(1000)
        fh.seek(0)
        delim = ";" if sample.count(";") > sample.count(",") else ","
        reader = csv.DictReader(fh, delimiter=delim)
        if reader.fieldnames:
            reader.fieldnames = [name.lstrip("\ufeff").strip() for name in reader.fieldnames]
        if not reader.fieldnames or not {"X", "Y", "Z"}.issubset(reader.fieldnames):
            raise ValueError("CSV needs columns X, Y, Z (and usually Index, Nom)")
        rows = list(reader)

    if len(rows) != 20:
        raise ValueError(f"Need exactly 20 point rows (4x5), found {len(rows)}")

    points = []
    for pos, row in enumerate(rows):
        index = int(row.get("Index", pos) or pos)
        points.append({
            "index": index,
            "label": row.get("Nom", ""),
            "zone": parse_zone(row.get("Zone", "")) or parse_zone(row.get("Nom", "")),
            "x": number(row["X"]), "y": number(row["Y"]), "z": number(row["Z"]),
            "cx": optional_number(row.get("CenterX")),
            "cy": optional_number(row.get("CenterY")),
            "cz": optional_number(row.get("CenterZ")),
            "phase": (row.get("Phase") or "unknown").strip(),
            "session": (row.get("SessionId") or "unknown").strip(),
        })

    # Explicit Zone / Nom labels are preferred. Otherwise the existing project's
    # ControlManager/CoordinateManager list order must be 5 per zone.
    labels = [p["zone"] for p in points]
    explicit_valid = all(labels.count(zone) == 5 for zone in ZONES)
    if not explicit_valid:
        if len({p["index"] for p in points}) != 20:
            raise ValueError("Missing distinct Index values: cannot infer zone membership")
        points.sort(key=lambda p: p["index"])
        for i, p in enumerate(points):
            p["zone"] = ZONES[i // 5]
        print(f"  WARNING {path.name}: zones assigned by Index 0-4,5-9,10-14,15-19. "
              "Verify the Unity ObjectEntry order is truly Intime/Personnelle/Sociale/Publique.")

    return points


def polygon_metrics(group, path: Path):
    # If a saved body reference center exists, use it to order vertices.
    # Otherwise use geometric center ONLY for angular sorting; we do not
    # report body-relative radial distances without a body reference.
    center_available = all(p["cx"] is not None and p["cz"] is not None for p in group)
    if center_available:
        cx = mean(p["cx"] for p in group)
        cz = mean(p["cz"] for p in group)
        center_source = "saved_body_reference"
    else:
        cx = mean(p["x"] for p in group)
        cz = mean(p["z"] for p in group)
        center_source = "not_saved"

    ordered = sorted(group, key=lambda p: math.atan2(p["z"] - cz, p["x"] - cx))
    xs = [p["x"] for p in ordered]
    ys = [p["y"] for p in ordered]
    zs = [p["z"] for p in ordered]
    n = len(ordered)
    cross = sum(xs[i] * zs[(i+1) % n] - xs[(i+1) % n] * zs[i] for i in range(n))
    area = abs(cross) / 2.0
    perimeter = sum(math.hypot(xs[i]-xs[(i+1)%n], zs[i]-zs[(i+1)%n]) for i in range(n))
    if area < 1e-9:
        raise ValueError(f"{path.name}: zone {group[0]['zone']} has ~zero area; points may overlap")
    radials = [math.hypot(x - cx, z - cz) for x, z in zip(xs, zs)] if center_available else []
    return {
        "File": path.name, "SessionId": group[0]["session"], "Phase": group[0]["phase"],
        "Zone": group[0]["zone"], "Points": n,
        "Area_m2": area,
        "Equivalent_radius_m": math.sqrt(area / math.pi),
        "Equivalent_diameter_m": 2 * math.sqrt(area / math.pi),
        "Width_X_m": max(xs)-min(xs), "Depth_Z_m": max(zs)-min(zs),
        "Polygon_perimeter_m": perimeter,
        "Compactness_0_1": 4 * math.pi * area / (perimeter * perimeter) if perimeter > 0 else 0,
        "Radial_min_m": min(radials) if radials else None,
        "Radial_max_m": max(radials) if radials else None,
        "Y_spread_m": max(ys)-min(ys), "Center_source": center_source,
    }, ordered


def write_csv(path: Path, data, columns):
    with path.open("w", encoding="utf-8-sig", newline="") as f:
        w = csv.DictWriter(f, fieldnames=columns, delimiter=";", extrasaction="ignore")
        w.writeheader()
        for row in data:
            w.writerow({k: ("" if row.get(k) is None else (f"{row[k]:.6f}" if isinstance(row.get(k), float) else row.get(k, "")))
                        for k in columns})


def plot_svg(path: Path, rings):
    # Plain SVG preview requiring no matplotlib. X=horizontal, Z=vertical.
    allp = [p for ring in rings.values() for p in ring]
    minx, maxx = min(p["x"] for p in allp), max(p["x"] for p in allp)
    minz, maxz = min(p["z"] for p in allp), max(p["z"] for p in allp)
    w = max(maxx - minx, .1)
    h = max(maxz - minz, .1)
    span = max(w, h) * 1.20
    cx = (minx + maxx) / 2
    cz = (minz + maxz) / 2
    def project(p):
        return 340 + (p["x"] - cx) * 535 / span, 355 - (p["z"] - cz) * 535 / span
    lines = [
        '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 710 760">',
        '<rect width="710" height="760" fill="white"/>',
        '<text x="35" y="35" font-family="Arial" font-size="19" font-weight="bold">Bulle: four-zone X/Z outline (5-point polygon approximation)</text>',
        '<path d="M 35 355 H 660 M 340 60 V 665" stroke="#dedede" stroke-width="1"/>',
        '<text x="650" y="347" font-family="Arial" font-size="13">X</text>',
        '<text x="349" y="70" font-family="Arial" font-size="13">+Z</text>',
    ]
    for zone in reversed(ZONES):
        poly = rings[zone]
        coords = " ".join(f"{x:.1f},{y:.1f}" for x, y in map(project, poly))
        color = COLORS[zone]
        lines.append(f'<polygon points="{coords}" fill="{color}" fill-opacity="0.06" stroke="{color}" stroke-width="3"/>')
        for p in poly:
            x, y = project(p)
            lines.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="4" fill="{color}"/>')
    x = 40
    for zone in ZONES:
        lines.append(f'<circle cx="{x}" cy="702" r="6" fill="{COLORS[zone]}"/>')
        lines.append(f'<text x="{x+11}" y="707" font-family="Arial" font-size="13">{escape(zone)}</text>')
        x += {"Intime": 110, "Personnelle": 165, "Sociale": 125, "Publique": 130}[zone]
    lines.append('<text x="35" y="741" font-family="Arial" font-size="12" fill="#666">Relative X/Z plot; not a full reconstruction of Unity splines.</text>')
    lines.append('</svg>')
    path.write_text("\n".join(lines), encoding="utf-8")


def main():
    parser = argparse.ArgumentParser(description="Analyze FourZones 20-point CSV exports")
    here = Path(__file__).resolve().parent.parent
    parser.add_argument("--data", type=Path, default=here / "Data", help="CSV folder, defaults to ../Data")
    parser.add_argument("--out", type=Path, default=here / "Reports", help="Output folder, defaults to ../Reports")
    args = parser.parse_args()
    if not args.data.is_dir():
        print("Data folder not found:", args.data)
        return 2
    files = sorted(args.data.glob("*.csv"))
    if not files:
        print("No CSV files found in", args.data)
        return 2
    args.out.mkdir(parents=True, exist_ok=True)
    all_metrics = []
    completed = 0
    for path in files:
        try:
            points = read_points(path)
            groups = defaultdict(list)
            for p in points: groups[p["zone"]].append(p)
            if any(len(groups[z]) != 5 for z in ZONES):
                raise ValueError("One or more zones does not have exactly five points")
            metrics = []
            ordered_rings = {}
            for zone in ZONES:
                item, ordered = polygon_metrics(groups[zone], path)
                metrics.append(item)
                ordered_rings[zone] = ordered
            write_csv(args.out / ("metrics_" + path.stem + ".csv"), metrics, OUTPUT_COLUMNS)
            plot_svg(args.out / ("outline_" + path.stem + ".svg"), ordered_rings)
            all_metrics.extend(metrics)
            completed += 1
            print("\n", path.name, sep="")
            for m in metrics:
                print(f"  {m['Zone']:<13} area={m['Area_m2']:.3f} m²  "
                      f"R_eq={m['Equivalent_radius_m']:.3f} m  "
                      f"width={m['Width_X_m']:.3f} m  depth={m['Depth_Z_m']:.3f} m")
            areas = [m["Area_m2"] for m in metrics]
            if not all(areas[i] < areas[i+1] for i in range(3)):
                print("  WARNING: area not strictly increasing from intimate to public zone; check list ordering or crossing.")
            if any(m["Y_spread_m"] > .08 for m in metrics):
                print("  WARNING: at least one zone has >8cm vertical spread; check horizontal-plane calibration.")
        except (ValueError, KeyError, OSError) as err:
            print(f"SKIPPED {path.name}: {err}", file=sys.stderr)
    if not all_metrics:
        return 1
    write_csv(args.out / "summary_all.csv", all_metrics, OUTPUT_COLUMNS)

    # Within-participant paired comparison: one baseline + latest final per zone.
    paired = defaultdict(lambda: {"initial": [], "final": []})
    for m in all_metrics:
        phase = normalized(m["Phase"])
        if phase in ("initial", "final") and m["SessionId"] != "unknown":
            paired[(m["SessionId"], m["Zone"])][phase].append(m)
    changes = []
    for (session, zone), phases in paired.items():
        if not phases["initial"] or not phases["final"]:
            continue
        before = sorted(phases["initial"], key=lambda x: x["File"])[0]
        after = sorted(phases["final"], key=lambda x: x["File"])[-1]
        a0, a1 = before["Area_m2"], after["Area_m2"]
        r0, r1 = before["Equivalent_radius_m"], after["Equivalent_radius_m"]
        changes.append({
            "SessionId": session, "Zone": zone,
            "Initial_file": before["File"], "Final_file": after["File"],
            "Initial_area_m2": a0, "Final_area_m2": a1,
            "Delta_area_m2": a1 - a0,
            "Area_change_pct": 100.0 * (a1 - a0) / a0 if a0 else None,
            "Initial_req_m": r0, "Final_req_m": r1, "Delta_req_m": r1 - r0,
        })
    if changes:
        change_fields = ["SessionId", "Zone", "Initial_file", "Final_file",
                         "Initial_area_m2", "Final_area_m2", "Delta_area_m2", "Area_change_pct",
                         "Initial_req_m", "Final_req_m", "Delta_req_m"]
        write_csv(args.out / "paired_changes.csv", changes, change_fields)
        print(f"Paired changes: {len(changes)} zone comparisons -> paired_changes.csv")

    print(f"\nDONE: {completed} CSV files analyzed; output: {args.out}")
    print("Measurements are a 5-point polygon approximation, not an exact Unity spline boundary.")
    if any(r["Center_source"] == "not_saved" for r in all_metrics):
        print("Legacy CSV without CenterX/CenterZ: area/width are valid, but body-centered radial distances cannot be inferred.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
