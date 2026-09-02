#!/usr/bin/env python3
"""Generate Tidepool's original Kelp Curtain and Rocky Shelf tile art.

Matches the existing Kenney-style obstacle sprite convention (soft
rounded silhouette with darker accent dots for texture, transparent
background, 64x64) but reads as actual kelp/rock instead of a
retinted generic shrub blob.
"""

from __future__ import annotations

import math
import os
import random

from PIL import Image, ImageDraw

SUPERSAMPLE = 4
SIZE = 64
OUTPUT_DIR = os.path.join("Assets", "Art", "Tiles", "KenneyRpgBase")

def _rgba(r: float, g: float, b: float, a: int) -> tuple[int, int, int, int]:
    return (int(r * 255), int(g * 255), int(b * 255), a)


KELP_BACK = _rgba(0.20, 0.46, 0.32, 255)
KELP_FRONT = _rgba(0.30, 0.62, 0.42, 255)
KELP_ACCENT = _rgba(0.16, 0.36, 0.25, 200)

ROCK_BASE = _rgba(0.56, 0.57, 0.55, 255)
ROCK_SHADE = _rgba(0.40, 0.41, 0.40, 220)
ROCK_HIGHLIGHT = _rgba(0.72, 0.73, 0.70, 200)
ROCK_MOSS = _rgba(0.42, 0.56, 0.30, 190)


def new_canvas() -> Image.Image:
    return Image.new("RGBA", (SIZE * SUPERSAMPLE, SIZE * SUPERSAMPLE), (0, 0, 0, 0))


def save(image: Image.Image, filename: str) -> None:
    image = image.resize((SIZE, SIZE), Image.LANCZOS)
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    path = os.path.join(OUTPUT_DIR, filename)
    image.save(path)
    print(f"saved {path}")


def draw_blade(draw: ImageDraw.ImageDraw, base_x: int, base_y: int, height: int,
                width: int, sway: int, color: tuple[int, int, int, int]) -> None:
    points = []
    steps = 14
    for i in range(steps + 1):
        t = i / steps
        y = base_y - height * t
        x = base_x + sway * math.sin(t * math.pi * 0.9)
        w = width * (1.0 - t) ** 0.6
        points.append((x - w, y))
    for i in range(steps, -1, -1):
        t = i / steps
        y = base_y - height * t
        x = base_x + sway * math.sin(t * math.pi * 0.9)
        w = width * (1.0 - t) ** 0.6
        points.append((x + w, y))
    draw.polygon(points, fill=color)


def draw_kelp() -> Image.Image:
    image = new_canvas()
    draw = ImageDraw.Draw(image)
    s = SUPERSAMPLE
    base_y = 60 * s

    draw_blade(draw, 20 * s, base_y, 50 * s, 7 * s, -10 * s, KELP_BACK)
    draw_blade(draw, 44 * s, base_y, 46 * s, 7 * s, 12 * s, KELP_BACK)
    draw_blade(draw, 32 * s, base_y, 56 * s, 8 * s, -6 * s, KELP_FRONT)
    draw_blade(draw, 14 * s, base_y, 34 * s, 6 * s, 6 * s, KELP_FRONT)
    draw_blade(draw, 50 * s, base_y, 32 * s, 6 * s, -8 * s, KELP_FRONT)

    rng = random.Random(7)
    for _ in range(10):
        x = rng.randint(14, 50) * s
        y = rng.randint(14, 55) * s
        r = rng.randint(1, 2) * s
        draw.ellipse([x - r, y - r, x + r, y + r], fill=KELP_ACCENT)

    return image


def draw_rock() -> Image.Image:
    image = new_canvas()
    draw = ImageDraw.Draw(image)
    s = SUPERSAMPLE

    def boulder(cx: int, cy: int, r: int, jitter: int, seed: int, color) -> None:
        rng = random.Random(seed)
        points = []
        steps = 10
        for i in range(steps):
            angle = 2 * math.pi * i / steps
            radius = r + rng.randint(-jitter, jitter)
            points.append((cx + radius * math.cos(angle), cy + radius * math.sin(angle)))
        draw.polygon(points, fill=color)

    boulder(24 * s, 40 * s, 17 * s, 3 * s, 1, ROCK_SHADE)
    boulder(42 * s, 36 * s, 15 * s, 3 * s, 2, ROCK_BASE)
    boulder(32 * s, 24 * s, 13 * s, 3 * s, 3, ROCK_BASE)
    boulder(28 * s, 20 * s, 6 * s, 2 * s, 4, ROCK_HIGHLIGHT)

    rng = random.Random(11)
    for _ in range(6):
        x = rng.randint(16, 48) * s
        y = rng.randint(36, 52) * s
        r = rng.randint(1, 2) * s
        draw.ellipse([x - r, y - r, x + r, y + r], fill=ROCK_MOSS)

    return image


def main() -> None:
    save(draw_kelp(), "kelp_tall.png")
    save(draw_rock(), "rock_mossy.png")


if __name__ == "__main__":
    main()
