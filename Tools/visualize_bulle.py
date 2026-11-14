
from pathlib import Path
import csv
import math
import re
import sys

try:
    import matplotlib.pyplot as plt
    import numpy as np
except (ImportError, ValueError) as exc:
    print("Matplotlib / NumPy environment error:", exc)
    print("Try: conda install -c defaults matplotlib numpy")
    sys.exit(1)

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / "Data"
OUT = ROOT / "Reports" / "figures"
OUT.mkdir(parents=True, exist_ok=True)

ZONES = ["Intime", "Personnelle", "Sociale", "Publique"]
COLORS = ["#E76F51", "#E9C46A", "#2A9D8F", "#457B9D"]

# Handles filenames such as:
# Arthur1_Coordonnees_2026-10-30.csv
# Arthur2_Coordonnees_2026-10-30.csv
NAME_PATTERN = re.compile(
    r"^(.+?)([12])_Coordonnees_.*\.csv$",
    re.IGNORECASE
)


def read_coordinates(path):
    # utf-8-sig removes one BOM; lstrip handles repeated BOMs
    with path.open(
        "r", encoding="utf-8-sig", newline=""
    ) as file:
        reader = csv.DictReader(file, delimiter=";")

        if reader.fieldnames is None:
            raise ValueError("Empty CSV")

        reader.fieldnames = [
            name.lstrip("\ufeff").strip()
            for name in reader.fieldnames
        ]

        rows = list(reader)

    if len(rows) != 20:
        raise ValueError(
            f"Expected 20 points, got {len(rows)}"
        )

    rows.sort(key=lambda row: int(row["Index"]))

    indices = [int(row["Index"]) for row in rows]

    if indices != list(range(20)):
        raise ValueError(
            "Indices must be 0 through 19"
        )

    return [
        (float(row["X"]), float(row["Z"]))
        for row in rows
    ]


def polygon_area(points):
    total = 0.0
    for i in range(len(points)):
        x1, z1 = points[i]
        x2, z2 = points[(i + 1) % len(points)]
        total += x1 * z2 - x2 * z1

    return abs(total) / 2.0


def analyze():
    records = []

    if not DATA.exists():
        raise FileNotFoundError(
            f"Data folder not found: {DATA}"
        )

    for path in sorted(DATA.glob("*.csv")):
        match = NAME_PATTERN.match(path.name)

        if not match:
            print("Skipping filename:", path.name)
            continue

        participant = match.group(1)
        trial = int(match.group(2))

        try:
            points = read_coordinates(path)

            for zone_index, zone in enumerate(ZONES):
                start = zone_index * 5
                group = points[start:start + 5]

                area = polygon_area(group)
                radius = math.sqrt(area / math.pi)

                records.append({
                    "participant": participant,
                    "trial": trial,
                    "zone": zone,
                    "area": area,
                    "radius": radius,
                })

            print("Loaded:", path.name)

        except (ValueError, KeyError) as exc:
            print("ERROR:", path.name, exc)

    return records


def get_value(records, participant, trial, zone, key):
    matches = [
        row[key] for row in records
        if row["participant"] == participant
        and row["trial"] == trial
        and row["zone"] == zone
    ]

    return matches[0] if matches else float("nan")


def plot_radius(records, participants):
    fig, ax = plt.subplots(figsize=(13, 7))

    x = np.arange(len(participants))
    width = 0.19

    for j, zone in enumerate(ZONES):
        values = [
            get_value(records, p, 1, zone, "radius")
            for p in participants
        ]

        ax.bar(
            x + (j - 1.5) * width,
            values,
            width,
            label=zone,
            color=COLORS[j]
        )

    ax.set_xticks(x)
    ax.set_xticklabels(participants, rotation=25)
    ax.set_ylabel("Equivalent radius (m)")
    ax.set_title(
        "Personal Space by Participant - First Acquisition"
    )
    ax.legend()
    ax.grid(axis="y", alpha=0.2)
    ax.set_axisbelow(True)

    fig.tight_layout()
    fig.savefig(
        OUT / "radius_by_participant.png",
        dpi=200
    )
    plt.close(fig)


