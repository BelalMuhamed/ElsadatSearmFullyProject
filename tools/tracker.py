"""Update Docs/docs/refactor/TRACKER.xlsx the same way in every session.

Keeps the sheets' formatting, validation and the summary formulas: it only
writes cell values (and copies the date format from the row above).

Usage (run from the repo root; on Windows use `py -3` instead of `python3`):
  python3 tools/tracker.py next
  python3 tools/tracker.py show F1-S2
  python3 tools/tracker.py session F1-S2 --status جاري --start today
  python3 tools/tracker.py session F1-S2 --status خلص --end today --commit abc1234 --notes "build ناجح ..."
  python3 tools/tracker.py remove F2-S2            # a cancelled session; keep its story in the README
  python3 tools/tracker.py decision D20 --status اتقرر --choice "(ج) ..." --date today --session F1-S2 --impact "Program.cs"

Requires: pip install openpyxl
"""
from __future__ import annotations

import argparse
import datetime as dt
import sys
from pathlib import Path

import openpyxl

TRACKER = Path(__file__).resolve().parent.parent / "Docs" / "docs" / "refactor" / "TRACKER.xlsx"

SESSION_STATUSES = ("لم يبدأ", "جاري", "متوقف", "خلص")
DECISION_STATUSES = ("اتقرر", "مفتوح")
NON_SESSION_SHEETS = ("الملخص", "القرارات")
DECISIONS_SHEET = "القرارات"

# Session sheets: one row per session (columns are 1-based).
S_CODE, S_STATUS, S_START, S_END, S_COMMIT, S_NOTES = 1, 7, 8, 9, 10, 12
S_HEADERS = ("الكود", "الجزء", "النوع", "الهدف", "المخرجات", "بيعتمد على",
             "الحالة", "البداية", "الخلاص", "Commit", "README", "ملاحظات")
# Decisions sheet.
D_CODE, D_TITLE, D_STATUS, D_CHOICE, D_DATE, D_SESSION, D_IMPACT = 1, 2, 3, 4, 5, 6, 7


def parse_date(value: str) -> dt.datetime:
    if value == "today":
        return dt.datetime.combine(dt.date.today(), dt.time())
    return dt.datetime.strptime(value, "%Y-%m-%d")


def session_sheets(wb):
    return [ws for ws in wb.worksheets if ws.title not in NON_SESSION_SHEETS]


def find_session(wb, code: str):
    for ws in session_sheets(wb):
        for row in range(2, ws.max_row + 1):
            if ws.cell(row, S_CODE).value == code:
                return ws, row
    sys.exit(f"session {code} not found in TRACKER.xlsx")


def print_session(ws, row: int) -> None:
    print(f"[{ws.title}]")
    for col, name in enumerate(S_HEADERS, start=1):
        value = ws.cell(row, col).value
        if isinstance(value, dt.datetime):
            value = value.date().isoformat()
        print(f"  {name}: {value if value is not None else '—'}")


def set_date(ws, row: int, col: int, value: dt.datetime) -> None:
    cell = ws.cell(row, col)
    cell.value = value
    above = ws.cell(row - 1, col) if row > 2 else None
    cell.number_format = above.number_format if above is not None and above.number_format != "General" else "yyyy-mm-dd"


def cmd_next(wb, _args) -> None:
    for ws in session_sheets(wb):
        for row in range(2, ws.max_row + 1):
            code = ws.cell(row, S_CODE).value
            if code and ws.cell(row, S_STATUS).value != "خلص":
                print_session(ws, row)
                return
    print("all sessions are «خلص»")


def cmd_show(wb, args) -> None:
    if args.code.startswith("D"):
        ws = wb[DECISIONS_SHEET]
        for row in range(2, ws.max_row + 1):
            if ws.cell(row, D_CODE).value == args.code:
                print([ws.cell(row, c).value for c in range(1, D_IMPACT + 1)])
                return
        sys.exit(f"decision {args.code} not found")
    print_session(*find_session(wb, args.code))


