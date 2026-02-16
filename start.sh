#!/bin/bash

# Banking Microservices Quick Start Script
# This script sets up and runs the entire banking microservices system

set -e  # Exit on error

echo "🏦 Banking Microservices - Quick Start"
echo "======================================"
echo ""

# Check Docker
echo "📦 Checking Docker..."
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker Desktop first."
    exit 1
fi

if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker Desktop."
    exit 1
fi

echo "✅ Docker is ready"
echo ""

# Check .NET SDK
echo "📦 Checking .NET SDK..."
if ! command -v dotnet &> /dev/null; then
    echo "⚠️  .NET SDK not found. Services will run in Docker only."
else
    dotnet_version=$(dotnet --version)
    echo "✅ .NET SDK version: $dotnet_version"
fi
echo ""

# Start infrastructure services
echo "🚀 Starting infrastructure services..."
echo "   - PostgreSQL (Account, Transaction, Customer, Auth DBs)"
echo "   - Redis Cache"
echo "   - RabbitMQ Message Broker"
echo "   - Elasticsearch & Kibana"
echo ""

docker-compose up -d postgres-account postgres-transaction postgres-customer postgres-auth redis rabbitmq elasticsearch kibana

echo "⏳ Waiting for services to be healthy..."
sleep 10

# Check service health
echo ""
echo "🔍 Checking service health..."

check_service() {
    service_name=$1
    port=$2
    max_attempts=30
    attempt=0

    while [ $attempt -lt $max_attempts ]; do
        if docker-compose ps | grep $service_name | grep -q "healthy\|Up"; then
            echo "✅ $service_name is ready"
            return 0
        fi
        attempt=$((attempt + 1))
        sleep 2
    done

    echo "⚠️  $service_name might not be fully ready yet"
    return 1
}

check_service "postgres-account" "5432"
check_service "postgres-transaction" "5433"
check_service "postgres-customer" "5434"
check_service "postgres-auth" "5435"
check_service "redis" "6379"
check_service "rabbitmq" "5672"
check_service "elasticsearch" "9200"

echo ""
echo "✅ Infrastructure services are running!"
echo ""

# Print access information
echo "📋 Service Access Information:"
echo "================================"
echo ""
echo "🐰 RabbitMQ Management UI:"
echo "   URL: http://localhost:15672"
echo "   Username: admin"
echo "   Password: Admin123!"
echo ""
echo "📊 Kibana (Elasticsearch UI):"
echo "   URL: http://localhost:5601"
echo ""
echo "🗄️  PostgreSQL Databases:"
echo "   Account DB:     localhost:5432"
echo "   Transaction DB: localhost:5433"
echo "   Customer DB:    localhost:5434"
echo "   Auth DB:        localhost:5435"
echo "   Username: admin"
echo "   Password: Admin123!"
echo ""
echo "⚡ Redis:"
echo "   Host: localhost:6379"
echo "   Password: Redis123!"
echo ""
echo "🔍 Elasticsearch:"
echo "   URL: http://localhost:9200"
echo ""

# Optional: Build and start microservices
echo "🚀 Do you want to build and start the microservices now? (y/n)"
read -r response

if [[ "$response" =~ ^[Yy]$ ]]; then    
    echo ""
    echo "⏳ Waiting for services to start..."
    sleep 15
    
    echo ""
    echo "✅ All services are running!"
    echo ""
    echo "🌐 API Gateway:"
    echo "   URL: http://localhost:5000"
    echo "   Swagger: http://localhost:5000/swagger"
    echo ""
    echo "📚 Check USAGE_GUIDE.md for API examples"
fi

echo ""
echo "🎉 Setup complete!"
echo ""
echo "Useful commands:"
echo "  - View logs: docker-compose logs -f [service-name]"
echo "  - Stop all: docker-compose down"
echo "  - Stop all and remove data: docker-compose down -v"
echo ""
