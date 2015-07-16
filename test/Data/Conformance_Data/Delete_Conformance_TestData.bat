@echo off

choice /C yn /M "are you going to delete all Conformance Data in this fold(Y/N)"
if ERRORLEVEL  2  goto END

for /d %%i in (*.Conformance.*) do (
    rmdir /s /q "%%i"
)

for %%i in (*.txt) do (
    if not "%%i" equ "ReadMe.txt" (
	    del "%%i"
    )
)

:END