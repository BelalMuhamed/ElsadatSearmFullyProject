---
description: Start a work session — load context and propose the next step
---
1. `git status` and `git pull origin fix/backend` (repo root). If there are local changes you didn't make, stop and ask me.
2. Read `CLAUDE.md`, then `PROJECT_CONTEXT.md` starting from its **Checkpoint**, then run `python tools/tracker.py next` (or `py -3 …`) and read that session's section in `Docs/docs/refactor/README.md` and its part in `Docs/docs/refactor/PLAN.md`.
3. Read the actual code the next part touches (backend root: `New folder (5)/AlSadat-Seram/AlSadat-Seram-Project/AlSadat-Seram`). Run `dotnet build AlSadat-Seram.slnx` there once so we know the starting state.
4. Reply in Egyptian Arabic with: (1) where we stopped, including any uncommitted work and the build result, (2) the part we're doing now (continue from the Checkpoint's "In progress / next part" — never redo finished parts), (3) the business + current state + exact files and changes you propose, (4) any decision you need from me as numbered options with the impact of each and your recommendation.
Do not change any file until I approve.
$ARGUMENTS
