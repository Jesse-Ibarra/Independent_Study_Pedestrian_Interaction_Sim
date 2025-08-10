#!/usr/bin/env python3
import sys
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
import math
import re
from pathlib import Path
from difflib import get_close_matches
from collections import Counter

# Robust label normalization
def normalize_label(label: str) -> str:
    label = str(label)
    label = label.encode("ascii", "ignore").decode()  # Remove non-ASCII chars
    label = label.replace("\u200b", "")               # Remove zero-width spaces
    label = re.sub(r'\s+', ' ', label)                # Collapse multiple spaces
    label = label.strip().lower()                     # Trim and lowercase
    return label

# Normalized target keys
target_angles = {
    "middle middle": (0, 0),
    "middle top": (0, 35),
    "right top": (35 * math.sin(math.radians(45)), 35 * math.cos(math.radians(45))),
    "right middle": (35, 0),
    "right bottom": (35 * math.sin(math.radians(45)), -35 * math.cos(math.radians(45))),
    "middle bottom": (0, -35),
    "left bottom": (-35 * math.sin(math.radians(45)), -35 * math.cos(math.radians(45))),
    "left middle": (-35, 0),
    "left top": (-35 * math.sin(math.radians(45)), 35 * math.cos(math.radians(45)))
}

def main(summary_path: str, samples_path: str):
    summary_csv = Path(summary_path)
    samples_csv = Path(samples_path)

    if not (summary_csv.exists() and samples_csv.exists()):
        sys.exit("❌ One or both CSV paths are invalid.")

    print(f"📂 Loading summary file: {summary_csv}")
    summary_df = pd.read_csv(summary_csv, skiprows=lambda x: str(x).startswith('---'))
    offset_col = 'GazeAngleOffset(deg)'

    if offset_col not in summary_df.columns:
        print(f"❌ Column '{offset_col}' not found in summary CSV.")
        print("📄 Columns found:", summary_df.columns.tolist())
        return

    summary_df[offset_col] = pd.to_numeric(summary_df[offset_col], errors='coerce')
    avg_offset = summary_df[offset_col].mean()
    print(f"✅ Average gaze-angle offset across all targets: {avg_offset:.2f}°")

    print(f"📂 Loading gaze sample file: {samples_csv}")
    samples_df = pd.read_csv(samples_csv, comment='-')
    print(f"📊 Pre-cleaning row count: {len(samples_df)}")
    print("🧮 Target label counts (raw):")
    print(samples_df['Target'].value_counts())

    samples_df['Yaw(deg)'] = pd.to_numeric(samples_df['Yaw(deg)'], errors='coerce')
    samples_df['Pitch(deg)'] = pd.to_numeric(samples_df['Pitch(deg)'], errors='coerce')

    raw_x = samples_df['Yaw(deg)'].tolist()
    raw_y = samples_df['Pitch(deg)'].tolist()

    corrected_labels = []
    shifted_x = []
    shifted_y = []
    corrections = {}

    for _, row in samples_df.iterrows():
        raw_label = row['Target']
        yaw = row['Yaw(deg)']
        pitch = row['Pitch(deg)']

        # ✅ Only skip rows where BOTH yaw and pitch are missing
        if pd.isna(yaw) and pd.isna(pitch):
            corrected_labels.append(None)
            continue

        norm_label = normalize_label(raw_label)

        if norm_label in target_angles:
            corrected = norm_label
        else:
            match = get_close_matches(norm_label, target_angles.keys(), n=1, cutoff=0.75)
            corrected = match[0] if match else None
            if corrected and corrected != norm_label:
                corrections[raw_label] = corrected

        corrected_labels.append(corrected)

        if corrected in target_angles:
            base_x, base_y = target_angles[corrected]
            sx = (yaw if not pd.isna(yaw) else 0) + base_x
            sy = (pitch if not pd.isna(pitch) else 0) + base_y
            shifted_x.append(sx)
            shifted_y.append(sy)
        else:
            print(f"🚫 No match for label: '{raw_label}' → normalized as '{norm_label}'")

    samples_df['CorrectedTarget'] = corrected_labels

    if corrections:
        print("\n🔧 Autocorrected target labels:")
        for orig, fixed in corrections.items():
            print(f"   • '{orig}' → '{fixed}'")

    unmatched_count = samples_df['CorrectedTarget'].isnull().sum()
    print(f"\n⚠️ Unlabeled gaze points (no match): {unmatched_count}")

    print("\n📊 Gaze samples per matched target:")
    print(Counter(samples_df['CorrectedTarget'].dropna()))

    print("\n🔎 Normalized label frequencies (including unmatched):")
    print(samples_df['CorrectedTarget'].value_counts(dropna=False))

    unmatched_df = samples_df[samples_df['CorrectedTarget'].isnull()]
    unmatched_csv_path = samples_csv.with_name("unmatched_gaze_samples.csv")
    unmatched_df.to_csv(unmatched_csv_path, index=False)
    print(f"🧾 Unmatched samples saved → {unmatched_csv_path.resolve()}")

    print("🧠 Generating heat map...")
    plt.figure(figsize=(8, 8))

    if shifted_x:
        sns.kdeplot(
            x=shifted_x,
            y=shifted_y,
            cmap='turbo',
            fill=True,
            thresh=0.05,
            levels=100,
            bw_adjust=0.3,
            label="Labeled KDE"
        )
        plt.scatter(shifted_x, shifted_y, color='black', s=3, alpha=0.6, label="Labeled Points")

    if raw_x:
        plt.scatter(raw_x, raw_y, color='blue', s=2, alpha=0.3, label="All Raw Points")

    for name, (x, y) in target_angles.items():
        plt.plot(x, y, 'kx', markersize=8, markeredgewidth=2)

    plt.gca().set_aspect('equal', adjustable='box')
    plt.xlim(-40, 40)
    plt.ylim(-40, 40)
    plt.xlabel("Yaw (°)")
    plt.ylabel("Pitch (°)")
    plt.title("Gaze Position Heat Map")
    plt.legend()

    out_png = summary_csv.with_name("gaze_heatmap.png")
    plt.savefig(out_png, dpi=300)
    plt.close()
    print(f"✅ Heatmap saved → {out_png.resolve()}")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python gaze_analysis.py summary.csv samples.csv")
        sys.exit(1)
    main(sys.argv[1], sys.argv[2])