def plot_changes(records, participants):
    fig, ax = plt.subplots(figsize=(13, 7))

    x = np.arange(len(participants))
    width = 0.19

    for j, zone in enumerate(ZONES):
        changes = []

        for participant in participants:
            before = get_value(
                records, participant, 1, zone, "area"
            )
            after = get_value(
                records, participant, 2, zone, "area"
            )

            if (
                not math.isfinite(before)
                or not math.isfinite(after)
                or before <= 0
            ):
                changes.append(float("nan"))
            else:
                changes.append(
                    (after - before) / before * 100
                )

        ax.bar(
            x + (j - 1.5) * width,
            changes,
            width,
            label=zone,
            color=COLORS[j]
        )

    ax.axhline(0, color="black", linewidth=1)
    ax.set_xticks(x)
    ax.set_xticklabels(participants, rotation=25)
    ax.set_ylabel("Area change (%)")
    ax.set_title(
        "Changes in Personal Space - Acquisition 1 vs 2"
    )
    ax.legend()
    ax.grid(axis="y", alpha=0.2)
    ax.set_axisbelow(True)

    fig.tight_layout()
    fig.savefig(
        OUT / "area_change.png",
        dpi=200
    )
    plt.close(fig)


def plot_average_areas(records):
    fig, ax = plt.subplots(figsize=(10, 6))

    x = np.arange(len(ZONES))
    width = 0.34

    for trial, shift, color in [
        (1, -width / 2, "#457B9D"),
        (2, width / 2, "#E76F51"),
    ]:
        means = []

        for zone in ZONES:
            values = [
                r["area"] for r in records
                if r["trial"] == trial
                and r["zone"] == zone
            ]

            means.append(
                sum(values) / len(values)
                if values else float("nan")
            )

        ax.bar(
            x + shift,
            means,
            width,
            label=f"Acquisition {trial}",
            color=color
        )

    ax.set_xticks(x)
    ax.set_xticklabels(ZONES)
    ax.set_ylabel("Mean polygon area (m²)")
    ax.set_title(
        "Mean Personal Space Area by Zone"
    )
    ax.legend()
    ax.grid(axis="y", alpha=0.2)
    ax.set_axisbelow(True)

    fig.tight_layout()
    fig.savefig(
        OUT / "mean_area_by_zone.png",
        dpi=200
    )
    plt.close(fig)


def export_summary(records):
    output = ROOT / "Reports" / "bulle_geometry_summary.csv"

    with output.open(
        "w", encoding="utf-8-sig", newline=""
    ) as file:
        writer = csv.DictWriter(
            file,
            fieldnames=[
                "Participant",
                "Trial",
                "Zone",
                "Area_m2",
                "Radius_m"
            ]
        )
        writer.writeheader()

        for row in records:
            writer.writerow({
                "Participant": row["participant"],
                "Trial": row["trial"],
                "Zone": row["zone"],
                "Area_m2": round(row["area"], 6),
                "Radius_m": round(row["radius"], 6)
            })

    print("Saved:", output)


def main():
    records = analyze()

    if not records:
        print("No valid CSV files found.")
        return

    participants = sorted({
        r["participant"] for r in records
    })

    print("\nParticipants:", participants)
    print("Zone measurements:", len(records))

    plot_radius(records, participants)
    plot_changes(records, participants)
    plot_average_areas(records)
    export_summary(records)

    print("\nVisualization complete!")
    print("Figures saved in:", OUT)

    for name in [
        "radius_by_participant.png",
        "area_change.png",
        "mean_area_by_zone.png"
    ]:
        print(" -", name)


if __name__ == "__main__":
    main()
