rem NET 4.7.1
mkdir src\.build\NuGet\Fuxion.Data\nuspec\lib\net471
copy  src\.build\NuGet\Fuxion.Data\out~\net471\Fuxion.Data.dll src\.build\NuGet\Fuxion.Data\nuspec\lib\net471\Fuxion.Data.dll
copy  src\.build\NuGet\Fuxion.Data\out~\net471\Fuxion.Data.pdb src\.build\NuGet\Fuxion.Data\nuspec\lib\net471\Fuxion.Data.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.Data\nuspec\lib\netstandard2.0
copy  src\.build\NuGet\Fuxion.Data\out~\netstandard2.0\Fuxion.Data.dll src\.build\NuGet\Fuxion.Data\nuspec\lib\netstandard2.0\Fuxion.Data.dll
copy  src\.build\NuGet\Fuxion.Data\out~\netstandard2.0\Fuxion.Data.pdb src\.build\NuGet\Fuxion.Data\nuspec\lib\netstandard2.0\Fuxion.Data.pdb
copy  src\.build\NuGet\Fuxion.Data\out~\netstandard2.0\Fuxion.Data.deps.json src\.build\NuGet\Fuxion.Data\nuspec\lib\netstandard2.0\Fuxion.Data.deps.json