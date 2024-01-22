#if INTERACTIVE
#r "nuget: FAKE.Core"
#r "nuget: Fake.Core.Target"
#r "nuget: Fake.IO.FileSystem"
#r "nuget: Fake.Core.ReleaseNotes"
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
nuget Fake.Core.ReleaseNotes
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

let release = ReleaseNotes.parse (System.IO.File.ReadAllLines "RELEASE_NOTES.md")


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

Target.create "PrePackaging" <| fun _ ->
    let template = """type file
id WebSharper.FsLexYacc
authors IntelliFactory
projectUrl https://websharper.com/
repositoryType git
repositoryUrl https://github.com/dotnet-websharper/FsLexYacc.Runtime/
licenseUrl https://github.com/dotnet-websharper/FsLexYacc.Runtime/blob/master/LICENSE.md
iconUrl https://github.com/dotnet-websharper/core/raw/websharper50/tools/WebSharper.png
description
    WebSharper proxy for FsLexYacc.Runtime
tags
    WebSharper Web JavaScript FSharp CSharp
dependencies
    framework: netstandard2.0
        WebSharper ~> LOCKEDVERSION:[3]
        FsLexYacc.Runtime ~> %VERSION%
files
    ../websharper/WebSharper.FsLexYacc.Runtime/bin/Release/netstandard2.0/WebSharper.FsLexYacc.Runtime.dll ==> lib/netstandard2.0

references
    WebSharper.FsLexYacc.Runtime.dll
"""

    let content v = template.Replace("%VERSION%", v)

    System.IO.File.WriteAllText("nuget/WebSharper.FsLexYacc.Runtime.paket.template", content release.NugetVersion)

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
    "PrePackaging" ==> "WS-Package"
    "FSLexYaccBuild" ==> "WS-BuildRelease"
    targets
|> RunTargets
