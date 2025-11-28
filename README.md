# Isotyp - Enterprise Data Architecture Platform

[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

Isotyp is an enterprise data architecture platform designed with safety, clarity, and compliance at its core. Unlike traditional ETL tools, Isotyp provides a comprehensive solution for managing data schemas, validating data quality, and evolving your data models over time—all while keeping sensitive data secure through local agent-based processing.

## Key Features

### 🔒 Local-First Data Processing
- **Data never leaves your environment**: All data processing runs locally via agents
- **Cloud sees metadata only**: Connection strings and actual data stay local
- **Secure by design**: Sensitive data is processed where it lives

### 📐 Versioned Schema Management
- **Canonical understanding**: Forms a single source of truth for data schemas
- **Semantic versioning**: Major.Minor.Patch versioning for all schema changes
- **Re-validation over time**: Continuously validates data against the canonical schema
- **ORM synchronization**: Updates database and ORM mappings together

### 🤖 AI-Assisted Evolution
- **Pattern recognition**: AI analyzes data patterns to suggest schema improvements
- **Never auto-applies**: AI suggestions require explicit human review
- **Confidence scoring**: Each suggestion includes a confidence score
- **Additive preference**: Favors additive changes over destructive ones

### ✅ Multi-Layer Approval System
All schema changes require explicit approval through multiple layers:

1. **Technical Review**: Validates schema correctness and technical feasibility
2. **Business Review**: Assesses business impact and alignment
3. **Data Governance**: Ensures compliance and security requirements

### 🔐 Schema Locking
- **No Lock**: Schema can be freely modified
- **Soft Lock**: AI suggestions allowed, but no auto-apply
- **Additive Only**: Only new fields/tables can be added
- **Hard Lock**: No modifications allowed

### 📋 Full Auditability
- **Complete audit trail**: Every action is logged
- **Correlation tracking**: Related operations are linked
- **State snapshots**: Before/after states preserved
- **Rollback-safe**: All changes can be reversed

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Cloud Layer                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐ │
│  │  API (REST) │  │  Metadata   │  │   Approval Workflows    │ │
│  │             │  │   Storage   │  │                         │ │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘ │
│                           │                                     │
│                  (Metadata Only)                                │
└───────────────────────────┼─────────────────────────────────────┘
                            │
┌───────────────────────────┼─────────────────────────────────────┐
│                    Local Agents                                  │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │  Data Processing  │  Validation  │  Pattern Analysis        ││
│  │       (Local)     │   (Local)    │      (Local)             ││
│  └─────────────────────────────────────────────────────────────┘│
│                            │                                     │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │            Local Data Sources (SQL, Files, etc.)            ││
│  └─────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
```

## Project Structure

```
Isotyp/
├── src/
│   ├── Isotyp.Core/           # Domain entities, enums, interfaces
│   ├── Isotyp.Application/    # Services, DTOs, business logic
│   ├── Isotyp.Infrastructure/ # EF Core, repositories, data access
│   ├── Isotyp.Agent/          # Local data processing agent
│   └── Isotyp.Api/            # REST API for cloud operations
├── tests/
│   ├── Isotyp.Core.Tests/
│   └── Isotyp.Application.Tests/
└── Isotyp.sln
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQLite (default) or your preferred database

### Building

```bash
# Clone the repository
git clone https://github.com/NischalPV/Isotyp.git
cd Isotyp

# Build the solution
dotnet build

# Run tests
dotnet test
```

### Running the API

```bash
cd src/Isotyp.Api
dotnet run
```

The API will be available at `http://localhost:5000` with Swagger documentation.

## API Endpoints

### Data Sources
- `GET /api/datasources` - List all data sources
- `POST /api/datasources` - Create a new data source
- `GET /api/datasources/{id}` - Get data source details
- `PUT /api/datasources/{id}` - Update a data source

### Schema Versions
- `GET /api/schemaversions/{id}` - Get schema version
- `POST /api/schemaversions` - Create new schema version
- `POST /api/schemaversions/{id}/submit` - Submit for approval
- `POST /api/schemaversions/{id}/approve?layer={layer}` - Approve at layer
- `POST /api/schemaversions/{id}/apply` - Apply approved schema
- `POST /api/schemaversions/{id}/rollback` - Rollback applied schema
- `POST /api/schemaversions/{id}/lock` - Apply schema lock

### Change Requests
- `GET /api/schemachangerequests/pending` - Get pending approvals
- `POST /api/schemachangerequests` - Create change request
- `POST /api/schemachangerequests/{id}/approve` - Approve/reject

### AI Suggestions
- `GET /api/aisuggestions/unreviewed` - Get pending AI suggestions
- `POST /api/aisuggestions/{id}/review` - Accept/reject suggestion

### Agents
- `GET /api/agents/connected` - Get connected agents
- `POST /api/agents/heartbeat` - Agent heartbeat

### Audit Logs
- `GET /api/auditlogs` - Query audit logs
- `GET /api/auditlogs/entity/{type}/{id}` - Get entity history

## Core Principles

### Safety First
- All destructive changes require extra approval
- Schema locks prevent unintended modifications
- Rollback scripts are required for all migrations

### Clarity
- Every change must have justification
- AI suggestions include reasoning and confidence
- Impact analysis is required for changes

### Additive Changes Preferred
- Adding columns/tables is safer than modifying
- Schema versioning follows semantic versioning
- Backward compatibility is prioritized

## Configuration

### Connection Strings
Connection strings are stored locally by agents and referenced by ID. They are never transmitted to the cloud layer.

### appsettings.json (API)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=isotyp.db"
  }
}
```

### Agent Configuration
```json
{
  "AgentKey": "unique-agent-identifier",
  "AgentName": "Production Agent 1",
  "CloudApiEndpoint": "https://your-isotyp-api.com",
  "LocalSecretStorePath": "./secrets",
  "HeartbeatIntervalSeconds": 30,
  "ValidationIntervalSeconds": 3600
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please read our contributing guidelines before submitting pull requests.

## Support

For support, please open an issue on GitHub or contact the maintainers.
