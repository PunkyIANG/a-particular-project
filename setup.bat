:: This script is necessary to set up the setupper script.
:: All of the essential logic is contained within the CLI, see python_cli/baton/baton.py
@echo off

:: Make sure both python and pip are present in PATH
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

:: Install the build tools
pip install --upgrade setuptools
pip install build
:: Install Baton
pip install ./python_cli
:: Setup the project
baton setup