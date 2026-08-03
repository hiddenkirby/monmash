#!/usr/bin/env python3
"""Generate Tidepool's small original v0.1 WAV set."""

from __future__ import annotations

import math
import os
import struct
import wave


SAMPLE_RATE = 44100
OUTPUT_DIR = os.path.join("Assets", "Audio")

ASSET_META_GUIDS = {
    "ambient_loop.wav": "6a7d58e67f9a4d94bb80b6433b3e1121",
    "catch_chime.wav": "f91b6d5bd29d4af3b60a9d8d4dce5122",
    "escape_note.wav": "b5f9d0a4ed784cf6b9bf2027e4b528d8",
    "ui_tap.wav": "2f0adfc74ad24dfab8634cc8a0d5b1e4",
}


def clamp(value: float) -> float:
    return max(-1.0, min(1.0, value))


def write_wav(filename: str, samples: list[float]) -> None:
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    path = os.path.join(OUTPUT_DIR, filename)
    with wave.open(path, "wb") as wav_file:
        wav_file.setnchannels(1)
        wav_file.setsampwidth(2)
        wav_file.setframerate(SAMPLE_RATE)
        packed = bytearray()
        for sample in samples:
            packed.extend(struct.pack("<h", int(clamp(sample) * 32767)))
        wav_file.writeframes(bytes(packed))

    write_audio_meta(filename)


def write_audio_meta(filename: str) -> None:
    meta_path = os.path.join(OUTPUT_DIR, f"{filename}.meta")
    with open(meta_path, "w", encoding="utf-8") as meta_file:
        meta_file.write(
            f"""fileFormatVersion: 2
guid: {ASSET_META_GUIDS[filename]}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: {SAMPLE_RATE}
    compressionFormat: 1
    quality: 1
    conversionMode: 0
  platformSettingOverrides: {{}}
  forceToMono: 0
  normalize: 1
  preloadAudioData: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 1
  userData:
  assetBundleName:
  assetBundleVariant:
"""
        )


def ambient_loop() -> list[float]:
    seconds = 8.0
    count = int(SAMPLE_RATE * seconds)
    samples: list[float] = []
    for i in range(count):
        t = i / SAMPLE_RATE
        base = (
            0.08 * math.sin(2 * math.pi * 0.25 * t)
            + 0.04 * math.sin(2 * math.pi * 0.5 * t + 0.7)
            + 0.025 * math.sin(2 * math.pi * 1.0 * t + 1.8)
        )
        shimmer = (
            0.012 * math.sin(2 * math.pi * 277.0 * t)
            + 0.008 * math.sin(2 * math.pi * 349.0 * t + 1.4)
            + 0.006 * math.sin(2 * math.pi * 523.25 * t + 2.1)
        )
        samples.append((base + shimmer) * 0.45)
    return samples


def catch_chime() -> list[float]:
    seconds = 1.15
    count = int(SAMPLE_RATE * seconds)
    notes = (523.25, 659.25, 783.99)
    samples: list[float] = []
    for i in range(count):
        t = i / SAMPLE_RATE
        envelope = math.exp(-3.6 * t)
        value = 0.0
        for offset, frequency in enumerate(notes):
            delay = offset * 0.055
            if t >= delay:
                local_t = t - delay
                value += 0.18 * math.exp(-4.2 * local_t) * math.sin(2 * math.pi * frequency * local_t)
                value += 0.05 * math.exp(-7.0 * local_t) * math.sin(2 * math.pi * frequency * 2.01 * local_t)
        samples.append(value * envelope)
    return samples


def escape_note() -> list[float]:
    seconds = 0.9
    count = int(SAMPLE_RATE * seconds)
    samples: list[float] = []
    for i in range(count):
        t = i / SAMPLE_RATE
        progress = t / seconds
        frequency = 392.0 - 110.0 * progress
        envelope = math.sin(math.pi * progress) * math.exp(-1.2 * progress)
        samples.append(0.18 * envelope * math.sin(2 * math.pi * frequency * t))
    return samples


def ui_tap() -> list[float]:
    seconds = 0.12
    count = int(SAMPLE_RATE * seconds)
    samples: list[float] = []
    for i in range(count):
        t = i / SAMPLE_RATE
        envelope = math.exp(-48.0 * t)
        tone = math.sin(2 * math.pi * 880.0 * t) + 0.5 * math.sin(2 * math.pi * 1320.0 * t)
        samples.append(0.12 * envelope * tone)
    return samples


def main() -> None:
    write_wav("ambient_loop.wav", ambient_loop())
    write_wav("catch_chime.wav", catch_chime())
    write_wav("escape_note.wav", escape_note())
    write_wav("ui_tap.wav", ui_tap())


if __name__ == "__main__":
    main()
