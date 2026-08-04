# SmartEstimate

# 05. Knowledge Model

Version: 1.0

Status: Draft

Author: SmartEstimate Team

Last Updated: 2026-08-03

---

# Purpose

The Knowledge Model defines the construction expertise used by SmartEstimate.

Unlike the Domain Model, which describes business entities, the Knowledge Model represents professional construction knowledge.

The Knowledge Model is the primary source of truth for:

- Construction works
- Construction processes
- Materials
- Dependencies
- Recommendations
- Consumption rules
- Construction stages
- Best practices

Artificial Intelligence never invents construction knowledge.

AI only interprets and applies the knowledge stored in this model.

---

# Philosophy

SmartEstimate should understand construction similarly to an experienced estimator.

Instead of storing isolated construction works, the platform stores structured construction knowledge.

Knowledge must be:

- reusable;
- explainable;
- versioned;
- independent from AI models;
- independent from implementation.

## Operational Storage and Interchange

PostgreSQL is the single source of truth for operational knowledge. Knowledge Studio
creates, changes, archives, searches, and reads records through the backend API and
PostgreSQL repositories. A running application never loads its catalogue from YAML.

YAML remains a portable representation reserved for future import, export, backup,
reference catalogues, and catalogue exchange. Import and export must be behind
dedicated abstractions so parsing and file formats never leak into domain rules or
estimate workflows.

---

# Knowledge Hierarchy

Construction

└── Categories

&nbsp;&nbsp;&nbsp;&nbsp;└── Subcategories

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── Construction Processes

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── Construction Works

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── Dependencies

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── Materials

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── Consumption Rules

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;├── Recommendations

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└── Quality Rules

---

# Categories

The first version contains:

- Demolition
- Foundation
- Masonry
- Roof
- Facade
- Electrical
- Plumbing
- Heating
- Ventilation
- Floor
- Walls
- Ceiling
- Windows
- Doors
- Interior Finishing
- Exterior Finishing
- Landscaping
- Cleaning

Categories are hierarchical.

Example

Interior Finishing

↓

Walls

↓

Painting

---

# Construction Process

A Construction Process represents a complete sequence of works required to achieve a result.

Example

Wall Painting Process

↓

Surface Inspection

↓

Primer

↓

Plaster

↓

Primer

↓

Putty

↓

Sanding

↓

Dust Removal

↓

Painting

↓

Quality Inspection

Processes may be:

- Complete
- Partial
- Optional
- Template-based

---

# Construction Work

Construction Work is the smallest executable unit.

Each work contains:

Identifier

Name

Description

Category

Measurement Unit

Difficulty

Typical Duration

Average Crew Size

Construction Stage

Related Works

Dependencies

Required Materials

Consumption Rules

Safety Rules

Quality Rules

Typical Mistakes

Market Price Reference

---

# Construction Stages

The first version defines the following stages.

Planning

Preparation

Demolition

Structural Work

Electrical

Plumbing

HVAC

Wall Preparation

Ceiling

Floor

Painting

Finishing

Cleaning

Inspection

Completion

Every Construction Work belongs to one stage.

---

# Work Dependencies

Dependencies describe logical relationships between works.

Types:

Required

Recommended

Optional

Alternative

Mutually Exclusive

Conditional

Example

Painting

requires

Primer

Primer

recommended after

Plaster

Tiles

requires

Tile Adhesive

---

# Material Rules

Every work may define:

Required Materials

Optional Materials

Alternative Materials

Consumables

Protective Equipment

Tools

Example

Painting

Required Materials

- Paint

- Primer

Consumables

- Masking Tape

- Plastic Film

Tools

- Roller

- Brush

- Paint Tray

---

# Consumption Rules

Consumption Rules define expected material usage.

Examples

Paint

0.125 L / m²

Primer

0.1 L / m²

Putty

1.2 kg / m²

Tile Adhesive

4 kg / m²

Rules may depend on:

Surface type

Material brand

Application method

Wall condition

Layer thickness

---

# Recommendation Rules

Recommendation Rules are used by AI.

Example

IF

Painting exists

AND

Primer missing

THEN

Recommend Primer

Priority

High

---

IF

Tiles exist

AND

Grout missing

THEN

Recommend Grout

---

IF

Laminate exists

AND

Underlay missing

THEN

Recommend Underlay

---

Recommendations contain:

Reason

Priority

Confidence

Explanation

Reference Rule

---

# Quality Rules

Quality Rules describe mandatory conditions.

Example

Painting requires:

Dry surface

Primer applied

Surface sanded

Dust removed

Temperature above minimum

Humidity below maximum

---

# Difficulty Levels

Beginner

Standard

Advanced

Professional

Expert

---

# Regional Rules

Knowledge may vary by:

Country

Region

City

Example

Ukraine

↓

Kyiv

↓

Recommended market prices

↓

Typical materials

↓

Building standards

---

# Templates

Templates define common project types.

Examples

One-room apartment

Two-room apartment

Private house

Office

Bathroom renovation

Kitchen renovation

Facade insulation

Roof replacement

Templates contain predefined processes and recommended works.

---

# Knowledge Sources

Knowledge may originate from:

Construction standards

Experienced estimators

Company expertise

Industry best practices

Verified AI suggestions

Approved user contributions

Every source must be traceable.

---

# AI Integration

AI uses the Knowledge Model as context.

AI never creates construction rules.

AI may:

Explain rules

Suggest works

Recommend materials

Calculate estimates

Generate project drafts

Find missing works

Calculate material quantities

Explain pricing

---

# Knowledge Versioning

Knowledge evolves over time.

Every rule has:

Version

Created Date

Modified Date

Author

Status

Change History

Backward compatibility should be maintained whenever possible.

In the Knowledge Studio MVP, `Version` is an optimistic-concurrency token and
`CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`, and `Status` are persisted for
categories, works, materials, and units. A complete immutable revision history is a
future capability; it must use stable identifiers and an explicit approval flow.

---

# Future Extensions

Building regulations

National standards

Safety standards

Supplier catalogs

Manufacturer recommendations

Construction videos

Photo examples

Typical project timelines

Risk analysis

Cost prediction

Carbon footprint

BIM integration

---

# Guiding Principles

Knowledge is more valuable than code.

Knowledge is independent from AI.

Knowledge is independent from the database.

Knowledge must be explainable.

Knowledge must be reusable.

Knowledge must evolve continuously.

---

# Summary

The Knowledge Model represents the collective construction expertise of SmartEstimate.

It is the foundation for:

- AI recommendations
- Material calculations
- Estimate generation
- Process validation
- Construction guidance
- Future intelligent features

Every intelligent feature of SmartEstimate must rely on the Knowledge Model rather than hardcoded logic or assumptions made by AI.

---

End of Document
