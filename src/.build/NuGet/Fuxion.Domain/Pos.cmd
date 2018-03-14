rem NET 4.7.1
mkdir src\.build\NuGet\Fuxion.Domain\nuspec\lib\net471
copy  src\.build\NuGet\Fuxion.Domain\out~\net471\Fuxion.Domain.dll src\.build\NuGet\Fuxion.Domain\nuspec\lib\net471\Fuxion.Domain.dll
copy  src\.build\NuGet\Fuxion.Domain\out~\net471\Fuxion.Domain.pdb src\.build\NuGet\Fuxion.Domain\nuspec\lib\net471\Fuxion.Domain.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.Domain\nuspec\lib\netstandard2.0
copy  src\.build\NuGet\Fuxion.Domain\out~\netstandard2.0\Fuxion.Domain.dll src\.build\NuGet\Fuxion.Domain\nuspec\lib\netstandard2.0\Fuxion.Domain.dll
copy  src\.build\NuGet\Fuxion.Domain\out~\netstandard2.0\Fuxion.Domain.pdb src\.build\NuGet\Fuxion.Domain\nuspec\lib\netstandard2.0\Fuxion.Domain.pdb
copy  src\.build\NuGet\Fuxion.Domain\out~\netstandard2.0\Fuxion.Domain.deps.json src\.build\NuGet\Fuxion.Domain\nuspec\lib\netstandard2.0\Fuxion.Domain.deps.json