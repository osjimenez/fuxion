rem NET 4.5
mkdir src\.build\NuGet\Fuxion.AspNet\nuspec\lib\net45
copy  src\.build\out~\Fuxion.AspNet\net45\Fuxion.AspNet.dll src\.build\NuGet\Fuxion.AspNet\nuspec\lib\net45\Fuxion.AspNet.dll
copy  src\.build\out~\Fuxion.AspNet\net45\Fuxion.AspNet.pdb src\.build\NuGet\Fuxion.AspNet\nuspec\lib\net45\Fuxion.AspNet.pdb

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.AspNet\nuspec\lib\net472
copy  src\.build\out~\Fuxion.AspNet\net472\Fuxion.AspNet.dll src\.build\NuGet\Fuxion.AspNet\nuspec\lib\net472\Fuxion.AspNet.dll
copy  src\.build\out~\Fuxion.AspNet\net472\Fuxion.AspNet.pdb src\.build\NuGet\Fuxion.AspNet\nuspec\lib\net472\Fuxion.AspNet.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.AspNet\nuspec\lib\netstandard2.0
copy  src\.build\out~\Fuxion.AspNet\netstandard2.0\Fuxion.AspNet.dll src\.build\NuGet\Fuxion.AspNet\nuspec\lib\netstandard2.0\Fuxion.AspNet.dll
copy  src\.build\out~\Fuxion.AspNet\netstandard2.0\Fuxion.AspNet.pdb src\.build\NuGet\Fuxion.AspNet\nuspec\lib\netstandard2.0\Fuxion.AspNet.pdb
copy  src\.build\out~\Fuxion.AspNet\netstandard2.0\Fuxion.AspNet.deps.json src\.build\NuGet\Fuxion.AspNet\nuspec\lib\netstandard2.0\Fuxion.AspNet.deps.json