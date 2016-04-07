param($installPath, $toolsPath, $package, $project)
$project.ProjectItems.Item("log4net.config").Properties.Item("CopyToOutputDirectory").Value = 2

$project = Get-Project
$buildProject = Get-MSBuildProject $project.ProjectName
$target = $buildProject.Xml.AddTarget("AfterBuild")
#$target.AfterTargets = "AfterBuild"
$task = $target.AddTask("Move")
$task.SetParameter("SourceFiles", "`$(OutputPath)log4net.config")
$task.SetParameter("DestinationFiles", "`$(OutputPath)`$(AssemblyName).exe.log4net")
$project.Save() #persists the changes




