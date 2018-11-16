rem NET 4.5
mkdir src\.build\NuGet\Fuxion.Identity\nuspec\lib\net45
copy  src\.build\NuGet\Fuxion.Identity\out~\net45\Fuxion.Identity.dll src\.build\NuGet\Fuxion.Identity\nuspec\lib\net45\Fuxion.Identity.dll
rem copy  src\.build\NuGet\Fuxion.Identity\out~\net45\Fuxion.Identity.pdb src\.build\NuGet\Fuxion.Identity\nuspec\lib\net45\Fuxion.Identity.pdb

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.Identity\nuspec\lib\net472
copy  src\.build\NuGet\Fuxion.Identity\out~\net472\Fuxion.Identity.dll src\.build\NuGet\Fuxion.Identity\nuspec\lib\net472\Fuxion.Identity.dll
rem copy  src\.build\NuGet\Fuxion.Identity\out~\net472\Fuxion.Identity.pdb src\.build\NuGet\Fuxion.Identity\nuspec\lib\net472\Fuxion.Identity.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.Identity\nuspec\lib\netstandard2.0
copy  src\.build\NuGet\Fuxion.Identity\out~\netstandard2.0\Fuxion.Identity.dll src\.build\NuGet\Fuxion.Identity\nuspec\lib\netstandard2.0\Fuxion.Identity.dll
rem copy  src\.build\NuGet\Fuxion.Identity\out~\netstandard2.0\Fuxion.Identity.pdb src\.build\NuGet\Fuxion.Identity\nuspec\lib\netstandard2.0\Fuxion.Identity.pdb
copy  src\.build\NuGet\Fuxion.Identity\out~\netstandard2.0\Fuxion.Identity.deps.json src\.build\NuGet\Fuxion.Identity\nuspec\lib\netstandard2.0\Fuxion.Identity.deps.json