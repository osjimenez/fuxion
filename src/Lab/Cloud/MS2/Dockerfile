FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Lab/Cloud/MS2/Fuxion.Lab.Cloud.MS2.csproj", "Lab/Cloud/MS2/"]
COPY ["Lab/Common/Fuxion.Lab.Common.csproj", "Lab/Common/"]
RUN dotnet restore "Lab/Cloud/MS2/Fuxion.Lab.Cloud.MS2.csproj"
COPY . .
WORKDIR "/src/Lab/Cloud/MS2"
RUN dotnet build "Fuxion.Lab.Cloud.MS2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fuxion.Lab.Cloud.MS2.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fuxion.Lab.Cloud.MS2.dll"]
