---
name: unity-play-qa
description: Drive Tidepool's actual Unity Play Mode to verify golden paths (Boot → Overworld → encounter → catch → journal → contest → character select, etc.) instead of just checking that the project compiles. Connects to the running Editor via Unity's Pipeline CLI package, drives movement and UI through eval/reflection and simulated clicks, screenshots each step, reads console logs, safely isolates unrelated local file drift before testing, and files GitHub issues (repo conventions included) for any bugs found. Use this whenever asked to "QA the game," "test the golden paths," "check things still work," or before tagging a playable/release build — not for pure compile/lint checks, which `scripts/verify-*.sh` and `unity test` already cover.
---

# Unity Play Mode QA (Tidepool)

Compiling and passing `EditMode` tests is not the same as the game working. This
skill drives the actual running Editor in Play Mode — moves the player,
triggers encounters, plays the catch mini-game, opens the Journal, runs a
contest, picks a character — and looks at what actually renders and logs.

## 0. Before touching anything: check for local drift

This repo has a history of stale, uncommitted local changes to Unity-generated
files (`ProjectSettings/EditorBuildSettings.asset`, species `.asset` files,
scene `.unity` files) that silently break things — missing scenes from Build
Settings, stripped contest-move references, reverted scene edits — without
being real bugs in `main`. Testing against a dirty tree produces false bug
reports.

```bash
git status --short
git diff --stat            # anything unexpected modified?
```

For any modified file that isn't something you (or the user) are actively
working on in this session, **stash it, don't discard it**, with a clear label:

```bash
git stash push -u -m "local drift: <what> (pre-existing before this QA session)" -- <path>
```

Test against the resulting clean tree. Leave the stash in place afterward and
tell the user it exists (`git stash list`) — let them decide whether to
restore or drop it. Never `git checkout`/`reset` these away.

If you find something that *looks* like a bug, double-check `git diff` on the
specific file(s) involved before writing it up. A bug that only reproduces
against locally-modified files isn't a bug in the shipped game.

## 1. Connect to the Editor

The Unity CLI wrapper lives at `~/.unity/bin/unity` (or `unity` on `PATH`).
Editor remote-control (Play Mode, eval, screenshots, scene inspection) needs
Unity's **Pipeline** package installed in the project:

```bash
unity pipeline install --project-path .   # idempotent; installs/updates com.unity.pipeline
```

This is a dev-only Editor package (passes `scripts/verify-no-network-guardrails.sh`).
If it wasn't already in `Packages/manifest.json`, ask the user whether to keep
it installed for future sessions or remove it when you're done — don't decide
silently either way.

Launch the Editor GUI (skip if one is already running — check `unity status`):

```bash
unity open . --non-interactive
```

Poll until it's connected (Pipeline package needs a domain reload to register
after a fresh install — can take 1-3 minutes):

```bash
for i in $(seq 1 40); do
  out=$(unity status --format json 2>&1)
  echo "$out" | grep -q '"pid"' && { echo "READY: $out"; break; }
  sleep 5
done
```

Prefer `Monitor` (poll loop) or `Bash` with `run_in_background` over blocking
`sleep` calls in the foreground.

Once connected, `unity list --format json` enumerates every available remote
command (140+: scene/GameObject inspection, `eval`, screenshots, build
settings, console logs, etc.) — check it if you need something not covered
below.

## 2. Drive the game

Open the scene you want to start from, clear the console, enter Play:

```bash
unity command open_scene --path "Assets/Scenes/Boot.unity" --format json
unity command clear_console --format json
unity command editor_play --format json
```

**Movement / input**: there's no raw tap/click-injection tool exposed. Drive
gameplay by invoking the same code paths a tap or click would, via `eval`
(Roslyn C#, runs in the live Editor process):

- Public API on a component you already found (`find_gameobjects` /
  `get_scene_hierarchy`): call it directly, e.g. `PlayerGridMover.MoveTo(cell)`,
  `CatchEncounterController.TryJarTap()`.
- A UI button: find it (`GameObject.Find("/Canvas/SafeArea/JournalButton")`)
  and call `.GetComponent<Button>().onClick.Invoke()`.
- A rare/random event (encounter roll, etc.): don't wait on RNG — invoke the
  private trigger method via reflection so you're still exercising the real
  code path, e.g.:

```csharp
var dir = UnityEngine.Object.FindObjectsByType<Tidepool.Runtime.EncounterDirector>(UnityEngine.FindObjectsSortMode.None)[1];
var m = typeof(Tidepool.Runtime.EncounterDirector).GetMethod("StartEncounter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
m.Invoke(dir, null);
```

Use `Object.FindAnyObjectByType<T>()` / `FindObjectsByType<T>(FindObjectsSortMode.None)`
(the `FindObjectOfType` family is obsolete and fails compilation in `eval`).
Namespaces: gameplay types are mostly `Tidepool.Runtime.*`, `Tidepool.Domain.*`,
UI controllers are `Tidepool.UI.*` — check with a quick `grep` if a class isn't
found where expected.

After each meaningful step, check for problems before moving on:

```bash
unity command get_console_logs --severity error --limit 1000 --format json
unity command list_open_scenes --format json   # confirm expected scenes loaded/unloaded
```

(Duplicate-EventSystem warnings on additive scene loads are a known, already-filed
issue — expected noise, not a new finding, unless the behavior itself changes.)

## 3. Capture evidence

```bash
unity command capture_game_view --source screen --save_path "Temp/qa-NN-name.png" --format json
```

Use `source=screen` (not the default `camera`) to include Screen Space -
Overlay UI (HUD buttons, panels) — `camera` misses it silently.

`save_path` is relative to the **Assets/** authoring root, so this writes into
`Assets/Temp/` as a real tracked-looking asset (with a `.meta` file). Before
finishing the session:

```bash
unity command delete_asset --asset "Assets/Temp" --confirm true --format json
```

Copy screenshots you want to keep or hand to the user out to your scratchpad
directory *before* deleting `Assets/Temp` — `Read` the PNG to look at it
yourself first (don't just assume the render is correct).

## 4. Wrap up

```bash
unity command editor_stop --format json
git status --short   # should show nothing but intentional changes + any Pipeline package install
```

Report clearly: what you drove and confirmed working, what broke (with
screenshots), and any local drift you stashed instead of testing against.

## 5. Filing bugs found

This repo tracks work as GitHub issues under version-labeled epics
(`gh issue list --label epic`). Match existing conventions:

- **Title**: `[vX.Y] Verb phrase describing the fix` — match the version
  label to the feature area (check `gh label list` for `v0.1`..`v0.7`, or use
  the general `v0.1` "polish, QA, and handoff" epic for cross-cutting bugs).
- **Labels**: `bug` + the matching version label.
- **Body**: `## Description` (what's broken, root cause if known, file:line),
  `### Steps to Reproduce`, `### Expected`, `### Actual`, `### Acceptance
  Criteria` checklist, then `Epic: #N` linking the matching `[Epic] vX.Y — ...`
  issue.

```bash
gh issue create --title "[v0.X] ..." --label "bug,v0.X" --body "$(cat <<'EOF'
...
EOF
)"
```

**Screenshots**: `gh gist create` (the usual way to get a real
`githubusercontent.com` image URL into an issue body without polluting the
repo) is blocked by the auto-mode permission classifier in this environment —
don't fight it or look for workarounds. Ask the user how they want it handled:
send the screenshots back to them to drag-and-drop into the issue (simplest,
no repo footprint), commit them into the repo via Git LFS under a docs/
folder, or retry gist creation with their explicit approval.
