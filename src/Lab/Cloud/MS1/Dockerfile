FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Lab/Cloud/MS1/Fuxion.Lab.Cloud.MS1.csproj", "Lab/Cloud/MS1/"]
COPY ["Lab/Common/Fuxion.Lab.Common.csproj", "Lab/Common/"]
RUN dotnet restore "Lab/Cloud/MS1/Fuxion.Lab.Cloud.MS1.csproj"
COPY . .
WORKDIR "/src/Lab/Cloud/MS1"
RUN dotnet build "Fuxion.Lab.Cloud.MS1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fuxion.Lab.Cloud.MS1.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fuxion.Lab.Cloud.MS1.dll"]
