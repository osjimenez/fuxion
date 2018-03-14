rem NET 4.7.1
mkdir src\.build\NuGet\Fuxion.Web\nuspec\lib\net471
copy  src\.build\NuGet\Fuxion.Web\out~\net471\Fuxion.Web.dll src\.build\NuGet\Fuxion.Web\nuspec\lib\net471\Fuxion.Web.dll
copy  src\.build\NuGet\Fuxion.Web\out~\net471\Fuxion.Web.pdb src\.build\NuGet\Fuxion.Web\nuspec\lib\net471\Fuxion.Web.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.Web\nuspec\lib\netstandard2.0
copy  src\.build\NuGet\Fuxion.Web\out~\netstandard2.0\Fuxion.Web.dll src\.build\NuGet\Fuxion.Web\nuspec\lib\netstandard2.0\Fuxion.Web.dll
copy  src\.build\NuGet\Fuxion.Web\out~\netstandard2.0\Fuxion.Web.pdb src\.build\NuGet\Fuxion.Web\nuspec\lib\netstandard2.0\Fuxion.Web.pdb
copy  src\.build\NuGet\Fuxion.Web\out~\netstandard2.0\Fuxion.Web.deps.json src\.build\NuGet\Fuxion.Web\nuspec\lib\netstandard2.0\Fuxion.Web.deps.json