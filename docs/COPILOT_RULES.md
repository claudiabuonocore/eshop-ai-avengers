Copilot rules

Scope
- Intended for AI coding assistants (Copilot, Copilot Chat) used by the project team.

Rules
1. Verbosity
   - Keep responses concise and focused on the user's request.
   - Avoid unnecessary prose. If a longer explanation is required, provide a brief summary and offer to expand.

2. Markdown
   - Do not create a markdown summary unless explicitly requested as a final prompt.

3. Certainty
   - Never present a solution as absolutely correct. Use hedging language where appropriate and list potential caveats.

4. Personas
   - When applicable, include viewpoints or considerations from these personas: developer, architect, product owner, quality engineer, UX (accessibility-focused).

5. FedRAMP compliance
   - All interactions with language models must comply with FedRAMP restrictions. Do not share sensitive data, ensure data handling follows approved processes, and follow organizational policies for FedRAMP.
   - Treat any fields or entities marked as confidential or regulated in the codebase's data classification as sensitive by default. Do not emit these values into logs, test data, or AI prompts. Treat vector embeddings derived from such data as sensitive as well.
   - Prefer patterns that use existing data-classification and masking/redaction helpers; do not suggest logging or printing fields marked as sensitive or regulated.
   - When generating or modifying domain or data models, apply the existing data-classification scheme to new fields and entities (for example, public / internal / confidential / regulated) and, where appropriate, update any central data-classification documentation.

6. Database files
   - Do not inspect or read database files and folders when making code change recommendations.
   - Do not recommend changes that would alter the database schema.

7. Workspace-specific guidance
   - Respect project-specific frameworks and target runtimes (e.g., .NET 10, .NET MAUI, Blazor, Razor Pages).

8. When unsure
   - State uncertainty and provide next steps to verify the recommendation (tests to run, files to inspect, or configuration to check).

9. Logging and PII
   - When suggesting logging or telemetry, never include secrets, tokens, passwords, or raw PII (such as full names, emails, addresses, payment details, or full webhook payloads).
   - Prefer structured logs that reference non-sensitive identifiers and outcomes (for example, correlation IDs, order IDs, or status codes) and assume masking/redaction helpers are in place.

Usage examples
- Good: "I think changing X to Y will likely address the issue, but verify by running these tests..."
- Bad: "This is the definitive fix."