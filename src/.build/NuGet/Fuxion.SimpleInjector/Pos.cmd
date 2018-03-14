rem NET 4.7.1
mkdir src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\net471
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\net471\Fuxion.SimpleInjector.dll src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\net471\Fuxion.SimpleInjector.dll
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\net471\Fuxion.SimpleInjector.pdb src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\net471\Fuxion.SimpleInjector.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\netstandard2.0
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\netstandard2.0\Fuxion.SimpleInjector.dll src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\netstandard2.0\Fuxion.SimpleInjector.dll
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\netstandard2.0\Fuxion.SimpleInjector.pdb src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\netstandard2.0\Fuxion.SimpleInjector.pdb
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\netstandard2.0\Fuxion.SimpleInjector.deps.json src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\netstandard2.0\Fuxion.SimpleInjector.deps.json