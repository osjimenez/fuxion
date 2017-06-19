using System.IO;
using Microsoft.Build.Framework;
using System.Diagnostics;
using System.Windows;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class BuildTask : Microsoft.Build.Utilities.Task
{
    public override bool Execute()
    {
        Log.LogError("PROJECT: " + Path.GetFileNameWithoutExtension(BuildEngine.ProjectFileOfTaskNode));
        switch (TargetName)
        {
            case "BeforeBuild":
                return BeforeBuild();
            case "AfterBuild":
                return AfterBuild();
            default:
                return true;
        }
    }
    [Required]
    public string TargetName { get; set; }
    //[Required] 
    //public string VersionFile { get; set; }
    [Required]
    public string NugetPath { get; set; }
    private bool BeforeBuild()
    {
        //Log.LogError("PROJECT: " + Path.GetFileNameWithoutExtension(BuildEngine.ProjectFileOfTaskNode));
        var version = GetVersion();
        var path = Path.Combine(Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode), @"Properties\AssemblyInfo.cs");
        Action<string> setFile = ver => File.AppendAllLines(path, new string[] { ver });
        if (!SearchAndSet(version, path, assemblyVersionRegex, GetAssemblyVersionSentence))
            setFile(GetAssemblyVersionSentence(version));
        if (!SearchAndSet(version, path, assemblyFileVersionRegex, GetAssemblyFileVersionSentence))
            setFile(GetAssemblyFileVersionSentence(version));
        if (!SearchAndSet(version, path, assemblyInformationalVersionRegex, GetAssemblyInformationalVersionSentence))
            setFile(GetAssemblyInformationalVersionSentence(version));

        return true;
    }
    private bool AfterBuild()
    {
        var projectFile = BuildEngine.ProjectFileOfTaskNode;
        var nugetPath = Path.Combine(Path.GetDirectoryName(projectFile), NugetPath);
        foreach (var nuspecPath in Directory.GetFiles(Path.GetDirectoryName(projectFile), "*.nuspec", SearchOption.TopDirectoryOnly)
            .Where(f => !Path.GetFileNameWithoutExtension(f).Contains("~")))
        {
            var nuspecTempPath = Path.Combine(Path.GetDirectoryName(nuspecPath), Path.GetFileNameWithoutExtension(nuspecPath) + "~.nuspec");
            var semanticVersion = VersionToSemanticVersion(GetVersion());
            File.WriteAllText(nuspecTempPath, File.ReadAllText(nuspecPath)
                .Replace("###year###", DateTime.Now.Year.ToString())
                .Replace("###version###", semanticVersion));

            //var nugetDefaultOutputPath = @"bin\Nuget";
            var nugetDefaultOutputPath = Path.Combine(Path.GetDirectoryName(projectFile), @"bin\Nuget");
            var nugetOutputPaths = new List<string>();
            if (File.Exists(nugetDefaultOutputPath))
            {
                nugetOutputPaths = File.ReadAllLines(nugetDefaultOutputPath)
                    .Select(p => Path.IsPathRooted(p) ? p : Path.Combine(Path.GetDirectoryName(projectFile), p))
                    .ToList();
            }
            else if (!Directory.Exists(nugetDefaultOutputPath))
            {
                Directory.CreateDirectory(nugetDefaultOutputPath);
                nugetOutputPaths.Add(nugetDefaultOutputPath);
            }
            else
            {
                nugetOutputPaths.Add(nugetDefaultOutputPath);
            }
            var dir = new DirectoryInfo(Path.GetDirectoryName(projectFile));
            while(dir != null)
            {
                var outputFile = dir.GetFiles().FirstOrDefault(f => f.Name == "NuGet.output");
                if(outputFile != null)
                {
                    nugetOutputPaths.AddRange(File.ReadAllLines(outputFile.FullName)
                        .Select(p => Path.IsPathRooted(p) ? p : Path.Combine(Path.GetDirectoryName(projectFile), p)));
                }
                dir = dir.Parent;
            }
            foreach (var outputPath in nugetOutputPaths)
            {
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                ProcessStartInfo psi = new ProcessStartInfo(new DirectoryInfo(nugetPath).FullName, "pack " + nuspecTempPath + " -OutputDirectory " + outputPath);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                var proc = new Process();
                proc.StartInfo = psi;
                proc.OutputDataReceived += new DataReceivedEventHandler
                (
                    (sender, e) =>
                    {
                        BuildEngine.LogMessageEvent(new BuildMessageEventArgs("NUGET => "+e.Data, "", "", MessageImportance.High));
                    }
                );
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs("NUGET => Starting process: " + psi.FileName + " " + psi.Arguments, "", "", MessageImportance.High));
                proc.Start();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                    Log.LogError("NuGet exit code: " + proc.ExitCode);
            }
            File.Delete(nuspecTempPath);
        }
        return !Log.HasLoggedErrors;
    }
    private Version GetVersion()
    {
        var projectFile = BuildEngine.ProjectFileOfTaskNode;
        
        var versionFilePath = Directory.GetFiles(Path.GetDirectoryName(projectFile), "*.version", SearchOption.TopDirectoryOnly).FirstOrDefault();
        if(versionFilePath == null)
        {
            var actualDir = Path.GetDirectoryName(projectFile);
            //Log.LogError("actualDir1=" + actualDir);
            while (!string.IsNullOrWhiteSpace(actualDir))
            {
                var files = Directory.GetFiles(actualDir, "*.version", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    versionFilePath = files[0];
                    break;
                }
                actualDir = Path.GetDirectoryName(actualDir);
                //Log.LogError("actualDir=" + actualDir);
            }
        }
        if (string.IsNullOrWhiteSpace(versionFilePath)) throw new FileNotFoundException("File *.version cannot be found");
        return new Version(File.ReadAllText(Path.Combine(Path.GetDirectoryName(BuildEngine.ProjectFileOfTaskNode), versionFilePath)));
    }
    private static string VersionToSemanticVersion(Version version)
    {
        return version.Major + "." + version.Minor + "." + version.Build + RevisionToSemanticVersion(version.Revision);
    }
    private static string RevisionToSemanticVersion(int revision)
    {
        // TODO - Implements semantics versioning 2
        // https://github.com/GitTools/GitVersion/blob/master/src/GitVersionCore/SemanticVersion.cs
        if (revision <= 0)
            return "";
        if (revision < 10000)
            //return "-alpha." + revision + "+date." +
            //    DateTime.Now.Year.ToString("0000") +
            //    DateTime.Now.Month.ToString("00") +
            //    DateTime.Now.Day.ToString("00") +
            //    DateTime.Now.Hour.ToString("00") +
            //    DateTime.Now.Minute.ToString("00") +
            //    DateTime.Now.Second.ToString("00");
            return "-alpha" + revision.ToString("000");
        else if (revision < 20000)
            return "-beta" + (revision - 10000).ToString("000");
        else if (revision < 30000)
            return "-rc" + (revision - 20000).ToString("000");
        throw new FormatException("The revision number of the version with value '" + revision + "' is not convertible to semantic version");
    }
    #region Assembly info
    string assemblyVersionRegex = @"^\[\s*assembly:\s*AssemblyVersion\(.(\d+)\.(\d+)\.(\d+)\.*(\d*).\)\]";
    private string GetAssemblyVersionSentence(Version version) { return "[assembly: AssemblyVersion(\"" + version.ToString() + "\")]"; }
    string assemblyFileVersionRegex = @"^\[\s*assembly:\s*AssemblyFileVersion\(.(\d+)\.(\d+)\.(\d+)\.*(\d*).\)\]";
    private string GetAssemblyFileVersionSentence(Version version) { return "[assembly: AssemblyFileVersion(\"" + version.ToString() + "\")]"; }
    string assemblyInformationalVersionRegex = @"^\[\s*assembly:\s*AssemblyInformationalVersion\(.(\d+)\.(\d+)\.(\d+)-{0,1}(.+)\)\]";
    private string GetAssemblyInformationalVersionSentence(Version version) { return "[assembly: AssemblyInformationalVersion(\"" + VersionToSemanticVersion(version) + "\")]"; }
    private bool SearchAndSet(Version version, string path, string regex, Func<Version,string> getVersion)
    {
        var res = false;
        var lines = new List<string>();
        foreach (var line in File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)))
        {
            Regex re = new Regex(regex);
            Match ma = re.Match(line);
            if (ma.Success)
            {
                lines.Add(getVersion(version));
                res = true;
            }
            else lines.Add(line);
        }
        StreamWriter writer = File.CreateText(path);
        foreach (string line in lines)
            writer.WriteLine(line);
        writer.Close();
        return res;
    }
    #endregion
}
