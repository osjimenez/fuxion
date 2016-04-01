param($installPath, $toolsPath, $package, $project)
$project.ProjectItems.Item("log4net.config").Properties.Item("CopyToOutputDirectory").Value = 2