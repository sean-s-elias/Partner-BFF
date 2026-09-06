FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PartnerBFF.Api.sln ./
COPY PartnerBFF.Api/PartnerBFF.Api.csproj PartnerBFF.Api/
COPY PartnerBFF.Application/PartnerBFF.Application.csproj PartnerBFF.Application/
COPY PartnerBFF.Domain/PartnerBFF.Domain.csproj PartnerBFF.Domain/
COPY PartnerBFF.Persistence/PartnerBFF.Persistence.csproj PartnerBFF.Persistence/
COPY PartnerBFF.Tests/PartnerBFF.Tests.csproj PartnerBFF.Tests/

RUN dotnet restore PartnerBFF.Api.sln

COPY . .

FROM build AS test
WORKDIR /src/PartnerBFF.Tests
RUN dotnet test --no-restore -c Release

FROM build AS publish
WORKDIR /src/PartnerBFF.Api
RUN dotnet publish PartnerBFF.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "PartnerBFF.Api.dll"]