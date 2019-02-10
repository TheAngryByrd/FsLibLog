# FsLibLog

FsLibLog is a single file you can copy paste or add through [Paket Github dependencies](https://fsprojects.github.io/Paket/github-dependencies.html) to provide your library with a logging abstraction.


## Getting started

### 1. Put the file into your project

#### Option 1

Copy/paste [FsLibLog.fs](https://github.com/TheAngryByrd/FsLibLog/blob/master/src/FsLibLog/FsLibLog.fs) into your library 

#### Option 2

Read over [Paket Github dependencies](https://fsprojects.github.io/Paket/github-dependencies.html).

Add the following line to your `paket.depedencies` file.

```
github TheAngryByrd/FsLibLog src/FsLibLog/FsLibLog.fs
```

Then add the following line to projects with `paket.references` file you want FsLibLog to be available to.

```
File: FsLibLog.fs
```


### 2. Replace its namespace with yours

To alleviate potential naming conflicts, it's best to replace FsLibLog namespace with your own.

Here is an example with FAKE 5:

```fsharp
Target.create "Replace" <| fun _ ->
  Shell.ReplaceInFiles
    [ "FsLibLog", "MyLib.Logging" ]
    (!! "paket-files/TheAngryByrd/FsLibLog/src/FsLibLog/FsLibLog.fs")
```


## Currently supported providers

- [Serilog](https://github.com/serilog/serilog)
- Console

---

## Builds

MacOS/Linux | Windows
--- | ---
[![Travis Badge](https://travis-ci.org/TheAngryByrd/FsLibLog.svg?branch=master)](https://travis-ci.org/TheAngryByrd/FsLibLog) | [![Build status](https://ci.appveyor.com/api/projects/status/github/TheAngryByrd/FsLibLog?svg=true)](https://ci.appveyor.com/project/TheAngryByrd/FsLibLog)
[![Build History](https://buildstats.info/travisci/chart/TheAngryByrd/FsLibLog)](https://travis-ci.org/TheAngryByrd/FsLibLog/builds) | [![Build History](https://buildstats.info/appveyor/chart/TheAngryByrd/FsLibLog)](https://ci.appveyor.com/project/TheAngryByrd/FsLibLog)  


---

### Building


Make sure the following **requirements** are installed in your system:

* [dotnet SDK](https://www.microsoft.com/net/download/core) 2.0 or higher
* [Mono](http://www.mono-project.com/) if you're on Linux or macOS.

```
> build.cmd // on windows
$ ./build.sh  // on unix
```

#### Environment Variables

* `CONFIGURATION` will set the [configuration](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build?tabs=netcore2x#options) of the dotnet commands.  If not set it will default to Release.
  * `CONFIGURATION=Debug ./build.sh` will result in things like `dotnet build -c Debug`
* `GITHUB_TOKEN` will be used to upload release notes and nuget packages to github.
  * Be sure to set this before releasing

### Watch Tests

The `WatchTests` target will use [dotnet-watch](https://github.com/aspnet/Docs/blob/master/aspnetcore/tutorials/dotnet-watch.md) to watch for changes in your lib or tests and re-run your tests on all `TargetFrameworks`

```
./build.sh WatchTests
```

### Releasing
* [Start a git repo with a remote](https://help.github.com/articles/adding-an-existing-project-to-github-using-the-command-line/)

```
git add .
git commit -m "Scaffold"
git remote add origin origin https://github.com/user/MyCoolNewLib.git
git push -u origin master
```

* [Add your nuget API key to paket](https://fsprojects.github.io/Paket/paket-config.html#Adding-a-NuGet-API-key)

```
paket config add-token "https://www.nuget.org" 4003d786-cc37-4004-bfdf-c4f3e8ef9b3a
```

* [Create a GitHub OAuth Token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/)
    * You can then set the `GITHUB_TOKEN` to upload release notes and artifacts to github
    * Otherwise it will fallback to username/password


* Then update the `RELEASE_NOTES.md` with a new version, date, and release notes [ReleaseNotesHelper](https://fsharp.github.io/FAKE/apidocs/fake-releasenoteshelper.html)

```
#### 0.2.0 - 2017-04-20
* FEATURE: Does cool stuff!
* BUGFIX: Fixes that silly oversight
```

* You can then use the `Release` target.  This will:
    * make a commit bumping the version:  `Bump version to 0.2.0` and add the release notes to the commit
    * publish the package to nuget
    * push a git tag

```
./build.sh Release
```


### Code formatting

To format code run the following target

```
./build.sh FormatCode
```

This uses [Fantomas](https://github.com/fsprojects/fantomas) to do code formatting.  Please report code formatting bugs to that repository.
