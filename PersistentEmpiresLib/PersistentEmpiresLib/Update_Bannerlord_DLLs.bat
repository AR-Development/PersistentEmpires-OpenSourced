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

REM Set the path to the directory where your Bannerlord server DLLs are located
set "serverSourceDir=%mbServerFolder%\bin\Win64_Shipping_Server"

REM Set the path to the target directory for the server DLLs relative to the current directory
set "serverTargetDir=.\ServerReferences"

REM Create the server target directory if it doesn't exist
if not exist "%serverTargetDir%" mkdir "%serverTargetDir%"

REM Set the paths to the directories where your Bannerlord client DLLs are located
set "clientSourceDir=%mbClientFolder%\bin\Win64_Shipping_Client"
set "clientSourceDirEditor=%mbClientFolder%\bin\Win64_Shipping_wEditor"

REM Set the path to the target directory for the client DLLs relative to the current directory
set "clientTargetDir=.\ClientReferences"

REM Create the client target directory if it doesn't exist
if not exist "%clientTargetDir%" mkdir "%clientTargetDir%"

REM Delete existing files in the server target directory
if exist "%serverTargetDir%\*.dll" del "%serverTargetDir%\*.dll"

REM Copy server DLLs to the target directory
copy "%serverSourceDir%\*.dll" "%serverTargetDir%"

REM Delete existing files in the client target directory
if exist "%clientTargetDir%\*.dll" del "%clientTargetDir%\*.dll"

REM Copy client DLLs from Win64_Shipping_Client to the target directory
copy "%clientSourceDir%\*.dll" "%clientTargetDir%"

REM Copy client DLLs from Win64_Shipping_wEditor to the target directory
copy "%clientSourceDirEditor%\*.dll" "%clientTargetDir%"

echo DLL files copied successfully.

:end
pause