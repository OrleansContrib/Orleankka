#r "System.Xml"
#r "System.Xml.Linq"
#r "System.IO.Compression"
#r "System.IO.Compression.FileSystem"

using static Nake.FS;
using static Nake.Run;
using static Nake.Log;
using static Nake.Env;
using static Nake.App;

using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO.Compression;

const string CoreProject = "Orleankka";
const string RuntimeProject = "Orleankka.Runtime";
const string TestKitProject = "Orleankka.TestKit";

const string RootPath = "%NakeScriptDirectory%";
const string OutputPath = RootPath + @"\Output";

var Version = "0.0.0-dev";
var PackagePath = @"{OutputPath}\Package";
var ReleasePath = @"{PackagePath}\Release";

var AppVeyor = Var["APPVEYOR"] == "True";
var GES = "EventStore-OSS-Win-v3.9.4";
var Nuget = @"{RootPath}\Packages\NuGet.CommandLine\tools\Nuget.exe";
var Vs17Versions = new [] {"Community", "Enterprise", "Professional"};
var MsBuildExe = GetVisualStudio17MSBuild();

/// Installs dependencies and builds sources in Debug mode
[Task] void Default()
{
    Restore();
    Build();
}

/// Builds sources using specified configuration and output path
[Step] void Build(string config = "Debug", string outDir = OutputPath, bool verbose = false) => 
    Exec(MsBuildExe, "{CoreProject}.sln /p:Configuration={config};OutDir=\"{outDir}\";ReferencePath=\"{outDir}\"" + (verbose ? "/v:d" : ""));

/// Runs unit tests 
[Step] void Test(string outDir = OutputPath, bool slow = false)
{
    Build("Debug", outDir);

    var tests = new FileSet{@"{outDir}\*.Tests.dll"}.ToString(" ");
    var results = @"{outDir}\nunit-test-results.xml";

    try
    {
        Exec("dotnet", 
            @"vstest {tests} --logger:trx;LogFileName={results} " +
            (AppVeyor||slow ? "" : "--TestCaseFilter:TestCategory!=Slow"));
    }
    finally
    {    	
	    if (AppVeyor)
	        new WebClient().UploadFile("https://ci.appveyor.com/api/testresults/nunit/%APPVEYOR_JOB_ID%", results);
	}
}

/// Builds official NuGet packages 
[Step] void Package(bool skipFullCheck = false)
{
    Test(@"{PackagePath}\Debug", !skipFullCheck);
    Build("Package", ReleasePath);

    Pack(CoreProject);    
    Pack(RuntimeProject);    
    Pack(TestKitProject);
}

void Pack(string project) =>
    Cmd(@"{Nuget} pack Build\{project}.nuspec -Version {Version} " +
         "-OutputDirectory {PackagePath} -BasePath {RootPath} -NoPackageAnalysis");

/// Publishes package to NuGet gallery
[Step] void Publish()
{
    Push(CoreProject); 
    Push(RuntimeProject); 
    Push(TestKitProject); 
}

void Push(string package) => 
    Cmd(@"{Nuget} push {PackagePath}\{package}.{Version}.nupkg " + 
        "%NuGetApiKey% -Source https://nuget.org/");

/// Installs binary dependencies 
[Task] void Restore()
{
    Exec("dotnet", "restore {CoreProject}.sln");

    var packagesDir = @"{RootPath}\Packages";

    if (!Directory.Exists(@"{packagesDir}\{GES}"))
    {
        Info("EventStore binaries were not found. Downloading ...");

        new WebClient().DownloadFile(
            "https://eventstore.org/downloads/{GES}.zip", 
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

string GetVisualStudio17MSBuild()
{
    foreach (var each in Vs17Versions) 
    {
        var msBuildPath = @"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\{each}\MSBuild\15.0\Bin\MSBuild.exe";
        if (File.Exists(msBuildPath))
            return msBuildPath;
    }

    Error("MSBuild not found!");
    Exit();

    return null;
}