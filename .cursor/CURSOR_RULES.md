Cursor rules

Scope
- Intended for the Cursor code assistant used by the project team.

Rules
1. Verbosity
   - Keep messages concise and actionable.
   - Avoid long-form prose unless requested.

2. Markdown
   - Do not create a markdown summary unless explicitly requested as a final prompt.

3. Certainty
   - Avoid absolute claims. Present solutions with appropriate caveats and alternative suggestions.

4. Personas
   - Include perspectives from the following personas when relevant: developer, architect, product owner, quality engineer, UX (accessibility-focused).

5. FedRAMP compliance
   - Ensure all interactions with language models follow FedRAMP restrictions. Avoid exposing sensitive data and follow organizational policies.
   - Treat any fields or entities marked as confidential or regulated in the codebase's data classification as sensitive by default. Do not emit these values into logs, test data, or AI prompts. Treat vector embeddings derived from such data as sensitive as well.
   - Prefer patterns that use existing data-classification and masking/redaction helpers; do not suggest logging or printing fields marked as sensitive or regulated.
   - When generating or modifying domain or data models, apply the existing data-classification scheme to new fields and entities (for example, public / internal / confidential / regulated) and, where appropriate, update any central data-classification documentation.

6. Database files
   - Do not examine database files or folders when recommending code changes.
   - Do not suggest changes that would alter the database schema.

7. Tooling and frameworks
   - Prefer guidance aligned with workspace characteristics (e.g., .NET 10, .NET MAUI, Blazor, Razor Pages).

8. When uncertain
   - Clearly state any uncertainty and provide steps for verification or further investigation.

9. Logging and PII
   - When suggesting logging or telemetry, never include secrets, tokens, passwords, or raw PII (such as full names, emails, addresses, payment details, or full webhook payloads).
   - Prefer structured logs that reference non-sensitive identifiers and outcomes (for example, correlation IDs, order IDs, or status codes) and assume masking/redaction helpers are in place.

System rules
- Restrict model access by role and environment (dev/test/prod); require MFA for privileged access.
- Automatically block or redact secrets, keys, and PII/CUI from prompts sent to external models.
- Centralize and apply redaction before logging or sending any text to models.
- Store AI interaction logs in restricted, compliance-ready storage and enforce retention policies.

Behavior rules
- State a confidence level and list key assumptions for every recommendation.
- Cite file paths or docs used to form suggestions when applicable.
- Prefer smallest, reversible code changes and include rollback steps.
- Surface edge cases, validation needs, and failure modes.

Operational rules
- Require PRs for all changes; PRs must include tests, a brief risk assessment, and a docs update when relevant.
- Enforce automated static analysis, security scans, and license checks on CI for every PR.
- Label any LLM-generated content in PR descriptions and require a manual review before merge.
- Require approvals from code owners for security-sensitive or infra changes.

Implementation rules
- Require unit and integration tests for behavior changes and include coverage targets for modified areas.
- Prohibit direct database schema changes without a migration plan and DBA sign-off.
- Use feature flags for risky rollouts and include telemetry and canary deployment plans.
- Provide backward-compatibility notes for public API changes.

Compliance & security
- Log every AI interaction with timestamp, user, prompt hash, and model used; store logs in compliant storage.
- Route FedRAMP/CUI data only through accredited endpoints with pre-approval.
- Require security review for any new third-party SDK or service.

Testing & quality
- Define acceptance criteria and require regression tests for major changes.
- Run automated accessibility checks for UI changes and provide remediation plans for failures.

Governance & lifecycle
- Keep a documented approval path for adding new LLMs/providers and review it periodically.
- Update these rules at least quarterly and keep a changelog of updates.
- Define who may override rules and require recorded approvals for overrides.

Examples
- Preferred: "I recommend trying X; verify with unit tests A and B."- Preferred: "I recommend trying X; verify with unit tests A and B."