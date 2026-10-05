FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY PasswordGenerator.Api/PasswordGenerator.Api.csproj PasswordGenerator.Api/
RUN dotnet restore PasswordGenerator.Api/PasswordGenerator.Api.csproj

COPY PasswordGenerator.Api/ PasswordGenerator.Api/
RUN dotnet publish PasswordGenerator.Api/PasswordGenerator.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/data && chown app:app /app/data

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__Default="Data Source=/app/data/passwords.db"

USER app
EXPOSE 8080
VOLUME /app/data

ENTRYPOINT ["dotnet", "PasswordGenerator.Api.dll"]
