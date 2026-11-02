
from pathlib import Path
import csv
import math
import re

import pandas as pd
import matplotlib.pyplot as plt

ROOT = Path(__file__).resolve().parents[1]
DATA = ROOT / "Data"
OUT = ROOT / "Reports" / "figures"
OUT.mkdir(parents=True, exist_ok=True)

ZONES = ["Intime", "Personnelle", "Sociale", "Publique"]


def polygon_area(points):
    """Shoelace formula on horizontal XZ plane."""
    area = 0.0
    for i in range(len(points)):
        x1, z1 = points[i]
        x2, z2 = points[(i + 1) % len(points)]
        area += x1 * z2 - x2 * z1
    return abs(area) / 2


def read_csv(path):
    with path.open("r", encoding="utf-8-sig", newline="") as f:
        reader = csv.DictReader(f, delimiter=";")
        rows = list(reader)

    required = {"Index", "X", "Z"}
    if not rows or not required.issubset(rows[0]):
        raise ValueError("Missing Index, X or Z column")

    rows.sort(key=lambda r: int(r["Index"]))

    if len(rows) != 20:
        raise ValueError(f"Expected 20 points, found {len(rows)}")

    return rows


records = []

for path in sorted(DATA.glob("*.csv")):
    # Example: Arthur1_Coordonnees_2026-10-30_...
    match = re.match(r"(.+?)([12])_Coordonnees_", path.name)
    if not match:
        print("Skipped filename:", path.name)
        continue

    person, trial = match.group(1), int(match.group(2))

    try:
        rows = read_csv(path)

        for i, zone in enumerate(ZONES):
            group = rows[i * 5:(i + 1) * 5]
            points = [
                (float(r["X"]), float(r["Z"]))
                for r in group
            ]

            area = polygon_area(points)
            radius = math.sqrt(area / math.pi)

            records.append({
                "Participant": person,
                "Trial": trial,
                "Zone": zone,
                "Area_m2": area,
                "Radius_m": radius,
            })

    except (ValueError, KeyError) as e:
        print(f"Skipped {path.name}: {e}")

df = pd.DataFrame(records)

if df.empty:
    raise SystemExit("No valid CSV files found")

df.to_csv(
    ROOT / "Reports" / "bulle_geometry_summary.csv",
    index=False,
    encoding="utf-8-sig"
)

# Figure 1: equivalent radius for each participant, trial 1
first = df[df["Trial"] == 1]

pivot = first.pivot(
    index="Participant",
    columns="Zone",
    values="Radius_m"
).reindex(columns=ZONES)

ax = pivot.plot(kind="bar", figsize=(12, 6))
ax.set_title("Equivalent radius by participant - Trial 1")
ax.set_xlabel("Participant")
ax.set_ylabel("Equivalent radius (m)")
ax.legend(title="Zone")
ax.grid(axis="y", alpha=0.25)
plt.xticks(rotation=30, ha="right")
plt.tight_layout()
plt.savefig(OUT / "radius_by_participant.png", dpi=180)
plt.close()

# Figure 2: percentage area change, trial 1 to trial 2
paired = df.pivot_table(
    index=["Participant", "Zone"],
    columns="Trial",
    values="Area_m2"
)

if 1 in paired.columns and 2 in paired.columns:
    paired = paired.dropna(subset=[1, 2])
    paired["Change_pct"] = (
        (paired[2] - paired[1])
        / paired[1].replace(0, float("nan"))
        * 100
    )

    changes = paired["Change_pct"].unstack("Zone")
    changes = changes.reindex(columns=ZONES)

    ax = changes.plot(kind="bar", figsize=(12, 6))
    ax.axhline(0, color="black", linewidth=0.8)
    ax.set_title("Area change between trials 1 and 2")
    ax.set_xlabel("Participant")
    ax.set_ylabel("Area change (%)")
    ax.legend(title="Zone")
    ax.grid(axis="y", alpha=0.25)
    plt.xticks(rotation=30, ha="right")
    plt.tight_layout()
    plt.savefig(OUT / "area_change.png", dpi=180)
    plt.close()

print("\nMean equivalent radius, trial 1:")
print(first.groupby("Zone")["Radius_m"].mean())

print("\nFigures saved to:", OUT)
print("Summary saved to: Reports/bulle_geometry_summary.csv")
