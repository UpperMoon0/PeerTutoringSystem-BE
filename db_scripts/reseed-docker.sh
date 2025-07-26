#!/bin/bash

# Configuration
CONTAINER_NAME="mssql-server"
DB_USER="sa"
DB_PASSWORD="Password123!"
SCRIPT_DIR=$(dirname "$0")
INIT_SCRIPT="Init.sql"
SEED_SCRIPT="SeedData.sql"

# Create scripts directory in container
docker exec -i $CONTAINER_NAME mkdir -p /var/opt/mssql/scripts

# Copy scripts to container
docker cp "$SCRIPT_DIR/$INIT_SCRIPT" "$CONTAINER_NAME:/var/opt/mssql/scripts/$INIT_SCRIPT"
docker cp "$SCRIPT_DIR/$SEED_SCRIPT" "$CONTAINER_NAME:/var/opt/mssql/scripts/$SEED_SCRIPT"

# Clear caches
echo "Clearing SQL Server caches..."
docker exec -i $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd -U $DB_USER -P $DB_PASSWORD -Q "DBCC DROPCLEANBUFFERS;" -C -N
docker exec -i $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd -U $DB_USER -P $DB_PASSWORD -Q "DBCC FREEPROCCACHE;" -C -N

# Execute scripts
docker exec -i $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd -U $DB_USER -P $DB_PASSWORD -i "/var/opt/mssql/scripts/$INIT_SCRIPT" -C -N
docker exec -i $CONTAINER_NAME /opt/mssql-tools18/bin/sqlcmd -U $DB_USER -P $DB_PASSWORD -i "/var/opt/mssql/scripts/$SEED_SCRIPT" -C -N

echo "Database reseeding complete."