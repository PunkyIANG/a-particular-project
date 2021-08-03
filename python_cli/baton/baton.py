from genericpath import exists
import click, shutil, os, subprocess, sys
import baton.git_hooks as git_hooks
from baton.solution import combine_solutions, SolutionFileError

# Environment variable shenanigans only exist for windows
import platform
IS_WINDOWS = platform.system().lower().startswith('win')
if IS_WINDOWS:
    from baton.registry_hijacking import set_env, get_env
    def quote(path : str) -> str:
        return '"' + path + '"'
# Assume Linux
else:
    def quote(path : str) -> str:
        return "'" + path + "'"

MSBUILD_INTERMEDIATE_OUTPUT_PATH = None
MSBUILD_OUTPUT_PATH = None       # Build/bin
GIT_SOURCE_HOOKS_PATH = None     # /git_hooks
DOT_GIT_HOOKS_PATH = None        # /.git/hooks
KARI_GENERATOR_PATH = None       # the built dll path
PROJECT_DIRECTORY = None         # /
UNITY_PROJECT_DIRECTORY = None   # /Game
UNITY_ASSETS_DIRECTORY = None    # /Game/Assets

def set_global_and_env(name, value):
    globals()[name] = value
    os.environ[name] = value

def set_global(name, value):
    globals()[name] = value

@click.group()
@click.option("-build_directory", envvar="BUILD_DIRECTORY", default=os.path.abspath("Build"))
@click.option("-project_directory", envvar="PROJECT_DIRECTORY", default=os.path.abspath("."))
# @click.option("-update", isFlag=True, help="Whether to update itself before running the command")
def cli(build_directory, project_directory):
    """Prepares environment and global variables"""
    set_global_and_env("MSBUILD_INTERMEDIATE_OUTPUT_PATH", os.path.join(build_directory, "obj"))
    set_global_and_env("MSBUILD_OUTPUT_PATH", os.path.join(build_directory, "bin"))
    set_global("PROJECT_DIRECTORY", project_directory)
    set_global("GIT_SOURCE_HOOKS_PATH", os.path.join(project_directory, "git_hooks"))
    set_global("DOT_GIT_HOOKS_PATH", os.path.join(project_directory, ".git", "hooks"))
    set_global("KARI_GENERATOR_PATH", 
        os.path.join(MSBUILD_OUTPUT_PATH, "Kari.Generator", "Release", "netcoreapp3.1", "publish", "kari.exe"))
    set_global("UNITY_PROJECT_DIRECTORY", os.path.join(project_directory, "Game"))
    set_global("UNITY_ASSETS_DIRECTORY", os.path.join(UNITY_PROJECT_DIRECTORY, "Assets"))

@cli.command("setup")
@click.option("-skip_unity_editor_envvar", is_flag=True, default=False)
def setup(skip_unity_editor_envvar):
    """Does the setup and the initial build"""
    copy_github_hooks.callback()
    git_hooks.update_submodules(PROJECT_DIRECTORY)    # Initialize the submodules
    build_kari.callback(clean=False, retry=True)
    generate_code_for_unity.callback()

    if not skip_unity_editor_envvar:
        set_unity_editor_envvar.callback(info=True)


@cli.command("update")
def update_self():
    """Recompiles and reinstalls Baton globally"""
    
    subprocess.Popen("pip install " + quote(os.path.join(PROJECT_DIRECTORY, "python_cli")))
    exit('Self-Update started. Press Enter when you see "Successfully installed baton"')


