FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Lab/OnPremise/A1/Fuxion.Lab.OnPremise.A1.csproj", "Lab/OnPremise/A1/"]
RUN dotnet restore "Lab/OnPremise/A1/Fuxion.Lab.OnPremise.A1.csproj"
COPY . .
WORKDIR "/src/Lab/OnPremise/A1"
RUN dotnet build "Fuxion.Lab.OnPremise.A1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Fuxion.Lab.OnPremise.A1.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fuxion.Lab.OnPremise.A1.dll"]
