# FedRAMP Findings – eShop

## Context

eShop is a .NET 10 / .NET Aspire microservices reference app with:
- Blazor Server `WebApp` as the main UI.
- Microservices: `Catalog.API`, `Basket.API`, `Ordering.API`, `Identity.API`, `Webhooks.API`, background workers, RabbitMQ, Redis, and PostgreSQL (pgvector).
- Optional AI integrations (OpenAI / Azure OpenAI / Ollama).

This document summarizes the top FedRAMP-related compliance concerns observed from the codebase and configuration.

---

## Top 10 FedRAMP-Related Compliance Concerns

1. **IAM Hardening (Identity & WebApp)**
   - Long-lived tokens (`TokenLifetimeMinutes = 120`, `PermanentTokenLifetimeDays = 365`) and no explicit MFA/RBAC baseline.
   - FedRAMP Areas: AC-2, AC-6, IA-2, IA-5, AC-7.
   - Risk: Sessions and administrative access may not meet FedRAMP expectations without additional controls.

2. **Secrets Management & Configuration Hygiene**
   - Connection strings and service URLs present in `appsettings.json` and Aspire configuration.
   - FedRAMP Areas: SC-12, SC-13, IA-5, CM-6.
   - Risk: Secrets may be exposed or unmanaged in production if not moved to a FedRAMP-compliant secret store.

3. **Encryption in Transit (TLS Everywhere)**
   - HTTP (non-TLS) used for internal URLs (e.g., `Identity:Url = "http://localhost:5223"`), and no explicit TLS enforcement for RabbitMQ, Redis, Postgres.
   - FedRAMP Areas: SC-8, SC-12, SC-13.
   - Risk: Data-in-transit protection may be insufficient without mandated TLS 1.2+ and strong ciphers.

4. **Encryption at Rest & Key Management**
   - Postgres/pgvector and other stateful services run as containers with no explicit at-rest encryption or KMS integration visible.
   - FedRAMP Areas: SC-28, SC-12, SC-13, MP-6.
   - Risk: Sensitive data (identities, orders, embeddings) may not be encrypted at rest with FedRAMP-approved key management.

5. **Audit Logging, Traceability, and Log Integrity**
   - Basic logging exists, but no unified audit logging model for security events across services is evident.
   - FedRAMP Areas: AU-2, AU-3, AU-6, AU-8, AU-9.
   - Risk: Inability to reliably trace security-relevant events, detect anomalous behavior, and support incident investigations.

6. **Data Minimization, PII Handling, and DLP**
   - Services process customer and order data; AI integrations may process product/user-related information.
   - FedRAMP Areas: PL-2, MP-5, SC-7, SC-13 (and privacy overlays).
   - Risk: PII/regulated data may be logged, retained indefinitely, or sent to external AI services without clear controls.

7. **Boundary Protection, Egress Control, and Webhook Security**
   - `Webhooks.API` and `WebhookClient` enable outbound calls; optional AI calls external providers.
   - FedRAMP Areas: SC-7, SC-7(12), SC-7(5), CA-3.
   - Risk: Unconstrained egress and weak webhook validation can expand the authorization boundary and attack surface.

8. **Vulnerability Management & Container/Image Hardening**
   - Containers use tags like `ankane/pgvector:latest`; no explicit hardened images or SBOMs are documented here.
   - FedRAMP Areas: RA-5, SI-2, CM-2, CM-6, SI-7.
   - Risk: Lack of version pinning and scanning can leave known vulnerabilities unaddressed.

9. **Backup, Recovery, and Data Retention**
   - Persistent lifetimes configured in Aspire, but backup/restore procedures and retention policies are not described in code.
   - FedRAMP Areas: CP-2, CP-6, CP-7, CP-9.
   - Risk: Gaps in disaster recovery, restore testing, and data retention/purge could violate FedRAMP expectations.

10. **Environment Separation, Configuration Baselines, and Change Control**
    - Repo is optimized for local/Aspire dev; environment-specific FedRAMP baselines and IaC definitions are not visible here.
    - FedRAMP Areas: CM-2, CM-3, CM-5, CM-6, SA-10.
    - Risk: Without strict environment separation and change control, production environments may drift and become non-compliant.

---

## Summary

These findings highlight where additional design, configuration, and process work is required to align eShop with FedRAMP requirements. The companion features backlog document outlines concrete remediation work.