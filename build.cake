#tool nuget:?package=OctopusTools
#tool nuget:?package=ReportGenerator
#tool nuget:?package=NUnit.ConsoleRunner
#tool nuget:?package=NUnit.Extension.TeamCityEventListener&include=./**/*
#tool nuget:?package=JetBrains.dotCover.CommandLineTools
#tool nuget:?package=MSBuild.SonarQube.Runner.Tool

#addin nuget:?package=Cake.Npm&version=0.17.0
#addin "Cake.Yarn"
#addin nuget:?package=Cake.Sonar

using Cake.Common.Build;

var target = Argument ("target", "Default");
var configuration = Argument ("configuration", "Release");

//Setup variables
var assemblyVersion = Argument<string> ("assemblyVersion", "0.0.0.1");
var releaseVersion = Argument<string> ("releaseVersion", "0.0.0.1");

var BUILDVERSION = Argument<string> ("BUILDVERSION", "");
var BUILDVCSNUMBER = Argument<string> ("BUILDVCSNUMBER", "");

//Octopus related variables

// var server = Argument<string> ("server", "");
// var apikey = Argument<string> ("apikey", "");
// var channel = Argument<string> ("channel", "");

//sonar variables
// var sonarLoginKey = Argument<string> ("sonarLoginKey", "");
// var sonarKey = Argument<string> ("sonarKey", "");
// var sonarUrl = Argument<string> ("sonarUrl", "");

//VCCAPI related variables
// var VCCAPINugetServiceAccountLogin = Argument<string> ("VCCAPINugetServiceAccountLogin", "");
// var VCCAPINugetServiceAccountPassword = Argument<string> ("VCCAPINugetServiceAccountPassword", "");
// var VCCAPINugetUrl = Argument<string> ("VCCAPINugetUrl", "");

//PATHS

var projectArtifactsPath = "./artifacts/App/";
// var rgArtifactsPath = $"./artifacts/ResourceGroup/";
// var azureFunctionArtifactsPath = $"./artifacts/AzureFunctions/";
// var externalApiArtifactsPath = $"./artifacts/ExternalApi/";
// var externalApiPublishedWebsitePath = $"{externalApiArtifactsPath}_PublishedWebsites/Volvo.DigitalCommerce.DealerPricing.ExternalApi/";

//Package output
var packageOutput = "./artifacts/packages";

//Testing result paths
// var artifactsReportPath = System.IO.Path.GetFullPath("./artifacts/Report/");
// var dotCoverReportFile = artifactsReportPath + "report.dcvr";
// var dotCoverResultFile = artifactsReportPath + "result.html";

var appSolution = "./SandboxApp.sln";

//Individual projects
// var cloudServiceProject = "./Volvo.DigitalCommerce.DealerPricing.App.CloudService/Volvo.DigitalCommerce.DealerPricing.App.CloudService.ccproj";
// var azureFunctionProject = "./Volvo.DigitalCommerce.DealerPricing.AzureFunctions/Volvo.DigitalCommerce.DealerPricing.AzureFunctions.csproj";
// var externalApiProject = "./Volvo.DigitalCommerce.DealerPricing.ExternalApi/Volvo.DigitalCommerce.DealerPricing.ExternalApi.csproj";
// var externalApiBuildPath = "./Volvo.DigitalCommerce.DealerPricing.ExternalApi/bin/";
// var rgBuildPath = $"./Volvo.DigitalCommerce.DealerPricing.ResourceGroup/";
var project = "./SandboxApp/SandboxApp.csproj";


Setup(context => {
    assemblyVersion = BUILDVERSION;
    releaseVersion = $"{BUILDVERSION}-{BUILDVCSNUMBER.Substring(0, 7)}";
    Information($"The build number was {assemblyVersion}");
    Information($"The build number was {releaseVersion}");
});
Task("Clean")
    .Does (() => {
        CleanDirectories ("./*/bin");
        CleanDirectories ("./*/obj");
        CleanDirectories ("./*/node_modules");
        CleanDirectory ("./artifacts");
    });

Task("Nuget")
    .IsDependentOn ("Clean")
    .Does (() => {
        var settings = new NuGetRestoreSettings() {
            Verbosity = NuGetVerbosity.Quiet
        };

        NuGetRestore (appSolution, settings);
    });

