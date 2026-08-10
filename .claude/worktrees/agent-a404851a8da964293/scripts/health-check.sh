#!/bin/bash

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}NovaCore Health Check${NC}"
echo "=================================="
echo ""

# Check services
check_service() {
    local service=$1
    local port=$2
    local url=$3

    local response=$(curl -s -o /dev/null -w "%{http_code}" "$url" 2>/dev/null)

    if [ "$response" = "200" ]; then
        echo -e "${GREEN}✓${NC} $service (port $port) - HTTP $response"
        return 0
    else
        echo -e "${RED}✗${NC} $service (port $port) - HTTP $response"
        return 1
    fi
}

# Check database connectivity
check_database() {
    local db=$1
    local type=$2

    if [ "$type" = "postgres" ]; then
        local result=$(docker-compose exec -T postgres pg_isready -U postgres 2>/dev/null)
        if echo "$result" | grep -q "accepting connections"; then
            echo -e "${GREEN}✓${NC} PostgreSQL - Ready"
            return 0
        else
            echo -e "${RED}✗${NC} PostgreSQL - Not ready"
            return 1
        fi
    elif [ "$type" = "mongodb" ]; then
        local result=$(docker-compose exec -T mongo mongosh --eval 'db.adminCommand("ping")' 2>/dev/null)
        if echo "$result" | grep -q "ok"; then
            echo -e "${GREEN}✓${NC} MongoDB - Ready"
            return 0
        else
            echo -e "${RED}✗${NC} MongoDB - Not ready"
            return 1
        fi
    fi
}

# Check container status
check_container() {
    local container=$1

    local status=$(docker-compose ps $container 2>/dev/null | grep "$container" | awk '{print $NF}')

    if [ "$status" = "Up" ] || [[ "$status" == *"Up"* ]]; then
        echo -e "${GREEN}✓${NC} $container - Running"
        return 0
    else
        echo -e "${RED}✗${NC} $container - Not running ($status)"
        return 1
    fi
}

echo -e "${YELLOW}API Services:${NC}"
check_service "YARP Gateway" "5000" "http://localhost:5000/health"
check_service "Auth Service" "5100" "http://localhost:5100/health"
check_service "Inventory Service" "5101" "http://localhost:5101/health"
check_service "Order Service" "5102" "http://localhost:5102/health"
check_service "Product Service" "5103" "http://localhost:5103/health"
check_service "User Service" "5104" "http://localhost:5104/health"

echo ""
echo -e "${YELLOW}Databases:${NC}"
check_database "PostgreSQL" "postgres"
check_database "MongoDB" "mongodb"

echo ""
echo -e "${YELLOW}Cache & Queue:${NC}"
if docker-compose exec -T redis redis-cli ping 2>/dev/null | grep -q "PONG"; then
    echo -e "${GREEN}✓${NC} Redis - Healthy"
else
    echo -e "${RED}✗${NC} Redis - Not healthy"
fi

if docker-compose exec -T kafka kafka-topics --bootstrap-server localhost:9092 --list 2>/dev/null | grep -q .; then
    echo -e "${GREEN}✓${NC} Kafka - Healthy"
else
    echo -e "${RED}✗${NC} Kafka - Not healthy"
fi

echo ""
echo -e "${YELLOW}Infrastructure:${NC}"
if curl -s http://localhost:5341/api/events > /dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} Seq (Logging) - Healthy"
else
    echo -e "${RED}✗${NC} Seq (Logging) - Not healthy"
fi

if curl -s http://localhost:9200/_cluster/health 2>/dev/null | grep -q "status"; then
    echo -e "${GREEN}✓${NC} Elasticsearch - Healthy"
else
    echo -e "${RED}✗${NC} Elasticsearch - Not healthy"
fi

if curl -s http://localhost:5601/api/status 2>/dev/null | grep -q "state"; then
    echo -e "${GREEN}✓${NC} Kibana - Healthy"
else
    echo -e "${RED}✗${NC} Kibana - Not healthy"
fi

echo ""
echo "=================================="
echo "Health check complete"
