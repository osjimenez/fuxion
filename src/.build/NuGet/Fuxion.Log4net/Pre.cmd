rem Clean output folder
mkdir src\.build\out~\Fuxion.Log4net
rmdir src\.build\out~\Fuxion.Log4net /s /q

rem Clean NuGet folder
mkdir src\.build\Nuget\Fuxion.Log4net\nuspec\lib
rmdir src\.build\Nuget\Fuxion.Log4net\nuspec\lib /s /q
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\tools
rmdir src\.build\NuGet\Fuxion.Log4net\nuspec\tools /s /q
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\content
rmdir src\.build\NuGet\Fuxion.Log4net\nuspec\content /s /q