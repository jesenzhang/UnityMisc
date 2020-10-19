@echo off

REM set path=%~d0%~p0

set PWD=%~dp0
echo %PWD%

python "%PWD%textureprocess.py "

pause
