# a-particular-project
 
## Required programs:

- Unity ver 2020.3 LTS, download via Unity Hub: https://unity.com/download
- GitHub Desktop for easy version control: https://desktop.github.com/
- Git for version control via console: https://git-scm.com/
- Git LFS for work with big files: https://git-lfs.github.com/
- Python 3, needed to run the custom build script.
- .NET Core 3.1 SDK, needed to compile and run the code generator.

I recommend installing both Python and .NET via Visual Studio, even if you're not planning to be using their IDE. 
It's just going to save you some troubleshooting (I've struggled for days with bugs and inconveniences). 
Go to this link, it should start the download: https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&rel=16
When that is done, open up the installer and select the .NET and Python workloads, it should just work.


Alternatively, here is Python https://www.python.org/downloads/, and here is .NET Core 3.1 https://dotnet.microsoft.com/download/dotnet/3.1

## Setup Instructions

1. Install all the aforementioned programs.

2. Clone this repo via github: `Add -> Clone repository -> URL -> https://github.com/PunkyIANG/a-particular-project`. 
    > Alternatively, do `git clone https://github.com/PunkyIANG/a-particular-project --recursive` via the command line.
    > The option `--recursive` is needed to initialize the submodules.

3. Initialize and run the batch script at the root of the repository, by calling `setup` via the command line. 
It will build and install **Baton**, the python CLI, build and run **Kari**, the code generator, on the Unity project and enable the `post-merge` and `pre-commit` git hooks.
   > The hooks will ensure meta files stay in sync and will alert you if you attempt to commit a >100mb file, which github will reject. 
   > It will reject the commit, allowing you to revise it to remove or reduce the size of the offending file(s). 
   > **These scripts have to be enabled individually on each computer you clone the repo to. Please ensure your teammates have enabled these as well.**

4. The script will ask you to provide the path to the folder with the Unity Editor. 
For me it is `C:\Program Files\Unity\Editor`, for you it might be nested in a folder with the version name. 
You may do it via `Unity Hub -> Installs -> Three dots above the required version -> Show in Explorer`. 
If the needed version is not showing up, you have either failed to install it or installed it separately, in which case you'd have to `Locate` it.

> If running `pip` or `python` says these commands are unrecognized, you'll have to add the paths to them to your `PATH` environment variable. 
> If you have istalled Python via Visual Studio, like I did, the path to `python` is similar to `C:\Program Files (x86)\Microsoft Visual Studio\Shared\Python37_64`, and the path to `pip` should be similar to `C:\Program Files (x86)\Microsoft Visual Studio\Shared\Python37_64\Scripts`. 
> After you're done, reload the computer and **repeat step 3**.

5. To get Unity to generate `.csproj` files for Intellisense in e.g. VS Code, open Unity Editor, hit `Edit -> Preferences -> External Tools`. Under `Generate .csproj files for` check the first 4 checkboxes. Hit `Regenerate project files`. 

> If Intellisense ever behaves weirdly, go to this menu and regenerate the project files.
> It is likely going to fix the issue.

6. Since the actual project root is one directory level up, that is the folder that should open when you double-click a script via the Unity Editor. To make it work this way, in the same `External Tools` window in the editor, set `External Script Editor Args` to `"$(ProjectPath)/.." -g "$(File)":$(Line):$(Column)`.

> I may find a way to do the steps 5 and 6 automatically.