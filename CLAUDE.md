# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Architecture Overview

MoneyMate is a personal finance API with a microservices architecture:

- **MoneyMateApi/** - Main .NET 8 API (C#) that handles transaction management, categories, payers/payees, and analytics
- **MoneyMateDb/** - Database setup with Flyway migrations for CockroachDB
- **utilities/** - Supporting services:
  - `categoryInitialiser/` - Go Lambda function for initializing user categories
  - `database_migrator/` - TypeScript utility for data migration between DynamoDB and CockroachDB
  - `categoryModifier/` - Go utility for transaction processing
- **infra/** - Terraform infrastructure as code for AWS deployment
- **Auth0/** - Authentication configuration and Terraform setup

The API uses:
- ASP.NET Core with Lambda hosting (both local and AWS Lambda)
- CockroachDB as the primary database
- Auth0 for authentication via JWT tokens
- AutoMapper for object mapping
- Dapper for database access
- FluentValidation for request validation
- Google Places API for location enrichment

## Development Commands

### Main API (MoneyMateApi/)
```bash
# Start local development environment with mocked services
cd MoneyMateApi && make start_debug_environment

# Stop development environment
cd MoneyMateApi && make kill

# Run tests
cd MoneyMateApi && dotnet test

# Build the solution
cd MoneyMateApi && dotnet build
```

### Database (MoneyMateDb/)
```bash
# Start database only
cd MoneyMateDb && make start_db_alone

# Start database with migrations
cd MoneyMateDb && make start_db

# Run standalone database migration
cd MoneyMateDb && make run_standalone_db_migration

# Stop database
cd MoneyMateDb && make stop_db
```

### Database Migrator Utility (utilities/database_migrator/)
```bash
cd utilities/database_migrator
npm run build      # Compile TypeScript
npm run runApp     # Run migration utility
npm run test       # Run tests
```

### Category Initialiser Utility (utilities/categoryInitialiser/)
```bash
cd utilities/categoryInitialiser
go build           # Build the Go binary
go test ./...      # Run tests
```

## Project Structure

### API Layer (`MoneyMateApi/src/`)
- `Controllers/` - REST API endpoints organized by domain (Analytics, Categories, PayersPayees, etc.)
- `Domain/Models/` - Core business entities (Transaction, Category, PayerPayee, etc.)
- `Domain/Services/` - Business logic services with interfaces
- `Repositories/` - Data access layer with CockroachDB implementation
- `Middleware/` - Authentication, exception handling, and user context
- `Connectors/` - External service integrations (Google Places API)

### Key Architectural Patterns
- Repository pattern for data access with interfaces (`ITransactionRepository`, etc.)
- Service layer for business logic (`ITransactionHelperService`, etc.)
- Specification pattern for complex queries (`TransactionSpecification`)
- Command/operation pattern for category updates (`UpdateCategoryOperations/`)
- AutoMapper profiles for DTO/entity mapping
- Middleware pipeline for cross-cutting concerns

### Testing Structure
- `MoneyMateApi.Tests/` - Unit tests
- `MoneyMateApi.IntegrationTests/` - Integration tests with test database
- `MoneyMateApi.Tests.Common/` - Shared test utilities

### Configuration
The API uses environment-specific appsettings files:
- `appsettings.json` - Base configuration
- `appsettings.dev.json` - Development overrides
- `appsettings.test.json` - Test environment
- `appsettings.prod.json` - Production settings

## Domain Models

Core entities include:
- **Transaction** - Financial transactions with categories, payers/payees, and tags
- **Category/Subcategory** - Hierarchical categorization system
- **PayerPayee** - Transaction counterparties with Google Places enrichment
- **Profile** - User profile management
- **Tag** - Additional transaction labeling

## Authentication & Authorization

- JWT-based authentication via Auth0
- User context middleware extracts user info from tokens
- Profile-based authorization ensures users only access their data
- Special handling for health and initialization endpoints (no auth required)