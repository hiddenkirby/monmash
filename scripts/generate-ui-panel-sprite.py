#!/usr/bin/env python3
"""Generate Tidepool's original rounded-rect UI panel sprite (9-slice base).

A single tintable white rounded rectangle used as the shared visual
primitive for buttons and panels across every generated scene, so the
game reads as one consistent design instead of flat placeholder rects.
Supersampled for clean anti-aliased edges, then downsampled.
"""

from __future__ import annotations

import os

from PIL import Image, ImageDraw

SUPERSAMPLE = 4
SIZE = 128
CORNER_RADIUS = 28
OUTPUT_DIR = os.path.join("Assets", "Art", "UI")
OUTPUT_FILE = "rounded_panel.png"


def main() -> None:
    big_size = SIZE * SUPERSAMPLE
    big_radius = CORNER_RADIUS * SUPERSAMPLE

    image = Image.new("RGBA", (big_size, big_size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle(
        [(0, 0), (big_size - 1, big_size - 1)],
        radius=big_radius,
        fill=(255, 255, 255, 255),
    )

    image = image.resize((SIZE, SIZE), Image.LANCZOS)

    output_path = os.path.join(OUTPUT_DIR, OUTPUT_FILE)
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    image.save(output_path)
    print(f"saved {output_path} ({SIZE}x{SIZE}, corner radius {CORNER_RADIUS}px)")


if __name__ == "__main__":
    main()
