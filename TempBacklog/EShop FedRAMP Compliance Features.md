# eShop FedRAMP Compliance Roadmap

## Context

eShop is a .NET 10 / .NET Aspire microservices reference app with:
- Blazor Server `WebApp` as the main UI.
- Microservices: `Catalog.API`, `Basket.API`, `Ordering.API`, `Identity.API`, `Webhooks.API`, background workers, RabbitMQ, Redis, and PostgreSQL (pgvector).
- Optional AI integrations (OpenAI / Azure OpenAI / Ollama).
- Aspire-based orchestration in `eShop.AppHost`.

This document outlines the top FedRAMP-related compliance concerns and a prioritized feature backlog to address them. It is intentionally environment-agnostic (cloud provider-neutral) and should be refined with your actual FedRAMP boundary, SSP, and CSP controls.

---

## Top 10 FedRAMP-Related Compliance Concerns

1. **Identity & Access Management (IAM) Hardening**
   - **Observation:** `Identity.API` issues relatively long-lived tokens (`TokenLifetimeMinutes` = 120, `PermanentTokenLifetimeDays` = 365). There is no explicit mention of MFA, privileged access segregation, or administrative workflow controls in the repo.
   - **FedRAMP Areas:** AC-2, AC-6, IA-2, IA-5, AC-7.
   - **Concern:** Token/session lifetimes and lack of explicit MFA/RBAC baselines may not meet FedRAMP High/Moderate expectations without additional controls (e.g., short token lifetimes, enforced MFA, and strong administrative separation).

2. **Secrets Management & Configuration Hygiene**
   - **Observation:** Local `appsettings.json` files contain connection strings (e.g., `ConnectionStrings.EventBus = "amqp://localhost"`; Redis/Postgres endpoints configured directly). Aspire `Program.cs` uses Docker images with inline configuration.
   - **FedRAMP Areas:** SC-12, SC-13, IA-5, CM-6.
   - **Concern:** For production/FedRAMP-authorized deployments, secrets and sensitive config (DB passwords, RabbitMQ credentials, API keys) must be managed via a FedRAMP-compliant secret store with rotation, auditing, and strict access controls—not via configuration files or environment variables without governance.

3. **Encryption in Transit (TLS Everywhere)**
   - **Observation:** Development configs and identity URLs use HTTP (`Identity:Url = "http://localhost:5223"`). Aspire wiring does not show TLS enforcement for intra-service communication, RabbitMQ, Redis, or Postgres.
   - **FedRAMP Areas:** SC-8, SC-12, SC-13.
   - **Concern:** Production traffic (external and internal) must be protected via TLS 1.2+ (or 1.3), strong ciphers, and certificate management, including for message bus and data-store connections. Non-TLS paths must be disabled outside of dev.

4. **Encryption at Rest & Key Management**
   - **Observation:** Postgres (with `ankane/pgvector:latest`) and other data stores are configured as containers. No explicit at-rest encryption configs or key management are visible in code.
   - **FedRAMP Areas:** SC-28, MP-6, SC-12, SC-13.
   - **Concern:** Customer and operational data (orders, user identities, logs, vector embeddings) must be encrypted at rest with keys managed by a FedRAMP-authorized KMS/HSM solution. Containerized dev images using `latest` are not adequate as-is for FedRAMP environments.

5. **Audit Logging, Traceability, and Log Integrity**
   - **Observation:** Basic logging is configured (`Logging.LogLevel`), but there’s no explicit audit logging layer (e.g., “who did what, when, and from where”) or cross-service correlation strategy described.
   - **FedRAMP Areas:** AU-2, AU-3, AU-6, AU-8, AU-9.
   - **Concern:** FedRAMP requires comprehensive, tamper-evident audit logs for security-relevant events (authentication, authorization changes, webhook registrations, payment events, admin actions) with centralized collection, retention, and alerting.

6. **Data Minimization, PII/PHI Handling, and DLP**
   - **Observation:** The app handles customer data (orders, baskets, payments, webhooks). AI integrations (OpenAI/Ollama) can potentially process user/product data. No explicit data classification, log scrubbing, or DLP mechanisms are evident.
   - **FedRAMP Areas:** PL-2, MP-5, SC-7, SC-13, AR/Privacy overlays where applicable.
   - **Concern:** PII (and any sensitive transaction data) must be classified, masked/minimized in logs, and subject to retention limits and deletion workflows (right to delete per policy and contract). AI endpoints must not receive sensitive data without explicit risk acceptance and SSP coverage.

