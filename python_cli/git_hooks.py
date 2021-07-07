import re
from git import Repo
from git.diff import Diff, DiffIndex
import os

def precommit():

    pass

if __name__ == '__main__':
    repo = Repo("E:/Coding/C#/some_project")

    # name of the current head
    # print(repo.head.reference)

    # print(repo.head.commit)

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
    #

    index = repo.index

    extra_files = {}

    META : int = 0
    FILE : int = 1

    diff : DiffIndex = index.diff(repo.head.commit)

    def is_ignored_by_unity(file : Diff) -> bool:
        first = os.path.basename(file.a_path)[0]
        return first in ['.', '~']

    def is_in_assets(file : Diff) -> bool:
        return file.a_path.startswith('Game/Assets')

    def is_meta(file : Diff) -> bool:
        return file.a_path.endswith('.meta') \
            and len(file.a_path) > 5  # simply '.meta' is still a valid file? 

    def strip_meta(file : Diff) -> str:
        return file.a_path[:-5]

    def update_extra_files_with(key, type):
        if key in extra_files:
            value = extra_files.get(key)

            assert(value != type)
        
            extra_files.pop(key)
        else:
            extra_files[key] = type

    def is_empty(collection) -> bool:
        return collection == []


    # 'D' stands for deleted, because it was added to the index and we're comparing the index to the commit
    # which means that for new files, in order to get from the index to the latest commit we would need 
    # to remove that new file, which is why 'D' gives new files while 'A' gives deleted files.
    for file in diff.iter_change_type('D'):
        # if it is in game assets and it has been just created
        if is_in_assets(file) and not is_ignored_by_unity(file):
            if is_meta(file):
                update_extra_files_with(strip_meta(file), META)
            else:
                update_extra_files_with(file.a_path, FILE)


    for file_path, type in extra_files.items():
        if type == META:
            print('Extra meta: ' + file_path + '.meta')
        else:
            print('Missing meta for: ' + file_path)

    if not is_empty(extra_files):
        # return 1
        print('fail')
            
    # Access blob objects
    # for (_path, _stage), entry in index.entries.items():
    #     print(entry)
    # new_file_path = os.path.join(repo.working_tree_dir, 'new-file-name')
    # open(new_file_path, 'w').close()
    # index.add([new_file_path])                                             # add a new file to the index
    # index.remove(['LICENSE'])                                              # remove an existing one
    # assert os.path.isfile(os.path.join(repo.working_tree_dir, 'LICENSE'))  # working tree is untouched