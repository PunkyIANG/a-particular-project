from genericpath import exists
import click
import shutil
import os
import subprocess

MSBUILD_INTERMEDIATE_OUTPUT_PATH = ""
MSBUILD_OUTPUT_PATH = ""
CUSTOM_LOCAL_FEED = ""
GIT_SOURCE_HOOKS_PATH = ""
MSBUILD_DOT_NUGET = ""
DOT_GIT_HOOKS_PATH = ""
UNITY_NUGET_PACKAGES_PATH = ""

def set_global(name, value):
    globals()[name] = value
    os.environ[name] = value

@click.group()
@click.option("-build_directory", envvar="BUILD_DIRECTORY", default=os.path.abspath("Build"))
@click.option("-local_feed_directory", envvar="CUSTOM_LOCAL_FEED", default=os.path.abspath("NuGet_Packages"))
@click.option("-unity_nuget_packages", default=os.path.abspath("Game/Assets/Packages"))
# @click.option("-MSBUILD_INTERMEDIATE_OUTPUT_PATH", default=os.path.abspath("Build/obj"))
# @click.option("-MSBUILD_OUTPUT_PATH", default=os.path.abspath("Build/bin"))
# @click.option("-CUSTOM_LOCAL_FEED", default=os.path.abspath("Build/bin/Packages"))
def main(build_directory, local_feed_directory, unity_nuget_packages):
    """Prepares environment and global variables"""
    set_global("MSBUILD_INTERMEDIATE_OUTPUT_PATH", os.path.join(build_directory, "obj"))
    set_global("MSBUILD_OUTPUT_PATH", os.path.join(build_directory, "bin"))
    set_global("MSBUILD_DOT_NUGET", os.path.join(build_directory, ".nuget"))
    set_global("CUSTOM_LOCAL_FEED", local_feed_directory)
    set_global("UNITY_NUGET_PACKAGES_PATH", unity_nuget_packages)
    set_global("GIT_SOURCE_HOOKS_PATH", os.path.abspath("git_hooks"))
    set_global("DOT_GIT_HOOKS_PATH", os.path.abspath(".git/hooks"))


@main.command('fresh')
def fresh():
    """Does a clean build"""
    copy_hooks.callback()
    build_kari.callback(clean=True)


@main.command('hooks')
def copy_hooks():
    """Copies github hooks from git_hooks"""
    copy_all_files(GIT_SOURCE_HOOKS_PATH, DOT_GIT_HOOKS_PATH)
    print("Copied github hooks")


@main.command('kari')
@click.option('-clean', is_flag=True)
def build_kari(clean):
    """Builds the Kari code generator"""
    # Clear all previous output
    if clean:
        try_delete(MSBUILD_INTERMEDIATE_OUTPUT_PATH)
        try_delete(MSBUILD_OUTPUT_PATH)
    
    # A list of projects to be compiled into nuget packages
    # By convention, also their assembly names
    nuget_projects = ["Kari.Generators", "Kari.Shared", "Kari"]

    try:
        os.chdir("Kari")
        try_make_dir(CUSTOM_LOCAL_FEED)

        # Invoke nuget packing commands
        for p in nuget_projects:
            run_sync(f'dotnet pack {p}')
        
        # TODO: actually test if it works
        run_sync("dotnet run -p Kari.Test")

        for p in nuget_projects:
            dest_dir   = os.path.join(UNITY_NUGET_PACKAGES_PATH, p)
            source_dir = os.path.join(MSBUILD_DOT_NUGET, p)
            copy_tree_if_modified(dest_dir, source_dir)

    except subprocess.CalledProcessError as err:
        print(f'Build process exited with error code {err.returncode}')

    finally:
        os.chdir("..")


def copy_tree_if_modified(dest_dir, source_dir):

    if os.path.exists(dest_dir):
        modification_time_source = os.stat(source_dir).st_mtime
        modification_time_dest   = os.stat(dest_dir).st_mtime

        # Check if the source has changed, i.e. it is older than the destination
        if modification_time_dest >= modification_time_source:
            return

        # TODO: ?
        # Check if the files match
        # If they do not match, rewrite to the destination

        shutil.rmtree(dest_dir)

    shutil.copytree(source_dir, dest_dir)


def copy_all_files(source_dir, dest_dir):
    scripts = os.listdir(source_dir)

    os.makedirs(source_dir, exist_ok=True)

    for script in scripts:
        source_file = os.path.join(source_dir, script)
        dest_file   = os.path.join(dest_dir, script)

        if os.path.exists(dest_file):
            # in case of the src and dst are the same file
            if os.path.samefile(source_file, dest_file):
                continue
            os.remove(dest_file)

        shutil.copyfile(source_file, dest_file)

def try_delete(file_path):
    shutil.rmtree(file_path, ignore_errors = True)

def run_command_generator(command):
    with subprocess.Popen(command, stdout=subprocess.PIPE, bufsize=1, universal_newlines=True) as p:
        for line in p.stdout:
            yield line 

    if p.returncode != 0:
        raise subprocess.CalledProcessError(p.returncode, command)

def run_command_sync(command):
    print(command)
    for output_line in run_command_generator(command):
        print(output_line, end="")

run_sync = run_command_sync


def try_make_dir(path):
    os.makedirs(path, exist_ok = True)


if __name__ == '__main__':
    main()