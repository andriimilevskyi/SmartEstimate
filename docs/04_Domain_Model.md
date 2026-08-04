# SmartEstimate

# 04. Domain Model

Version: 1.0

Status: Draft

Author: SmartEstimate Team

Last Updated: 2026-08-03

---

# Purpose

This document defines the business domain of SmartEstimate.

The goal is to describe all business entities, their relationships, responsibilities, and business rules without considering implementation details.

The Domain Model is independent of:

- Database
- ASP.NET Core
- React
- Entity Framework
- REST API
- Python AI Service

The domain represents the business itself.

---

# Domain Philosophy

SmartEstimate is built around business concepts rather than technical components.

Every entity represents a real object or concept used by construction professionals.

Business rules always belong inside the domain.

The database, API, and UI must adapt to the domain — never the opposite.

---

# Domain Principles

- Domain First
- Business before Technology
- Framework Independent
- Rich Domain Model
- SOLID
- Single Responsibility
- Explicit Business Rules
- Event Driven
- AI Assisted
- Future Ready

---

# Ubiquitous Language

The following terminology must be used consistently across the entire project.

| Business Term | Description |
|---------------|-------------|
| Company | Construction company using SmartEstimate |
| User | Authenticated system user |
| Customer | End customer ordering construction work |
| Project | Construction object |
| Estimate | Commercial construction estimate |
| Estimate Item | Single work inside an estimate |
| Construction Work | Work that can be performed |
| Material | Material used for construction |
| Category | Construction work category |
| Dependency | Relationship between works |
| Recommendation | Suggested work or action |
| Price | Company or market price |
| Market Analysis | AI-generated pricing information |
| Construction Stage | Phase of construction |
| Knowledge Base | Construction expertise database |

---

# Bounded Contexts

The system consists of independent business contexts.

## Identity

Responsible for:

- Users
- Authentication
- Authorization
- Roles
- Permissions

Main Entities

- User
- Role
- Permission
- RefreshToken

---

## Company

Responsible for:

- Company settings
- Employees
- Branding
- Localization

Main Entities

- Company
- Employee
- CompanySettings

---

## Customer

Responsible for:

- Customer information
- Contacts
- Addresses
- Notes

Main Entities

- Customer
- Address
- Contact

---

## Project

Responsible for:

- Construction objects
- Rooms
- Measurements
- Photos
- Status

Main Entities

- Project
- Room
- Measurement
- Attachment

---

## Estimate

Responsible for:

- Estimates
- Estimate items
- Totals
- Discounts
- Taxes
- Export

Main Entities

- Estimate
- EstimateItem
- EstimateGroup
- Discount
- Tax

---

## Knowledge

Responsible for construction knowledge.

Main Entities

- ConstructionWork
- Category
- Unit
- Dependency
- Recommendation
- ConstructionStage
- Difficulty
- ConsumptionRule

Knowledge is managed as a dedicated bounded context. Its operational records are
persisted in PostgreSQL, while YAML is an interchange format only. The context
exposes repository abstractions to Application use cases; neither Estimate nor AI
integrations access storage directly.

---

## Pricing

Responsible for pricing.

Main Entities

- CompanyPrice
- MarketPrice
- PriceHistory
- PriceRecommendation
- PriceRegion
- PriceSource

---

## Materials

Responsible for material calculations.

Main Entities

- Material

For the Knowledge Studio MVP, Category, ConstructionWork, Material, and Unit carry
`Id`, `Version`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, and `Status`.
Status is Draft, Active, or Archived. Archived records are soft-deleted and remain
recoverable. Only Active works, materials, and units are selectable in estimates.
- MaterialCategory
- MaterialConsumption
- Supplier
- Package

---

## AI

Responsible for AI recommendations.

Main Entities

- PriceAgent
- EstimateAgent
- RecommendationAgent
- MaterialAgent
- VoiceAgent
- OCRAgent

---

## Reporting

