rem NET 4.5
mkdir src\.build\NuGet\Fuxion\nuspec\lib\net45
mkdir src\.build\NuGet\Fuxion\nuspec\lib\net45\es
copy src\.build\NuGet\Fuxion\out~\net45\Fuxion.dll src\.build\NuGet\Fuxion\nuspec\lib\net45\Fuxion.dll
copy src\.build\NuGet\Fuxion\out~\net45\Fuxion.pdb src\.build\NuGet\Fuxion\nuspec\lib\net45\Fuxion.pdb
copy src\.build\NuGet\Fuxion\out~\net45\es\Fuxion.resources.dll src\.build\NuGet\Fuxion\nuspec\lib\net45\es\Fuxion.resources.dll

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion\nuspec\lib\net472
mkdir src\.build\NuGet\Fuxion\nuspec\lib\net472\es
copy src\.build\NuGet\Fuxion\out~\net472\Fuxion.dll src\.build\NuGet\Fuxion\nuspec\lib\net472\Fuxion.dll
rem copy src\.build\NuGet\Fuxion\out~\net472\Fuxion.pdb src\.build\NuGet\Fuxion\nuspec\lib\net472\Fuxion.pdb
copy src\.build\NuGet\Fuxion\out~\net472\es\Fuxion.resources.dll src\.build\NuGet\Fuxion\nuspec\lib\net472\es\Fuxion.resources.dll

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0
mkdir src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\es
copy src\.build\NuGet\Fuxion\out~\netstandard2.0\Fuxion.dll src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\Fuxion.dll
rem copy src\.build\NuGet\Fuxion\out~\netstandard2.0\Fuxion.pdb src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\Fuxion.pdb
copy src\.build\NuGet\Fuxion\out~\netstandard2.0\Fuxion.deps.json src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\Fuxion.deps.json
copy src\.build\NuGet\Fuxion\out~\netstandard2.0\es\Fuxion.resources.dll src\.build\NuGet\Fuxion\nuspec\lib\netstandard2.0\es\Fuxion.resources.dll