dotnet tool restore
dotnet paket restore

dotnet fake run build.fsx Build

cd websharper-tests/WebSharper.JsonLexAndYaccExample
dotnet build
cd ../WebSharper.LexAndYaccMiniProject
dotnet build