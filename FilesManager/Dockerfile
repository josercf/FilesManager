FROM microsoft/aspnetcore:2.0 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0 AS build
WORKDIR /src
COPY FilesManager/FilesManager.csproj FilesManager/
COPY FilesManager.Storage/FilesManager.Storage.csproj FilesManager.Storage/
RUN dotnet restore FilesManager/FilesManager.csproj
COPY . .
WORKDIR /src/FilesManager
RUN dotnet build FilesManager.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish FilesManager.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FilesManager.dll"]
