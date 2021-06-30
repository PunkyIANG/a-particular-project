# a-particular-project
 
Required programs:
- Unity ver 2020.3 LTS, download via Unity Hub: https://unity.com/download
- GitHub Desktop for easy version control: https://desktop.github.com/
- Git for version control via console: https://git-scm.com/
- Git LFS for work with big files: https://git-lfs.github.com/

Setup Instructions
- Install all the aforementioned programs.
- Clone this repo via github: `Add -> Clone repository -> URL -> https://github.com/PunkyIANG/a-particular-project`
- Download the .gitconfig file from https://github.com/NYUGameCenter/Unity-Git-Config and place it in the project folder.
- Edit .gitconfig with a text editor, replacing <path to UnityYAMLMerge> with the location of your Unity install's merge tool (note that these locations can vary if you picked a different install folder during unity install.) On Windows it's usually `C:\\Program Files\\Unity\\Hub\\Editor\\2020.3.12f1\\Editor\\Data\\Tools\\UnityYAMLMerge.exe`
- Download the [pre-commit](https://github.com/NYUGameCenter/Unity-Git-Config/blob/master/pre-commit) & [post-merge](https://github.com/NYUGameCenter/Unity-Git-Config/blob/master/post-merge) scripts. Enable them in your repo by moving them into the folder `<your_repo>/.git/hooks/`.  These will ensure that meta files stay in sync. It will also alert you if you attempt to commit a >100mb file, which github will reject. It will reject the commit, allowing you to revise it to remove or reduce the size of the offending file(s). **These scripts have to be installed individually on each computer you clone the repo to. Please ensure your teammates have installed these as well.**



Other docs
- If shit hits the fan with git: https://github.com/NYUGameCenter/Unity-Git-Config
