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

Examples
- Preferred: "I recommend trying X; verify with unit tests A and B."