rem NET 4.5
mkdir src\.build\NuGet\Fuxion.Domain\nuspec\lib\net45
copy  src\.build\NuGet\Fuxion.Domain\out~\net45\Fuxion.Domain.dll src\.build\NuGet\Fuxion.Domain\nuspec\lib\net45\Fuxion.Domain.dll
rem copy  src\.build\NuGet\Fuxion.Domain\out~\net45\Fuxion.Domain.pdb src\.build\NuGet\Fuxion.Domain\nuspec\lib\net45\Fuxion.Domain.pdb

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.Domain\nuspec\lib\net472
copy  src\.build\NuGet\Fuxion.Domain\out~\net472\Fuxion.Domain.dll src\.build\NuGet\Fuxion.Domain\nuspec\lib\net472\Fuxion.Domain.dll
rem copy  src\.build\NuGet\Fuxion.Domain\out~\net472\Fuxion.Domain.pdb src\.build\NuGet\Fuxion.Domain\nuspec\lib\net472\Fuxion.Domain.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.Domain\nuspec\lib\netstandard2.0
copy  src\.build\NuGet\Fuxion.Domain\out~\netstandard2.0\Fuxion.Domain.dll src\.build\NuGet\Fuxion.Domain\nuspec\lib\netstandard2.0\Fuxion.Domain.dll
rem copy  src\.build\NuGet\Fuxion.Domain\out~\netstandard2.0\Fuxion.Domain.pdb src\.build\NuGet\Fuxion.Domain\nuspec\lib\netstandard2.0\Fuxion.Domain.pdb
copy  src\.build\NuGet\Fuxion.Domain\out~\netstandard2.0\Fuxion.Domain.deps.json src\.build\NuGet\Fuxion.Domain\nuspec\lib\netstandard2.0\Fuxion.Domain.deps.json