FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/Gamestore.WebApi/Gamestore.WebApi.csproj", "src/Gamestore.WebApi/"]
COPY ["src/Gamestore.BLL/Gamestore.BLL.csproj", "src/Gamestore.BLL/"]
COPY ["src/Gamestore.DAL/Gamestore.DAL.csproj", "src/Gamestore.DAL/"]
COPY ["src/Gamestore.Domain/Gamestore.Domain.csproj", "src/Gamestore.Domain/"]
RUN dotnet restore "./src/Gamestore.WebApi/Gamestore.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Gamestore.WebApi"
RUN dotnet build "./Gamestore.WebApi.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gamestore.WebApi.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gamestore.WebApi.dll"]