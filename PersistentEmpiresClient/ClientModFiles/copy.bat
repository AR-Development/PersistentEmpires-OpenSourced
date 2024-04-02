@echo off
set "source_folder=.\GUI"
set "destination_folder=D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\Modules\PersistentEmpires\GUI"

echo Copying GUI folder...

xcopy /s /i "%source_folder%" "%destination_folder%"

echo GUI folder copied successfully.
pause