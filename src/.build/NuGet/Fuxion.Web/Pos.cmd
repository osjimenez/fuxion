rem NET 4.5
mkdir src\.build\NuGet\Fuxion.Web\nuspec\lib\net45
copy  src\.build\NuGet\Fuxion.Web\out~\net45\Fuxion.Web.dll src\.build\NuGet\Fuxion.Web\nuspec\lib\net45\Fuxion.Web.dll
copy  src\.build\NuGet\Fuxion.Web\out~\net45\Fuxion.Web.pdb src\.build\NuGet\Fuxion.Web\nuspec\lib\net45\Fuxion.Web.pdb

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.Web\nuspec\lib\net472
copy  src\.build\NuGet\Fuxion.Web\out~\net472\Fuxion.Web.dll src\.build\NuGet\Fuxion.Web\nuspec\lib\net472\Fuxion.Web.dll
copy  src\.build\NuGet\Fuxion.Web\out~\net472\Fuxion.Web.pdb src\.build\NuGet\Fuxion.Web\nuspec\lib\net472\Fuxion.Web.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.Web\nuspec\lib\netstandard2.0
copy  src\.build\NuGet\Fuxion.Web\out~\netstandard2.0\Fuxion.Web.dll src\.build\NuGet\Fuxion.Web\nuspec\lib\netstandard2.0\Fuxion.Web.dll
copy  src\.build\NuGet\Fuxion.Web\out~\netstandard2.0\Fuxion.Web.pdb src\.build\NuGet\Fuxion.Web\nuspec\lib\netstandard2.0\Fuxion.Web.pdb
copy  src\.build\NuGet\Fuxion.Web\out~\netstandard2.0\Fuxion.Web.deps.json src\.build\NuGet\Fuxion.Web\nuspec\lib\netstandard2.0\Fuxion.Web.deps.json