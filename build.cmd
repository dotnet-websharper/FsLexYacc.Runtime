@echo off
dotnet tool restore
dotnet paket update -g wsbuild --no-install

dotnet fsi ./build.fsx -t %*