7. **Boundary Protection, Egress Control, and Webhook Security**
   - **Observation:** `Webhooks.API` and `WebhookClient` enable outbound calls to arbitrary webhook endpoints. Optional AI integrations might call external services. External HTTP calls are not obviously constrained.
   - **FedRAMP Areas:** SC-7, SC-7(12), SC-7(5), CA-3.
   - **Concern:** FedRAMP boundaries require strong ingress/egress control (allowlists, proxies, WAFs) and robust webhook security (HMAC signatures, IP allowlists, rate limiting, replay protection) with config under change control.

8. **Vulnerability Management & Container/Image Hardening**
   - **Observation:** Aspire uses containers like `ankane/pgvector:latest`. There’s no explicit pinned versions or hardened images documented here.
   - **FedRAMP Areas:** RA-5, SI-2, CM-2, CM-6, SI-7.
   - **Concern:** FedRAMP requires continuous vulnerability scanning (SCA, container scans), patch management, and hardened baselines. `latest` tags and non-FIPS-validated images are red flags without additional compensating controls.

9. **Backup, Recovery, and Business Continuity**
   - **Observation:** Persistent lifetime is configured for Postgres and RabbitMQ, but documentation around backups, DR, and recovery procedures is not evident.
   - **FedRAMP Areas:** CP-2, CP-6, CP-7, CP-9.
   - **Concern:** FedRAMP requires documented, tested backup and recovery procedures (RPO/RTO), including encrypted backups, access controls, and recovery testing for all in-scope data stores.

10. **Environment Separation, Configuration Management, and Change Control**
    - **Observation:** The repo targets local/developer experiences via Aspire. There’s no explicit split shown here for dev/test/stage/prod configurations, change approval workflows, or infrastructure-as-code definitions for FedRAMP environments.
    - **FedRAMP Areas:** CM-2, CM-3, CM-5, CM-6, SA-10.
    - **Concern:** FedRAMP demands strict separation of environments, baseline configurations, and controlled promotion paths. Configuration drift, ad-hoc changes, and manual infra management are non-compliant.

---

## Prioritized Feature Backlog (FedRAMP-Focused)

Below are recommended features to prioritize. Each item links to the concerns above and includes risks, rollback notes, and FedRAMP considerations.

### F1 – FedRAMP IAM Baseline for `Identity.API` and WebApp

- **Maps to Concerns:** 1, 10
- **Goal:** Align authentication and authorization with FedRAMP expectations: shorter token lifetimes, enforced MFA (via external IdP if applicable), role-based access control (RBAC), and secure session management in `WebApp`.
- **Scope (high level):**
  - Introduce configuration-driven token lifetimes aligned with policy (e.g., ≤30–60 min access tokens, short-lived refresh tokens).
  - Support MFA hooks and integration patterns with a FedRAMP-approved IdP.
  - Define and enforce RBAC for admin vs. user vs. service accounts.
  - Harden session cookie settings (`Secure`, `HttpOnly`, `SameSite`, idle timeout) in `WebApp`.
- **Risks:**
  - Breaking existing login flows or automated tests; user friction due to shorter sessions.
- **Rollback Strategy:**
  - Feature-flag new lifetimes and RBAC policies.
  - Ability to revert to previous token lifetimes via configuration only.
- **FedRAMP Considerations:**
  - Document alignment with AC-2, AC-6, IA-2, and IA-5 in the SSP.
  - Ensure logs capture failed/successful MFA attempts for AU controls.

### F2 – Centralized Audit Logging & Security Event Model

- **Maps to Concerns:** 5, 1, 7
- **Goal:** Implement a cross-service audit logging pattern with structured events that feed into a SIEM/SOC pipeline.
- **Scope:**
  - Define a unified “security event” schema (user ID, subject, action, resource, outcome, timestamp, correlation ID).
  - Instrument key flows: logins, role changes, webhook registrations, order/payment changes, configuration updates.
  - Configure shipping to a centralized log collector with integrity controls.
- **Risks:**
  - Log volume and performance impacts; risk of logging sensitive data if not properly designed.
- **Rollback Strategy:**
  - Wrap audit logging in a library and toggle via configuration.
  - Ability to downgrade event volume (from “verbose” to “essentials”) without code change.
