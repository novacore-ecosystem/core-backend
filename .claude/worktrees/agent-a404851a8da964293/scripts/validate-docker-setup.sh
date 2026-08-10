#!/bin/bash

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Counters
PASS=0
FAIL=0

echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}NovaCore Docker Setup Validator${NC}"
echo -e "${YELLOW}========================================${NC}"
echo ""

# Check Docker installation
echo -e "${YELLOW}[1] Checking Docker installation...${NC}"
if command -v docker &> /dev/null; then
    DOCKER_VERSION=$(docker --version)
    echo -e "${GREEN}✓${NC} Docker is installed: $DOCKER_VERSION"
    PASS=$((PASS + 1))
else
    echo -e "${RED}✗${NC} Docker is not installed"
    FAIL=$((FAIL + 1))
fi

# Check Docker Compose installation
echo ""
echo -e "${YELLOW}[2] Checking Docker Compose installation...${NC}"
if command -v docker-compose &> /dev/null; then
    COMPOSE_VERSION=$(docker-compose --version)
    echo -e "${GREEN}✓${NC} Docker Compose is installed: $COMPOSE_VERSION"
    PASS=$((PASS + 1))
else
    echo -e "${RED}✗${NC} Docker Compose is not installed"
    FAIL=$((FAIL + 1))
fi

# Check Docker daemon
echo ""
echo -e "${YELLOW}[3] Checking Docker daemon...${NC}"
if docker ps &> /dev/null; then
    echo -e "${GREEN}✓${NC} Docker daemon is running"
    PASS=$((PASS + 1))
else
    echo -e "${RED}✗${NC} Docker daemon is not running"
    FAIL=$((FAIL + 1))
fi

# Check docker-compose.yml
echo ""
echo -e "${YELLOW}[4] Checking docker-compose.yml...${NC}"
if [ -f "docker-compose.yml" ]; then
    echo -e "${GREEN}✓${NC} docker-compose.yml found"
    PASS=$((PASS + 1))
else
    echo -e "${RED}✗${NC} docker-compose.yml not found"
    FAIL=$((FAIL + 1))
fi

# Check Dockerfiles
echo ""
echo -e "${YELLOW}[5] Checking Dockerfiles...${NC}"
DOCKERFILE_COUNT=0
REQUIRED_DOCKERFILES=(
    "src/ApiGateways/YarpApiGateway/Dockerfile"
    "src/Services/Auth/Auth.API/Dockerfile"
    "src/Services/Inventory/Dockerfile"
    "src/Services/Order/Dockerfile"
    "src/Services/Product/Dockerfile"
    "src/Services/User/Dockerfile"
)

for dockerfile in "${REQUIRED_DOCKERFILES[@]}"; do
    if [ -f "$dockerfile" ]; then
        DOCKERFILE_COUNT=$((DOCKERFILE_COUNT + 1))
    else
        echo -e "${RED}✗${NC} Missing: $dockerfile"
        FAIL=$((FAIL + 1))
    fi
done

