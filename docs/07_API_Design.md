# SmartEstimate

# 07. API Design

Version: 1.0

Status: Draft

Author: SmartEstimate Team

Last Updated: 2026-08-03

---

# Purpose

This document defines the API design principles for SmartEstimate.

The API provides communication between:

- React Frontend
- ASP.NET Core Backend
- Python AI Services
- Future Mobile Applications
- Future Third-Party Integrations

The API must remain stable, consistent, secure, and scalable.

---

# API Style

SmartEstimate uses RESTful API architecture.

Characteristics:

- Stateless
- Resource-Oriented
- JSON Based
- HTTPS Only
- Versioned
- Predictable
- Consistent

---

# API Versioning

Every endpoint is versioned.

Current version:

/api/v1/

Future versions:

/api/v2/

/api/v3/

Version changes must never break existing clients without a migration strategy.

---

# Communication Format

All requests and responses use JSON.

Content-Type

application/json

UTF-8 encoding

---

# Naming Convention

Resources use plural nouns.

Examples

/api/v1/customers

/api/v1/projects

/api/v1/estimates

/api/v1/materials

/api/v1/prices

/api/v1/users

Avoid verbs in URLs whenever possible.

Good

POST /api/v1/customers

Bad

POST /api/v1/createCustomer

---

# HTTP Methods

GET

Read resources

POST

Create resources

PUT

Replace resources

PATCH

Partial updates

DELETE

Soft delete resources

---

# Authentication

Authentication uses JWT Bearer Tokens.

Authorization Header

Authorization: Bearer <token>

Refresh Tokens are required.

HTTPS is mandatory.

---

# Authorization

Role-Based Access Control (RBAC)

Initial Roles

Administrator

Owner

Estimator

Employee

Future

Custom Permissions

---

# Request Validation

Every request must be validated.

Validation occurs in:

Application Layer

Never inside Controllers.

Validation includes:

Required fields

Maximum length

Minimum length

Range validation

Business validation

---

# Response Format

Every response follows a consistent structure.

Successful Response

{
  "success": true,
  "data": { }
}

Error Response

{
  "success": false,
  "error": {
    "code": "ValidationError",
    "message": "Customer name is required.",
    "traceId": "..."
  }
}

---

# Error Handling

The API must never expose stack traces.

Standard HTTP Codes

200 OK

201 Created

204 No Content

400 Bad Request

401 Unauthorized

403 Forbidden

404 Not Found

409 Conflict

422 Validation Error

500 Internal Server Error

---

# Pagination

All collection endpoints support pagination.

Query Parameters

page

pageSize

Example

GET /api/v1/customers?page=1&pageSize=20

---

# Sorting

Collections support sorting.

Example

sort=name

sort=-createdAt

---

# Filtering

Collections support filtering.

Examples

status

category

dateFrom

dateTo

priceMin

priceMax

search

Multiple filters may be combined.

---

# Searching

Search is supported where applicable.

Example

GET /api/v1/customers?search=John

Future versions may support Full Text Search.

---

# Resource Relationships

Nested resources are allowed when appropriate.

Examples

/api/v1/projects/{projectId}/estimates

/api/v1/estimates/{estimateId}/items

/api/v1/customers/{customerId}/projects

Deep nesting should be avoided.

---

# Idempotency

GET

PUT

DELETE

must be idempotent.

POST is not required to be idempotent.

---

# Concurrency

Optimistic Concurrency Control.

Entities include Version fields.

Conflicts return HTTP 409.

---

# File Upload

Files are uploaded separately.

Metadata is stored in PostgreSQL.

Binary files are stored externally.

Supported Files

Images

PDF

Documents

Voice Files

Future

Videos

---

# API Documentation

The API must expose:

Swagger UI

OpenAPI 3.1 Specification

JSON Schema

The OpenAPI specification is the source of truth for all endpoints.

---

# Security

HTTPS only

JWT Authentication

Refresh Tokens

Rate Limiting

Input Validation

Output Encoding

Audit Logging

Role-Based Authorization

Future

API Keys

OAuth2

---

# Rate Limiting

Authentication

10 requests/minute

General API

300 requests/minute

Export

20 requests/minute

AI

30 requests/minute

Limits must be configurable.

---

# Logging

Every request logs:

Timestamp

User

Route

HTTP Method

Execution Time

Response Code

TraceId

Sensitive information must never be logged.

---

# Performance

Average Response Time

< 300 ms

Maximum

< 1000 ms

Long-running operations must be asynchronous.

---

# AI Communication

Python AI Services communicate through REST.

The AI Service never accesses PostgreSQL directly.

Communication

React

↓

ASP.NET Core API

↓

Python AI Service

↓

Response

The API decides whether AI recommendations are accepted.

---

# Endpoint Groups

Authentication

Users

Companies

Customers

Projects

Estimates

Estimate Items

Knowledge

Materials

Pricing

Reports

AI

Administration

Settings

Future

Notifications

Marketplace

Suppliers

Analytics

---

# API Evolution

Breaking changes require a new API version.

Backward compatibility should be maintained whenever possible.

Deprecated endpoints must remain available during the transition period.

---

# Future Extensions

GraphQL

WebSockets

Public API

Partner API

Webhook Support

SDK Generation

Mobile SDK

Offline Synchronization

---

# Summary

The SmartEstimate API is designed to be:

- Consistent
- Secure
- Predictable
- Scalable
- Cloud Ready
- AI Ready

The OpenAPI specification will define every endpoint and become the authoritative contract between backend, frontend, AI services, and future integrations.

---

End of Document