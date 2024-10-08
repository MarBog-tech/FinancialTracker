FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinancialTracker.Client.csproj", "FinancialTracker.Client/"]
RUN dotnet restore "FinancialTracker.Client.csproj"
COPY . .
WORKDIR "/src/FinancialTracker.Client"
RUN dotnet build "FinancialTracker.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinancialTracker.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinancialTracker.Client.dll"]
