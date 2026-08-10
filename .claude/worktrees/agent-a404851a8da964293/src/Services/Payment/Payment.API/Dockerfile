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
COPY ["src/BuildingBlocks/BuildingBlock.Persistence.Ef/BuildingBlock.Persistence.Ef.csproj", "src/BuildingBlocks/BuildingBlock.Persistence.Ef/"]
COPY ["src/BuildingBlocks/BuildingBlock.Observability/BuildingBlock.Observability.csproj", "src/BuildingBlocks/BuildingBlock.Observability/"]
COPY ["src/BuildingBlocks/BuildingBlock.Grpc/BuildingBlock.Grpc.csproj", "src/BuildingBlocks/BuildingBlock.Grpc/"]
COPY ["src/BuildingBlocks/BuildingBlock.Web/BuildingBlock.Web.csproj", "src/BuildingBlocks/BuildingBlock.Web/"]
COPY ["src/Services/Payment/Payment.API/Payment.API.csproj", "src/Services/Payment/Payment.API/"]
COPY ["src/Services/Payment/Payment.Application/Payment.Application.csproj", "src/Services/Payment/Payment.Application/"]
COPY ["src/Services/Payment/Payment.Domain/Payment.Domain.csproj", "src/Services/Payment/Payment.Domain/"]
COPY ["src/Services/Payment/Payment.Infrastructure/Payment.Infrastructure.csproj", "src/Services/Payment/Payment.Infrastructure/"]
COPY ["src/Services/Payment/Payment.Persistence/Payment.Persistence.csproj", "src/Services/Payment/Payment.Persistence/"]

RUN dotnet restore "src/Services/Payment/Payment.API/Payment.API.csproj" \
    --disable-parallel \
    --force

COPY . .

WORKDIR "/app/src/Services/Payment/Payment.API"
RUN dotnet build "Payment.API.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "Payment.API.csproj" -c Debug -o /app/publish

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

ENTRYPOINT ["dotnet", "Payment.API.dll"]
