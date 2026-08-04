# SmartEstimate

# System Architecture

Version: 0.1

Status: Draft

Author: SmartEstimate Team

---

# Architecture Goals

The architecture must satisfy the following goals.

• Modular

• Maintainable

• Testable

• Cloud Ready

• AI Ready

• Multi Company

• Multi Country

• High Performance

• Easy to Extend

---

# High Level Architecture

                  React Frontend
                         │
                         │ REST API
                         ▼
                ASP.NET Core API
                         │
        ┌────────────────┴─────────────────┐
        │                                  │
        ▼                                  ▼
 Application Layer                Background Jobs
        │                                  │
        ▼                                  ▼
      Domain                     AI Integration
        │                                  │
        ▼                                  ▼
 Infrastructure                  Python AI Service
        │
        ▼
 PostgreSQL

---

# Architectural Style

Clean Architecture

Domain Driven Design

SOLID

Vertical Slice Architecture (inside Application layer)

CQRS where appropriate

Repository pattern only when necessary

Dependency Injection

Event Driven communication

---

# Knowledge Persistence Decision

PostgreSQL is the single source of truth for operational construction knowledge.
Knowledge Studio, the Estimate Editor, and all backend use cases access categories,
construction works, materials, and measurement units through Application-layer
repository abstractions. Infrastructure implements those abstractions with EF Core
and PostgreSQL. Normal application requests never read YAML files.

The repository-level `knowledge/` directory remains a non-operational interchange
format, reserved for future import, export, backup, and catalogue exchange. YAML
is not a runtime dependency of the backend and does not own production data.

---

# Solution Structure

SmartEstimate.sln

src/

    SmartEstimate.Api/

    SmartEstimate.Application/

    SmartEstimate.Domain/

    SmartEstimate.Infrastructure/

    SmartEstimate.Contracts/

    SmartEstimate.Shared/

tests/

    SmartEstimate.UnitTests/

    SmartEstimate.IntegrationTests/

docs/

knowledge/

python-ai/

frontend/

docker/

.github/

The `knowledge/` directory contains import/export artefacts only. It is not mounted
or read by the application during normal operation.

---

# Responsibilities

API

HTTP

Authentication

Controllers

Swagger

Validation

No business logic.

---

Application

Use Cases

Commands

Queries

Handlers

Validators

DTO

Mapping

Transactions

---

Domain

Business Rules

Entities

Aggregates

Value Objects

Events

Interfaces

Enums

No EF Core references.

---

Infrastructure

EF Core

PostgreSQL

Repositories

External APIs

PDF

Email

Storage

Caching

Logging

---

Contracts

DTO

Requests

Responses

Events

Shared Contracts

---

Shared

Common utilities

Result objects

Exceptions

Constants

Extensions

Base classes

---

# Communication

React

↓

REST

↓

API

↓

Application

↓

Domain

↓

Infrastructure

↓

Database

Business logic always flows inward.

Dependencies never point toward UI.

---

# Dependency Rule

Allowed

API

↓

Application

↓

Domain

Infrastructure → Domain

Forbidden

Domain → Infrastructure

Domain → API

Application → API

React → Database

Python → Database
