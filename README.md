# a-particular-project
 
Required programs:
- Unity ver 2020.3 LTS, download via Unity Hub: https://unity.com/download
- GitHub Desktop for easy version control: https://desktop.github.com/
- Git for version control via console: https://git-scm.com/
- Git LFS for work with big files: https://git-lfs.github.com/

Setup Instructions
- Install all the aforementioned programs.
- Clone this repo via github: `Add -> Clone repository -> URL -> https://github.com/PunkyIANG/a-particular-project`
- Open the project in Unity, hit `Project Setup -> Get path to UnityYAMLMerge folder`. This will copy the path to the tool `UnityYAMLMerge`, used for merging scenes. You'll have to then edit your `PATH` environment variable, appending the copied path.
- Enable the `post-merge` and `pre-commit` git hooks, by hitting `Project Setup -> Initialize git hooks`. It will copy the files from `git_hooks` into your local `.git/hooks`.
The hooks will ensure meta files stay in sync and will alert you if you attempt to commit a >100mb file, which github will reject. It will reject the commit, allowing you to revise it to remove or reduce the size of the offending file(s). **These scripts have to be enabled individually on each computer you clone the repo to. Please ensure your teammates have enabled these as well.**

Other docs
- If shit hits the fan with git: https://github.com/NYUGameCenter/Unity-Git-Config
