#!/bin/bash
set -e

echo "Seeding Northwind database..."

for file in /docker-entrypoint-initdb.d/data/*.csv; do
    collection=$(basename "$file" .csv)
    echo "Importing $collection..."
    
    mongoimport \
        --username admin \
        --password StrongPassword1234! \
        --authenticationDatabase admin \
        --db Northwind \
        --collection "$collection" \
        --type csv \
        --file "$file" \
        --headerline \
        --drop
    
    echo "Importing $collection done"
done

echo "Done!"
