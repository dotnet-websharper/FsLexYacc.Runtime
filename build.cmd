@echo off
dotnet tool restore
dotnet paket restore

call paket-files\wsbuild\github.com\dotnet-websharper\build-script\build.cmd %*
