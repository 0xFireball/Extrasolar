#!/usr/bin/env bash

#exit if any command fails
#set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then  
  rm -R $artifactsFolder
fi

# Dotnet info

dotnet --version

echo "Restoring packages..."
# Restore packages
dotnet restore

echo "Building project..."

# Build
dotnet build -c Release Extrasolar/src/Extrasolar

echo "Running tests..."

# Run tests
dotnet test Extrasolar/test/Extrasolar.Tests

echo "Packaging library..."

# Package library
dotnet pack -c Release Extrasolar/src/Extrasolar