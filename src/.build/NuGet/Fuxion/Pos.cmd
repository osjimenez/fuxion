rem NET 4.7.1
mkdir src\.build\NuGet\Fuxion\nuspec\lib\net471
mkdir src\.build\NuGet\Fuxion\nuspec\lib\net471\es
copy src\.build\NuGet\Fuxion\out~\net471\Fuxion.dll src\.build\NuGet\Fuxion\nuspec\lib\net471\Fuxion.dll
copy src\.build\NuGet\Fuxion\out~\net471\Fuxion.pdb src\.build\NuGet\Fuxion\nuspec\lib\net471\Fuxion.pdb
copy src\.build\NuGet\Fuxion\out~\net471\es\Fuxion.resources.dll src\.build\NuGet\Fuxion\nuspec\lib\net471\es\Fuxion.resources.dll

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0
mkdir src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\es
copy src\.build\NuGet\Fuxion\out~\netstandard2.0\Fuxion.dll src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\Fuxion.dll
copy src\.build\NuGet\Fuxion\out~\netstandard2.0\Fuxion.pdb src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\Fuxion.pdb
copy src\.build\NuGet\Fuxion\out~\netstandard2.0\Fuxion.deps.json src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\Fuxion.deps.json
copy src\.build\NuGet\Fuxion\out~\netstandard2.0\es\Fuxion.resources.dll src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\es\Fuxion.resources.dll