# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["NovaCore.sln", "Directory.Packages.props", "./"]
COPY ["src/BuildingBlocks/BuildingBlock.Domain/BuildingBlock.Domain.csproj", "src/BuildingBlocks/BuildingBlock.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlock.Infrastructure/BuildingBlock.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlock.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlock.Application/BuildingBlock.Application.csproj", "src/BuildingBlocks/BuildingBlock.Application/"]
COPY ["src/BuildingBlocks/BuildingBlock.Contract/BuildingBlock.Contract.csproj", "src/BuildingBlocks/BuildingBlock.Contract/"]
COPY ["src/BuildingBlocks/BuildingBlock.Criteria/BuildingBlock.Criteria.csproj", "src/BuildingBlocks/BuildingBlock.Criteria/"]
COPY ["src/BuildingBlocks/BuildingBlock.SharedKernel/BuildingBlock.SharedKernel.csproj", "src/BuildingBlocks/BuildingBlock.SharedKernel/"]
COPY ["src/BuildingBlocks/BuildingBlock.Messaging/BuildingBlock.Messaging.csproj", "src/BuildingBlocks/BuildingBlock.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlock.Messaging.Kafka/BuildingBlock.Messaging.Kafka.csproj", "src/BuildingBlocks/BuildingBlock.Messaging.Kafka/"]
COPY ["src/BuildingBlocks/BuildingBlock.Persistence/BuildingBlock.Persistence.csproj", "src/BuildingBlocks/BuildingBlock.Persistence/"]
COPY ["src/BuildingBlocks/BuildingBlock.Persistence.Ef/BuildingBlock.Persistence.Ef.csproj", "src/BuildingBlocks/BuildingBlock.Persistence.Ef/"]
COPY ["src/BuildingBlocks/BuildingBlock.Observability/BuildingBlock.Observability.csproj", "src/BuildingBlocks/BuildingBlock.Observability/"]
COPY ["src/BuildingBlocks/BuildingBlock.Web/BuildingBlock.Web.csproj", "src/BuildingBlocks/BuildingBlock.Web/"]
COPY ["src/Services/Shipping/Shipping.API/Shipping.API.csproj", "src/Services/Shipping/Shipping.API/"]
COPY ["src/Services/Shipping/Shipping.Application/Shipping.Application.csproj", "src/Services/Shipping/Shipping.Application/"]
COPY ["src/Services/Shipping/Shipping.Domain/Shipping.Domain.csproj", "src/Services/Shipping/Shipping.Domain/"]
COPY ["src/Services/Shipping/Shipping.Infrastructure/Shipping.Infrastructure.csproj", "src/Services/Shipping/Shipping.Infrastructure/"]
COPY ["src/Services/Shipping/Shipping.Persistence/Shipping.Persistence.csproj", "src/Services/Shipping/Shipping.Persistence/"]

RUN dotnet restore "src/Services/Shipping/Shipping.API/Shipping.API.csproj" \
    --disable-parallel \
    --force

COPY . .

WORKDIR "/app/src/Services/Shipping/Shipping.API"
RUN dotnet build "Shipping.API.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Shipping.API.csproj" -c Debug -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl unzip \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg \
    && rm -rf /var/lib/apt/lists/*

RUN useradd -m -u 1001 appuser && chown -R appuser:appuser /app
USER appuser

COPY --from=publish /app/publish .

EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "NovaCore.Shipping.API.dll"]
