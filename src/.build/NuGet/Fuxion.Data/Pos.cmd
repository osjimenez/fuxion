rem NET 4.5
mkdir src\.build\NuGet\Fuxion.Data\nuspec\lib\net45
copy  src\.build\NuGet\Fuxion.Data\out~\net45\Fuxion.Data.dll src\.build\NuGet\Fuxion.Data\nuspec\lib\net45\Fuxion.Data.dll
copy  src\.build\NuGet\Fuxion.Data\out~\net45\Fuxion.Data.pdb src\.build\NuGet\Fuxion.Data\nuspec\lib\net45\Fuxion.Data.pdb

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.Data\nuspec\lib\net472
copy  src\.build\NuGet\Fuxion.Data\out~\net472\Fuxion.Data.dll src\.build\NuGet\Fuxion.Data\nuspec\lib\net472\Fuxion.Data.dll
copy  src\.build\NuGet\Fuxion.Data\out~\net472\Fuxion.Data.pdb src\.build\NuGet\Fuxion.Data\nuspec\lib\net472\Fuxion.Data.pdb