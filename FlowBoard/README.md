# FlowBoard — Task Management & Collaboration Platform (.NET Edition)

> A microservices-based task management platform built with ASP.NET Core 8, Entity Framework Core, and PostgreSQL.

---

## Microservices Overview

| # | Service | Status | Port |
|---|---|---|---|
| 1 | **Auth/User-Service** (UC1) | ✅ Complete | `http://localhost:5151` |
| 2 | **Workspace-Service** (UC2) | ✅ Complete | `http://localhost:5195` |
| 3 | **Board-Service** (UC3) | ✅ Complete | `http://localhost:5231` |
| 4 | **List/Column-Service** (UC4) | ✅ Complete | `http://localhost:5004` |
| 5 | **Task/Card-Service** (UC5) | ✅ Complete | `http://localhost:5246` |
| 6 | **Comment/Attachment** (UC6) | ✅ Complete | `http://localhost:5006` |
| 7 | **Checklist/Label-Service** (UC7) | ✅ Complete | `http://localhost:5063` |
| 8 | **Notification & Email Service** (UC8 & UC10) | ✅ Complete | `http://localhost:5011` |
| 9 | **Web/MVC UI Service** (UC9) | ✅ Complete | `http://localhost:5079` |

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Database | PostgreSQL (via Npgsql) |
| Message Broker | RabbitMQ (via MassTransit) |
| Cloud Storage | AWS S3 (for Attachments) |
| Auth | JWT Bearer Tokens |
| Password Hashing | ASP.NET Core Identity PasswordHasher |
| API Docs | Swagger / Swashbuckle |

---

## Solution Structure

```
FlowBoard/
├── FlowBoard.sln
├── FlowBoard.Auth/                  # UC1 — Auth/User Service
├── FlowBoard.Workspace/             # UC2 — Workspace Service
├── FlowBoard.Board/                 # UC3 — Board Service
├── FlowBoard.Column/                # UC4 — List/Column Service
├── FlowBoard.Task/                  # UC5 — Task/Card Service
├── FlowBoard.Comment/               # UC6 — Comment & Attachment Service
├── FlowBoard.Checklist/             # UC7 — Checklist & Label Service
└── FlowBoard.Notification/          # UC8 & UC10 — Notification & Email Service
    ├── Controllers/
    ├── Exceptions/
    ├── Infrastructure/
    ├── Models/
    ├── Repositories/
    ├── Services/
    ├── Storage/                     # AWS S3 integration
    └── Events/                      # RabbitMQ Event definitions
└── FlowBoard.Web/                   # UC9 — Web/MVC UI Service
```

---

## UC5 — Task/Card Service

### Responsibilities
- Manage individual task cards within lists.
- **Activity Logging**: Automatically logs every action (created, moved, assigned, priority change).
- **Background Worker**: `OverdueCardWorker` runs periodically to scan for overdue tasks.
- **Filtering**: Multi-criteria search by assignee, priority, status, and due date.
- **SAGA Implementation**: Cascading deletion support.

### Setup — Task Service

```bash
cd FlowBoard.Task
dotnet ef database update
dotnet run
```

Swagger UI: `http://localhost:5246/swagger`

---

## UC6 — Comment & Attachment Service

### Responsibilities
- **Threaded Comments**: Support for top-level comments and replies.
- **Soft Delete**: Comments are marked as deleted but preserved in DB for history.
- **AWS S3 Integration**: High-performance file storage for task attachments.
- **Event-Driven**: Publishes `CommentAddedEvent` and `AttachmentAddedEvent` to RabbitMQ.
- **Global Filters**: EF Core query filters to automatically hide deleted comments.

### Key Components

| Component | Role |
|---|---|
| `Comment` | Entity — Supports parent/child threading and soft-delete |
| `Attachment` | Entity — Stores S3 public URLs and metadata |
| `IS3Service` | Service — Handles streaming uploads/deletes to AWS S3 |
| `MassTransit` | Bus — Publishes events to RabbitMQ for Notification Service |

### API Endpoints

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/comments` | ✅ | Add a new comment or reply |
| GET | `/api/comments/card/{id}` | ✅ | Get all comments for a card |
| PUT | `/api/comments/{id}` | ✅ | Update comment content (Author only) |
| DELETE | `/api/comments/{id}` | ✅ | Soft-delete a comment (Author only) |
| POST | `/api/attachments` | ✅ | Upload file to AWS S3 (multipart/form-data) |
| GET | `/api/attachments/card/{id}` | ✅ | List all attachments for a card |
| DELETE | `/api/attachments/{id}` | ✅ | Delete from S3 and DB (Uploader only) |

### Setup — Comment Service

#### 1. Configure `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=flowboard_comment;Username=postgres;Password=1234"
  },
  "RabbitMQ": {
    "Host": "localhost"
  },
  "AWS": {
    "Region": "ap-south-1",
    "BucketName": "flowboard-attachments"
  }
}
```

#### 2. Apply migrations & run

```bash
cd FlowBoard.Comment
dotnet ef database update
dotnet run
```

Swagger UI: `http://localhost:5006/swagger`

---

## UC7 — Checklist & Label Service

### Responsibilities
- **Checklist Management**: Create and manage task checklists.
- **Label Management**: Create and assign colored labels to task cards.
- **Progress Tracking**: Calculate completion percentages of checklists.

### Setup — Checklist Service

```bash
cd FlowBoard.Checklist
dotnet ef database update
dotnet run
```

Swagger UI: `http://localhost:5063/swagger`

---

## UC8 & UC10 — Notification & Email Service

### Responsibilities
- **Real-Time Updates**: Pushes instant in-app notifications to users via SignalR.
- **Event-Driven**: Consumes RabbitMQ events for assignments, mentions, comments, and task movements.
- **Scheduled Reminders**: Runs background jobs with Quartz.NET to send due-date reminders (1 day / 1 hour before).
- **Email Fallback**: Integrates with SendGrid to send transactional emails.

### Setup — Notification Service

```bash
cd FlowBoard.Notification
dotnet ef database update
dotnet run
```

Swagger UI: `http://localhost:5011/swagger`

---

## UC9 — Web/MVC UI Service

### Responsibilities
- **Frontend Layer**: Acts as the central UI for members, board owners, and administrators using ASP.NET Core MVC & Razor Pages.
- **API Coordinator**: Uses `IHttpClientFactory` to securely orchestrate inter-service communication with the backend microservices.
- **Session Auth**: Manages user authentication and securely stores the JWT in a server-side distributed session cache.
- **Real-Time Collaboration**: Implements `BoardHub` via ASP.NET Core SignalR to push live board updates (card moves, comments, checklist updates) to clients.

### Setup — Web Service

```bash
cd FlowBoard.Web
dotnet run
```

Access the Web Application: `http://localhost:5079/`

---

## Running the Architecture

To run the full stack, open terminal windows for all services:

```bash
# Start Docker (for RabbitMQ and Postgres)
docker-compose up -d

# Start Services
dotnet run --project FlowBoard.Auth
dotnet run --project FlowBoard.Workspace
dotnet run --project FlowBoard.Board
dotnet run --project FlowBoard.Column
dotnet run --project FlowBoard.Task
dotnet run --project FlowBoard.Comment
dotnet run --project FlowBoard.Checklist
dotnet run --project FlowBoard.Notification
dotnet run --project FlowBoard.Web
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (Local or Docker)
- [AWS Account](https://aws.amazon.com/) (for S3 storage)