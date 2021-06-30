call bitsadmin /transfer myDownloadJob1 /download /priority normal https://raw.githubusercontent.com/NYUGameCenter/Unity-Git-Config/master/post-merge "%~dp0.git/hooks/post-merge"
call bitsadmin /transfer myDownloadJob2 /download /priority normal https://raw.githubusercontent.com/NYUGameCenter/Unity-Git-Config/master/pre-commit "%~dp0.git/hooks/pre-commit"
