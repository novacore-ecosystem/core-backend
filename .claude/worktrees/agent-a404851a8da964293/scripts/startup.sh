#!/bin/bash

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}"
echo "╔════════════════════════════════════════╗"
echo "║   NovaCore Docker Startup Script     ║"
echo "╚════════════════════════════════════════╝"
echo -e "${NC}"

# Change to project root
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$PROJECT_ROOT" || exit 1

# Check prerequisites
echo -e "${YELLOW}[1] Validating setup...${NC}"
if [ ! -f "docker-compose.yml" ]; then
    echo -e "${RED}✗ docker-compose.yml not found${NC}"
    exit 1
fi

if ! command -v docker &> /dev/null; then
    echo -e "${RED}✗ Docker is not installed${NC}"
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}✗ Docker Compose is not installed${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Prerequisites met${NC}"
echo ""

# Parse arguments
BUILD_FRESH=false
DETACHED="-d"
PULL_IMAGES=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --rebuild)
            BUILD_FRESH=true
            shift
            ;;
        --logs)
            DETACHED=""
            shift
            ;;
        --pull)
            PULL_IMAGES=true
            shift
            ;;
        --help)
            echo "Usage: startup.sh [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --rebuild    Rebuild Docker images from scratch"
            echo "  --logs       Keep logs in foreground (don't detach)"
            echo "  --pull       Pull latest base images before build"
            echo "  --help       Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Pull latest images if requested
if [ "$PULL_IMAGES" = true ]; then
    echo -e "${YELLOW}[2] Pulling latest base images...${NC}"
    docker-compose pull
    echo ""
fi

# Build images
echo -e "${YELLOW}[3] Building Docker images...${NC}"
if [ "$BUILD_FRESH" = true ]; then
    echo -e "${YELLOW}    (Full rebuild - no cache)${NC}"
    docker-compose build --no-cache
else
    docker-compose build
fi

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Docker build failed${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Build completed${NC}"
echo ""

# Start services
echo -e "${YELLOW}[4] Starting services...${NC}"
if [ -z "$DETACHED" ]; then
    docker-compose up
else
    docker-compose up $DETACHED
    echo -e "${GREEN}✓ Services started in background${NC}"
    echo ""
fi

if [ $? -ne 0 ]; then
    echo -e "${RED}✗ Failed to start services${NC}"
    exit 1
fi

# Wait for services to be ready
if [ ! -z "$DETACHED" ]; then
    echo -e "${YELLOW}[5] Waiting for services to be ready...${NC}"
    sleep 15
    echo ""

    # Run health check
    echo -e "${YELLOW}[6] Running health checks...${NC}"
    bash "$PROJECT_ROOT/scripts/health-check.sh"
    echo ""

    # Display useful information
    echo -e "${BLUE}════════════════════════════════════════${NC}"
    echo -e "${GREEN}✓ Startup Complete!${NC}"
    echo -e "${BLUE}════════════════════════════════════════${NC}"
    echo ""
    echo -e "${YELLOW}API Endpoints:${NC}"
    echo -e "  Gateway:    ${GREEN}http://localhost:5000${NC}"
    echo -e "  Auth:       ${GREEN}http://localhost:5100${NC}"
    echo -e "  Inventory:  ${GREEN}http://localhost:5101${NC}"
    echo -e "  Order:      ${GREEN}http://localhost:5102${NC}"
    echo -e "  Product:    ${GREEN}http://localhost:5103${NC}"
    echo -e "  User:       ${GREEN}http://localhost:5104${NC}"
    echo ""
    echo -e "${YELLOW}Infrastructure:${NC}"
    echo -e "  PostgreSQL: ${GREEN}localhost:5432${NC} (user: postgres, pass: postgres123)"
    echo -e "  MongoDB:    ${GREEN}localhost:27017${NC} (user: admin, pass: password123)"
    echo -e "  Redis:      ${GREEN}localhost:6379${NC}"
    echo -e "  Kafka:      ${GREEN}localhost:9092${NC}"
    echo ""
    echo -e "${YELLOW}Monitoring & UI:${NC}"
    echo -e "  Seq (Logs):      ${GREEN}http://localhost:5341${NC}"
    echo -e "  Elasticsearch:   ${GREEN}http://localhost:9200${NC}"
    echo -e "  Kibana:          ${GREEN}http://localhost:5601${NC}"
    echo ""
    echo -e "${YELLOW}Useful Commands:${NC}"
    echo -e "  View logs:      ${GREEN}docker-compose logs -f${NC}"
    echo -e "  Stop services:  ${GREEN}docker-compose down${NC}"
    echo -e "  Health check:   ${GREEN}bash scripts/health-check.sh${NC}"
    echo -e "  PgAdmin:        ${GREEN}make dev-tools${NC} (then http://localhost:5050)"
    echo ""
    echo -e "${BLUE}════════════════════════════════════════${NC}"
fi

exit 0
