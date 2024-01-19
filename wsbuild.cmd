dotnet tool restore
dotnet paket restore

dotnet fake build build.fsx WSBuild

dotnet fake run wsbuild.fsx %*