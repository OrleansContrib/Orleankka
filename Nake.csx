#r "Packages/Nake/2.4.0/tools/net45/Meta.dll"
#r "Packages/Nake/2.4.0/tools/net45/Utility.dll"

#r "System.Xml"
#r "System.Xml.Linq"
#r "System.IO.Compression"
#r "System.IO.Compression.FileSystem"

using Nake;
using static Nake.FS;
using static Nake.Run;
using static Nake.Log;
using static Nake.Env;
using static Nake.App;

using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

const string CoreProject = "Orleankka";
const string RuntimeProject = "Orleankka.Runtime";
const string LegacyRuntimeProject = "Orleankka.Runtime.Legacy";
const string LegacyRuntimeSupportProject = "Orleankka.Runtime.Legacy.Support";
const string TestKitProject = "Orleankka.TestKit";

var RootPath = "%NakeScriptDirectory%";
var ArtifactsPath = $@"{RootPath}\Artifacts";
var ReleasePackagesPath = $@"{ArtifactsPath}\Release";

string AppVeyorJobId = null;
var GES = "EventStore-OSS-Win-v3.9.4";

var Version = "2.0.0-dev";

// global init
MakeDir(ArtifactsPath);

/// Installs dependencies and builds sources in Debug mode
[Task] void Default()
{
    Restore();
    Build();
}

/// Builds sources using specified configuration
[Step] void Build(string config = "Debug", bool verbose = false) => 
    Exec("dotnet", $"build {CoreProject}.sln /p:Configuration={config}" + (verbose ? "/v:d" : ""));

/// Runs unit tests 
[Step] void Test(bool slow = false)
{
    Build("Debug");

    var tests = new FileSet{$@"{RootPath}\**\bin\Debug\**\*.Tests.dll"}.ToString(" ");
    var results = $@"{ArtifactsPath}\nunit-test-results.xml";

    try
    {
        Exec("dotnet", 
            $@"vstest {tests} --logger:trx;LogFileName={results} " +
            (AppVeyorJobId != null||slow ? "" : "--TestCaseFilter:TestCategory!=Slow"));
    }
    finally
    {    	
	    if (AppVeyorJobId != null)
        {
            var workerApi = "https://ci.appveyor.com/api/testresults/mstest/{AppVeyorJobId}";
            Info($"Uploading {results} to {workerApi} using job id {AppVeyorJobId} ...");
            
            var response = new WebClient().UploadFile(workerApi, results);
            var result = System.Text.Encoding.UTF8.GetString(response);
                      
            Info($"Appveyor response is: {result}");
        }
	}
}

/// Builds official NuGet packages 
[Step] void Pack(bool skipFullCheck = false)
{
    Test(!skipFullCheck);
    Build("Release");
    Exec("dotnet", $"pack -c Release -p:PackageVersion={Version} {CoreProject}.sln");
    PackTemplate();
}

/// Publishes package to NuGet gallery
[Step] void Publish()
{
    Push(CoreProject); 
    Push(RuntimeProject);
    Push(LegacyRuntimeProject);
    Push(LegacyRuntimeSupportProject);  
    Push(TestKitProject); 
}

void Push(string package) => Exec("dotnet", 
    @"nuget push {ReleasePackagesPath}\{package}.{Version}.nupkg " +
    "-k %NuGetApiKey% -s https://nuget.org/ -ss https://nuget.smbsrc.net");

void Mirror(string source, string destination)
{
    foreach (string dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        Directory.CreateDirectory(dir.Replace(source, destination));

    foreach (string file in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
        File.Copy(file, file.Replace(source, destination), true);
}


/// Restores build-time packages and 3rd party dependencies used in demo projects
[Task] void Restore(bool packagesOnly = false)
{
    Exec("dotnet", "restore {CoreProject}.sln");
    
    if (packagesOnly)
        return;
    
    RestoreGetEventStoreBinaries();
}

void RestoreGetEventStoreBinaries()
{
    if (Directory.Exists($@"{RootPath}/Packages/{GES}"))
        return;

    Info("EventStore binaries were not found. Downloading ...");

    new WebClient().DownloadFile(
        "https://eventstore.org/downloads/{GES}.zip",
        $@"{RootPath}/Packages/{GES}.zip"
    );

    Info("Success! Extracting ...");

    ZipFile.ExtractToDirectory($@"{RootPath}/Packages/{GES}.zip", $@"{RootPath}/Packages/{GES}");
    File.Delete($@"{RootPath}/Packages/{GES}.zip");

    Info("Done!");
}

/// Runs 3rd party software, on which samples are dependent upon
[Task] void Run()
{
    if (IsRunning("EventStore.ClusterNode"))
        return;

    Info("Starting local GES node ...");
    Exec($@"{RootPath}/Packages/{GES}/EventStore.ClusterNode.exe", "");
}

bool IsRunning(string processName)
{
    var processes = Process.GetProcesses().Select(x => x.ProcessName).ToList();
    return (processes.Any(p => p == processName));
}

class Docs
{
    const string RootPath = "%NakeScriptDirectory%";

    /// Builds documentation
    [Task] void Build() => Exec("bash", "build.sh", workingDirectory: $@"{RootPath}/Docs");

    /// Releases documentation
    [Task] void Release() => Exec("bash", "release.sh 'https://github.com/OrleansContrib/Orleankka'", workingDirectory: $@"{RootPath}/Docs", ignoreExitCode: true);

    /// Serves documentation from local _site folder
    [Task] void Serve() => Exec("bash", "serve.sh", workingDirectory: $@"{RootPath}/Docs");
}