//Commented out tasks deal with UI/FrontEnd
// Task ("NpmInstall")
//     .IsDependentOn ("Clean")
//     .Does (() => {
//         var settings = new NpmInstallSettings();

//         settings.LogLevel = NpmLogLevel.Silent;
//         settings.WorkingDirectory = uiPath;

//         NpmInstall (settings);
//     });

// Task ("RunLint")
//   .IsDependentOn ("NpmInstall")
//   .IsDependentOn ("Clean")
//   .Does (() => {
//     var settings = new NpmRunScriptSettings();

//     settings.LogLevel = NpmLogLevel.Silent;
//     settings.WorkingDirectory = uiPath;
//     settings.ScriptName = "lint";
    
//     NpmRunScript(settings);
//   });

// Task ("RunFrontendTests")
//   .IsDependentOn ("RunLint")
//   .IsDependentOn ("NpmInstall")
//   .IsDependentOn ("Clean")
//   .Does (() => {
//     var settings = new NpmRunScriptSettings();

//     settings.LogLevel = NpmLogLevel.Silent;
//     settings.WorkingDirectory = uiPath;
//     settings.ScriptName = "test";
    
//     NpmRunScript(settings);
//   });

//Task ("BuildWebpack")
//  .IsDependentOn("RunFrontendTests")
//   .IsDependentOn ("NpmInstall")
//   .IsDependentOn ("Clean")
//   .Does (() => {
//     var settings = new NpmRunScriptSettings();

//     settings.LogLevel = NpmLogLevel.Silent;
//     settings.WorkingDirectory = uiPath;
//     settings.ScriptName = "build";
//     settings.WithArguments("../../Sandbox.sln");
    
//     NpmRunScript(settings);
//   });

Task ("AssemblyVersion")
  .IsDependentOn ("Clean")
  .Does (() => {
    var globalAssemblyInfoPath = "./GlobalAssemblyInfo.cs";
    var assemblyInfoSettings = new AssemblyInfoSettings {
      Version = assemblyVersion,
      FileVersion = assemblyVersion,
      InformationalVersion = releaseVersion
    };
    CreateAssemblyInfo (globalAssemblyInfoPath, assemblyInfoSettings);
  });

//Needs Sonar license so can't be performed in a test evironment
//   Task ("Sonar-Init")
//   .IsDependentOn ("AssemblyVersion")
//   .Does (() => {
//     SonarBegin (new SonarBeginSettings {
//         Url = sonarUrl,
//         Login = sonarLoginKey,
//         Key = sonarKey,
//         Version = releaseVersion,
//         Name = "DCOM.PRICING.APPLICATION",
//         Exclusions = "**/App_Data/**,**/AppDynamics/**,**/Content/**,**/Migrations/**,**/Gcc/GCCDataExtract.cs,*.xml,*.xsd",
//         DotCoverReportsPath = dotCoverResultFile,
//         JavascriptCoverageReportsPath = uiPath + "/coverage"
//     });
//   });

Task ("Build")
  .IsDependentOn ("AssemblyVersion")
//  .IsDependentOn ("BuildWebpack")
  .IsDependentOn ("Nuget")
  .IsDependentOn ("Clean")
//  .IsDependentOn ("Sonar-Init")
  .Does (() => {
    MSBuild (appSolution,
      settings => {
        settings.SetConfiguration (configuration)
        .SetVerbosity (Verbosity.Minimal)
        .WithProperty ("targetProfile", "CI");
      }
    );
  });

// Task ("RunTests")
//   .IsDependentOn ("Build")
//   .Does (() => {
//     EnsureDirectoryExists (artifactsReportPath);

