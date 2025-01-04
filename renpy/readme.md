I wrote check_renpy.py because I wanted to run mypy on Python code in .rpy files.

Ren'Py does offer lint and force recompile.

Lint catches errors like this in an .rpy file:

```renpy
jump no_such_label
no_such_character "test"
show chars_tom_small_happy at right with no_such_transition
```

Force recompile catches errors like this:

```renpy
python early:
    import no_such_module
```

However, neither catches errors like this:

```renpy
menu:
    "Which way should we go?"
    "Left" if state.no_such_character.mood > 8 :

$ state = day_1_code.no_such_function (state)
```

check_renpy.py extracts Python code from the following places in .rpy files:
- `python early`/`init python`/`python` blocks
- `default` statements
- Python statements denoted by `$`
- conditionals (`if`/`elif`/`while`)

Run check_renpy.py as follows:

```bash
python3 /media/data/renpy-8.3.3-sdk/forest_hike_20241123/game/user_defined/check_renpy.py --help
```

Usage:

```bash
usage: check_renpy.py [-h] [-i [INPUT]] [-o [OUTPUT]] [-ne] [-nm] [-lc LINT LINT] [-s [SCRIPT]]

optional arguments:
  -h, --help            show this help message and exit
  -i [INPUT], --input [INPUT]
                        Input folder that contains .rpy and .py files for mypy. If not specified, the current folder is used. -ne/--no-mypy overrides this option.
  -o [OUTPUT], --output [OUTPUT]
                        Temporary output file to extract Python code from .rpy files. If not specified, a default name is used. -ne/--no-extract overrides this option.
  -ne, --no-extract     Do not extract Python code from .rpy files. Overrides -o/--output.
  -nm, --no-mypy        Do not run mypy on Python files. Implies -ne/--no-extract. Overrides -i/--input and -o/--output.
  -lc LINT LINT, --lint LINT LINT
                        Run lint and compile. The first argument is the path of the folder that contains renpy.sh. The second argument is the base folder of the project. If
                        not present, lint and compile are skipped.
  -s [SCRIPT], --script [SCRIPT]
                        Path of the folder that contains script.rpy. If not specified, the --input folder is used.
```
