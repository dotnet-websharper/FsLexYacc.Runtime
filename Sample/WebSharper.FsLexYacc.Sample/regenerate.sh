#!/usr/bin/env bash
# Regenerates the committed lexers/parsers from the .fsl/.fsy sources and
# re-applies the [<WebSharper.JavaScript>] attribute that fsyacc does not emit
# (fslex picks it up from the .fsl header).
#
# Requires the FsLexYacc 13.0 toolkit; set FSLEXYACC_TOOLS to its build folder,
# e.g. FSLEXYACC_TOOLS=~/.nuget/packages/fslexyacc/13.0.0/build ./regenerate.sh
set -euo pipefail

TOOLS="${FSLEXYACC_TOOLS:-$HOME/.nuget/packages/fslexyacc/13.0.0/build}"

dotnet "$TOOLS/fsyacc/net10.0/fsyacc.dll" -o JsonParser.fs --module JsonParser JsonParser.fsy
dotnet "$TOOLS/fsyacc/net10.0/fsyacc.dll" -o HelloParser.fs --module HelloParser HelloParser.fsy
dotnet "$TOOLS/fslex/net10.0/fslex.dll" -o JsonLexer.fs --unicode JsonLexer.fsl
dotnet "$TOOLS/fslex/net10.0/fslex.dll" -o HelloLexer.fs --unicode HelloLexer.fsl
dotnet "$TOOLS/fslex/net10.0/fslex.dll" -o AsciiLexer.fs AsciiLexer.fsl

rm -f *.fsi

# fsyacc emits `module X` on the second line; mark it for WebSharper.
python3 - <<'PY'
import pathlib
for name in ["JsonParser.fs", "HelloParser.fs"]:
    p = pathlib.Path(name)
    lines = p.read_text().splitlines(keepends=True)
    for i, line in enumerate(lines):
        if line.startswith("module "):
            lines.insert(i, "[<WebSharper.JavaScript>]\n")
            break
    p.write_text("".join(lines))
    print("patched", name)
PY
