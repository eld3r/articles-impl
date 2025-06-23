FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Articles.Api/Articles.Api.csproj", "Articles.Api/"]
COPY ["Articles.Services/Articles.Services.csproj", "Articles.Services/"]
COPY ["Articles.Domain/Articles.Domain.csproj", "Articles.Domain/"]
COPY ["Articles.Services.Impl/Articles.Services.Impl.csproj", "Articles.Services.Impl/"]
COPY ["Articles.Dal/Articles.Dal.csproj", "Articles.Dal/"]
COPY ["Articles.Dal.PostgresEfCore/Articles.Dal.PostgresEfCore.csproj", "Articles.Dal.PostgresEfCore/"]
RUN dotnet restore "Articles.Api/Articles.Api.csproj"
COPY . .
WORKDIR "/src/Articles.Api"
RUN dotnet build "./Articles.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Articles.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Articles.Api.dll"]
