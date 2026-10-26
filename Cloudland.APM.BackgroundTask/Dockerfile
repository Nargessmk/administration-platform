FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MAYSA.APM.BackgroundTask/MAYSA.APM.BackgroundTask.csproj", "MAYSA.APM.BackgroundTask/"]
RUN dotnet restore "MAYSA.APM.BackgroundTask/MAYSA.APM.BackgroundTask.csproj"
COPY . .
WORKDIR "/src/MAYSA.APM.BackgroundTask"
RUN dotnet build "MAYSA.APM.BackgroundTask.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "MAYSA.APM.BackgroundTask.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MAYSA.APM.BackgroundTask.dll"]
