set SOLUTION_DIR=%1
set PROJECT_DIR=%~dp0

ntc "%PROJECT_DIR%Pages\*.htm" -r -ext ".auto.cs" /src
