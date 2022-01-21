FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AzisFood.DataEngine.ManualTest/AzisFood.DataEngine.ManualTest.csproj", "AzisFood.DataEngine.ManualTest/"]
RUN dotnet restore "AzisFood.DataEngine.ManualTest/AzisFood.DataEngine.ManualTest.csproj"
COPY . .
WORKDIR "/src/AzisFood.DataEngine.ManualTest"
RUN dotnet build "AzisFood.DataEngine.ManualTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AzisFood.DataEngine.ManualTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AzisFood.DataEngine.ManualTest.dll"]
