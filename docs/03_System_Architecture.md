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
