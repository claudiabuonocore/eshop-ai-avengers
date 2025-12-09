Visual Studio rules for AI assistants

Scope
- Intended for Visual Studio users, Copilot in Visual Studio, and automated assistants integrated into the IDE.

Core rules
- Follow the same baseline rules as VS Code and Cursor: brevity, no markdown summaries unless requested, no absolute claims, FedRAMP compliance, do not inspect database files or recommend schema changes.

System & access
- Restrict model access by Visual Studio user role and environment; require MFA for privileged operations.
- Disable external model access for production builds by default; require explicit opt-in and approval for production model usage.

Behavior & implementation
- Encourage inline suggestions to include confidence and assumptions.
- Prefer code changes that are small and reversible; present rollback commands or git commands.
- When auto-applying fixes, always create a local branch and a PR-ready commit rather than committing directly to the active branch.

Operational
- Every AI-suggested change must produce a draft PR with tests, changelog entries, and a short risk assessment.
- Integrate pre-merge checks: static analysis, security scanning, license compliance, and unit test execution.
- Tag LLM-generated code in commit messages and PR descriptions.

Compliance
- Redact secrets and PII from prompts and logs automatically.
- Log AI interactions with user, timestamp, prompt hash, and model used; store logs in an auditable, compliant store.

IDE workflow
- Provide quick links in suggestions to relevant tests, related files, and documentation.
- If a suggested change affects multiple projects, include a cross-project impact summary.

Maintenance
- Keep these rules versioned and review quarterly.
- Provide a visible changelog within the .visualstudio folder for rule updates.
