# a-particular-project
 
## Required programs:

- Unity ver 2020.3 LTS, download via Unity Hub: https://unity.com/download
- GitHub Desktop for easy version control: https://desktop.github.com/
- Git for version control via console: https://git-scm.com/
- Git LFS for work with big files: https://git-lfs.github.com/
- Python 3.

## Setup Instructions

- Install all the aforementioned programs.
- Clone this repo via github: `Add -> Clone repository -> URL -> https://github.com/PunkyIANG/a-particular-project`
- Find the path to the Unity Editor. For me it is `C:\Program Files\Unity\Editor`, for you it might be nested in a folder with the version name. Go to envioronment variables, and add a new variable `UNITY_EDITOR`, setting it to this path.
- Run the python script at the root of repository, by doing `python setup.py fresh` in the console. It will build Kari and enable the `post-merge` and `pre-commit` git hooks.
The hooks will ensure meta files stay in sync and will alert you if you attempt to commit a >100mb file, which github will reject. It will reject the commit, allowing you to revise it to remove or reduce the size of the offending file(s). **These scripts have to be enabled individually on each computer you clone the repo to. Please ensure your teammates have enabled these as well.**