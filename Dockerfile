FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["backend-api-dotnet9.csproj", "./"]
RUN dotnet restore "backend-api-dotnet9.csproj"

COPY . .
RUN dotnet publish "backend-api-dotnet9.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "backend-api-dotnet9.dll"]
