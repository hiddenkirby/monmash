#!/usr/bin/env python3
"""Generate Tidepool's original glass-jar app icon PNG."""

from __future__ import annotations

import os
import struct
import zlib


SIZE = 1024
OUTPUT_DIR = os.path.join("Assets", "Art", "UI")
OUTPUT_FILE = "app_icon_glass_jar.png"
META_GUID = "54d15c9d8a6f43ea90f6440fd81db727"


def blend_pixel(pixels: bytearray, x: int, y: int, color: tuple[int, int, int, int]) -> None:
    if x < 0 or y < 0 or x >= SIZE or y >= SIZE:
        return

    index = (y * SIZE + x) * 4
    sr, sg, sb, sa = color
    alpha = sa / 255.0
    inv_alpha = 1.0 - alpha
    pixels[index] = int(sr * alpha + pixels[index] * inv_alpha)
    pixels[index + 1] = int(sg * alpha + pixels[index + 1] * inv_alpha)
    pixels[index + 2] = int(sb * alpha + pixels[index + 2] * inv_alpha)
    pixels[index + 3] = 255


def fill_rect(pixels: bytearray, x0: int, y0: int, x1: int, y1: int, color: tuple[int, int, int, int]) -> None:
    for y in range(max(0, y0), min(SIZE, y1)):
        for x in range(max(0, x0), min(SIZE, x1)):
            blend_pixel(pixels, x, y, color)


def fill_ellipse(pixels: bytearray, cx: int, cy: int, rx: int, ry: int, color: tuple[int, int, int, int]) -> None:
    rx2 = rx * rx
    ry2 = ry * ry
    for y in range(cy - ry, cy + ry + 1):
        for x in range(cx - rx, cx + rx + 1):
            dx = x - cx
            dy = y - cy
            if dx * dx * ry2 + dy * dy * rx2 <= rx2 * ry2:
                blend_pixel(pixels, x, y, color)


def fill_round_rect(
    pixels: bytearray,
    x0: int,
    y0: int,
    x1: int,
    y1: int,
    radius: int,
    color: tuple[int, int, int, int],
) -> None:
    fill_rect(pixels, x0 + radius, y0, x1 - radius, y1, color)
    fill_rect(pixels, x0, y0 + radius, x1, y1 - radius, color)
    fill_ellipse(pixels, x0 + radius, y0 + radius, radius, radius, color)
    fill_ellipse(pixels, x1 - radius, y0 + radius, radius, radius, color)
    fill_ellipse(pixels, x0 + radius, y1 - radius, radius, radius, color)
    fill_ellipse(pixels, x1 - radius, y1 - radius, radius, radius, color)


def stroke_rect(pixels: bytearray, x0: int, y0: int, x1: int, y1: int, width: int, color: tuple[int, int, int, int]) -> None:
    fill_rect(pixels, x0, y0, x1, y0 + width, color)
    fill_rect(pixels, x0, y1 - width, x1, y1, color)
    fill_rect(pixels, x0, y0, x0 + width, y1, color)
    fill_rect(pixels, x1 - width, y0, x1, y1, color)


def make_png() -> bytes:
    pixels = bytearray([180, 225, 226, 255] * SIZE * SIZE)

    fill_ellipse(pixels, 512, 830, 310, 74, (94, 151, 154, 62))
    fill_round_rect(pixels, 332, 225, 692, 810, 82, (242, 254, 255, 118))
    fill_round_rect(pixels, 356, 254, 668, 785, 58, (145, 210, 218, 56))
    fill_round_rect(pixels, 390, 165, 634, 255, 36, (232, 249, 250, 156))
    stroke_rect(pixels, 392, 172, 632, 248, 18, (71, 128, 137, 190))
    stroke_rect(pixels, 332, 225, 692, 810, 18, (58, 119, 130, 175))
    fill_rect(pixels, 358, 560, 666, 770, (60, 166, 184, 158))
    fill_ellipse(pixels, 512, 560, 154, 34, (93, 198, 211, 130))
    fill_ellipse(pixels, 430, 496, 36, 26, (255, 255, 255, 130))
    fill_ellipse(pixels, 590, 420, 54, 40, (255, 255, 255, 94))
    fill_rect(pixels, 404, 304, 440, 692, (255, 255, 255, 70))
    fill_ellipse(pixels, 512, 695, 74, 50, (249, 249, 226, 130))
    fill_ellipse(pixels, 512, 682, 44, 30, (255, 255, 248, 165))

    raw_rows = []
    stride = SIZE * 4
    for y in range(SIZE):
        raw_rows.append(b"\x00" + bytes(pixels[y * stride : (y + 1) * stride]))
    raw = b"".join(raw_rows)

    def chunk(kind: bytes, data: bytes) -> bytes:
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF)

    png = bytearray(b"\x89PNG\r\n\x1a\n")
    png.extend(chunk(b"IHDR", struct.pack(">IIBBBBB", SIZE, SIZE, 8, 6, 0, 0, 0)))
    png.extend(chunk(b"IDAT", zlib.compress(raw, 9)))
    png.extend(chunk(b"IEND", b""))
    return bytes(png)


def write_meta() -> None:
    path = os.path.join(OUTPUT_DIR, f"{OUTPUT_FILE}.meta")
    with open(path, "w", encoding="utf-8") as meta_file:
        meta_file.write(
            f"""fileFormatVersion: 2
guid: {META_GUID}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: 100
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 1
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
  spritePackingTag:
  pSDRemoveMatte: 0
  pSDShowRemoveMatteOption: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""
        )


def main() -> None:
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    with open(os.path.join(OUTPUT_DIR, OUTPUT_FILE), "wb") as png_file:
        png_file.write(make_png())
    write_meta()


if __name__ == "__main__":
    main()
