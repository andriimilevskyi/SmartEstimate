# SmartEstimate

# 10. AI Platform Architecture

Version: 1.0

Status: Draft

Author: SmartEstimate Team

Last Updated: 2026-08-03

---

# Purpose

This document defines the architecture of the SmartEstimate Artificial Intelligence Platform.

The AI Platform provides intelligent assistance for construction professionals while remaining independent from business logic and user interface implementation.

Artificial Intelligence never owns business data.

Artificial Intelligence assists decision making.

The backend remains the single source of truth.

---

# Vision

The AI Platform is not a chatbot.

It is an intelligent construction assistant.

Its purpose is to:

- reduce repetitive work;
- increase estimate quality;
- explain recommendations;
- automate calculations;
- improve productivity.

The user always makes the final decision.

---

# AI Design Principles

AI is an assistant.

AI never replaces the user.

AI never modifies business data directly.

AI always explains recommendations.

AI is replaceable.

AI providers are interchangeable.

Knowledge is more important than AI.

Construction expertise must remain outside LLMs.

---

# High Level Architecture

                    Frontend

                        │

                        ▼

              ASP.NET Core Backend

                        │

                        ▼

                  AI Gateway

        ┌────────┬────────┬────────┐

        │        │        │        │

   Knowledge   Agents   Memory  Providers

                        │

          ┌─────────────┼─────────────┐

          │             │             │

      OpenAI        Claude      Local Models

---

# AI Responsibilities

The AI Platform is responsible for:

Natural Language Understanding

Construction Reasoning

Estimate Draft Generation

Market Analysis

Material Calculation

Recommendation Generation

Voice Processing

Document Analysis

Future Predictive Analytics

---

# AI Gateway

The AI Gateway is the single entry point.

Responsibilities

Model Selection

Prompt Routing

Authentication

Rate Limiting

Logging

Retry Logic

Fallback Strategy

Cost Tracking

Monitoring

The rest of the system never communicates directly with AI providers.

---

# AI Providers

Supported Providers

OpenAI

Anthropic Claude

Google Gemini

Azure OpenAI

Ollama

Future Local Models

Provider selection must be configurable.

No provider-specific logic should exist outside the AI Gateway.

---

# Knowledge Adapter

The Knowledge Adapter connects AI with the Knowledge Model.

Responsibilities

Load construction rules

Load dependencies

Load templates

Load consumption rules

Load recommendations

Provide context to AI

AI should reason using structured knowledge rather than relying solely on model memory.

---

# Prompt Library

All prompts are centrally managed.

Categories

Estimate Generation

Market Analysis

Material Calculation

Recommendation

Voice Processing

OCR

Translation

Prompt versioning is required.

Prompts must never be hardcoded inside business logic.

---

# AI Memory

AI sessions maintain contextual information.

Memory Types

Conversation Memory

Project Context

Estimate Context

Knowledge Context

Temporary Session Context

Long-term business data is never stored in AI memory.

---

# AI Agents

SmartEstimate uses specialized agents.

Each agent has a single responsibility.

---

## Estimate Agent

Purpose

Generate estimate drafts.

Input

Project description

Output

Draft estimate

---

## Price Agent

Purpose

Analyze market prices.

Input

Construction works

Region

Current company prices

Output

Price recommendations

Price explanation

Confidence score

---

## Material Agent

Purpose

Calculate materials.

Input

Construction works

Measurements

Knowledge rules

Output

Material list

Consumption

Waste

Estimated cost

---

## Recommendation Agent

Purpose

Validate estimates.

Example

Painting exists

Primer missing

↓

Recommendation

Add Primer

---

## Voice Agent

Purpose

Speech-to-estimate.

Pipeline

Speech

↓

Transcription

↓

Intent Detection

↓

Construction Understanding

↓

Estimate Draft

---

## OCR Agent

Purpose

Extract information from

PDF

Photos

Drawings

Invoices

Future

Construction plans

---

# AI Request Lifecycle

User Action

↓

Backend

↓

AI Gateway

↓

Knowledge Context

↓

Prompt Builder

↓

AI Provider

↓

Response Validator

↓

Backend

↓

User

---

# Prompt Builder

Prompt Builder combines:

System Prompt

Knowledge Context

Project Context

User Request

Formatting Rules

Output Schema

This ensures consistent AI responses.

---

# Structured Output

Whenever possible, AI returns structured JSON.

Example

Estimate Draft

Material List

Recommendations

Price Analysis

Free text should be avoided for business operations.

---

# Confidence

Every recommendation includes:

Confidence Score

Reason

Knowledge References

AI Explanation

Users should understand why a recommendation was generated.

---

# Safety

AI must never:

Delete data

Modify prices automatically

Approve estimates

Create invoices

Send emails

Execute business actions

AI may only recommend.

---

# Logging

Every AI interaction logs:

Timestamp

Provider

Model

Execution Time

Cost

Token Usage

Success

Failure

TraceId

Sensitive prompts must never be logged.

---

# Cost Control

Track

Input Tokens

Output Tokens

Estimated Cost

Provider

Model

Future

Daily Limits

Monthly Budgets

Company Quotas

---

# Monitoring

Monitor

Latency

Availability

Failure Rate

Hallucination Reports

Recommendation Acceptance Rate

Model Usage

Cost

---

# Model Selection

Different tasks may use different models.

Example

Voice

↓

Whisper

Market Analysis

↓

GPT-5

OCR

↓

Specialized OCR Model

Recommendations

↓

Smaller Local Model

The architecture must support intelligent model routing.

---

# Fallback Strategy

If the preferred provider fails:

Retry

↓

Alternative Provider

↓

Local Model

↓

User Notification

Business operations should continue whenever possible.

---

# Future AI Features

Image Analysis

Blueprint Understanding

Construction Timeline Prediction

Risk Detection

Cost Forecasting

Supplier Recommendation

Automatic Specification Generation

Project Assistant

Conversation Memory

Multi-Agent Collaboration

---

# Guiding Principles

Knowledge before AI.

AI before Automation.

Recommendations before Decisions.

Explain before Suggest.

Human always approves.

Business logic never depends on a specific AI provider.

---

# Success Criteria

AI reduces estimate preparation time.

AI recommendations are explainable.

Providers are interchangeable.

Knowledge remains the primary source of expertise.

The AI Platform can evolve independently from the backend.

---

# Summary

The SmartEstimate AI Platform is a modular intelligence layer that enhances construction estimating through specialized AI agents, structured construction knowledge, and provider-independent architecture.

The platform is designed to evolve continuously while keeping business logic stable, transparent, and fully controlled by the user.

---

End of Document