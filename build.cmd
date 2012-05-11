@echo off

rem Set the environment
if "%DevEnvDir%"=="" call "%VS100COMNTOOLS%..\..\VC\vcvarsall.bat"
set SubWCRev="C:\Program Files\TortoiseSVN\bin\SubWCRev.exe"
set ISCC="c:\apps\dev\Inno Setup 5\ISCC.exe"

cd %~dp0


rem Clean the current build
rem call clean

rem Update the version info
%SubWCRev% . GlobalAssemblyInfo.cs.in GlobalAssemblyInfo.cs
copy GlobalAssemblyInfo.cs ui\Properties\
copy GlobalAssemblyInfo.cs Decompiler\Properties\
del GlobalAssemblyInfo.cs

rem Build
msbuild ui\exceptionexplorer.csproj /m /t:Rebuild /p:Configuration=Release;OutDir=%~dp0/release/Build/
rem Sign the executables
%sign% release\build\ExceptionExplorer.exe release\build\Decompiler.dll

rem Move the debug info
mkdir release\pdb\
move release\build\*.pdb release\pdb\

%ISCC% setup\ExceptionExplorer.iss

pause