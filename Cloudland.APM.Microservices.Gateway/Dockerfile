FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MAYSA.APM.Microservices.Gateway/MAYSA.APM.Microservices.Gateway.csproj", "MAYSA.APM.Microservices.Gateway/"]
RUN dotnet restore "MAYSA.APM.Microservices.Gateway/MAYSA.APM.Microservices.Gateway.csproj"
COPY . .
WORKDIR "/src/MAYSA.APM.Microservices.Gateway"
RUN dotnet build "MAYSA.APM.Microservices.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MAYSA.APM.Microservices.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MAYSA.APM.Microservices.Gateway.dll"]
