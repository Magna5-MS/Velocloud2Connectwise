FROM mcr.microsoft.com/dotnet/core/runtime:2.1-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY Velocloud2Connectwise/Velocloud2Connectwise.csproj Velocloud2Connectwise/
RUN dotnet restore "Velocloud2Connectwise/Velocloud2Connectwise.csproj"
COPY . .
WORKDIR "/src/Velocloud2Connectwise"
RUN dotnet build "Velocloud2Connectwise.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Velocloud2Connectwise.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Velocloud2Connectwise.dll"]
