# eShop AI Agents (VS Code Extension)

Registers per-persona agents (Architect, QE, Engineer, Product Owner, DBA, DevOps) and injects their prompts into Copilot Chat.

Setup
- npm install
- npm run compile
- Press F5 to launch Extension Development Host

Usage
- Run command: "AI: Select Agent"
- Pick an agent; instructions are copied to clipboard and Copilot Chat is opened

Config
- Agents loaded from `.github/chatmodes/*.md` using YAML front matter fields: `name`, `description`, `instructions`, `tools`.

Notes
- Copilot APIs are limited; clipboard injection ensures reliability.
- Keep FedRAMP constraints in prompts; do not expose secrets.
