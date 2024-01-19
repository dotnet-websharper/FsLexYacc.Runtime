#if INTERACTIVE
#r "nuget: FAKE.Core"
#r "nuget: Fake.Core.Target"
#r "nuget: Fake.IO.FileSystem"
#r "nuget: Fake.Tools.Git"
#r "nuget: Fake.DotNet.Cli"
#r "nuget: Fake.DotNet.AssemblyInfoFile"
#r "nuget: Fake.DotNet.Paket"
#r "nuget: Paket.Core"
#else
#r "paket:
nuget FSharp.Core 5.0.0
nuget FAKE.Core
nuget Fake.Core.Target
nuget Fake.IO.FileSystem
nuget Fake.Tools.Git
nuget Fake.DotNet.Cli
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.Paket
nuget Paket.Core prerelease //"
#endif

#load "paket-files/wsbuild/github.com/dotnet-websharper/build-script/WebSharper.Fake.fsx"
open WebSharper.Fake
open Fake.DotNet
open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.DotNet


Target.create "FSLexYaccBuild" (fun _ ->
    for framework in [ "net6.0" ] do
        [
            "src/FsLex.Core/fslexlex.fs"
            "src/FsLex.Core/fslexpars.fs"
            "src/FsLex.Core/fslexpars.fsi"
            "src/FsYacc.Core/fsyacclex.fs"
            "src/FsYacc.Core/fsyaccpars.fs"
            "src/FsYacc.Core/fsyaccpars.fsi"
        ]
        |> File.deleteAll

        for project in [ "src/FsLex/fslex.fsproj"; "src/FsYacc/fsyacc.fsproj" ] do
            DotNet.exec id $"publish" $"{project} -c Release /v:n -f {framework}"

    for project in
        [
            "src/FsLexYacc.Runtime/FsLexYacc.Runtime.fsproj"
        ] do
        DotNet.exec id $"build" $"{project} -c Release /v:n")



let WithProjects projects args =
    { args with BuildAction = Projects projects }

LazyVersionFrom "WebSharper" |> WSTargets.Default
|> fun args ->
    { args with
        Attributes = [
            AssemblyInfo.Company "IntelliFactory"
            AssemblyInfo.Copyright "(c) IntelliFactory 2023"
            AssemblyInfo.Title "https://github.com/dotnet-websharper/ui"
            AssemblyInfo.Product "WebSharper FsLexYacc.Runtime"
        ]
    }
|> WithProjects [
    "websharper/WebSharper.FsLexYacc.Runtime/WebSharper.FsLexYacc.Runtime.fsproj"
]
|> MakeTargets
|> fun targets ->
    "FSLexYaccBuild" ==> "WS-BuildRelease"
    targets
|> RunTargets