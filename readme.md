# Identity

An OAuth2 identity provider system.

## What It Does

Identity provides OAuth2 authentication and authorization services with multiple grant flows (Resource Owner Password, Client Credentials, Authorization Code, Device Code, Implicit). It manages users, roles, audiences, issuers, and tokens.

## Key Components

| Project | Purpose |
|---------|---------|
| `Bhbk.WebApi.Identity.Sts` | Security Token Service (OAuth2 endpoints) |
| `Bhbk.WebApi.Identity.Admin` | Administrative management API |
| `Bhbk.WebApi.Identity.User` | User self-service API |
| `Bhbk.WebApi.Alert` | Alert/notification service |
| `Bhbk.Lib.Identity.Domain` | Business logic and authorization handlers |
| `Bhbk.Lib.Identity.Data.EF` | Entity Framework Core data access |
| `Bhbk.Lib.Identity` | OAuth2 JWT factory and grant implementations |
| `Bhbk.Lib.Identity.Primitives` | Shared constants and enums |
| `Bhbk.Cli.Identity` | Identity management CLI |
| `Bhbk.Cli.Alert` | Alert operations CLI |
| `Bhbk.Mssql.Identity` | SQL Server database project |

## Tech Stack

- .NET 8.0 / ASP.NET Core
- Entity Framework Core 8
- SQL Server
- JWT Bearer authentication
- OAuth2 (multiple grant flows)
- Angular (admin and user UIs)
- Quartz.NET for job scheduling

## OAuth2 Grant Flows

- Resource Owner Password Grant (v1/v2)
- Client Credentials Grant
- Authorization Code
- Device Code
- Implicit
