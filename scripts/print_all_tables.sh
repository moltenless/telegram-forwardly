#!/bin/bash

SERVER="db"                         
USER="sa"                          
PASSWORD=${MSSQL_SA_PASSWORD}
DATABASE="TelegramForwardly"

TABLES=(
  "chat_types"
  "client_current_states"
  "topic_grouping_types"
  "clients"
  "chats"
  "keywords"
  "__EFMigrationsHistory"
)

for TABLE in "${TABLES[@]}"; do
  echo "=============================="
  echo "Data from table: $TABLE"
  echo "=============================="
  
  /opt/mssql-tools18/bin/sqlcmd -S "$SERVER" -U "$USER" -P "$PASSWORD" -d "$DATABASE" -C -Q "SELECT * FROM [$TABLE];"
  
  echo -e "\n"
done
