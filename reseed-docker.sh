#!/bin/bash

# Configuration
CONTAINER_NAME="mssql-server"
DB_USER="sa"
DB_PASSWORD="Password123!"
SCRIPT_DIR=$(dirname "$0")
INIT_SCRIPT="Init.sql"
SEED_SCRIPT="SeedData.sql"

# Copy scripts to container
docker cp "$SCRIPT_DIR/$INIT_SCRIPT" "$CONTAINER_NAME:/var/opt/mssql/scripts/$INIT_SCRIPT"
docker cp "$SCRIPT_DIR/$SEED_SCRIPT" "$CONTAINER_NAME:/var/opt/mssql/scripts/$SEED_SCRIPT"

# Execute scripts
docker exec -i $CONTAINER_NAME /opt/mssql-tools/bin/sqlcmd -U $DB_USER -P $DB_PASSWORD -i "/var/opt/mssql/scripts/$INIT_SCRIPT"
docker exec -i $CONTAINER_NAME /opt/mssql-tools/bin/sqlcmd -U $DB_USER -P $DB_PASSWORD -i "/var/opt/mssql/scripts/$SEED_SCRIPT"

echo "Database reseeding complete."