@cli.command("set_unity_editor")
@click.option("-info", is_flag=True, default=False, help="whether to show more information about the UNITY_EDITOR variable")
def set_unity_editor_envvar(info):
    """
    Helps to set the persistent UNITY_EDITOR environment variable.
    This variable needs to be set for the merge tool to work properly.
    Only works on Windows!
    """

    if info:
        print("UNITY_EDITOR is the path to the folder with the Unity.exe executable. On my machine, this path is C:\\Program Files\\Unity\\Editor. for you it might be nested in a folder with the version name. You may do it via `Unity Hub -> Installs -> Three dots above the required version -> Show in Explorer`. If the needed version is not showing up, you have either failed to install it or installed it separately, in which case you'd have to `Locate` it.")

    if not IS_WINDOWS:
        print("This feature is unavailable for non-windows machines")
        return

    current_path = get_env("UNITY_EDITOR")
    if current_path == "":
        print("Currently, the variable UNITY_EDITOR has no value.")
    else:
        print("The current value of UNITY_EDITOR is: " + current_path)
    
    # Here, the user would go into Unity hub and discover the path to the Unity editor
    input_path = input("Enter the new value (or just press Enter to skip): ")

    # If user hits Enter, we get an empty string here
    if input_path != '':
        # As far as I know, the registry requires paths to have backslashed.
        # Now I do not know what form the output of `os.path.abspath` has,
        # so I'm replacing the slashes here just in case.
        editor_path = os.path.abspath(input_path).replace('/', '\\')

        # Hopefully this check will save someone a couple of minutes of debugging
        if not os.path.exists(editor_path):
            print(f"Warning: the path {editor_path} does not exist in the filesystem")
        
        if current_path != editor_path:
            set_env("UNITY_EDITOR", editor_path)
            print("UNITY_EDITOR has been set to " + editor_path)
        else:
            print("You have entered the same value for the path")
    else:
        print("Skipped")


@cli.command("master_sln")
@click.option("-output_path", type=str, default=None)
def master_sln(output_path):
    """Generates the master sln by combining Game.sln and Kari.sln"""
    prev_dir = os.curdir
    os.chdir(PROJECT_DIRECTORY)

    try:
        combine_solutions(["Game/Game.sln", "Kari/Kari.sln"], output_path or "Master.sln")
    except SolutionFileError as exception:
        print(f'Failed to read one of the solution files: {exception}')
        return False
    except Exception as exception:
        print(f'Failed to generate the solution file: {exception}')
        return False
    finally:
        os.chdir(prev_dir)
    
    return True


@cli.command("hooks")
def copy_github_hooks():
    """Copies github hooks from git_hooks"""
    copy_all_files(GIT_SOURCE_HOOKS_PATH, DOT_GIT_HOOKS_PATH)
    print("Copied github hooks")

# TODO: 
# Read docs more carefully to find out how to extend an existing group from other modules.
# My idea is to have the extensions reference the group, not the other way around. 
@cli.command("git_precommit")
def github_pre_commit():
    """Script called by git before commiting, used to validate the commit"""
    exit_code = git_hooks.precommit(PROJECT_DIRECTORY)
    sys.exit(exit_code)

# It may be too slow, this should check the version ideally 
# TODO: does not work
@cli.command("git_postcheckout")
def git_post_checkout():
    copy_github_hooks.callback()
    update_self.callback()


@cli.group("kari")
def kari():
    """Has to do with code generation"""
    pass


