#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SocialNetwork.Web/SocialNetwork.Web.csproj", "SocialNetwork.Web/"]
COPY ["SocialNetwork.Infrastructure/SocialNetwork.Infrastructure.csproj", "SocialNetwork.Infrastructure/"]
COPY ["SocialNetwork.Core/SocialNetwork.Core.csproj", "SocialNetwork.Core/"]
RUN dotnet restore "SocialNetwork.Web/SocialNetwork.Web.csproj"
COPY . .
WORKDIR "/src/SocialNetwork.Web"
RUN dotnet build "SocialNetwork.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SocialNetwork.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocialNetwork.Web.dll"]