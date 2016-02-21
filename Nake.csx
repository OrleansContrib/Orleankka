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
const string Beta = "";

const string RootPath = "$NakeScriptDirectory$";
const string OutputPath = RootPath + @"\Output";

var PackagePath = @"{OutputPath}\Package";
var ReleasePath = @"{PackagePath}\Release";

var AppVeyor = Var["APPVEYOR"] == "True";
var GES = "EventStore-OSS-Win-v3.0.3";
var Nuget = @"{PackagePath}\NuGet.CommandLine\tools\Nuget.exe";

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
[Step] void Build(string config = "Debug", string outDir = OutputPath, bool verbose = false)
{    
    Clean(outDir);

    Exec(@"$ProgramFiles(x86)$\MSBuild\14.0\Bin\MSBuild.exe", 
          "{CoreProject}.sln /p:Configuration={config};OutDir={outDir};ReferencePath={outDir}" + 
           (verbose ? "/v:d" : ""));
}

/// Runs unit tests 
[Step] void Test(string outDir = OutputPath, bool slow = false)
{
    Build("Debug", outDir);

    var tests = new FileSet{@"{outDir}\*.Tests.dll"}.ToString(" ");
    var results = @"{outDir}\nunit-test-results.xml";

    try
    {
        Cmd(@"Packages\NUnit.Runners.2.6.4\tools\nunit-console.exe " + 
            @"/xml:{results} /framework:net-4.0 /noshadow /nologo {tests} " +
            (AppVeyor||slow ? "/include:Always,Slow" : ""));
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

    Pack(CoreProject);    
    Pack(TestKitProject, "core_version={Version(CoreProject)}");
    Pack(AzureProject,   "core_version={Version(CoreProject)}");
    Pack(FSharpProject,  "core_version={Version(CoreProject)}");
}

void Pack(string project, string properties = null)
{
    Cmd(@"{Nuget} pack Build\{project}.nuspec -Version {Version(project)} " +
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
        case "all":
            Push(CoreProject); 
            Push(AzureProject); 
            Push(TestKitProject); 
            Push(FSharpProject);  
            break;      
        default:
            throw new ArgumentException("Available values are: core, azure, testkit, fsharp or all");   
    }
}

void Push(string project)
{
    Cmd(@"{Nuget} push {PackagePath}\{project}.{Version(project)}.nupkg $NuGetApiKey$");
}

string Version(string project)
{
    var result = FileVersionInfo
            .GetVersionInfo(@"{ReleasePath}\{project}.dll")
            .FileVersion;

    if (Beta != "")
        result = result.Substring(0, result.LastIndexOf(".")) + "-{Beta}";

    return result;
}

/// Installs binary dependencies 
[Task] void Install()
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

/// Runs 3rd party software, on which samples are dependent upon
[Task] void Run(string what = "all")
{
    switch (what)
    {
        case "all":
            RunAzure();
            RunGES();
            break;            
        case "ges":
            RunGES();
            break;
        case "azure":
            RunAzure(); 
            break;            
        default:
            throw new ArgumentException("Available values are: all, ges, azure ...");
    }
}

void RunGES() 
{
    if (IsRunning("EventStore.ClusterNode"))
        return;

    Info("Starting local GES node ...");
    Exec(@"{RootPath}/Packages/{GES}/EventStore.ClusterNode.exe", "");
}

void RunAzure()
{
    if (IsRunning("AzureStorageEmulator"))
        return;

    Info("Starting storage emulator ...");
    Exec(@"C:\Program Files (x86)\Microsoft SDKs\Azure\Storage Emulator\AzureStorageEmulator.exe", "start");
}

bool IsRunning(string processName)
{
    var processes = Process.GetProcesses().Select(x => x.ProcessName).ToList();
    return (processes.Any(p => p == processName));
}