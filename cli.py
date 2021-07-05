from genericpath import exists
import click
import shutil
import os
import subprocess

MSBUILD_INTERMEDIATE_OUTPUT_PATH = ""
MSBUILD_OUTPUT_PATH = ""
GIT_SOURCE_HOOKS_PATH = ""
DOT_GIT_HOOKS_PATH = ""
KARI_GENERATOR_PATH = ""
PROJECT_DIRECTORY = ""

def set_global(name, value):
    globals()[name] = value
    os.environ[name] = value

@click.group()
@click.option("-build_directory", envvar="BUILD_DIRECTORY", default=os.path.abspath("Build"))
@click.option("-project_directory", envvar="PROJECT_DIRECTORY", default=os.path.abspath("."))
def main(build_directory, project_directory):
    """Prepares environment and global variables"""
    set_global("MSBUILD_INTERMEDIATE_OUTPUT_PATH", os.path.join(build_directory, "obj"))
    set_global("MSBUILD_OUTPUT_PATH", os.path.join(build_directory, "bin"))
    set_global("PROJECT_DIRECTORY", project_directory)
    set_global("GIT_SOURCE_HOOKS_PATH", os.path.join(project_directory, "git_hooks"))
    set_global("DOT_GIT_HOOKS_PATH", os.path.join(project_directory, ".git/hooks"))
    set_global("KARI_GENERATOR_PATH", f"{MSBUILD_OUTPUT_PATH}/Kari.Generator/Release/net5.0/kari.dll")


@main.command("setup")
def setup():
    """Does the setup and the initial build"""
    copy_hooks.callback()
    build_kari.callback(clean=True)


@main.command("hooks")
def copy_hooks():
    """Copies github hooks from git_hooks"""
    copy_all_files(GIT_SOURCE_HOOKS_PATH, DOT_GIT_HOOKS_PATH)
    print("Copied github hooks")


@main.group("kari")
def kari():
    """Has to do with code generation"""
    pass

@kari.command("compile")
@click.option("-clean", is_flag=True)
def build_kari(clean):
    """Builds the Kari code generator"""
    # Clear all previous output
    if clean: nuke_kari.callback()
    
    current_directory = os.curdir
    os.chdir(f"{PROJECT_DIRECTORY}/Kari")

    try:
        # run_sync("dotnet restore")
        run_sync("dotnet publish Kari.Generator/Kari.Generator.csproj --configuration Release --no-self-contained")
        
        print(f"The final dll has been written to {KARI_GENERATOR_PATH}")
        print("To run it, do `cli kari run`, passing in the flags`")
        # TODO: actually run tests
        # run_sync("dotnet run -p Kari.Test")

    except subprocess.CalledProcessError as err:
        print(f"Build process exited with error code {err.returncode}")
        return False

    finally:
        os.chdir(current_directory)
    
    return True


@kari.command(name="run", context_settings={
  "ignore_unknown_options": True
})
@click.option("-rebuild", is_flag=True)
@click.argument("unprocessed_args", nargs=-1, type=click.UNPROCESSED)
def generate_with_kari(rebuild, unprocessed_args):
    """Equivalent to calling Kari from the command line"""

    if rebuild:
        if not build_kari.callback(clean=False):
            return False

    elif not os.path.exists(KARI_GENERATOR_PATH):
        print("Initiating build, since Kari has not been built")
        if not build_kari.callback(clean=False):
            return False
    
    try:
        # Pass along all of the unparsed commands
        command = ["dotnet", KARI_GENERATOR_PATH]
        command.extend(unprocessed_args)
        run_sync(" ".join(command))

    except subprocess.CalledProcessError as err:
        print(f"Generation failed with error code {err.returncode}")
        return False
    
    return True


@kari.command("unity")
def generate_for_unity():
    """Generates code for the unity project """

    # TODO: maybe generate in a single file to minimize .meta's, which is possible
    return generate_with_kari.callback(False, 
        ["-input", PROJECT_DIRECTORY + "/Game/Assets", "-output",PROJECT_DIRECTORY + "/Game/Assets/Generated"])

@kari.command("nuke")
def nuke_kari():
    """Nukes the build output"""
    try_delete(MSBUILD_INTERMEDIATE_OUTPUT_PATH)
    try_delete(MSBUILD_OUTPUT_PATH)


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


if __name__ == "__main__":
    main()