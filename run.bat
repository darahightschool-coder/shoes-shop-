@echo off
title Shoe Shop Web Application
echo ========================================================
echo Starting Shoe Shop Web Application on http://localhost:5000
echo ========================================================
set "DOTNET_ROOT=C:\Users\TUF\AppData\Local\Microsoft\dotnet"
set "PATH=C:\Users\TUF\AppData\Local\Microsoft\dotnet;C:\Users\TUF\.dotnet\tools;%PATH%"
start http://localhost:5000
dotnet run --urls "http://localhost:5000"
pause
