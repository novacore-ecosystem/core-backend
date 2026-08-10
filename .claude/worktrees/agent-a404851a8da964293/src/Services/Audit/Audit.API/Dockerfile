# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["NovaCore.sln", "./"]
COPY ["src/BuildingBlocks/BuildingBlock.Domain/BuildingBlock.Domain.csproj", "src/BuildingBlocks/BuildingBlock.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlock.Infrastructure/BuildingBlock.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlock.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlock.Application/BuildingBlock.Application.csproj", "src/BuildingBlocks/BuildingBlock.Application/"]
COPY ["src/BuildingBlocks/BuildingBlock.Contract/BuildingBlock.Contract.csproj", "src/BuildingBlocks/BuildingBlock.Contract/"]
COPY ["src/BuildingBlocks/BuildingBlock.SharedKernel/BuildingBlock.SharedKernel.csproj", "src/BuildingBlocks/BuildingBlock.SharedKernel/"]
COPY ["src/BuildingBlocks/BuildingBlock.Messaging/BuildingBlock.Messaging.csproj", "src/BuildingBlocks/BuildingBlock.Messaging/"]
COPY ["src/BuildingBlocks/BuildingBlock.Messaging.Kafka/BuildingBlock.Messaging.Kafka.csproj", "src/BuildingBlocks/BuildingBlock.Messaging.Kafka/"]
COPY ["src/BuildingBlocks/BuildingBlock.Persistence/BuildingBlock.Persistence.csproj", "src/BuildingBlocks/BuildingBlock.Persistence/"]
COPY ["src/BuildingBlocks/BuildingBlock.Persistence.Mongo/BuildingBlock.Persistence.Mongo.csproj", "src/BuildingBlocks/BuildingBlock.Persistence.Mongo/"]
COPY ["src/BuildingBlocks/BuildingBlock.Web/BuildingBlock.Web.csproj", "src/BuildingBlocks/BuildingBlock.Web/"]
COPY ["src/Services/Audit/Audit.API/Audit.API.csproj", "src/Services/Audit/Audit.API/"]
COPY ["src/Services/Audit/Audit.Application/Audit.Application.csproj", "src/Services/Audit/Audit.Application/"]
COPY ["src/Services/Audit/Audit.Domain/Audit.Domain.csproj", "src/Services/Audit/Audit.Domain/"]
COPY ["src/Services/Audit/Audit.Infrastructure/Audit.Infrastructure.csproj", "src/Services/Audit/Audit.Infrastructure/"]
COPY ["src/Services/Audit/Audit.Persistence/Audit.Persistence.csproj", "src/Services/Audit/Audit.Persistence/"]

RUN dotnet restore "src/Services/Audit/Audit.API/Audit.API.csproj" \
    --disable-parallel \
    --force

COPY . .

WORKDIR "/app/src/Services/Audit/Audit.API"
RUN dotnet build "Audit.API.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Audit.API.csproj" -c Debug -o /app/publish

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

ENTRYPOINT ["dotnet", "Audit.API.dll"]
