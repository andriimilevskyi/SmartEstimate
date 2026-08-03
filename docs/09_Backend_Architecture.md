# SmartEstimate

# 09. Backend Architecture

Version: 1.0

Status: Draft

Author: SmartEstimate Team

Last Updated: 2026-08-03

---

# Purpose

This document defines the backend architecture of SmartEstimate.

The backend is responsible for:

- Business Logic
- Authentication
- Authorization
- Estimate Management
- Knowledge Integration
- AI Coordination
- Reporting
- Data Persistence

The backend must remain modular, scalable, testable, and independent of frontend implementation.

---

# Technology Stack

Language

C#

Framework

ASP.NET Core (.NET 9)

ORM

Entity Framework Core

Database

PostgreSQL

Validation

FluentValidation

Authentication

JWT

Logging

Serilog

Mapping

Mapster

Caching

Redis (future)

Background Jobs

Hangfire (future)

Documentation

Swagger / OpenAPI

---

# Architectural Style

Hybrid Architecture

Clean Architecture

+

Vertical Slice Architecture

The backend is organized by business capabilities rather than technical layers.

---

# Solution Structure

src/

SmartEstimate.Api

SmartEstimate.Application

SmartEstimate.Domain

SmartEstimate.Knowledge

SmartEstimate.Infrastructure

SmartEstimate.Contracts

SmartEstimate.Shared

tests/

SmartEstimate.UnitTests

SmartEstimate.IntegrationTests

---

# Project Responsibilities

Api

HTTP

Authentication

Middleware

Swagger

Dependency Injection

Controllers (minimal)

Application

Use Cases

Commands

Queries

Validators

DTO

Business Workflows

Domain

Business Rules

Entities

Value Objects

Events

Knowledge

Construction Rules

Dependencies

Templates

Consumption Rules

Infrastructure

Database

Repositories

Email

Storage

PDF

External Services

Contracts

DTO

API Contracts

Shared Models

Shared

Common Utilities

Result

Exceptions

Extensions

---

# Vertical Slice Structure

Application/

Customers/

CreateCustomer/

Command.cs

Handler.cs

Validator.cs

Mapping.cs

Response.cs

DeleteCustomer/

UpdateCustomer/

Projects/

CreateProject/

Estimates/

CreateEstimate/

EstimateItems/

Knowledge/

AI/

Every business feature lives in one folder.

---

# Request Flow

Client

↓

API

↓

Validation

↓

Application

↓

Domain

↓

Infrastructure

↓

Database

↓

Response

---

# Dependency Rule

Allowed

API

↓

Application

↓

Domain

Infrastructure

↓

Domain

Forbidden

Domain

↓

Infrastructure

Application

↓

API

Knowledge

↓

Infrastructure

---

# Business Logic

Business rules belong only to the Domain Layer.

Controllers contain no business logic.

Repositories contain no business logic.

UI contains no business logic.

---

# Validation

Validation uses FluentValidation.

Validation occurs before business execution.

Business validation remains inside Domain.

---

# Result Pattern

Every use case returns a Result object.

Result

Success

Failure

Validation Errors

Business Errors

---

# Exception Handling

Global Exception Middleware

Centralized Error Responses

TraceId

Logging

Problem Details

---

# Dependency Injection

Constructor Injection

No Service Locator

No Static Services

---

# Mapping

Mapster

Mapping occurs between

Entities

↓

DTO

↓

API Models

Domain entities never leave the Application Layer.

---

# Logging

Serilog

Log Levels

Information

Warning

Error

Critical

Audit Events

Estimate Created

Estimate Updated

Price Changed

AI Recommendation Accepted

AI Recommendation Rejected

---

# Background Processing

Future

Hangfire

Jobs

PDF Generation

Market Analysis

AI Processing

Notifications

Cleanup

---

# Caching

Future

Redis

Used for

Knowledge

Categories

Market Prices

Settings

Reference Data

---

# File Storage

Backend stores metadata.

Files stored externally.

Supported

Images

PDF

Documents

Voice

Future

Videos

---

# Security

JWT

HTTPS

Role-Based Authorization

Rate Limiting

Refresh Tokens

Input Validation

Audit Logging

---

# Testing

Unit Tests

Integration Tests

Application Tests

Domain Tests

Repository Tests

API Tests

---

# Performance

Asynchronous Programming

Cancellation Tokens

Pagination

Streaming where appropriate

Minimal allocations

Database indexes

---

# Configuration

Environment-based configuration.

Development

Testing

Production

Secrets never stored in source code.

---

# AI Integration

Backend coordinates AI.

Backend owns business data.

Python owns AI processing.

Flow

Frontend

↓

Backend

↓

Python AI

↓

Backend

↓

Database

↓

Frontend

Python never modifies business data directly.

---

# Future Evolution

Current

Modular Monolith

Future

Microservices

Possible Services

Identity

Estimate

Knowledge

AI

Notifications

Reporting

Storage

No domain logic should require major changes during migration.

---

# Guiding Principles

Business before Framework.

Knowledge before AI.

Simple before Clever.

Readable before Compact.

Explicit before Implicit.

Every feature should be independently understandable.

The architecture must remain maintainable after five years of continuous development.

---

# Summary

The SmartEstimate backend is built as a modular monolith using Clean Architecture and Vertical Slice Architecture.

This approach provides:

- high maintainability;
- clear separation of concerns;
- scalability;
- testability;
- future migration to microservices;
- seamless AI integration.

The backend remains the single source of truth for all business operations.

---

End of Document