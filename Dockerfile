FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Discord.Oidc/Discord.Oidc.csproj", "Discord.Oidc/"]
RUN dotnet restore "Discord.Oidc/Discord.Oidc.csproj"
COPY . .
WORKDIR "/src/Discord.Oidc"
RUN dotnet build "Discord.Oidc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Discord.Oidc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Discord.Oidc.dll"]
