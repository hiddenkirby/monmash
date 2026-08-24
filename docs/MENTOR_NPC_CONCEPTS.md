# Mentor NPC Concepts

Issue: #136

These concepts are review options for the v0.7 mentor NPC. The mentor should feel like a
nearby tidepool helper: warm, curious, never scolding, and never a trainer, shopkeeper, rival,
or quest authority. The final sprite should be generated only after Ryan picks one direction.

## Shared Art Direction

Use the same base style as the committed Tideling sprites so the mentor belongs in the same
world while still reading as a guide character.

```text
Use case: illustration-story
Asset type: Tidepool game mentor NPC sprite
Primary request: Children's book illustration of a friendly tidepool guide character, soft watercolor and ink style, warm daylight, gentle rounded shapes, front three-quarter view, centered, full body. [concept-specific request]
Scene/backdrop: perfectly flat solid #ff00ff chroma-key background for background removal.
Composition/framing: single character only, centered, full body, generous padding, character fills about 80% of the frame.
Lighting/mood: warm daylight, calm, curious, and welcoming.
Constraints: no text, no border, no watermark, no cast shadow, no contact shadow, no reflection. Background must be one uniform #ff00ff color with no gradients, shadows, texture, floor plane, or lighting variation. Do not use #ff00ff anywhere in the character. Avoid protected franchise references, existing character resemblance, red/white capture-device imagery, and living artist styles.
```

## Option 1: Drift the Heron

Drift is a tall, patient shore heron who watches the tidepools from the edge of the water. She
tilts her head when Reagan finds something interesting and points with her beak or a long step
toward the next place to explore.

Personality:
- Patient and observant.
- Speaks in short, noticing-focused lines.
- Feels like a quiet field guide who waits for the player to discover things.

Visual description:
- Tall soft-gray heron with long legs, gentle eyes, and a small shell pin tied to a simple
  neck ribbon.
- Holds a tiny waterproof field notebook tucked under one wing.
- Rounded silhouette, no sharp or stern posture.

Voice sample:

```text
The meadow is waving today. Want to see what lives there?
```

Prompt:

```text
Use case: illustration-story
Asset type: Tidepool game mentor NPC sprite
Primary request: Children's book illustration of a friendly tidepool guide character, soft watercolor and ink style, warm daylight, gentle rounded shapes, front three-quarter view, centered, full body. A tall patient shore heron with soft gray feathers, long legs, gentle eyes, a tiny shell pin on a simple neck ribbon, and a small waterproof field notebook tucked under one wing, calm and welcoming.
Scene/backdrop: perfectly flat solid #ff00ff chroma-key background for background removal.
Composition/framing: single character only, centered, full body, generous padding, character fills about 80% of the frame.
Lighting/mood: warm daylight, calm, curious, and welcoming.
Constraints: no text, no border, no watermark, no cast shadow, no contact shadow, no reflection. Background must be one uniform #ff00ff color with no gradients, shadows, texture, floor plane, or lighting variation. Do not use #ff00ff anywhere in the character. Avoid protected franchise references, existing character resemblance, red/white capture-device imagery, and living artist styles.
```

## Option 2: Pebble the Sea Turtle

Pebble is a small old sea turtle who knows the pools slowly and carefully. She moves like she
has nowhere urgent to be, which makes her a good fit for the calm collection loop.

Personality:
- Gentle, unhurried, and specific.
- Gives place hints without sounding like instructions.
- Feels like the keeper of old tidepool paths.

Visual description:
- Small ancient sea turtle with a rounded shell carrying a tiny garden of mossy sea plants.
- Warm eyes, soft flippers, and a little tidepool notebook strapped lightly to the shell.
- Friendly, sturdy silhouette that echoes Mossback without looking like a Tideling.

Voice sample:

```text
The kelp listens when you learn the meadow first.
```

Prompt:

```text
Use case: illustration-story
Asset type: Tidepool game mentor NPC sprite
Primary request: Children's book illustration of a friendly tidepool guide character, soft watercolor and ink style, warm daylight, gentle rounded shapes, front three-quarter view, centered, full body. A small ancient sea turtle guide with warm eyes, soft flippers, a rounded shell carrying a tiny garden of mossy sea plants, and a little waterproof tidepool notebook strapped lightly to the shell, gentle and unhurried.
Scene/backdrop: perfectly flat solid #ff00ff chroma-key background for background removal.
Composition/framing: single character only, centered, full body, generous padding, character fills about 80% of the frame.
Lighting/mood: warm daylight, calm, curious, and welcoming.
Constraints: no text, no border, no watermark, no cast shadow, no contact shadow, no reflection. Background must be one uniform #ff00ff color with no gradients, shadows, texture, floor plane, or lighting variation. Do not use #ff00ff anywhere in the character. Avoid protected franchise references, existing character resemblance, red/white capture-device imagery, and living artist styles.
```

## Option 3: Barnacle Bill

Barnacle Bill is an old tidepool lobster who sounds a little grumbly but is always kind
underneath. He can make directions feel playful, but his voice needs the most care so it stays
warm and never scolding.

Personality:
- Affectionately gruff, warm underneath.
- Gives directions with small jokes and simple place names.
- Best if the game wants a more comic mentor than a quiet one.

Visual description:
- Old barnacle-speckled lobster with one larger waving claw, rounded friendly features, and a
  little kelp satchel for notes.
- Warm expression, no menace, no sharp aggressive posing.
- Weathered tidepool colors: coral, stone gray, and soft kelp green.

Voice sample:

```text
That kelp is thick today. Meadow first, then we will peek again.
```

Prompt:

```text
Use case: illustration-story
Asset type: Tidepool game mentor NPC sprite
Primary request: Children's book illustration of a friendly tidepool guide character, soft watercolor and ink style, warm daylight, gentle rounded shapes, front three-quarter view, centered, full body. An old barnacle-speckled lobster guide with one larger waving claw, rounded friendly features, warm eyes, a small kelp satchel for notes, coral and stone gray shell colors, affectionate and welcoming, not scary.
Scene/backdrop: perfectly flat solid #ff00ff chroma-key background for background removal.
Composition/framing: single character only, centered, full body, generous padding, character fills about 80% of the frame.
Lighting/mood: warm daylight, calm, curious, and welcoming.
Constraints: no text, no border, no watermark, no cast shadow, no contact shadow, no reflection. Background must be one uniform #ff00ff color with no gradients, shadows, texture, floor plane, or lighting variation. Do not use #ff00ff anywhere in the character. Avoid protected franchise references, existing character resemblance, red/white capture-device imagery, and living artist styles.
```

## Recommendation

Pebble is the safest first pick. She fits the tidepool fiction, keeps the tone calm, and reads
as a mentor without implying competition or authority. Drift is the strongest visual silhouette
if the mentor needs to be recognizable at small UI sizes. Barnacle Bill is the most playful but
needs careful copy review to avoid sounding too gruff for the game's gentle tone.

## Approval Notes

- Ryan should pick one concept before any sprite is generated.
- The two unpicked concepts should remain archived in this doc for future reference only.
- After generation, add the chosen sprite under `Assets/Art/Characters/`, keep Unity-generated
  `.meta` files, verify Git LFS, and update `Assets/ASSET_MANIFEST.md` from proposed prompt to
  committed asset row.