Responsible for:

- Reports
- Dashboard
- Analytics

---

# Aggregate Roots

The following entities are Aggregate Roots.

- Company
- Customer
- Project
- Estimate
- ConstructionWork
- Material

No external entity should directly modify child entities.

---

# Entities

## Company

Represents one construction company.

Responsibilities

- Owns all business data
- Owns employees
- Owns projects
- Owns customers
- Owns price lists

---

## Customer

Represents a construction customer.

Responsibilities

- Contact information
- Project ownership
- Communication history

---

## Project

Represents one construction object.

Responsibilities

- Physical location
- Measurements
- Photos
- Estimates
- Status

---

## Estimate

Represents one commercial proposal.

Responsibilities

- Work list
- Material list
- Totals
- History
- Export

---

## Estimate Item

Represents one construction work.

Contains

- Construction Work
- Quantity
- Unit
- Price
- Notes

---

## Construction Work

Represents one construction activity.

Contains

- Name
- Category
- Description
- Unit
- Difficulty
- Related Works
- Consumption Rules
- Recommendations

---

## Material

Represents one construction material.

Contains

- Name
- Unit
- Package Size
- Consumption Formula
- Waste Factor

---

# Value Objects

The following concepts are immutable.

Money

Quantity

Area

Length

Volume

Percentage

PhoneNumber

Email

Address

Dimensions

Currency

MeasurementUnit

---

# Domain Events

EstimateCreated

EstimateUpdated

EstimateDeleted

EstimateApproved

EstimateExported

CustomerCreated

CustomerUpdated

ProjectCreated

ProjectClosed

PriceUpdated

MarketAnalysisCompleted

RecommendationGenerated

RecommendationAccepted

RecommendationRejected

MaterialCalculated

VoiceEstimateCreated

---

# Business Rules

## Company

A company owns all business data.

Users cannot access data from another company.

---

## Estimate

Estimate must belong to exactly one project.

Estimate must belong to exactly one company.

Estimate total cannot be negative.

Estimate history cannot be modified.

Deleted estimates remain recoverable.

---

## Estimate Item

Quantity must be greater than zero.

Price must be greater than or equal to zero.

Construction work cannot be empty.

---

## Construction Work

Every work belongs to one category.

Every work has one measurement unit.

Every work may contain multiple dependencies.

---

## Materials

Material consumption must never be negative.

Package size must be greater than zero.

Waste factor must be configurable.

---

## Pricing

Price history is immutable.

Market prices never overwrite company prices automatically.

Only users approve AI recommendations.

---

## AI

AI never modifies business data directly.

AI only produces recommendations.

The user always makes the final decision.

---

# Relationships

Company

├── Employees

├── Customers

├── Projects

├── Estimates

├── Price Lists

└── Settings

Customer

└── Projects

Project

├── Rooms

├── Attachments

├── Photos

└── Estimates

Estimate

├── Estimate Items

├── Discounts

├── Taxes

├── History

└── Attachments

Construction Work

├── Dependencies

├── Materials

├── Recommendations

├── Difficulty

├── Construction Stage

└── Consumption Rules

---

# Dependency Rules

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

Domain

↓

API

Domain

↓

Database

React

↓

Database

Python

↓

Database

---

# Future Contexts

CRM

Finance

Notifications

Calendar

Scheduling

Inventory

Marketplace

Supplier Integration

Equipment Management

Employee Time Tracking

Document Management

Offline Synchronization

---

# Domain Goals

The domain model should remain stable even if:

- the frontend changes;
- the database changes;
- the AI implementation changes;
- the backend framework changes.

Business knowledge must remain independent from technology.

---

# Summary

The SmartEstimate Domain Model defines the business language and rules of the system.

It serves as the foundation for:

- Database Design
- API Design
- Backend Implementation
- Frontend Development
- AI Integration
- Future Scalability

The Domain Model must always be considered the single source of business truth.

---

End of Document
