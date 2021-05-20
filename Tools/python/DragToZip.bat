@echo off

REM set path=%~d0%~p0

set PWD=%~dp0
echo %PWD%

python "%PWD%ZipTool.py " --path  %1 --out %1

pause

REM :start
REM D:/App/Python/python.exe "%path%texture_checkname.py " --path  %1

REM shift
REM if NOT x%1==x goto start

pause

 