- **FedRAMP Considerations:**
  - Map events to AU-2, AU-6; define retention and access restrictions.
  - Ensure time synchronization (AU-8) via underlying platform (e.g., NTP).

### F3 – Secretless Configuration and KMS-backed Secrets

- **Maps to Concerns:** 2, 4, 10
- **Goal:** Replace static secrets in config files with references to a FedRAMP-authorized secret manager backed by KMS/HSM keys.
- **Scope:**
  - Introduce a secret provider abstraction for connection strings and credentials (Postgres, RabbitMQ, Redis, AI providers).
  - Configure environment-specific secret stores (e.g., Azure Key Vault / AWS Secrets Manager / GCP Secret Manager) outside of code.
  - Add rotation playbooks for DB, RabbitMQ, and app secrets.
- **Risks:**
  - Misconfiguration that breaks connectivity; secret store outages impacting app availability.
- **Rollback Strategy:**
  - Support dual-path configuration: secret store first, fallback to environment variables or existing settings in non-FedRAMP environments.
  - Gradual migration service-by-service.
- **FedRAMP Considerations:**
  - Ensure KMS is FedRAMP-authorized; define key rotation intervals.
  - Document SC-12, SC-13 implementations in SSP.

### F4 – End-to-End TLS Enforcement (Internal and External)

- **Maps to Concerns:** 3, 7, 10
- **Goal:** Require TLS 1.2+ (or 1.3) for all external and internal API endpoints, message bus, caches, and databases.
- **Scope:**
  - Configure HTTPS-only endpoints on all ASP.NET Core services; disable HTTP in non-dev environments.
  - Require TLS for RabbitMQ, Redis, and Postgres connections with certificate validation.
  - Establish a certificate management strategy (ACME/PKI integration, rotation).
- **Risks:**
  - Certificate misconfiguration causing outages; integration issues with legacy clients.
- **Rollback Strategy:**
  - Phase-in via environment flags (dev/test first, then staging, then prod).
  - Preserve HTTP-only in dedicated dev profiles while locking production to TLS.
- **FedRAMP Considerations:**
  - Use FIPS-validated crypto libraries and FedRAMP-approved ciphers.
  - Align with SC-8 and SC-13 requirements and document in SSP.

### F5 – Data Classification, PII Redaction, and Safe Logging

- **Maps to Concerns:** 6, 5
- **Goal:** Implement a data classification matrix and a reusable PII/secret redaction library used by all services.
- **Scope:**
  - Define data classes (public / internal / confidential / regulated).
  - Introduce logging helpers that automatically scrub PII (emails, names, addresses, payment tokens) and secrets.
  - Add unit tests to ensure no PII is emitted in logs for key paths.
- **Risks:**
  - Undetected PII paths if coverage is incomplete; debugging harder if logs become too sparse.
- **Rollback Strategy:**
  - Support diagnostic modes to temporarily relax masking in non-prod with strict access control.
- **FedRAMP Considerations:**
  - Align with data handling strategies in the SSP; reference AU and SC families.
  - Ensure production access to unmasked data is restricted and audited.

### F6 – Webhook and Egress Security Hardening

- **Maps to Concerns:** 7, 3, 10
- **Goal:** Secure webhook subscriptions and outbound calls, aligned with strict boundary protection requirements.
- **Scope:**
  - Require HMAC signatures or JWT-based verification for incoming webhook calls.
  - Support IP/domain allowlists for outbound webhook targets.
  - Rate limiting and replay protection for inbound webhook endpoints.
- **Risks:**
  - Breaking existing integration patterns; partner friction if they must update implementations.
- **Rollback Strategy:**
  - Feature-flag signature enforcement and allow legacy mode for a limited period.
- **FedRAMP Considerations:**
  - Map to SC-7 and CA-3; ensure egress rules are enforced at the network layer as well (e.g., firewalls, proxies).

### F7 – Container & Image Hardening + SBOM

- **Maps to Concerns:** 8, 4
- **Goal:** Harden all container images and dependencies; provide SBOMs and integrate vulnerability scanning into CI.
- **Scope:**
  - Replace `latest` tags (e.g., `ankane/pgvector:latest`) with pinned, hardened, FedRAMP-acceptable images.
  - Generate SBOMs (e.g., SPDX/CycloneDX) for services and containers.
  - Add automated SCA and container scanning in CI/CD with policy gates.
