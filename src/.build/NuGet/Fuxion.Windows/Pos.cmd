rem NET 4.5
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45
copy  src\.build\NuGet\Fuxion.Windows\out~\net45\Fuxion.Windows.dll src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45\Fuxion.Windows.dll
copy  src\.build\NuGet\Fuxion.Windows\out~\net45\Fuxion.Windows.pdb src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45\Fuxion.Windows.pdb
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45\es
copy  src\.build\NuGet\Fuxion.Windows\out~\net45\es\Fuxion.Windows.resources.dll src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45\es\Fuxion.Windows.resources.dll

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472
copy  src\.build\NuGet\Fuxion.Windows\out~\net472\Fuxion.Windows.dll src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472\Fuxion.Windows.dll
rem copy  src\.build\NuGet\Fuxion.Windows\out~\net472\Fuxion.Windows.pdb src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472\Fuxion.Windows.pdb
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472\es
copy  src\.build\NuGet\Fuxion.Windows\out~\net472\es\Fuxion.Windows.resources.dll src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472\es\Fuxion.Windows.resources.dll