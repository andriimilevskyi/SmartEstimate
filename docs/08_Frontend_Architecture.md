# SmartEstimate

# 08. Frontend Architecture

Version: 1.0

Status: Draft

Author: SmartEstimate Team

Last Updated: 2026-08-03

---

# Purpose

This document defines the frontend architecture of SmartEstimate.

The frontend is designed to provide a fast, intuitive, and modern user experience for construction professionals.

The application must support:

- Desktop
- Laptop
- Tablet

Mobile support will be introduced in a future release.

The frontend must remain scalable, maintainable, and independent of backend implementation details.

---

# Frontend Philosophy

The frontend is not a collection of pages.

It is a professional workspace for construction estimation.

The application should help users complete tasks quickly with minimal clicks and minimal cognitive load.

Every interface decision should prioritize productivity over decoration.

---

# Design Principles

The frontend should be:

- Clean
- Fast
- Predictable
- Consistent
- Responsive
- Accessible
- Keyboard Friendly
- AI Ready

---

# Technology Stack

Framework

React

Language

TypeScript

Build Tool

Vite

Routing

React Router

Styling

TailwindCSS

UI Components

shadcn/ui

Icons

Lucide React

Forms

React Hook Form

Validation

Zod

Data Fetching

TanStack Query

State Management

Zustand

Notifications

Sonner

Tables

TanStack Table

Charts

Recharts

---

# Architectural Style

The frontend follows Feature-Sliced Design (FSD).

Layers

app/

pages/

widgets/

features/

entities/

shared/

The architecture separates business logic from presentation.

---

# Application Shell

The application shell contains:

Sidebar

Top Navigation

Workspace

Notification Center

Modal Layer

Toast Layer

Command Palette

---

# Navigation

Primary navigation is located in the left sidebar.

Main sections

Dashboard

Customers

Projects

Estimates

Knowledge Base

Materials

Pricing

Reports

AI Assistant

Settings

The sidebar must support collapsing.

---

# Workspaces

The application is organized into workspaces.

## Dashboard Workspace

Purpose

Provides an overview of company activity.

Widgets

Quick Actions

Recent Projects

Recent Estimates

AI Recommendations

Statistics

Notifications

---

## Customer Workspace

Manage customers.

Features

Customer List

Customer Details

Project History

Search

### Knowledge Studio

Knowledge Studio is the administrative workspace for Categories, Construction
Works, Materials, and Units. It reads and writes exclusively through the versioned
SmartEstimate REST API. It provides searchable, sortable, filterable, paged lists
and detail/create/edit views. Archive is a recoverable status action. The workspace
does not read or edit YAML files in the browser.

Construction Work detail views reserve a disabled “Fill with AI” action for the
future AI Knowledge Assistant. It must not invoke AI or generate content until that
capability is implemented.

Filtering

Attachments

---

## Project Workspace

Manage construction projects.

Features

Project Details

Photos

Measurements

Rooms

Timeline

Notes

Estimates

---

## Estimate Workspace

The Estimate Workspace is the core of SmartEstimate.

Layout

Left Panel

Knowledge Explorer

Center Panel

Estimate Editor

Right Panel

Properties

Bottom Panel

Totals

History

Validation

AI Suggestions

---

## Knowledge Workspace

Construction knowledge browser.

Contains

Categories

Construction Works

Processes

Materials

Dependencies

Templates

Search

---

## AI Workspace

Displays AI interactions.

Features

Recommendations

Market Analysis

Material Calculations

Voice Sessions

History

---

# Widgets

Widgets are reusable business blocks.

Examples

Recent Projects

Recent Estimates

Statistics

Quick Actions

Activity Feed

Notifications

Market Prices

AI Suggestions

---

# Features

Features represent user actions.

Examples

Create Customer

Create Project

Create Estimate

Duplicate Estimate

Export Estimate

Analyze Market

Generate Materials

Accept Recommendation

Reject Recommendation

---

# Entities

Entities represent business objects.

Examples

Customer

Project

Estimate

Estimate Item

Construction Work

Material

Company

User

Entities contain:

Models

API

Validation

Business Helpers

---

# Shared Layer

Contains reusable functionality.

Examples

UI Components

Hooks

Utilities

API Client

Configuration

Constants

Types

Localization

Icons

Theme

---

# State Management

Global State

Zustand

Remote Data

TanStack Query

Forms

React Hook Form

Local UI State

React State

Business logic should not be stored inside UI components.

---

# API Integration

The frontend communicates only with ASP.NET Core REST API.

Direct database access is forbidden.

Direct communication with Python AI services is forbidden.

Flow

React

↓

ASP.NET Core API

↓

Python AI Service

---

# Error Handling

Errors must be user friendly.

Technical information should never be displayed.

Every error should contain

Title

Description

Possible Action

---

# Loading States

Every asynchronous operation must provide:

Loading

Success

Empty

Error

Timeout

No blank screens are allowed.

---

# Accessibility

Keyboard Navigation

Visible Focus

ARIA Labels

High Contrast

Screen Reader Support

Minimum WCAG AA Compliance

---

# Responsive Design

Desktop

Primary platform.

Tablet

Fully supported.

Mobile

Future implementation.

---

# Internationalization

The frontend must support localization.

Initial Languages

English

Ukrainian

Future

German

Polish

Spanish

French

Text must never be hardcoded.

---

# Theming

Light Theme

Dark Theme

System Theme

Theme selection stored per user.

---

# Performance

Lazy Loading

Code Splitting

Route Based Loading

Image Optimization

Memoization where appropriate

Virtualized Tables for large datasets

---

# Security

HTTPS Only

JWT Authentication

Secure Storage

Role-Based UI

Input Validation

Output Encoding

No sensitive information stored in Local Storage.

---

# Folder Structure

src/

app/

pages/

widgets/

features/

entities/

shared/

assets/

styles/

---

# Component Principles

Components should be:

Small

Reusable

Composable

Independent

Testable

Business logic belongs to features and entities, not presentation components.

---

# Future Extensions

Offline Mode

Progressive Web App (PWA)

Desktop Wrapper

Mobile Application

Real-Time Collaboration

Multi-Window Support

Drag & Drop Layout

Custom Dashboards

---

# Success Criteria

The frontend should allow an experienced estimator to:

Create a project in under 2 minutes.

Create a complete estimate in under 10 minutes.

Export a professional PDF in under 30 seconds.

Navigate the application without training.

Use the application efficiently on desktop and tablet devices.

---

# Summary

The SmartEstimate frontend is designed as a modern productivity workspace rather than a traditional CRUD application.

The architecture emphasizes usability, scalability, maintainability, and future AI integration while keeping the interface clean and intuitive for construction professionals.

---

End of Document
