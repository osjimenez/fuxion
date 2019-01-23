rem NET Standard 2.0
mkdir src\.build\NuGet\Fuxion.EventStore\nuspec\lib\netstandard2.0
copy  src\.build\out~\Fuxion.EventStore\netstandard2.0\Fuxion.EventStore.dll src\.build\NuGet\Fuxion.EventStore\nuspec\lib\netstandard2.0\Fuxion.EventStore.dll
copy  src\.build\out~\Fuxion.EventStore\netstandard2.0\Fuxion.EventStore.pdb src\.build\NuGet\Fuxion.EventStore\nuspec\lib\netstandard2.0\Fuxion.EventStore.pdb
copy  src\.build\out~\Fuxion.EventStore\netstandard2.0\Fuxion.EventStore.deps.json src\.build\NuGet\Fuxion.EventStore\nuspec\lib\netstandard2.0\Fuxion.EventStore.deps.json