# Project-Isotyp

## Overview

**Project-Isotyp** is an enterprise-grade **Data Architecture & Onboarding Platform** designed to help teams ingest, understand, govern, and safely evolve data over time.

Unlike traditional ETL tools, Project-Isotyp treats data architecture as a **living, versioned system**. It builds a canonical understanding of data meaning and structure, continuously re-validates that understanding as data evolves, and enables deliberate, human-approved changes to schemas and application models with full auditability and guaranteed rollback.

Project-Isotyp does **not** autonomously mutate production systems. Instead, it empowers architects, developers, and operators to evolve data safely and intentionally.

---

## Core Problem

Modern systems face recurring challenges when onboarding and evolving data:

* Data arrives from heterogeneous sources with unclear or shifting semantics
* One-time migrations create brittle schemas that do not age well
* Schema drift accumulates silently until it causes production issues
* Application ORM models and databases fall out of sync
* Automated systems are distrusted because rollback and accountability are unclear

Project-Isotyp addresses these problems by making data understanding, governance, and evolution **explicit, explainable, and reversible**.

---

## What Project-Isotyp Is (and Is Not)

### ✅ What It Is

* A platform for **canonical data understanding**
* A governed, versioned system for **data model evolution**
* A bridge between **raw data ingestion and production-ready schemas**
* A safety-first system where **AI advises and humans decide**

### ❌ What It Is Not

* Not a low-code ETL builder
* Not a self-healing or auto-mutating schema tool
* Not an analytics or BI platform
* Not a one-time migration utility

ETL is a **subsystem**, not the product.

---

## Core Principles

Project-Isotyp is built on the following non-negotiable principles:

1. **Human authority always wins**
2. **Nothing changes without explicit approval**
3. **Rollback must always be possible**
4. **Models evolve; data is never destroyed by default**
5. **AI advises; humans decide**
6. **Stability is preferable to cleverness**
7. **Trust is more important than automation**

Any feature that violates these principles does not ship.

---

## High-Level Architecture

Project-Isotyp consists of two major planes:

### 1. Local Execution Plane (Customer Network)

A **local agent** runs entirely inside the customer’s environment and is responsible for:

* Connecting to local data sources (databases, files)
* Performing schema discovery and data profiling
* Executing ETL jobs locally
* Collecting runtime statistics

**Security guarantees:**

* Secrets never leave the agent
* Raw data never leaves the network
* Communication is outbound-only

### 2. Control Plane (Portal)

The control plane:

* Builds canonical data model snapshots
* Tracks versions and timelines of understanding
* Performs drift detection and semantic analysis
* Manages approvals, locks, and governance
* Orchestrates safe execution on local agents

---

## Canonical Data Understanding

Each execution produces a **Model Snapshot**, which represents Project-Isotyp’s current understanding of the data.

A snapshot includes:

* Entities and relationships
* Cardinalities and constraints
* Semantic interpretations
* Confidence scores
* Explicit reasoning and assumptions

Rules:

* Snapshots are immutable
* Snapshots are versioned
* Snapshots represent hypotheses, not absolute truth

Snapshots form a **timeline of architectural understanding**.

---

## Continuous Re-Validation & Learning

As data changes over time, Project-Isotyp:

* Re-profiles datasets via the local agent
* Compares new profiles against prior snapshots
* Detects semantic drift (cardinality shifts, growth patterns, mutation frequency)
* Produces **recommendations for review**, not automatic actions

Learning is:

* Stability-biased
* Memory-aware
* Resistant to noisy fluctuations

---

## Semantic Optimization (Model-Level, Not Just Performance)

Project-Isotyp can suggest modeling improvements such as:

* Normalization or denormalization
* Entity extraction or consolidation
* Reference and lookup modeling
* Temporal pattern modeling
* Derived vs stored attributes
* Read vs write model separation

Every recommendation includes:

* Supporting evidence
* Tradeoffs and risks
* Expected impact

All suggestions respect user-defined locks and invariants.

---

## Governance, Locks & Approvals

### Locks

Users can lock:

* Structural evolution
* Semantic interpretation
* Storage behavior
* Entire entities or schemas

Locked segments:

* Are never reinterpreted
* Never receive evolution or optimization suggestions

### Multi-Layer Approvals

Changes require explicit approvals, which may include:

1. Data architecture approval
2. Application / ORM impact approval
3. Operational / DBA approval
4. Final execution approval

No approval layer can be bypassed.

---

## Schema, ORM & Application Alignment

When changes are approved:

* Database migrations are generated
* ORM models are updated (initially **PostgreSQL + EF Core**)
* Additive evolution is the default
* Breaking changes require explicit opt-in

Schema and application models evolve **together**, not independently.

---

## Rollback & Safety

Rollback is a first-class capability:

* Every change is versioned
* Rollback restores the last known good version
* Data integrity is preserved
* Destructive operations are avoided by default

If rollback is unsafe or ambiguous, the change does not execute.

---

## Auditability & Compliance

Project-Isotyp maintains a complete audit trail:

* Who proposed a change
* Who approved or rejected it
* What evidence was considered
* What version is currently active

AI is always recorded as an **advisory source**, never as the actor.

---

## Exit Strategy (No Platform Lock-In)

Customers can:

* Export final database schemas
* Export ORM models
* Export migration history
* Continue operating without Project-Isotyp at runtime

Project-Isotyp does not impose a hard dependency in production.

---

## Initial Scope (v1)

To ensure safety and focus, v1 is intentionally constrained:

* PostgreSQL only
* EF Core ORM only
* Additive schema evolution
* Conservative learning thresholds

Generalization will be driven by real usage, not speculation.

---

## Project Status

Project-Isotyp is currently in **active design and early development**.

The focus is on correctness, safety, and trust — not rapid feature expansion.

---

## Vision

Project-Isotyp aims to become the **system of record for how data architecture is allowed to evolve** — safely, deliberately, and reversibly.

> Data should change with intent, not accident.
