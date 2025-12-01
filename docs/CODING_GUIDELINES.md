# Coding Guidelines

## General Principles
- Favor readability and explicitness; avoid magic values and implicit behavior.
- Keep services small and cohesive; follow single-responsibility principles per project/namespace.
- Prefer composition over inheritance; embrace record types and immutability where possible.

## .NET Specific
- Target **.NET 8** with nullable reference types enabled and `ImplicitUsings` turned on where appropriate.
- Enforce async-first APIs; avoid sync-over-async. Use `CancellationToken` on public async methods.
- Validate inputs early with guard clauses; return typed results (one-of/Result types) instead of exceptions for flow control.
- Use dependency injection for all services; avoid static state and singletons except for pure stateless helpers.
- Use `ILogger<T>` with structured logging; never log secrets or PII. Prefer log scopes for correlation IDs.
- Employ analyzers (StyleCop/FXCop/IDisposable) and treat warnings as errors in CI.
- Organize files by feature (vertical slices) inside each service project to align with microservice boundaries.

## API & Contracts
- REST: Plural nouns, resource-oriented URLs, pagination/filtering standards, RFC7807 problem details for errors.
- gRPC: Versioned `.proto` files with explicit backward-compatibility rules; avoid breaking field reordering.
- Events: Protobuf/JSON with envelope metadata (correlation/causation IDs, tenant, schema version). Enforce idempotency keys.

## Testing & Quality
- Unit tests for domain and application layers; contract tests for service boundaries; integration tests for data access.
- Use test data builders and fixtures; avoid shared mutable state. Prefer deterministic tests with in-memory fakes when possible.
- Include load/performance benchmarks (BenchmarkDotNet) for critical code paths.

## Security & Compliance
- Apply input validation and output encoding; default-deny authorization policies.
- Secrets via configuration providers (Key Vault/Secrets Manager); no secrets in code or config files.
- Ensure PII redaction in logs and events. Follow least-privilege for DB and message broker credentials.

## Repository Hygiene
- Require code owners and PR templates; gated merges with checks. Keep `main` releasable.
- Use conventional commits; prefer small, focused PRs with good descriptions and traceability to roadmap items.
