rem framework independent
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\tools
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\content

rem NET 4.5
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\lib\net45
copy  src\.build\out~\Fuxion.Log4net\net45\Fuxion.Log4net.dll src\.build\NuGet\Fuxion.Log4net\nuspec\lib\net45\Fuxion.Log4net.dll
copy  src\.build\out~\Fuxion.Log4net\net45\Fuxion.Log4net.pdb src\.build\NuGet\Fuxion.Log4net\nuspec\lib\net45\Fuxion.Log4net.pdb
copy  src\.build\out~\Fuxion.Log4net\net45\install.ps1 src\.build\NuGet\Fuxion.Log4net\nuspec\tools\install.ps1
copy  src\.build\out~\Fuxion.Log4net\net45\log4net.config src\.build\NuGet\Fuxion.Log4net\nuspec\content\log4net.config

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\lib\net472
copy  src\.build\out~\Fuxion.Log4net\net472\Fuxion.Log4net.dll src\.build\NuGet\Fuxion.Log4net\nuspec\lib\net472\Fuxion.Log4net.dll
copy  src\.build\out~\Fuxion.Log4net\net472\Fuxion.Log4net.pdb src\.build\NuGet\Fuxion.Log4net\nuspec\lib\net472\Fuxion.Log4net.pdb
copy  src\.build\out~\Fuxion.Log4net\net472\install.ps1 src\.build\NuGet\Fuxion.Log4net\nuspec\tools\install.ps1
copy  src\.build\out~\Fuxion.Log4net\net472\log4net.config src\.build\NuGet\Fuxion.Log4net\nuspec\content\log4net.config

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.Log4net\nuspec\lib\netstandard2.0
copy  src\.build\out~\Fuxion.Log4net\netstandard2.0\Fuxion.Log4net.dll src\.build\NuGet\Fuxion.Log4net\nuspec\lib\netstandard2.0\Fuxion.Log4net.dll
copy  src\.build\out~\Fuxion.Log4net\netstandard2.0\Fuxion.Log4net.pdb src\.build\NuGet\Fuxion.Log4net\nuspec\lib\netstandard2.0\Fuxion.Log4net.pdb
copy  src\.build\out~\Fuxion.Log4net\netstandard2.0\Fuxion.Log4net.deps.json src\.build\NuGet\Fuxion.Log4net\nuspec\lib\netstandard2.0\Fuxion.Log4net.deps.json
copy  src\.build\out~\Fuxion.Log4net\netstandard2.0\install.ps1 src\.build\NuGet\Fuxion.Log4net\nuspec\tools\install.ps1
copy  src\.build\out~\Fuxion.Log4net\netstandard2.0\log4net.config src\.build\NuGet\Fuxion.Log4net\nuspec\content\log4net.config