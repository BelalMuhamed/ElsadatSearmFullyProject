---
description: End a session — update PROJECT_CONTEXT.md, the tracker and the README so the next session (any account/device) can continue
---
1. Make sure the last part is built and tested (`dotnet build` / `dotnet test` in `BACKEND`) and note the real result.
2. Update `PROJECT_CONTEXT.md` (repo root):
   - Tick finished items in "Refactor status" and set the next step.
   - Rewrite the **Checkpoint** for the next session (what's next, uncommitted changes, open questions).
   - Update "Known issues" if needed.
   - Add one line to "Session log": date, what was done (files touched), what's left.
   - Record any new decision I took (and update the decisions block in `CLAUDE.md` if it's a lasting rule).
3. Update the tracker with `python tools/tracker.py session <code> …` (status, dates, commits, notes) and `python tools/tracker.py decision <Dn> …` for new decisions. A session is «خلص» only when its goal is met and the build/tests pass.
4. Update the session's section in `Docs/docs/refactor/README.md` (what changed, commits, how I can verify it).
5. Commit and push the docs updates to `fix/backend`.
Keep it short and factual. Then summarize the change to me in Egyptian Arabic (2–4 lines).
$ARGUMENTS
