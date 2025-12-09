#Prompts:
THe following iterative prompts were used to interrogate the application source, identify findings, create features and generate issues on Github (using MCP).

* Review the application and identify the top 10 fedramp related compliance concerns and create a file with features that we can prioritize for development efforts.
* Save the Findings and Features to separate files in the tempbacklog folder
* Break the features into individual issues. Each issue should be able to be completed by 1 developer within one sprint. Use Gerkin for the acceptance criteria.
* Adapt them for GitHub issues
* Save the issue_template so that other team members will use the same rultes.
* Create the identified issues on the github server associated with the origin.
* Use the Github mcp server to create the issues on the server.

* Show me the final prompt

## Generated final prompt for reuse.

You are GitHub Copilot working in the `claudiabuonocore/eshop-ai-avengers` repository (eShop, .NET Aspire microservices reference app).

Goals:
1. Review the architecture and configuration to identify the top 10 FedRAMP-related compliance concerns.
2. Produce a concise FedRAMP roadmap file under `TempBacklog` describing:
   - Context (architecture summary).
   - Top 10 FedRAMP-related concerns (each with Observation, FedRAMP Areas, Concern).
   - A prioritized feature backlog (F1–F10), each with: mapped concerns, goal, high-level scope, risks, rollback strategy, FedRAMP considerations.
3. Break each roadmap feature (F1–F10) into 2–4 developer-sized GitHub issues (each completable by 1 dev in 1 sprint) with:
   - A clear, GitHub-ready title (prefixed by theme, e.g., “[IAM] …”).
   - A 1–3 sentence summary.
   - A short “Background / FedRAMP Concern” section referencing the roadmap and relevant controls.
   - A list of “Affected Areas” (paths like `src/Identity.API`, `src/WebApp`, `src/eShop.AppHost/Program.cs`).
   - 1–3 acceptance criteria written in Gherkin (Scenario / Given / When / Then).
4. Use the GitHub MCP server to create one GitHub issue per developer task in the `eshop-ai-avengers` repo, applying at least the labels `fedramp` and `security`.

Constraints and style:
- Follow the project’s Copilot rules in `docs/COPILOT_RULES.md`, including FedRAMP awareness and data-handling constraints.
- Do not inspect or modify database files or schemas.
- Keep all descriptions concise and implementation-oriented; avoid long prose.
- Each issue must have at least one Gherkin scenario in the body.
- Use `.github/ISSUE_TEMPLATE/fedramp-feature.yml` as the conceptual shape (Summary, Background / FedRAMP Concern, Affected Areas, Acceptance Criteria (Gherkin), Links), but you may create issues directly via MCP rather than interactively using the template.
- Prefer references like “see TempBacklog/EShop FedRAMP Compliance Features.md” rather than duplicating full documents.

Operational guidance (important):
- First, scan:
  - `Onboarding/architect.md`
  - `docs/COPILOT_RULES.md`
  - `TempBacklog/EShop FedRAMP Compliance Features.md` (if it exists; otherwise create it)
  - Representative `appsettings*.json` in `src/Identity.API`, `src/WebApp`, `src/Webhooks.API`, `src/Basket.API`
  - `src/eShop.AppHost/Program.cs`
- If the roadmap file doesn’t exist, create `TempBacklog/EShop FedRAMP Compliance Features.md` and write the roadmap there.
- Then derive issues (roughly 20–30 total) grouped by themes:
  - IAM, Logging/Audit, Secrets, TLS, Data/PII, Webhooks/Egress, Containers/SBOM, Backup/DR, Environment Baselines, AI Guardrails.
- Use the GitHub MCP tools:
  - `mcp_github_issue_write` with `method: "create"` to open issues in `claudiabuonocore/eshop-ai-avengers`.
  - Set `title`, `body`, `labels: ["fedramp","security"]`.
- After creating all issues, add one “meta” issue:
  - Title: “[Data] Update FedRAMP backlog links to GitHub issues”
  - Summary: Keep `TempBacklog/EShop FedRAMP Compliance Features.md` in sync with created GitHub issues.
- Finally, update the roadmap file to add the created issue numbers/links next to each feature ID (F1–F10) and mention that all related GitHub issues carry the `fedramp` label.

Output expectations:
- In the chat, summarize:
  - The path to the roadmap file you created/updated.
  - The count of GitHub issues created and example URLs.
- Do not paste full file contents; just key paths, counts, and examples.