- **Risks:**
  - Build pipeline failures until vulnerabilities are resolved; additional maintenance overhead.
- **Rollback Strategy:**
  - Introduce scanning in “report-only” mode first; then enforce block-on-fail after baselines are met.
- **FedRAMP Considerations:**
  - RA-5 and SI-2: Document vulnerability management cadence and exception handling.
  - Ensure base images meet FIPS and CIS baseline requirements where applicable.

### F8 – Backup, Restore, and Data Retention Policy Implementation

- **Maps to Concerns:** 9, 6
- **Goal:** Formalize backup, restore, and data retention behavior for all in-scope data stores.
- **Scope:**
  - Define backup frequency and retention for Postgres databases (catalog, identity, ordering, webhooks) and any additional stateful components.
  - Implement and document restore procedures; execute periodic restore tests.
  - Implement data retention and purge jobs for logs and PII in line with policy.
- **Risks:**
  - Incorrect purge logic causing data loss; backup storage costs.
- **Rollback Strategy:**
  - Start with non-destructive archiving; enable hard deletes only after validation.
- **FedRAMP Considerations:**
  - Map to CP-2, CP-6, CP-9; ensure backups are encrypted and access-controlled.

### F9 – Environment Separation, Configuration Baselines, and Change Control

- **Maps to Concerns:** 10, 2, 8
- **Goal:** Codify environment-specific baselines (dev/test/stage/prod), including Aspire configuration, infra, and application settings under change control.
- **Scope:**
  - Introduce IaC (if not already in use externally) for FedRAMP environments.
  - Clearly separate dev/test configs from production (no shared credentials or endpoints).
  - Add release pipelines with approvals and audit trails.
- **Risks:**
  - Additional process overhead; misalignment between IaC and existing manual infra.
- **Rollback Strategy:**
  - Pilot IaC and strict baselines on a non-critical environment first.
- **FedRAMP Considerations:**
  - Align with CM-2, CM-3, CM-5, CM-6, SA-10.
  - Ensure each environment has documented purpose, data classification, and access controls.

### F10 – AI Integration Guardrails (OpenAI / Azure OpenAI / Ollama)

- **Maps to Concerns:** 6, 7, 2
- **Goal:** Ensure AI integrations operate within FedRAMP boundaries and do not leak sensitive data.
- **Scope:**
  - Make AI features opt-in, disabled by default in FedRAMP-authorized environments.
  - Introduce an allowlist of models and providers that comply with your FedRAMP boundary.
  - Implement prompt/response scrubbing and logging controls (no PII in prompts, limited logging in production).
- **Risks:**
  - Reduced functionality if AI-assisted features are restricted; complexity in prompt redaction.
- **Rollback Strategy:**
  - Gate AI usage behind a global feature flag and environment checks.
- **FedRAMP Considerations:**
  - Confirm whether AI services are in-scope for the ATO; adjust SSP, data flow diagrams, and privacy assessments accordingly.

---

## Overall Risks and Assumptions

- **Assumptions:**
  - The current repo primarily reflects a reference architecture and development environment, not a hardened FedRAMP production deployment.
  - You will deploy to a FedRAMP-authorized CSP that provides baseline controls (e.g., physical security, underlying OS patching, FIPS crypto).
- **Key Risks:**
  - Underestimating the time and organizational coordination required (security, compliance, ops, development).
  - Partial implementation of features without process updates (e.g., no SOPs or runbooks) will not satisfy auditors.

---

## Rollback & Feature-Flag Strategy (Cross-Cutting)

For all features above:

- Use **config-driven toggles** and **feature flags** to:
  - Enable new security features in dev/test first.
  - Gradually roll out to staging and then production.
  - Provide a controlled rollback path without code changes.
- Maintain **versioned configuration baselines** and **change records** so that:
  - Each change is traceable with a ticket, approval, and implementation details.
  - You can revert to a previous known-good baseline in case of issues.

---

## Next Steps

1. **Prioritize F1–F4** as immediate security hardening wins (IAM, audit logs, secrets, TLS).
2. **Plan F5–F7** in parallel with CI/CD and infra teams (logging, DLP, containers).
3. **Schedule F8–F10** with compliance and architecture stakeholders to align with SSP updates and ATO timelines.
4. **Update Documentation:** As each feature ships, update your FedRAMP SSP, diagrams, and runbooks.
