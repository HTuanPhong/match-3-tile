@echo off
set OUTPUT=codebase.txt

echo. > "%OUTPUT%"

for /r %%F in (*.cs) do (
    echo %%F | findstr /i /c:"\Library\" /c:"\Temp\" /c:"\Logs\" /c:"\obj\" /c:"\bin\" >nul
    if errorlevel 1 (
        echo FILE: %%F>>"%OUTPUT%"
        echo ```csharp>>"%OUTPUT%"
        type "%%F">>"%OUTPUT%"
        echo.>>"%OUTPUT%"
        echo ```>>"%OUTPUT%"
        echo.>>"%OUTPUT%"
    )
)

echo Saved to %OUTPUT%
pause