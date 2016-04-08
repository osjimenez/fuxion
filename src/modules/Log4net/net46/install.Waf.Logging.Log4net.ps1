param($installPath, $toolsPath, $package, $project)

Write-Host "Configuring project for Fuxion.log4net behavior"

$project.ProjectItems.Item("log4net.config").Properties.Item("CopyToOutputDirectory").Value = 2
$buildProject = Get-MSBuildProject $project.ProjectName
ForEach ($child in $buildProject.Xml.Targets){
	If ($child.Name -eq "Fuxion-Log4net-Rename-Config-File")
	{
		$buildProject.Xml.RemoveChild($child)
	}
}
$target = $buildProject.Xml.AddTarget("Fuxion-Log4net-Rename-Config-File")
$target.BeforeTargets = "AfterBuild"
$task = $target.AddTask("Move")
$task.SetParameter("SourceFiles", "`$(OutputPath)log4net.config")
$task.SetParameter("DestinationFiles", "`$(OutputPath)`$(AssemblyName).exe.log4net")
# $task.SetParameter("ContinueOnError", "`true")
$project.Save()