if [ $DOCKERFILE_COUNT -eq ${#REQUIRED_DOCKERFILES[@]} ]; then
    echo -e "${GREEN}✓${NC} All Dockerfiles found ($DOCKERFILE_COUNT/${#REQUIRED_DOCKERFILES[@]})"
    PASS=$((PASS + 1))
fi

# Check initialization scripts
echo ""
echo -e "${YELLOW}[6] Checking initialization scripts...${NC}"
INIT_SCRIPTS=(
    "scripts/postgres/init.sql"
    "scripts/mongodb/init-mongo.js"
)

INIT_FOUND=0
for script in "${INIT_SCRIPTS[@]}"; do
    if [ -f "$script" ]; then
        INIT_FOUND=$((INIT_FOUND + 1))
    else
        echo -e "${RED}✗${NC} Missing: $script"
        FAIL=$((FAIL + 1))
    fi
done

if [ $INIT_FOUND -eq ${#INIT_SCRIPTS[@]} ]; then
    echo -e "${GREEN}✓${NC} All initialization scripts found ($INIT_FOUND/${#INIT_SCRIPTS[@]})"
    PASS=$((PASS + 1))
fi

# Check .NET SDK
echo ""
echo -e "${YELLOW}[7] Checking .NET SDK...${NC}"
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "${GREEN}✓${NC} .NET SDK is installed: $DOTNET_VERSION"
    PASS=$((PASS + 1))
else
    echo -e "${YELLOW}⚠${NC} .NET SDK is not installed (optional for Docker builds)"
fi

# Check docker-compose.yml syntax
echo ""
echo -e "${YELLOW}[8] Validating docker-compose.yml syntax...${NC}"
if docker-compose config > /dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} docker-compose.yml syntax is valid"
    PASS=$((PASS + 1))
else
    echo -e "${RED}✗${NC} docker-compose.yml has syntax errors"
    docker-compose config
    FAIL=$((FAIL + 1))
fi

# Check available disk space
echo ""
echo -e "${YELLOW}[9] Checking available disk space...${NC}"
AVAILABLE_SPACE=$(df -BG . | tail -1 | awk '{print $4}' | sed 's/G//')
if [ "$AVAILABLE_SPACE" -ge 50 ]; then
    echo -e "${GREEN}✓${NC} Sufficient disk space available: ${AVAILABLE_SPACE}GB"
    PASS=$((PASS + 1))
else
    echo -e "${YELLOW}⚠${NC} Low disk space: ${AVAILABLE_SPACE}GB (50GB recommended)"
fi

# Check system memory
echo ""
echo -e "${YELLOW}[10] Checking available memory...${NC}"
if command -v free &> /dev/null; then
    AVAILABLE_MEM=$(free -g | grep Mem | awk '{print $7}')
    if [ "$AVAILABLE_MEM" -ge 4 ]; then
        echo -e "${GREEN}✓${NC} Sufficient memory available: ${AVAILABLE_MEM}GB"
        PASS=$((PASS + 1))
    else
        echo -e "${YELLOW}⚠${NC} Low memory: ${AVAILABLE_MEM}GB (8GB recommended)"
    fi
else
    echo -e "${YELLOW}⚠${NC} Cannot determine available memory"
fi

# Check Docker image availability
echo ""
echo -e "${YELLOW}[11] Checking Docker image availability...${NC}"
REQUIRED_IMAGES=(
    "postgres:16-alpine"
    "mongo:7"
    "redis:7-alpine"
    "confluentinc/cp-kafka:7.5.0"
    "datalust/seq:latest"
    "docker.elastic.co/elasticsearch/elasticsearch:8.10.0"
    "docker.elastic.co/kibana/kibana:8.10.0"
)

IMAGES_AVAILABLE=0
for image in "${REQUIRED_IMAGES[@]}"; do
    if docker image inspect "$image" &> /dev/null; then
        IMAGES_AVAILABLE=$((IMAGES_AVAILABLE + 1))
    fi
done

if [ $IMAGES_AVAILABLE -gt 0 ]; then
    echo -e "${GREEN}✓${NC} Found $IMAGES_AVAILABLE/${#REQUIRED_IMAGES[@]} base images locally"
    echo -e "${YELLOW}ℹ${NC} Missing images will be pulled during first build"
    PASS=$((PASS + 1))
else
    echo -e "${YELLOW}ℹ${NC} No base images found locally (will be pulled during build)"
fi

# Summary
echo ""
echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}Validation Summary${NC}"
echo -e "${YELLOW}========================================${NC}"
echo -e "${GREEN}Passed: $PASS${NC}"
echo -e "${RED}Failed: $FAIL${NC}"

if [ $FAIL -eq 0 ]; then
    echo ""
    echo -e "${GREEN}✓ Setup is ready! Run: docker-compose up -d${NC}"
    exit 0
else
    echo ""
    echo -e "${RED}✗ Setup has issues. Please fix them before running docker-compose.${NC}"
    exit 1
fi
