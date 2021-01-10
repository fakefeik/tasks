### Как запустить
```
docker-compose up -d
dotnet tool restore
dotnet build --configuration Release ./tasks.sln
dotnet ef database update --project ./Tasks.Api/Tasks.Api.csproj --no-build --configuration Release
./Tasks.Api/bin/Release/net5.0/Tasks.Api.exe
dotnet test --no-build --configuration Release ./Tasks.Tests/Tasks.Tests.csproj
```