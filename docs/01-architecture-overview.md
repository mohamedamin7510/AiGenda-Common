# System Architecture Overview

This document outlines the current system architecture of AIgenda, mapped directly to the goal of creating a "Unified Workspace" (Layer 1) capable of powering the "AI Intelligence Engine" (Layer 2) as defined in the core business analysis.

## 1. Tech Stack
- **Framework**: .NET 9 (Web API)
- **Data Access**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens) with custom authentication services.
- **Testing**: xUnit with Moq/WireMock (integration testing via `AI_genda_API.IntegrationTests`).

## 2. Current Folder Structure & Layering
The codebase follows a modular monolithic structure designed to support Layer 1 functionalities alongside integration and data gathering:
- **`Controllers/`**: The entry points for the workspace UI (e.g., `AuthController`, `TasksController`, `WorkspaceController`, `AppsConnectionController`, `FocusSessionController`).
- **`Models/` (Domain Entities)**: Represents the core Layer 1 structures (`Task`, `SubTask`, `WorkSpace`, `NoteContent`, `FocusSession`, `ConnectedApp`).
- **`Services/`**: Contains the core business logic handling Layer 1 data generation:
  - `TaskService`, `SubTaskService`, `SpaceService`, `WorkSpaceService`
  - `NoteService`, `FocusSessionService`
  - `ProfileService`, `RoleService`
- **`Services/AppConnectionService/` & Integration Services**: Centralizes scattered data to break "The Fragmentation Problem."
- **`Presistiences/`** (Infrastructure): EF Core database context (`ApplicationDbContext`) and entity configurations.
- **`Settings/`**: Configuration objects mapping to `appsettings.json`.

## 3. OAuth Integrations Pipeline
As defined in the business vision, "Every feature belongs in one place — no integrations as a crutch." However, retrieving data from external toolsets enables unified workflows. 
The current OAuth and Integration pipeline operates as follows:
1. **Initiation**: `AppsConnectionController` receives connection requests for external tools (GitHub, Google Calendar).
2. **Factory Routing**: `IAppConnectorFactory` routes the request to the appropriate connector (`GitHubConnector`, `GoogleCalendarConnector`).
3. **Token Management**: `TokenManagerService` securely handles OAuth tokens, utilizing `TokenEncryptionService` to protect user credentials at rest.
4. **Data Sync**: Dedicated services (`GitHubIntegrationService`, `GmailIntegrationService`, `GoogleCalendarIntegrationService`) ingest external work into the unified Layer 1 environment.

## 4. Main Domain Entities
- **ApplicationUser & Profile**: The central actor generating behavioral data.
- **WorkSpace & WorkspaceMembers**: Facilitates team coordination.
- **FocusSession**: Tracks time allocation (direct input for AI analysis).
- **Tasks & Subtasks**: Core tracking units for priority and completion patterns.
- **Notes (Voice & Text)**: Knowledge capture and journaling.
- **ConnectedApp**: Maps third-party identities to the AIgenda user profile.