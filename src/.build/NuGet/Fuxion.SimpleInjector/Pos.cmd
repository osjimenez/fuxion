rem NET 4.5
mkdir src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\net45
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\net45\Fuxion.SimpleInjector.dll src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\net45\Fuxion.SimpleInjector.dll
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\net45\Fuxion.SimpleInjector.pdb src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\net45\Fuxion.SimpleInjector.pdb

rem NET 4.7.2
mkdir src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\net472
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\net472\Fuxion.SimpleInjector.dll src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\net472\Fuxion.SimpleInjector.dll
rem copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\net472\Fuxion.SimpleInjector.pdb src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\net472\Fuxion.SimpleInjector.pdb

rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\netstandard2.0
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\netstandard2.0\Fuxion.SimpleInjector.dll src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\netstandard2.0\Fuxion.SimpleInjector.dll
rem copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\netstandard2.0\Fuxion.SimpleInjector.pdb src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\netstandard2.0\Fuxion.SimpleInjector.pdb
copy  src\.build\NuGet\Fuxion.SimpleInjector\out~\netstandard2.0\Fuxion.SimpleInjector.deps.json src\.build\NuGet\Fuxion.SimpleInjector\nuspec\lib\netstandard2.0\Fuxion.SimpleInjector.deps.json