open System
open Fake.SystemHelper
open Fake.Core
open Fake.DotNet
open Fake.Tools
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.Api
open Fake.BuildServer
open Fake.JavaScript


let getCLIArgs () =
    System.Environment.GetCommandLineArgs()
    |> Array.skip 1 // The first element in the array contains the file name of the executing program

getCLIArgs ()
|> Array.toList
|> Context.FakeExecutionContext.Create false "build.fsx"
|> Context.RuntimeContext.Fake
|> Context.setExecutionContext


BuildServer.install [
    GitHubActions.Installer
]

//-----------------------------------------------------------------------------
// Metadata and Configuration
//-----------------------------------------------------------------------------

let rootDir = __SOURCE_DIRECTORY__ </> ".."

let release = lazy (Fake.Core.ReleaseNotes.load "RELEASE_NOTES.md")
let productName = "FsLibLog"
let sln = "FsLibLog.sln"
let srcGlob = rootDir  </> "src/**/*.??proj"
let testsGlob = rootDir  </> "tests/**/*.??proj"

let srcCodeGlob =
    !! (rootDir  </> "src/**/*.fs")
    ++ (rootDir  </> "src/**/*.fsx")
    -- (rootDir  </> "src/**/obj/**/*.fs")


let adaptersCodeGlob =
    !! (rootDir  </> "adapters/**/*.fs")
    ++ (rootDir  </> "adapters/**/*.fsx")
    -- (rootDir  </> "adapters/**/obj/**/*.fs")

let examplesCodeGlob =
    !! (rootDir  </> "examples/**/*.fs")
    ++ (rootDir  </> "examples/**/*.fsx")
    -- (rootDir  </> "examples/**/obj/**/*.fs")


let testsCodeGlob =
    !! (rootDir  </> "tests/**/*.fs")
    ++ (rootDir  </> "tests/**/*.fsx")
    -- (rootDir  </> "tests/**/obj/**/*.fs")


let srcAndTest =
    !! srcGlob
    ++ testsGlob

let distDir = rootDir  </> "dist"
let distGlob = distDir @@ "*.nupkg"
let toolsDir = rootDir  </> "tools"

let coverageReportDir =  rootDir  </> "docs" @@ "coverage"

let gitOwner = "TheAngryByrd"
let gitRepoName = "FsLibLog"

let releaseBranch = "master"


let githubToken = Environment.environVarOrNone "GITHUB_TOKEN"

//-----------------------------------------------------------------------------
// Helpers
//-----------------------------------------------------------------------------

let isRelease (targets : Target list) =
    targets
    |> Seq.map(fun t -> t.Name)
    |> Seq.exists ((=)"Release")

let invokeAsync f = async { f () }

let configuration (targets : Target list) =
    let defaultVal = if isRelease targets then "Release" else "Debug"
    match Environment.environVarOrDefault "CONFIGURATION" defaultVal with
    | "Debug" -> DotNet.BuildConfiguration.Debug
    | "Release" -> DotNet.BuildConfiguration.Release
    | config -> DotNet.BuildConfiguration.Custom config

let failOnBadExitAndPrint (p : ProcessResult) =
    if p.ExitCode <> 0 then
        p.Errors |> Seq.iter Trace.traceError
        failwithf "failed with exitcode %d" p.ExitCode

// CI Servers can have bizzare failures that have nothing to do with your code
let rec retryIfInCI times fn =
    match Environment.environVarOrNone "CI" with
    | Some _ ->
        if times > 1 then
            try
                fn()
            with
            | _ -> retryIfInCI (times - 1) fn
        else
            fn()
    | _ -> fn()

let isReleaseBranchCheck () =
    if Git.Information.getBranchName "" <> releaseBranch then failwithf "Not on %s.  If you want to release please switch to this branch." releaseBranch


module dotnet =
    let watch cmdParam program args =
        DotNet.exec cmdParam (sprintf "watch %s" program) args

    let tool optionConfig command args =
        DotNet.exec optionConfig (sprintf "%s" command) args
        |> failOnBadExitAndPrint

    let fantomas args =
        DotNet.exec id "fantomas" args

    let reportgenerator optionConfig args =
        tool optionConfig "reportgenerator" args

    let sourcelink optionConfig args =
        tool optionConfig "sourcelink" args




Target.create "Clean" <| fun _ ->
    ["bin"; "temp" ; distDir; coverageReportDir]
    |> Shell.cleanDirs

    !! srcGlob
    ++ testsGlob
    |> Seq.collect(fun p ->
        ["bin";"obj"]
        |> Seq.map(fun sp ->
             IO.Path.GetDirectoryName p @@ sp)
        )
    |> Shell.cleanDirs

Target.create "DotnetRestore" <| fun _ ->
    [sln]
    |> Seq.map(fun dir -> fun () ->
        let args =
            [
            ] |> String.concat " "
        DotNet.restore(fun c ->
            { c with
                Common =
                    c.Common
                    |> DotNet.Options.withCustomParams
                        (Some(args))
            }) dir)
    |> Seq.iter(retryIfInCI 10)

