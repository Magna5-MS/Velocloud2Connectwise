FROM mcr.microsoft.com/dotnet/core/runtime:2.1-stretch-slim AS base
WORKDIR /app

# Set environment variables
ENV httpPort=8077
ENV jobTimer=180
ENV cwBaseURL=https://na.myconnectwise.net/v4_6_release/apis/3.0/
ENV cwConsumerKey=WElqfwuw091OERQp
ENV cwConsumerSecret=jI87DQT0Jed1UqBV
ENV cwClientID=c7b06b83-e581-4693-98eb-1aa250a65435
ENV vcoBaseURL=https://vco160-usca1.velocloud.net/portal/rest/
ENV vcoConsumerKey=api@magna5global.com
ENV vcoConsumerSecret=WrwleCHn^e83@5lX
ENV vcoLoginURL=https://vco160-usca1.velocloud.net/magna5global/login/enterpriseLogin
ENV emailReportTo=growe@magna5global.com
ENV smtpRelay=mailrelay.magna5global.com
ENV SENTRY_DSN=https://da4d4d6ac04e4f858a1a2707c8b4fa14@sentry.io/2239777

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY Velocloud2Connectwise/Velocloud2Connectwise.csproj Velocloud2Connectwise/
# Below is necessary if adding packages from custom source besides nuget
# COPY Velocloud2Connectwise/Nuget.Config ./
# RUN dotnet restore "Velocloud2Connectwise/Velocloud2Connectwise.csproj" --configfile Nuget.Config
COPY . .
WORKDIR "/src/Velocloud2Connectwise"
RUN dotnet build "Velocloud2Connectwise.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Velocloud2Connectwise.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8077
ENTRYPOINT ["dotnet", "Velocloud2Connectwise.dll"]
