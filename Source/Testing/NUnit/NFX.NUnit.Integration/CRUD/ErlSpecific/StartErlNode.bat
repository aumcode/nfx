@echo off


for %%i in (werl.exe) do (
  if "x%%~$PATH:i" == "x" goto :not_found
  start werl -sname nfx@localhost -setcookie klubnika

  goto :end
)

:not_found
@echo werl.exe not found!
@pause

:end