Target.create "DotnetBuild" <| fun ctx ->
    let release = release.Value
    let args =
        [
            sprintf "/p:PackageVersion=%s" release.NugetVersion
            sprintf "/p:SourceLinkCreate=%b" (isRelease ctx.Context.AllExecutingTargets)
            "--no-restore"
        ] |> String.concat " "
    DotNet.build(fun c ->
        { c with
            Configuration = configuration (ctx.Context.AllExecutingTargets)
            Common =
                c.Common
                |> DotNet.Options.withCustomParams
                    (Some(args))
        }) sln



let coverageThresholdPercent = 80

Target.create "DotnetTest" <| fun ctx ->
    !! testsGlob
    |> Seq.iter (fun proj ->
        DotNet.test(fun c ->
            let args =
                [
                    "--no-build"
                    "/p:AltCover=true"
                    // sprintf "/p:AltCoverThreshold=%d" coverageThresholdPercent
                    sprintf "/p:AltCoverAssemblyExcludeFilter=%s" (IO.Path.GetFileNameWithoutExtension(proj))
                ] |> String.concat " "
            { c with
                Configuration = configuration (ctx.Context.AllExecutingTargets)
                Common =
                    c.Common
                    |> DotNet.Options.withCustomParams
                        (Some(args))
                }) proj)


Target.create "GenerateCoverageReport" <| fun _ ->
    let coverageReports =
        !!"tests/**/coverage*.xml"
        |> String.concat ";"
    let sourceDirs =
        !! srcGlob
        |> Seq.map Path.getDirectory
        |> String.concat ";"
    let independentArgs =
            [
                sprintf "-reports:\"%s\""  coverageReports
                sprintf "-targetdir:\"%s\"" coverageReportDir
                // Add source dir
                sprintf "-sourcedirs:\"%s\"" sourceDirs
                // Ignore Tests and if AltCover.Recorder.g sneaks in
                sprintf "-assemblyfilters:\"%s\"" "-*.Tests;-AltCover.Recorder.g"
                sprintf "-Reporttypes:%s" "Html"
            ]
    let args =
        independentArgs
        |> String.concat " "
    dotnet.reportgenerator id args


Target.create "WatchTests" <| fun _ ->
    !! testsGlob
    |> Seq.map(fun proj -> fun () ->
        dotnet.watch
            (fun opt ->
                opt |> DotNet.Options.withWorkingDirectory (IO.Path.GetDirectoryName proj))
            "test"
            ""
        |> ignore
    )
    |> Seq.iter (invokeAsync >> Async.Catch >> Async.Ignore >> Async.Start)

    printfn "Press Ctrl+C (or Ctrl+Break) to stop..."
    let cancelEvent = Console.CancelKeyPress |> Async.AwaitEvent |> Async.RunSynchronously
    cancelEvent.Cancel <- true

let (|Fsproj|Csproj|Vbproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)

Target.create "RunNpmInstall" <| fun _ ->
    Npm.exec "install" (fun o -> { o with WorkingDirectory = "./tests/FsLibLog.Tests" } )

Target.create "RunNpmTests" <| fun _ ->
    Npm.exec "test" (fun o -> { o with WorkingDirectory = "./tests/FsLibLog.Tests" } )

Target.create "AssemblyInfo" <| fun _ ->
    let release = release.Value
    let releaseChannel =
        match release.SemVer.PreRelease with
        | Some pr -> pr.Name
        | _ -> "release"
    let getAssemblyInfoAttributes projectName =
        [ AssemblyInfo.Title (projectName)
          AssemblyInfo.Product productName
          AssemblyInfo.Version release.AssemblyVersion
          AssemblyInfo.Metadata("ReleaseDate", release.Date.Value.ToString("o"))
          AssemblyInfo.FileVersion release.AssemblyVersion
          AssemblyInfo.InformationalVersion release.AssemblyVersion
          AssemblyInfo.Metadata("ReleaseChannel", releaseChannel)
          AssemblyInfo.Metadata("GitHash", Git.Information.getCurrentSHA1(null))
        ]

    let getProjectDetails (projectPath : string) =
        let projectName = IO.Path.GetFileNameWithoutExtension(projectPath)
        ( projectPath,
          projectName,
          IO.Path.GetDirectoryName(projectPath),
          (getAssemblyInfoAttributes projectName)
        )

    srcAndTest
    |> Seq.map getProjectDetails
    |> Seq.iter (fun (projFileName, _, folderName, attributes) ->
        match projFileName with
        | Fsproj -> AssemblyInfoFile.createFSharp (folderName @@ "AssemblyInfo.fs") attributes
        | Csproj -> AssemblyInfoFile.createCSharp ((folderName @@ "Properties") @@ "AssemblyInfo.cs") attributes
        | Vbproj -> AssemblyInfoFile.createVisualBasic ((folderName @@ "My Project") @@ "AssemblyInfo.vb") attributes
        )


