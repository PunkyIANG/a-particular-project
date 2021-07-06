@echo off

where python
IF %ERRORLEVEL% EQU 1   GOTO no_python

where pip
IF %ERRORLEVEL% EQU 1   GOTO no_pip

goto python_cli:

:no_python
echo 'python' has not been found in path. See readme for instructions.
exit /b 1

:no_pip
echo 'pip' has not been found in path. See readme for instructions.
exit /b 1

:python_cli

@REM Install the build tools
pip install --upgrade setuptools
pip install build
@REM Install Baton
pip install ./python_cli
@REM Setup the project
baton setup