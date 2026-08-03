# SmartEstimate

# Product Requirements Document (PRD)

Version: 0.1

Status: Draft

Author: SmartEstimate Team

Last Updated: 2026-08-03

---

# 1. Purpose

The purpose of SmartEstimate is to simplify and automate construction estimate creation while keeping the contractor fully in control of every decision.

The platform combines structured construction knowledge with artificial intelligence to significantly reduce estimate preparation time and improve estimate quality.

---

# 2. Scope

The first release focuses on construction estimate creation for the Ukrainian market.

The application will support:

• Individual contractors

• Small construction companies

• Interior renovation companies

Future versions will support multiple countries and enterprise customers.

---

# 3. User Roles

## Administrator

System configuration

User management

Reference data management

AI configuration

Company settings

---

## Company Owner

Access to all company data

Create users

Manage projects

Manage pricing

View analytics

---

## Estimator

Create estimates

Edit estimates

Export estimates

Manage clients

Manage projects

---

## Employee

Read assigned projects

View estimates

Upload photos

Add comments

---

# 4. Functional Requirements

## FR-001 Authentication

Description

Users must authenticate using email and password.

Acceptance Criteria

- JWT authentication
- Refresh tokens
- Secure password hashing
- Password reset

Priority

High

---

## FR-002 Company Management

A company can contain multiple users.

Each company has independent data.

Acceptance Criteria

- Create company
- Edit company
- Delete company
- Company settings

Priority

High

---

## FR-003 Client Management

The system shall allow managing customers.

Fields

First Name

Last Name

Phone

Email

Address

Notes

Priority

High

---

## FR-004 Project Management

Each client can have multiple projects.

Fields

Project Name

Address

Construction Type

Area

Floor Count

Room Count

Ceiling Height

Status

Description

Photos

Priority

High

---

## FR-005 Work Catalog

The application shall contain a centralized construction work catalog.

Each work contains

Name

Category

Description

Unit

Current Price

Market Price

Related Works

Material Rules

Status

Priority

Critical

---

## FR-006 Estimate Creation

Users shall create estimates from construction works.

Estimate supports

Adding works

Editing works

Removing works

Changing quantities

Changing prices

Sorting

Grouping

Comments

Priority

Critical

---

## FR-007 Estimate Totals

The system automatically calculates

Labor Cost

Material Cost

Discount

Taxes

Grand Total

Priority

Critical

---

## FR-008 Estimate History

Every modification must be recorded.

History contains

Date

User

Old Value

New Value

Priority

Medium

---

## FR-009 Export

Supported formats

PDF

Excel

Word

Priority

High

---

## FR-010 Dashboard

Dashboard displays

Projects

Estimates

Revenue

Recent Activity

Pending AI Suggestions

Priority

Medium

---

# 5. AI Requirements

The first MVP will NOT include AI implementation.

However, architecture must support future AI integration.

Future AI modules

Price Agent

Estimate Agent

Recommendation Agent

Material Agent

Voice Agent

OCR Agent

Document Agent

---

# 6. Non Functional Requirements

Performance

Average API response

< 300 ms

Maximum

< 1000 ms

---

Availability

99.9%

---

Security

JWT

HTTPS

Input validation

Role-based authorization

SQL Injection protection

XSS protection

CSRF protection

---

Scalability

Multi-company

Multi-country

Cloud-ready

Horizontal scaling

---

Localization

Initially

English

Ukrainian

Future

German

Polish

Spanish

French

---

Accessibility

Responsive design

Keyboard navigation

Dark mode

Tablet support

---

# 7. Out of Scope (MVP)

Accounting

Payroll

Tax reporting

Structural calculations

BIM

CAD

---

# 8. MVP Features

Authentication

Users

Companies

Clients

Projects

Construction Works

Estimate Editor

PDF Export

Dashboard

Settings

---

# 9. Future Features

Market Analysis

Voice Estimates

Construction Schedule

Material Purchase List

Supplier Integration

Mobile App

Offline Mode

CRM

Financial Reports

Analytics

Construction Timeline

---

# 10. Success Criteria

Create estimate in under 10 minutes.

Reduce forgotten construction work.

Provide professional PDF.

Support multiple projects.

Support thousands of construction works.

Provide scalable architecture.

---

# End of Document