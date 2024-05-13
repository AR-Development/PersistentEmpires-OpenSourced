@echo off

REM Set the path to your Bannerlord client installation directory
REM Example: set "mbClientFolder=C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord"
set "mbClientFolder=C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord"

REM Set the path to your Bannerlord server installation directory
REM Example: set "mbServerFolder=C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Dedicated Server"
set "mbServerFolder=C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Dedicated Server"

REM Check if the client and server paths are set correctly
if not exist "%mbClientFolder%" (
    echo Please install the Bannerlord client from Steam.
    goto end
)

if not exist "%mbServerFolder%" (
    echo Please install the Bannerlord dedicated server from Steam.
    goto end
)

REM Delete existing files in the server target directory
if exist ".\ServerReferences\*.dll" del ".\ServerReferences\*.dll"

REM Set the path to the directory where your Bannerlord server DLLs are located
set "serverSourceDir=%mbServerFolder%\bin\Win64_Shipping_Server"
REM Set the path to the target directory for the server DLLs relative to the current directory
set "serverTargetDir=.\ServerReferences"
copy "%serverSourceDir%\*.dll" "%serverTargetDir%"

REM Delete existing files in the client target directory
if exist ".\ClientReferences\*.dll" del ".\ClientReferences\*.dll"

REM Set the path to the directory where your Bannerlord client DLLs are located
set "clientSourceDir=%mbClientFolder%\bin\Win64_Shipping_Client"
REM Set the path to the target directory for the client DLLs relative to the current directory
set "clientTargetDir=.\ClientReferences"
copy "%clientSourceDir%\*.dll" "%clientTargetDir%"

echo DLL files copied successfully.

:end
pause