//     DotCoverCover (tool => {
//         tool.NUnit3 ("./*/bin/" + configuration + "/*.Tests.dll",
//           new NUnit3Settings {
//             NoResults = true,
//               Where = "cat==UnitTest",
//               Process = NUnit3ProcessOption.InProcess
//           });
//       },
//       new FilePath (dotCoverReportFile),
//       new DotCoverCoverSettings ()
//       .WithFilter ("-:class=*.Domain.*")
//       .WithFilter ("-:class=*.App_Start*")
//       .WithFilter ("-:class=*.EmailBrokeredMessage")
//       .WithFilter ("-:class=*.RoleWarmUp")
//       .WithFilter ("-:class=*.Startup")
//       .WithFilter ("-:class=*.WebRole")
//       .WithFilter ("-:class=*.FilterConfig")
//       .WithFilter ("-:class=*.UnityConfig")
//       .WithFilter ("-:class=*.WebApiConfig")
//       .WithAttributeFilter ("*.ExcludeFromCodeCoverageAttribute")
//     );

//     DotCoverReport (dotCoverReportFile, dotCoverResultFile, new DotCoverReportSettings { ReportType = DotCoverReportType.HTML });
//   });

// Task ("Sonar-Analyse")
//   .IsDependentOn ("RunTests")
//   .Does (() => {
//     SonarEnd (new SonarEndSettings {
//       Login = sonarLoginKey
//     });
//   });

Task ("CreatePackage")
  //.IsDependentOn ("Sonar-Analyse")
  .IsDependentOn ("Build")
  .Does (() => {

    EnsureDirectoryExists(packageOutput)

   MSBuild (project,
      settings => {
        settings.SetConfiguration (configuration)
        .SetVerbosity (Verbosity.Minimal)
        .WithTarget ("Package")
        .WithProperty("PackageLocation", "./artifacts/packages/package.zip");
      }
    );
    // MSBuild (project,
    //   settings => {
    //     settings.SetConfiguration (configuration)
    //     .SetVerbosity (Verbosity.Minimal)
    //     .WithProperty ("targetProfile", "CI")
    //     .WithProperty ("PublishDir", "." + projectArtifactsPath)
    //     .WithTarget ("publish");
    //   }
    // );
    

    // Zip(
    //         project,
    //         packageOutput
    //     );
    // MSBuild (azureFunctionProject,
    //   settings => {
    //     settings.SetConfiguration (configuration)
    //     .SetVerbosity (Verbosity.Minimal)
    //     .WithProperty ("PublishDir", "." + azureFunctionArtifactsPath)
    //     .WithTarget ("publish");
    //   }
    // );

    // MSBuild (externalApiProject,
    //   settings => {
    //     settings.SetConfiguration (configuration)
    //       .SetVerbosity (Verbosity.Minimal)
    //       .WithProperty ("OutDir", "." + externalApiArtifactsPath)
    //       .WithProperty ("DeployOnBuild", "true")
    //       .WithProperty ("DeployTarget", "Package")
    //       .WithProperty ("SkipInvalidConfigurations", "true");
    //   }
    // );

    // EnsureDirectoryExists (rgBuildPath);
    // CopyDirectory (rgBuildPath, rgArtifactsPath);
  });

//   Task ("OctoPack")
//   .IsDependentOn ("CreatePackage")
//   .Does (() => {
//     EnsureDirectoryExists (cloudServiceArtifactsPath);
//     var configurations = GetFiles ("./Volvo.DigitalCommerce.DealerPricing.App.CloudService/*.cscfg");
//     CopyFiles (configurations, cloudServiceArtifactsPath);

//     OctoPack ("Volvo.DigitalCommerce.DealerPricing.App", new OctopusPackSettings () {
//       BasePath = cloudServiceArtifactsPath,
//         OutFolder = octopackOutput,
//         Version = releaseVersion
//     });

//     OctoPack ("Volvo.DigitalCommerce.DealerPricing.AzureFunctions", new OctopusPackSettings () {
//       BasePath = azureFunctionArtifactsPath,
//         OutFolder = octopackOutput,
//         Version = releaseVersion
//     });

//     OctoPack ("Volvo.DigitalCommerce.DealerPricing.ExternalApi", new OctopusPackSettings () {
//       BasePath = externalApiPublishedWebsitePath,
//         OutFolder = octopackOutput,
//         Version = releaseVersion
//     });

//     OctoPack ("Volvo.DigitalCommerce.DealerPricing.ResourceGroup", new OctopusPackSettings () {
//       BasePath = rgArtifactsPath,
//         OutFolder = octopackOutput,
//         Version = releaseVersion
//     });
//   });

Task ("Default").IsDependentOn("CreatePackage");

RunTarget (target);