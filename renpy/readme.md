# check_renpy.py

I wrote check_renpy.py because I wanted to run mypy on Python code in .rpy files.

Ren'Py does offer lint and force recompile.

Lint catches errors like this in an .rpy file:

```renpy
jump no_such_label
no_such_character "test"
show my_char at right with no_such_transition
```

Force recompile catches errors like this:

```renpy
python early:
    import no_such_module
```

However, neither lint nor force recompile catches errors like this:

```renpy
menu:
    "Which way should we go?"
    "Left" if state.no_such_character.mood > 8 :

$ state = day_1_code.no_such_function (state)
```

check_renpy.py extracts Python code from the following places in .rpy files and saves them to a temporary .py file.
- `python early`/`init python`/`python` blocks
- `default` statements
- Python statements denoted by `$`
- conditionals (`if`/`elif`/`while`)

It then runs mypy on all .py files in the specified input folder, including the temporary .py file.
It also runs lint and forced recompile on the Ren'Py project with the specified base folder.

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

## AI

The first draft of check_renpy.py was written by OpenAI o1. I prompted it as follows:

```
1. Read all files with the extension ".rpy" in the designated folder and its subfolders.
2. For each file, extract all Python statements, append them to a list, and write the list out to the designated output file. A Python statement is indicated by any of the following:
2a. Any text that follows a dollar sign on a given line.
2b. Any text that follows the distinct word "if" on a given line.
2c. Any indented block of text that follows a line that contains the distinct word "python", with a colon as the last non-whitespace character in the line. For example:
init python:
    this line should be extracted
        this line should also be extracted
this line should not be extracted

3. Run mypy on the designated output file.
```

I initially wrote a much more detailed prompt. For example, for point 2c, I told o1 to record the indentation of the line that contained "python", then look for a line with the same or less indentation, which would tell it the python block had ended. I then realized I might as well be writing the program myself. Functional programmers like to say imperative programming is focused on "how" whereas functional programming is focused on "what". I decided I should try to apply this principle here, and gave o1 the less detailed prompt shown previously. As it turned out, o1 arrived at the same solution I had intended on its own.

I again had to do some cleanup of the code o1 produced, but it mostly involved things I had missed in the prompt.
