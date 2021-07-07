import re
from git import Repo
from git.diff import Diff, DiffIndex
import os

OK_EXIT_CODE = 0
META_DISCREPANCY_EXIT_CODE = 1
FILE_TOO_LARGE_EXIT_CODE = 2
FILE_SIZE_LIMIT_BYTES = 1024 * 1024 * 100


def precommit(project_directory):
    repo = Repo(project_directory)

    # ---------------------------------------------------
    # The Algorithm:
    # ---------------------------------------------------
    # 1. Staged files larger than 100MB are not allowed.
    #
    # 2. Every file in the index that:
    #       - is in the Game/Assets folder,
    #       - does not end with .meta,
    #       - is not in gitignore,
    #       - starts with '.' and thus is ignored by Unity
    #  must have a dedicated .meta file.
    #
    # 3. Every file in the index that:
    #       - is in the Game/Assets folder,
    #       - ends with .meta,
    #  must have a dedicated normal file or directory
    # > Special case:
    #       - it is associated with a nonexistent in the index directory,
    #       - directory is present in the workspace, but is is empty,
    #       - directory is not in gitignore,
    #  then we create a .gitkeep and add it to the commit (or just create it and throw error).
    #
    # 4. Every folder of the tree must have a corresponding .meta
    #
    # If anything goes wrong, fail, reverting the commit.

    META : int = 0
    FILE : int = 1

    def is_ignored_by_unity(file : Diff) -> bool:
        first = os.path.basename(file.a_path)[0]
        return first in ['.', '~']

    def is_in_assets(file : Diff) -> bool:
        return file.a_path.startswith('Game/Assets')

    def should_not_be_ignored(file):
        return is_in_assets(file) and not is_ignored_by_unity(file)

    def is_meta(file : Diff) -> bool:
        return file.a_path.endswith('.meta') \
            and len(file.a_path) > 5  # simply '.meta' is still a valid file? 

    def strip_meta(filename : str) -> str:
        return filename[:-5]

    def is_empty(collection) -> bool:
        return len(collection) == 0


    class IterHelper:
        def __init__(self):
            self.extra_files     : dict[str, int] = {}
            self.undeleted_files : dict[str, int] = {}

        def _update_with(self, dictionary, key, type):
            if key in dictionary:
                value = dictionary.get(key)
                assert(value != type)
                dictionary.pop(key)
            else:
                dictionary[key] = type

        def update_add(self, key, type):
            self._update_with(self.extra_files, key, type)

        def update_delete(self, key, type):
            self._update_with(self.undeleted_files, key, type)

        def is_good(self):
            return is_empty(self.extra_files) and is_empty(self.undeleted_files)

    index = repo.index
    diff : DiffIndex = index.diff(repo.head.commit)
    helper = IterHelper()

    # 'D' stands for deleted, because it was added to the index and we're comparing the index to the commit
    # which means that for new files, in order to get from the index to the latest commit we would need 
    # to remove that new file, which is why 'D' gives new files while 'A' gives deleted files.
    for file in diff.iter_change_type('D'):
        if should_not_be_ignored(file):
            if is_meta(file):
                helper.update_add(strip_meta(file.a_path), META)
            else:
                helper.update_add(file.a_path, FILE)
        
    # Same thing for deleted
    for file in diff.iter_change_type('A'):
        if should_not_be_ignored(file):
            if is_meta(file):
                helper.update_delete(strip_meta(file.a_path), META)
            else:
                helper.update_delete(file.a_path, FILE)

    # Renaming = deleting the older one and adding a newer one
    for file in diff.iter_change_type('R'):
        if should_not_be_ignored(file):
            if is_meta(file):
                helper.update_add(strip_meta(file.a_path), META)
                helper.update_delete(strip_meta(file.b_path), META)
            else:
                helper.update_add(file.a_path, FILE)
                helper.update_delete(file.b_path, FILE)

    # New metas and files
    for file_path, type in helper.extra_files.items():
        if type == META:
            # Check the empty directory case
            if os.path.splitext(file_path)[1] == '':
                # TODO: create the .keep file automatically
                print(f"Detected redundant meta potentially for an empty directory {file_path}")
                print("To fix, create a file inside of it, like an empty '.keep' file")
            else:
                print("Redundant meta: " + file_path + ".meta")
        else:
            print("Missing meta for: " + file_path)

    # Old metas and files
    for file_path, type in helper.undeleted_files.items():
        if type == META:
            print("Missing meta: " + file_path + ".meta")
        else:
            print("Redundant meta for deleted file: " + file_path)

    # Fail the first check
    if not helper.is_good():
        print('fail: meta discrepancy')
        return META_DISCREPANCY_EXIT_CODE

    is_a_file_too_large = False

    def is_file_too_large(file : Diff):
        # This gives incorrect results for non-text files, which kind of defeats the purpose
        return file.a_blob.size > FILE_SIZE_LIMIT_BYTES  # 100MB

    for file in diff.iter_change_type('D'):
        if is_file_too_large(file):
            is_a_file_too_large = True
            print(f'Added file {file.a_path} is over 100MB')

    for file in diff.iter_change_type('M'):
        if is_file_too_large(file):
            is_a_file_too_large = True
            print(f'Modified file {file.a_path} is over 100MB')

    if is_a_file_too_large:
        print('fail: file too large')
        return FILE_TOO_LARGE_EXIT_CODE

    return OK_EXIT_CODE