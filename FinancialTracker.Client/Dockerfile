FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FinancialTracker.Client.Razor/FinancialTracker.Client.Razor.csproj", "FinancialTracker.Client.Razor/"]
RUN dotnet restore "FinancialTracker.Client.Razor/FinancialTracker.Client.Razor.csproj"
COPY . .
WORKDIR "/src/FinancialTracker.Client.Razor"
RUN dotnet build "FinancialTracker.Client.Razor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FinancialTracker.Client.Razor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinancialTracker.Client.Razor.dll"]
