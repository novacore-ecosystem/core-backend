# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY ["NovaCore.sln", "./"]

# Copy BuildingBlocks
COPY ["src/BuildingBlocks/BuildingBlock.Domain/BuildingBlock.Domain.csproj", "src/BuildingBlocks/BuildingBlock.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlock.Application/BuildingBlock.Application.csproj", "src/BuildingBlocks/BuildingBlock.Application/"]
COPY ["src/BuildingBlocks/BuildingBlock.Infrastructure/BuildingBlock.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlock.Infrastructure/"]
COPY ["src/BuildingBlocks/BuildingBlock.Contract/BuildingBlock.Contract.csproj", "src/BuildingBlocks/BuildingBlock.Contract/"]
COPY ["src/BuildingBlocks/BuildingBlock.SharedKernel/BuildingBlock.SharedKernel.csproj", "src/BuildingBlocks/BuildingBlock.SharedKernel/"]
COPY ["src/BuildingBlocks/BuildingBlock.Web/BuildingBlock.Web.csproj", "src/BuildingBlocks/BuildingBlock.Web/"]

# Copy ApiGateway
COPY ["src/ApiGateways/YarpApiGateway/YarpApiGateway.csproj", "src/ApiGateways/YarpApiGateway/"]

RUN dotnet restore "src/ApiGateways/YarpApiGateway/YarpApiGateway.csproj" \
    --disable-parallel \
    --force

COPY . .

WORKDIR "/app/src/ApiGateways/YarpApiGateway"
RUN dotnet build "YarpApiGateway.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "YarpApiGateway.csproj" -c Debug -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl unzip \
    && curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 5000 5001

ENTRYPOINT ["dotnet", "YarpApiGateway.dll"]