@kari.command("build")
@click.option("-clean", is_flag=True, help="Whether to nuke all previous output before recompiling")
@click.option("-retry", is_flag=True, default=False, help="Whether to retry building a second time if failed")
@click.option("-debug", is_flag=True, default=False, help="Whether to do a debug build")
@click.option("-plugins", is_flag=True, default=False, help="Whether to also build all plugins")
def build_kari(clean, retry, debug, plugins):
    """Builds the Kari code generator"""
    # Clear all previous output
    if clean: nuke_kari.callback()
    
    current_directory = os.path.abspath(os.curdir)
    os.chdir(f"{PROJECT_DIRECTORY}/Kari")

    try:
        run_sync("dotnet tool restore")
        run_sync("dotnet restore")

        configuration = "Debug" if debug else "Release"

        cmds = [f"dotnet publish Kari.Generator/Kari.Generator.csproj --configuration {configuration} --no-self-contained"]

        if plugins:
            cmds.extend([
                f"dotnet publish Kari.Plugins/Terminal/Terminal.csproj --configuration {configuration} --no-self-contained",
                f"dotnet publish Kari.Plugins/Flags/Flags.csproj --configuration {configuration} --no-self-contained"
            ])
        # dotnet fails the first time for some strange reason if the repository has just been cloned
        if retry:
            def run_twice(cmd):
                try:
                    run_sync(cmd)
                except subprocess.CalledProcessError as err:
                    run_sync(cmd)
            execute = run_twice
        else:
            execute = run_sync

        for cmd in cmds:
            execute(cmd)
        
        print(f"The final dll has been written to {KARI_GENERATOR_PATH}")
        print("To run it, do `baton kari run`, passing in the flags`")
        # TODO: actually run tests
        # run_sync("dotnet run -p Kari.Test")

    except subprocess.CalledProcessError as err:
        print(f"Build process exited with error code {err.returncode}")
        return False

    finally:
        os.chdir(current_directory)
    
    return True


@kari.command(name="run", context_settings={"ignore_unknown_options": True})
@click.option("-rebuild", is_flag=True, help="Whether to recompile Kari before generating code.")
@click.argument("unprocessed_args", nargs=-1, type=click.UNPROCESSED)
def generate_with_kari(rebuild, unprocessed_args):
    """
    Equivalent to calling Kari from the command line.
    
    "unprocessed_args" are the arguments passed to Kari. Call this command without any arguments for more info."
    """

    if rebuild:
        if not build_kari.callback(clean=False, retry=False):
            return False

    elif not os.path.exists(KARI_GENERATOR_PATH):
        print("Initiating build, since Kari has not been built")
        if not build_kari.callback(clean=False, retry=False):
            return False
    
    try:
        # Pass along all of the unparsed commands
        command = [quote(KARI_GENERATOR_PATH)]
        command.extend(unprocessed_args)
        run_sync(" ".join(command))

    except subprocess.CalledProcessError as err:
        print(f"Generation failed with error code {err.returncode}")
        return False
    
    return True


@kari.command("unity")
def generate_code_for_unity():
    """Generates code for the unity project """

    plugin_path = MSBUILD_OUTPUT_PATH + "/{0}/Release/netcoreapp3.1/Kari.Plugins.{0}.dll"
    plugins = ",".join([plugin_path.format(p) for p in ["Terminal", "Flags"]])

    # TODO: maybe generate in a single file to minimize .meta's, which is possible
    return generate_with_kari.callback(
        rebuild=False, 
        unprocessed_args=
        [   "-input", os.path.join(quote(UNITY_ASSETS_DIRECTORY), "Source"), 
            "-pluginsLocations", quote(plugins),
            "-generatedName", "Generated",
            "-rootNamespace", "SomeProject",
            "-commonNamespace", "Common",
            "-clearOutput", "true",
            "-monolithicProject", "false"]
        # [   "-input", f"{PROJECT_DIRECTORY}/Kari/Kari.Test", 
        #     "-pluginsLocations", f"{MSBUILD_OUTPUT_PATH}/Terminal/Release/netcoreapp3.1/publish/Kari.Plugins.Terminal.dll,{MSBUILD_OUTPUT_PATH}/Flags/Release/netcoreapp3.1/publish/Kari.Plugins.Flags.dll",
        #     "-generatedName", "Generated",
        #     "-rootNamespace", "Kari",
        #     "-clearOutput", "true",
        #     "-monolithicProject", "true"]
        )


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

def run_command_sync(command):
    print(command)
    returncode = os.system(quote(command))
    if returncode != 0:
        raise subprocess.CalledProcessError(returncode, command)

run_sync = run_command_sync


def try_make_dir(path):
    os.makedirs(path, exist_ok = True)


if __name__ == "__main__":
    cli()