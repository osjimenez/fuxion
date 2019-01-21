rem NET 4.5
mkdir src\.build\NuGet\Fuxion.EntityFramework\nuspec\lib\net45
copy  src\.build\out~\Fuxion.EntityFramework\net45\Fuxion.EntityFramework.dll src\.build\NuGet\Fuxion.EntityFramework\nuspec\lib\net45\Fuxion.EntityFramework.dll
copy  src\.build\out~\Fuxion.EntityFramework\net45\Fuxion.EntityFramework.pdb src\.build\NuGet\Fuxion.EntityFramework\nuspec\lib\net45\Fuxion.EntityFramework.pdb

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.EntityFramework\nuspec\lib\net472
copy  src\.build\out~\Fuxion.EntityFramework\net472\Fuxion.EntityFramework.dll src\.build\NuGet\Fuxion.EntityFramework\nuspec\lib\net472\Fuxion.EntityFramework.dll
copy  src\.build\out~\Fuxion.EntityFramework\net472\Fuxion.EntityFramework.pdb src\.build\NuGet\Fuxion.EntityFramework\nuspec\lib\net472\Fuxion.EntityFramework.pdb