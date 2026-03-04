#!/bin/bash

# Set the name of the project
PROJECT_NAME="TelehealthBooking"

echo "🚀 Bootstrapping $PROJECT_NAME Enterprise Architecture in .NET 9..."

# 1. Create the Master Solution
dotnet new sln -n $PROJECT_NAME

# 2. Create the Architectural Layers (Forcing .NET 9)
echo "📦 Creating Projects..."
dotnet new classlib -n $PROJECT_NAME.Domain -f net9.0
dotnet new classlib -n $PROJECT_NAME.Application -f net9.0
dotnet new classlib -n $PROJECT_NAME.Infrastructure -f net9.0
dotnet new webapi -n $PROJECT_NAME.Api -f net9.0
dotnet new xunit -n $PROJECT_NAME.Tests -f net9.0

# 3. Add Projects to the Solution
echo "🔗 Adding Projects to Solution..."
dotnet sln add $PROJECT_NAME.Domain/$PROJECT_NAME.Domain.csproj
dotnet sln add $PROJECT_NAME.Application/$PROJECT_NAME.Application.csproj
dotnet sln add $PROJECT_NAME.Infrastructure/$PROJECT_NAME.Infrastructure.csproj
dotnet sln add $PROJECT_NAME.Api/$PROJECT_NAME.Api.csproj
dotnet sln add $PROJECT_NAME.Tests/$PROJECT_NAME.Tests.csproj

# 4. Set Up Clean Architecture Dependencies (The Dependency Inversion Rule)
echo "🏗️ Configuring Project References..."
dotnet add $PROJECT_NAME.Application/$PROJECT_NAME.Application.csproj reference $PROJECT_NAME.Domain/$PROJECT_NAME.Domain.csproj
dotnet add $PROJECT_NAME.Infrastructure/$PROJECT_NAME.Infrastructure.csproj reference $PROJECT_NAME.Application/$PROJECT_NAME.Application.csproj
dotnet add $PROJECT_NAME.Api/$PROJECT_NAME.Api.csproj reference $PROJECT_NAME.Application/$PROJECT_NAME.Application.csproj $PROJECT_NAME.Infrastructure/$PROJECT_NAME.Infrastructure.csproj
dotnet add $PROJECT_NAME.Tests/$PROJECT_NAME.Tests.csproj reference $