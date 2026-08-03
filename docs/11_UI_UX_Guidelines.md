# SmartEstimate

# 11. UI / UX Guidelines

Version: 1.0

Status: Draft

Author: SmartEstimate Team

Last Updated: 2026-08-03

---

# Purpose

This document defines the visual language and user experience principles of SmartEstimate.

The goal is to create a modern, professional application that enables construction professionals to work quickly, accurately, and comfortably throughout the day.

The interface should feel closer to Linear, Notion, Stripe, and ClickUp than to traditional ERP or accounting software.

---

# Design Philosophy

SmartEstimate is a productivity tool.

The interface should:

- Reduce cognitive load.
- Require as few clicks as possible.
- Highlight important information.
- Hide unnecessary complexity.
- Feel fast and responsive.
- Be visually clean and modern.

---

# Design Principles

- Simplicity
- Consistency
- Predictability
- Accessibility
- Readability
- Performance
- Scalability

Every screen should answer one question:

"What is the user trying to accomplish right now?"

---

# Layout

Application Layout

------------------------------------------------

Sidebar

Top Navigation

Workspace

Right Context Panel (optional)

Status Bar

Toast Layer

Modal Layer

------------------------------------------------

The main workspace must always receive the largest amount of screen space.

---

# Grid System

12-column responsive grid.

Spacing scale

4 px

8 px

12 px

16 px

24 px

32 px

48 px

64 px

Use an 8px spacing system whenever possible.

---

# Border Radius

Small

6px

Medium

10px

Large

16px

Cards

16px

Dialogs

20px

---

# Elevation

Minimal shadows.

Avoid heavy drop shadows.

Use elevation only to indicate hierarchy.

---

# Color System

Primary

Blue

Success

Green

Warning

Amber

Danger

Red

Information

Sky Blue

Neutral

Gray Scale

Avoid using color as the only way to communicate status.

---

# Typography

Font

Inter

Fallback

System UI

Hierarchy

Display

Heading 1

Heading 2

Heading 3

Body

Caption

Use consistent font weights.

Avoid more than three font sizes within one component.

---

# Icons

Lucide React

Rules

One visual style.

No mixed icon libraries.

Icons assist recognition.

Icons never replace labels.

---

# Navigation

Primary Navigation

Left Sidebar

Secondary Navigation

Tabs

Context Navigation

Breadcrumbs

Users should never be more than three clicks away from common actions.

---

# Dashboard

Purpose

Provide a quick overview.

Widgets

Quick Actions

Recent Projects

Recent Estimates

AI Suggestions

Statistics

Notifications

Recent Activity

Dashboard should load in under one second.

---

# Forms

Forms should be divided into logical sections.

Rules

Clear labels

Inline validation

Helpful placeholders

Keyboard navigation

Autosave where appropriate

---

# Tables

Tables must support

Sorting

Filtering

Searching

Column resizing

Pagination

Bulk actions

Sticky headers

Avoid horizontal scrolling whenever possible.

---

# Cards

Cards are the primary UI element.

Cards should display

Title

Key Information

Actions

Status

Cards should never become visually cluttered.

---

# Buttons

Primary

Main action

Secondary

Supporting action

Ghost

Low emphasis

Danger

Destructive action

Never display more than one primary button within the same logical area.

---

# Dialogs

Use dialogs only for:

Confirmation

Creation

Editing

Never place long workflows inside modal dialogs.

---

# Notifications

Toast Notifications

Success

Information

Warning

Error

Notifications disappear automatically unless user interaction is required.

---

# Empty States

Every empty screen must explain:

Why the screen is empty.

What the user should do next.

Provide a primary action.

Example

"No projects yet."

Create your first project.

[Create Project]

---

# Loading States

Use skeleton loaders.

Avoid full-screen loading indicators.

Show progress whenever possible.

---

# Error States

Errors must contain:

Simple title

Human-readable explanation

Suggested action

Never expose technical exception details.

---

# Estimate Workspace

This is the most important screen.

Layout

Left Panel

Knowledge Explorer

Center

Estimate Editor

Right Panel

Properties

Bottom

Totals

History

Validation

AI Suggestions

The user should complete most work without changing pages.

---

# AI Experience

AI should assist, not interrupt.

Recommendations appear as suggestions.

Every recommendation includes

Reason

Confidence

Explanation

Suggested Action

The user always decides whether to accept or reject.

---

# Search Experience

Global Search

Ctrl + K

Search supports

Projects

Customers

Estimates

Construction Works

Materials

Commands

---

# Keyboard Shortcuts

Ctrl + N

New Estimate

Ctrl + S

Save

Ctrl + K

Search

Ctrl + P

Command Palette

Esc

Close Dialog

Del

Delete Selected Item

Keyboard support is considered a first-class feature.

---

# Responsive Design

Desktop

Primary platform

Tablet

Fully supported

Mobile

Read-only for MVP

Editing support planned for future versions.

---

# Accessibility

WCAG AA

Keyboard navigation

Focus indicators

High contrast

Screen reader support

Minimum touch target

44 × 44 px

---

# Animations

Animations should be subtle.

Maximum duration

250 ms

Use animations only to:

Guide attention

Show transitions

Confirm actions

Avoid decorative animations.

---

# Design Tokens

Colors

Typography

Spacing

Radius

Shadows

Transitions

Icons

All visual values should come from centralized design tokens.

---

# Theme

Light Theme

Dark Theme

System Theme

Theme preference stored per user.

---

# Future UX

Drag & Drop Estimate Builder

Multi-monitor support

Split View

Custom Workspaces

Custom Dashboards

Offline Mode

Real-time Collaboration

---

# Success Metrics

New user creates a project without training.

Estimate creation takes less than 10 minutes.

Most actions require no more than three clicks.

AI recommendations are understandable without technical knowledge.

The interface remains responsive with thousands of estimate items.

---

# Summary

The SmartEstimate interface is designed as a professional workspace rather than a traditional business application.

Every design decision should improve productivity, reduce user effort, and increase confidence in the estimating process.

The UI should remain clean, modern, and scalable as the platform evolves.

---

End of Document