Target.create "DotnetPack" <| fun ctx ->
    let release = release.Value
    !! srcGlob
    |> Seq.iter (fun proj ->
        let args =
            [
                sprintf "/p:PackageVersion=%s" release.NugetVersion
                sprintf "/p:PackageReleaseNotes=\"%s\"" (release.Notes |> String.concat "\n")
                sprintf "/p:SourceLinkCreate=%b" (isRelease (ctx.Context.AllExecutingTargets))
            ] |> String.concat " "
        DotNet.pack (fun c ->
            { c with
                Configuration = configuration (ctx.Context.AllExecutingTargets)
                OutputPath = Some distDir
                Common =
                    c.Common
                    |> DotNet.Options.withCustomParams (Some args)
            }) proj
    )


Target.create "SourcelinkTest" <| fun _ ->
    !! distGlob
    |> Seq.iter (fun nupkg ->
        dotnet.sourcelink id (sprintf "test %s" nupkg)
    )

Target.create "Publish" <| fun _ ->
    isReleaseBranchCheck ()

    Paket.push(fun c ->
            { c with
                PublishUrl = "https://www.nuget.org"
                WorkingDir = "dist"
            }
        )

Target.create "GitRelease" <| fun _ ->
    let release = release.Value
    isReleaseBranchCheck ()

    let releaseNotesGitCommitFormat = release.Notes |> Seq.map(sprintf "* %s\n") |> String.concat ""

    Git.Staging.stageAll ""
    Git.Commit.exec "" (sprintf "Bump version to %s \n%s" release.NugetVersion releaseNotesGitCommitFormat)
    Git.Branches.push ""

    Git.Branches.tag "" release.NugetVersion
    Git.Branches.pushTag "" "origin" release.NugetVersion

Target.create "GitHubRelease" <| fun _ ->
    let release = release.Value
    let token =
       match githubToken with
       | Some s when not (String.IsNullOrWhiteSpace s) -> s
       | _ -> failwith "please set the github_token environment variable to a github personal access token with repro access."


    GitHub.createClientWithToken token
    |> GitHub.draftNewRelease gitOwner gitRepoName release.NugetVersion (release.SemVer.PreRelease <> None) release.Notes
    |> GitHub.publishDraft
    |> Async.RunSynchronously


let formatCode _ =
    let result =
        let args =
            [
                srcCodeGlob
                testsCodeGlob
                adaptersCodeGlob
                examplesCodeGlob
            ]
            |> Seq.collect id
            // Ignore AssemblyInfo
            |> Seq.filter(fun f -> f.EndsWith("AssemblyInfo.fs") |> not)
            |> String.concat " "

        dotnet.fantomas args

    if not result.OK then
        Trace.traceErrorfn "Errors while formatting all files: %A" result.Messages
        failwithf "Errors while formatting all files: %A" result.Messages


let checkFormatCode _ =
    let result =
        [
            srcCodeGlob
            testsCodeGlob
            adaptersCodeGlob
            examplesCodeGlob
        ]
        |> Seq.collect id
        // Ignore AssemblyInfo
        |> Seq.filter(fun f -> f.EndsWith("AssemblyInfo.fs") |> not)
        |> String.concat " "
        |> sprintf "%s --check"
        |> dotnet.fantomas

    if result.ExitCode = 0 then
        Trace.log "No files need formatting"
    elif result.ExitCode = 99 then
        failwith "Some files need formatting, check output for more info"
    else
        Trace.logf "Errors while formatting: %A" result.Errors


Target.create "FormatCode" formatCode
Target.create "CheckFormatCode" checkFormatCode

Target.create "Release" ignore

Option.iter(TraceSecrets.register "<GITHUB_TOKEN>" ) githubToken


// Only call Clean if DotnetPack was in the call chain
// Ensure Clean is called before DotnetRestore
"Clean" ?=> "DotnetRestore" |> ignore
"Clean" ==> "DotnetPack" |> ignore

// // Only call AssemblyInfo if Publish was in the call chain
// // Ensure AssemblyInfo is called after DotnetRestore and before DotnetBuild
"DotnetRestore" ?=> "AssemblyInfo" |> ignore
"AssemblyInfo" ?=> "DotnetBuild" |> ignore
"AssemblyInfo" ==> "Publish" |> ignore

"RunNpmInstall" ==> "RunNpmTests" |> ignore

"DotnetRestore"
==> "CheckFormatCode"
==> "DotnetBuild"
==> "DotnetTest"
==> "GenerateCoverageReport"
==> "RunNpmTests"
==> "DotnetPack"
==> "GitRelease"
==> "GitHubRelease"
==> "Release"
|> ignore

"DotnetRestore"
==> "WatchTests"
|> ignore


Target.runOrDefaultWithArguments "DotnetPack"

//-----------------------------------------------------------------------------
// Target Start
