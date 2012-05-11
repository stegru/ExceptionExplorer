@echo off

if "%DevEnvDir%"=="" call "%VS100COMNTOOLS%..\..\VC\vcvarsall.bat"

cd %~dp0

del /s/f/q release\build

msbuild /m /t:Clean /p:Configuration=Debug
msbuild /m /t:Clean /p:Configuration=Release


