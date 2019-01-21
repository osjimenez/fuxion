rem Clean output folder
mkdir src\.build\out~\Fuxion.EventStore
rmdir src\.build\out~\Fuxion.EventStore /s /q

rem Clean NuGet folder
mkdir src\.build\Nuget\Fuxion.EventStore\nuspec\lib
rmdir src\.build\Nuget\Fuxion.EventStore\nuspec\lib /s /q