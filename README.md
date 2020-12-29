# cmd-file-utilities

The utilities commands are made to be as easy and humanly readable as possible. They are intended to be used directly in the explorer address bar.

# Installaton
To install, simply put the exacutables in a folder that is in the `PATH` environment variable.

You can find the latest executables in the [releases page](https://github.com/MMauro94/cmd-file-utilities/releases).

# Utilities doc
Here you'll the documentation of each utiliy.

## flatten
The `flatten` command is useful when you have a lot of folders you want to extract the content of in the parent folder.

Optional arguments:
* `recursive`: flattens out the directory recursiveley
* `keepfolders`: doesn't remove the emptied out folders after finishing
* Duplicates files behavior. Must be one of:
  * `keepdups`: leaves files which have duplicate names where they are
  * `deletedups`: deletes files which have duplicate names
  * `renamedups`: renames the files which have duplicates names adding a progressive number (e.g. `file.png` becomes `file_1.png`, `file_2.png` and so on)
  * If no parameter is specified, the program will halt and report back before touching anything.
