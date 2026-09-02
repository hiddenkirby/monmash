#!/usr/bin/env python3
"""Generate Tidepool's original contest move category icons.

Simple white glyphs (droplet/starburst/shield) that sit on top of the
existing tinted category badges, replacing the tiny "!"/"@"/"[]" text
placeholders with something readable at a glance for a 7-year-old.
"""

from __future__ import annotations

import math
import os

from PIL import Image, ImageDraw

SUPERSAMPLE = 4
SIZE = 64
OUTPUT_DIR = os.path.join("Assets", "Art", "UI", "ContestIcons")
WHITE = (255, 255, 255, 255)


def new_canvas() -> Image.Image:
    return Image.new("RGBA", (SIZE * SUPERSAMPLE, SIZE * SUPERSAMPLE), (0, 0, 0, 0))


def save(image: Image.Image, filename: str) -> None:
    image = image.resize((SIZE, SIZE), Image.LANCZOS)
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    path = os.path.join(OUTPUT_DIR, filename)
    image.save(path)
    print(f"saved {path}")


def draw_attack_droplet() -> Image.Image:
    image = new_canvas()
    draw = ImageDraw.Draw(image)
    s = SUPERSAMPLE
    cx, cy = 32 * s, 36 * s
    r = 16 * s
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=WHITE)
    tip = [(cx, cy - r - 20 * s), (cx - 13 * s, cy - 2 * s), (cx + 13 * s, cy - 2 * s)]
    draw.polygon(tip, fill=WHITE)
    return image


def draw_focus_starburst() -> Image.Image:
    image = new_canvas()
    draw = ImageDraw.Draw(image)
    s = SUPERSAMPLE
    cx, cy = 32 * s, 32 * s
    outer = 22 * s
    inner = 9 * s
    points = []
    spikes = 4
    for i in range(spikes * 2):
        radius = outer if i % 2 == 0 else inner
        angle = math.pi / spikes * i - math.pi / 2
        points.append((cx + radius * math.cos(angle), cy + radius * math.sin(angle)))
    draw.polygon(points, fill=WHITE)
    draw.ellipse([cx - 5 * s, cy - 5 * s, cx + 5 * s, cy + 5 * s], fill=WHITE)
    return image


def draw_defend_shield() -> Image.Image:
    image = new_canvas()
    draw = ImageDraw.Draw(image)
    s = SUPERSAMPLE
    left, right = 14 * s, 50 * s
    top, mid, bottom = 12 * s, 34 * s, 54 * s
    cx = 32 * s
    outline = [
        (left, top),
        (right, top),
        (right, mid),
        (cx, bottom),
        (left, mid),
    ]
    draw.polygon(outline, fill=WHITE)
    return image


def main() -> None:
    save(draw_attack_droplet(), "attack.png")
    save(draw_focus_starburst(), "focus.png")
    save(draw_defend_shield(), "defend.png")


if __name__ == "__main__":
    main()
