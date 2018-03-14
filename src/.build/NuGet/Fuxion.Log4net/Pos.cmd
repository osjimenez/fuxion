rem NET 4.7.1
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\lib\net471
copy  src\.build\NuGet\Fuxion.Log4net\out~\net471\Fuxion.Log4net.dll src\.build\NuGet\Fuxion.Log4net\nuspec\lib\net471\Fuxion.Log4net.dll
copy  src\.build\NuGet\Fuxion.Log4net\out~\net471\Fuxion.Log4net.pdb src\.build\NuGet\Fuxion.Log4net\nuspec\lib\net471\Fuxion.Log4net.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\tools
copy  src\.build\NuGet\Fuxion.Log4net\out~\net461\install.ps1 src\.build\NuGet\Fuxion.Log4net\nuspec\tools\install.ps1
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\content
copy  src\.build\NuGet\Fuxion.Log4net\out~\net461\log4net.config src\.build\NuGet\Fuxion.Log4net\nuspec\content\log4net.config