def cmd_session(wb, args) -> bool:
    ws, row = find_session(wb, args.code)
    if args.status:
        if args.status not in SESSION_STATUSES:
            sys.exit(f"status must be one of {SESSION_STATUSES}")
        ws.cell(row, S_STATUS).value = args.status
    if args.start:
        set_date(ws, row, S_START, parse_date(args.start))
    if args.end:
        set_date(ws, row, S_END, parse_date(args.end))
    if args.commit:
        current = ws.cell(row, S_COMMIT).value
        commits = [c.strip() for c in str(current).split(",")] if current else []
        for c in args.commit:
            if c not in commits:
                commits.append(c)
        ws.cell(row, S_COMMIT).value = ", ".join(commits)
    if args.notes is not None:
        ws.cell(row, S_NOTES).value = args.notes
    print_session(ws, row)
    return True


def cmd_remove(wb, args) -> bool:
    ws, row = find_session(wb, args.code)
    if ws.cell(row, S_STATUS).value == "خلص":
        sys.exit(f"{args.code} is «خلص»: finished sessions are not removed")
    print_session(ws, row)
    ws.delete_rows(row)
    if ws.auto_filter.ref:  # the header row stays, the last row is one less
        first, last = ws.auto_filter.ref.split(":")
        column = "".join(ch for ch in last if ch.isalpha())
        ws.auto_filter.ref = f"{first}:{column}{ws.max_row}"
    return True


def cmd_decision(wb, args) -> bool:
    ws = wb[DECISIONS_SHEET]
    target = next((r for r in range(2, ws.max_row + 1) if ws.cell(r, D_CODE).value == args.code), None)
    if target is None:
        target = ws.max_row + 1
        for col in range(1, D_IMPACT + 1):  # new row looks like the one above
            ws.cell(target, col)._style = ws.cell(target - 1, col)._style
        ws.cell(target, D_CODE).value = args.code
    if args.status:
        if args.status not in DECISION_STATUSES:
            sys.exit(f"status must be one of {DECISION_STATUSES}")
        ws.cell(target, D_STATUS).value = args.status
    for value, col in ((args.title, D_TITLE), (args.choice, D_CHOICE),
                       (args.session, D_SESSION), (args.impact, D_IMPACT)):
        if value is not None:
            ws.cell(target, col).value = value
    if args.date:
        ws.cell(target, D_DATE).value = parse_date(args.date).date().isoformat()  # sheet keeps dates as text
    print([ws.cell(target, c).value for c in range(1, D_IMPACT + 1)])
    return True


def main() -> None:
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")  # Arabic on the Windows console
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = parser.add_subparsers(dest="cmd", required=True)
    sub.add_parser("next", help="first session not «خلص»")
    p = sub.add_parser("show", help="show a session (F1-S2) or a decision (D20)")
    p.add_argument("code")
    p = sub.add_parser("session", help="update a session row")
    p.add_argument("code")
    p.add_argument("--status", help=" / ".join(SESSION_STATUSES))
    p.add_argument("--start", help="YYYY-MM-DD or today")
    p.add_argument("--end", help="YYYY-MM-DD or today")
    p.add_argument("--commit", nargs="+", help="short hashes, appended (no duplicates)")
    p.add_argument("--notes", help="replaces the notes cell")
    p = sub.add_parser("remove", help="remove a cancelled session row")
    p.add_argument("code")
    p = sub.add_parser("decision", help="add or update a decision row")
    p.add_argument("code")
    p.add_argument("--title")
    p.add_argument("--status", help=" / ".join(DECISION_STATUSES))
    p.add_argument("--choice")
    p.add_argument("--date", help="YYYY-MM-DD or today")
    p.add_argument("--session")
    p.add_argument("--impact")
    args = parser.parse_args()

    wb = openpyxl.load_workbook(TRACKER)
    changed = {"next": cmd_next, "show": cmd_show, "session": cmd_session, "remove": cmd_remove,
               "decision": cmd_decision}[args.cmd](wb, args)
    if changed:
        wb.save(TRACKER)


if __name__ == "__main__":
    main()
