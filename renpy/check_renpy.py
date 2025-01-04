#!/usr/bin/env python3
import argparse
import os
import re
import subprocess
import sys
from typing import List, Optional

# References
# Python in Ren'Py
# https://www.renpy.org/doc/html/python.html
# https://www.renpy.org/doc/html/conditional.html
# lint and compile
# https://www.renpy.org/doc/html/cli.html
# Importing modules from subfolders. See "Method 3: Using the __init__.py"
# https://www.geeksforgeeks.org/python-import-module-outside-directory/

def extract_python_statements_from_file(file_path : str) -> List[str] :
    python_statements = []
    in_python_block = False
    current_block_indent : Optional[int] = None

    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

        comment_pattern = re.compile(r'^\s*#.*$')
# Note This pattern failed to match the "python early:" at the beginning of
# script.rpy because that file used encoding UTF-8 with BOM and EOL sequence
# CRLF. We changed its encoding to UTF-8 and EOL sequence to LF. After that,
# the pattern matched correctly.
        python_block_pattern = re.compile(r'^(\s|\w)*?python(\s|\w)*:\s*$')
        dollar_pattern = re.compile(r'^\s*\$\s*(.*)$')
        conditional_pattern = re.compile(r'^.*\s+(if|elif|while)\s+(.*):.*$')
# 20250104 Excluding define, because lint can check that. See
# https://www.renpy.org/doc/html/python.html
# Another advantage [of using the define statement] is that Lint will be able
# to check defined values, for example by detecting whether the same variable
# is defined twice, potentially with different values.
# (end)
# Also, define is typically used with Character, which my does not recognize.
# If you do want to include define, remember to get the content from group 2
# instead of group 1.
#        default_define_pattern = re.compile(r'^\s*(default|define)\s+(.*)$')
        default_define_pattern = re.compile(r'^\s*default\s+(.*)$')

        for i, line in enumerate(lines):
# Replace each tab with four spaces.
            stripped = line.rstrip('\n').replace('\t', '    ')

            dollar_match = dollar_pattern.search(stripped)
            conditional_match = conditional_pattern.search(stripped)
            default_define_match = default_define_pattern.search(stripped)

# Ignore comments.
            if comment_pattern.match(stripped):
                pass

# We should not check the other cases if we are already inside a python block.
# If we're in a python block, capture lines that are strictly more indented
# than the line that started the block.
# As soon as we find a line with equal or less indentation, we exit the block.
            elif in_python_block:
# Calculate current line's indent.
                line_indent = len(stripped) - len(stripped.lstrip(' '))
                if None != current_block_indent and line_indent > current_block_indent:
# This line belongs to the python block. Make sure it is not empty.
                    if stripped.strip():
# Subtract excess indentation.
                        python_statements.append(stripped[(line_indent - current_block_indent):])
                else:
# We reached the end of the block.
                    in_python_block = False
                    current_block_indent = None

            elif dollar_match:
                python_statements.append(dollar_match.group(1).strip())
            
            elif conditional_match:
                python_statements.append(conditional_match.group(2).strip())

            elif default_define_match:
                python_statements.append(default_define_match.group(1).strip())

            elif python_block_pattern.match(stripped):
# We will begin capturing all subsequent indented lines.
                in_python_block = True
# Record current line's leading spaces (indent)
                current_block_indent = len(stripped) - len(stripped.lstrip(' '))

    return python_statements

def extract(input_folder : str, output_file : str, script_folder : str) -> None :
# 1) Recursively find all .rpy files in input_folder. 2) Extract Python
# statements from each .rpy file. 3) Write them to output_file.
    all_statements = []

# Process script.rpy first.
    file_path = os.path.join(script_folder, "script.rpy")
    statements = extract_python_statements_from_file(file_path)
    all_statements.extend(statements)

# Step 1: Walk folder structure for .rpy files.
    for root, dirs, files in os.walk(input_folder):
        for f in files:
            if f.lower().endswith(".rpy"):
                file_path = os.path.join(root, f)
# Step 2: Extract Python statements from each .rpy file.
                statements = extract_python_statements_from_file(file_path)
                all_statements.extend(statements)

# Step 3: Write extracted statements to output_file.
    with open(os.path.join(input_folder, output_file), 'w', encoding='utf-8') as out:
        for statement in all_statements:
            out.write(f"{statement}\n")

def run_mypy(input_folder : str) -> None :
# Step 3: Run mypy on all .py files in the input folder, including
# <output_file>, to which we extracted the Python code from the .rpy files.
# We assume mypy is installed in the environment.
    print("mypy results:\n")
    subprocess.run(["mypy", "--python-version", "3.8", input_folder])

def main(input_folder : str, output_file : str, no_extract : bool, no_mypy : bool, lint : Optional[List[str]], script_folder : str) -> None :
    if False == no_mypy :
        if False == no_extract :
            extract (input_folder, output_file, script_folder)
        run_mypy (input_folder)

    if None != lint and len(lint) > 1 :
        renpy_folder = lint[0]
        project_folder = lint[1]
        print("\nlint results:\n")
        subprocess.run([os.path.join(renpy_folder, "renpy.sh"), project_folder, "lint"])
        print("\ncompile results:\n")
        subprocess.run([os.path.join(renpy_folder, "renpy.sh"), project_folder, "compile"])

parser = argparse.ArgumentParser()
parser.add_argument("-i", "--input", nargs="?", default=os.getcwd(), help="Input folder that contains .rpy and .py files for mypy. If not specified, the current folder is used. -ne/--no-mypy overrides this option.")
parser.add_argument("-o", "--output", nargs="?", default="TEMP_code_extracted_from_python.py", help="Temporary output file to extract Python code from .rpy files. If not specified, a default name is used. -ne/--no-extract overrides this option.")
parser.add_argument("-ne", "--no-extract", action='store_true', help="Do not extract Python code from .rpy files. Overrides -o/--output.")
parser.add_argument("-nm", "--no-mypy", action='store_true', help="Do not run mypy on Python files. Implies -ne/--no-extract. Overrides -i/--input and -o/--output.")
parser.add_argument("-lc", "--lint", nargs=2, help="Run lint and compile. The first argument is the path of the folder that contains renpy.sh. The second argument is the base folder of the project. If not present, lint and compile are skipped.")
parser.add_argument("-s", "--script", nargs="?", help="Path of the folder that contains script.rpy. If not specified, the --input folder is used.")
args = parser.parse_args()

if args.script is None :
    args.script = args.input

# Note argparse converts hyphens in arguments to underscores.
main(args.input, args.output, args.no_extract, args.no_mypy, args.lint, args.script)
