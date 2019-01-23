rem NET 4.5
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45\es
copy  src\.build\out~\Fuxion.Windows\net45\Fuxion.Windows.dll				src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45\Fuxion.Windows.dll
copy  src\.build\out~\Fuxion.Windows\net45\Fuxion.Windows.pdb				src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45\Fuxion.Windows.pdb
copy  src\.build\out~\Fuxion.Windows\net45\es\Fuxion.Windows.resources.dll	src\.build\NuGet\Fuxion.Windows\nuspec\lib\net45\es\Fuxion.Windows.resources.dll

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472\es
copy  src\.build\out~\Fuxion.Windows\net472\Fuxion.Windows.dll				src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472\Fuxion.Windows.dll
copy  src\.build\out~\Fuxion.Windows\net472\Fuxion.Windows.pdb				src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472\Fuxion.Windows.pdb
copy  src\.build\out~\Fuxion.Windows\net472\es\Fuxion.Windows.resources.dll	src\.build\NuGet\Fuxion.Windows\nuspec\lib\net472\es\Fuxion.Windows.resources.dll

rem NET Core 3.0
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\netcoreapp3.0
mkdir src\.build\NuGet\Fuxion.Windows\nuspec\lib\netcoreapp3.0\es
copy  src\.build\out~\Fuxion.Windows\netcoreapp3.0\Fuxion.Windows.dll				src\.build\NuGet\Fuxion.Windows\nuspec\lib\netcoreapp3.0\Fuxion.Windows.dll
copy  src\.build\out~\Fuxion.Windows\netcoreapp3.0\Fuxion.Windows.pdb				src\.build\NuGet\Fuxion.Windows\nuspec\lib\netcoreapp3.0\Fuxion.Windows.pdb
copy  src\.build\out~\Fuxion.Windows\netcoreapp3.0\Fuxion.Windows.deps.json			src\.build\NuGet\Fuxion.Windows\nuspec\lib\netcoreapp3.0\Fuxion.Windows.deps.json
copy  src\.build\out~\Fuxion.Windows\netcoreapp3.0\es\Fuxion.Windows.resources.dll	src\.build\NuGet\Fuxion.Windows\nuspec\lib\netcoreapp3.0\es\Fuxion.Windows.resources.dll