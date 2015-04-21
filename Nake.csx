#r "System.Xml"
#r "System.Xml.Linq"
#r "System.IO.Compression"
#r "System.IO.Compression.FileSystem"

using Nake.FS;
using Nake.Run;
using Nake.Log;
using Nake.Env;
using Nake.App;

using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO.Compression;

const string CoreProject = "Orleankka";
const string AzureProject = "Orleankka.Azure";
const string TestKitProject = "Orleankka.TestKit";
const string FSharpProject = "Orleankka.FSharp";

const string RootPath = "$NakeScriptDirectory$";
const string OutputPath = RootPath + @"\Output";

var PackagePath = @"{OutputPath}\Package";
var ReleasePath = @"{PackagePath}\Release";

var AppVeyor = Var["APPVEYOR"] == "True";
var GES = "EventStore-OSS-Win-v3.0.3";

/// Installs dependencies and builds sources in Debug mode
[Task] void Default()
{
    Install();
    Build();
}

/// Wipeout all build output and temporary build files
[Step] void Clean(string path = OutputPath)
{
    Delete(@"{path}\*.*|-:*.vshost.exe");
    RemoveDir(@"**\bin|**\obj|{path}\*|-:*.vshost.exe");
}

/// Builds sources using specified configuration and output path
[Step] void Build(string config = "Debug", string outDir = OutputPath)
{    
    Clean(outDir);

    Exec(@"$ProgramFiles(x86)$\MSBuild\12.0\Bin\MSBuild.exe", 
          "{CoreProject}.sln /p:Configuration={config};OutDir={outDir};ReferencePath={outDir}");
}

/// Runs unit tests 
[Step] void Test(string outDir = OutputPath)
{
    Build("Debug", outDir);

    var tests = new FileSet{@"{outDir}\*.Tests.dll"}.ToString(" ");
    var results = @"{outDir}\nunit-test-results.xml";

    try
    {
        Cmd(@"Packages\NUnit.Runners.2.6.3\tools\nunit-console.exe " + 
            @"/xml:{results} /framework:net-4.0 /noshadow /nologo {tests} " +
            (AppVeyor ? "/include:Slow" : ""));
    }
    finally
    {    	
	    if (AppVeyor)
	        new WebClient().UploadFile("https://ci.appveyor.com/api/testresults/nunit/$APPVEYOR_JOB_ID$", results);
	}
}

/// Builds official NuGet packages 
[Step] void Package()
{
    Test(@"{PackagePath}\Debug");
    Build("Package", ReleasePath);

    Merge();
    Pack(CoreProject);
    
    Pack(TestKitProject, "core_version={Version(CoreProject)}");
    Pack(AzureProject,   "core_version={Version(CoreProject)}");
    Pack(FSharpProject,  "core_version={Version(CoreProject)}");
}

void Merge()
{
    var mergeDir = @"{PackagePath}\Merged";
    
    if (!Directory.Exists(mergeDir))
        Directory.CreateDirectory(mergeDir);

    Cmd(@"Packages\ilmerge.2.14.1208\tools\ILMerge.exe /copyattrs /target:library /xmldocs /lib:{ReleasePath}" +
        @" /out:{mergeDir}\Orleankka.dll Orleankka.dll Orleankka.Core.dll");
}

void Pack(string project, string properties = null)
{
    Cmd(@"Tools\Nuget.exe pack Build\{project}.nuspec -Version {Version(project)} " +
         "-OutputDirectory {PackagePath} -BasePath {RootPath} -NoPackageAnalysis " + 
         (properties != null ? "-Properties {properties}" : ""));
}

/// Publishes package to NuGet gallery
[Step] void Publish(string project)
{
    switch (project)
    {
        case "core": 
            Push(CoreProject); 
            break;
        case "azure": 
            Push(AzureProject); 
            break;
        case "testkit": 
            Push(TestKitProject); 
            break;        
        case "fsharp": 
            Push(FSharpProject); 
            break;
        default:
            throw new ArgumentException("Available values are: core, azure, testkit or fsharp");   
    }
}

void Push(string project)
{
    Cmd(@"Tools\Nuget.exe push {PackagePath}\{project}.{Version(project)}.nupkg $NuGetApiKey$");
}

string Version(string project)
{
    return FileVersionInfo
            .GetVersionInfo(@"{ReleasePath}\{project}.dll")
            .FileVersion;
}

/// Installs dependencies (packages) 
[Task] void Install()
{
    InstallPackages();
    InstallBinaries();
}

void InstallPackages()
{
    var packagesDir = @"{RootPath}\Packages";

    var configs = XElement
        .Load(@"{packagesDir}\repositories.config")
        .Descendants("repository")
        .Select(x => x.Attribute("path").Value.Replace("..", RootPath)); 

    foreach (var config in configs)
        Cmd(@"Tools\NuGet.exe install {config} -o {packagesDir}");

    // install packages required for building/testing/publishing package
    Cmd(@"Tools\NuGet.exe install Build/Packages.config -o {packagesDir}");
}

void InstallBinaries()
{
    var packagesDir = @"{RootPath}\Packages";

    if (!Directory.Exists(@"{packagesDir}\{GES}"))
    {
        Info("EventStore binaries were not found. Downloading ...");

        new WebClient().DownloadFile(
            "http://download.geteventstore.com/binaries/{GES}.zip", 
            @"{packagesDir}\{GES}.zip"
        );

        Info("Success! Extracting ...");

        ZipFile.ExtractToDirectory(@"{packagesDir}\{GES}.zip", @"{packagesDir}\{GES}");
        File.Delete(@"{packagesDir}\{GES}.zip");

        Info("Done!");
    }
}

/// Runs software, on which samples are dependent
[Task] void Run(string what = "ges")
{
    var packagesDir = @"{RootPath}\Packages";

    switch (what)
    {
        case "ges":
            Cmd(@"start {packagesDir}/{GES}/EventStore.ClusterNode.exe"); 
            break;
        default:
            throw new ArgumentException("Available values are: ges, ...");   
    }
}