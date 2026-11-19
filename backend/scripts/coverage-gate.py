#!/usr/bin/env python3
"""Coverage gate for CI (docs/09-testing.md §8, roadmap 3.9).

Merges every Cobertura report produced by `dotnet test --coverage --coverage-output-format cobertura`
(one per test project), counts a source line as covered when any test project hit it, then checks
line coverage per assembly against the thresholds below. Exits 1 when any threshold is missed.

Usage: python3 scripts/coverage-gate.py TestResults
"""
import glob
import os
import sys
import xml.etree.ElementTree as ET
from collections import defaultdict

# Minimum line coverage (%) per assembly and for Domain + Application + Infrastructure + Api combined.
THRESHOLDS = {
    "SnackShop.Domain": 90.0,
    "SnackShop.Application": 80.0,
}
TOTAL_THRESHOLD = 75.0
GATED_PREFIX = "SnackShop."
EXCLUDED_PATH_PARTS = ("Migrations", "/obj/", "\\obj\\")


def main() -> int:
    root = sys.argv[1] if len(sys.argv) > 1 else "TestResults"
    reports = glob.glob(os.path.join(root, "**", "*.cobertura.xml"), recursive=True)
    if not reports:
        print(f"::error::No Cobertura reports under {root}")
        return 1

    # assembly -> (file, line) -> covered?
    lines: dict[str, dict[tuple[str, int], bool]] = defaultdict(dict)
    for report in reports:
        for package in ET.parse(report).getroot().iter("package"):
            assembly = package.get("name", "")
            if not assembly.startswith(GATED_PREFIX) or ".Tests" in assembly or "Tests" in assembly.split(".")[-1]:
                continue
            for cls in package.iter("class"):
                filename = cls.get("filename", "")
                if any(part in filename for part in EXCLUDED_PATH_PARTS):
                    continue
                for line in cls.iter("line"):
                    key = (filename, int(line.get("number", "0")))
                    hit = int(line.get("hits", "0")) > 0
                    lines[assembly][key] = lines[assembly].get(key, False) or hit

    failed = False
    total_covered = total_lines = 0
    print(f"{'Assembly':<32}{'Lines':>8}{'Covered':>9}{'%':>8}  Min")
    for assembly in sorted(lines):
        covered = sum(lines[assembly].values())
        count = len(lines[assembly])
        total_covered += covered
        total_lines += count
        pct = 100.0 * covered / count if count else 100.0
        minimum = THRESHOLDS.get(assembly)
        status = ""
        if minimum is not None and pct < minimum:
            status = "  FAIL"
            failed = True
            print(f"::error::{assembly} line coverage {pct:.1f}% < {minimum}%")
        print(f"{assembly:<32}{count:>8}{covered:>9}{pct:>7.1f}%  {minimum if minimum is not None else '-'}{status}")

    total_pct = 100.0 * total_covered / total_lines if total_lines else 100.0
    print(f"{'TOTAL':<32}{total_lines:>8}{total_covered:>9}{total_pct:>7.1f}%  {TOTAL_THRESHOLD}")
    if total_pct < TOTAL_THRESHOLD:
        print(f"::error::Total line coverage {total_pct:.1f}% < {TOTAL_THRESHOLD}%")
        failed = True

    summary = os.environ.get("GITHUB_STEP_SUMMARY")
    if summary:
        with open(summary, "a", encoding="utf-8") as out:
            out.write("### Coverage\n\n| Assembly | Line % | Min |\n|---|---:|---:|\n")
            for assembly in sorted(lines):
                count = len(lines[assembly])
                pct = 100.0 * sum(lines[assembly].values()) / count if count else 100.0
                out.write(f"| {assembly} | {pct:.1f} | {THRESHOLDS.get(assembly, '-')} |\n")
            out.write(f"| **Total** | **{total_pct:.1f}** | {TOTAL_THRESHOLD} |\n")

    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
