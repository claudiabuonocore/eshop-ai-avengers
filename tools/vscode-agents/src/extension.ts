import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';

interface AgentConfig {
  name: string;
  description?: string;
  instructions: string;
  tools?: Record<string, boolean>;
}

function loadAgents(workspaceRoot: string): AgentConfig[] {
  const dir = path.join(workspaceRoot, '.github', 'chatmodes');
  if (!fs.existsSync(dir)) return [];
  const files = fs.readdirSync(dir).filter(f => f.endsWith('.md'));
  const agents: AgentConfig[] = [];
  for (const file of files) {
    const content = fs.readFileSync(path.join(dir, file), 'utf8');
    // Simple front-matter parse: extract name, instructions
    const nameMatch = content.match(/name:\s*(.*)/);
    const instructionsMatch = content.match(/instructions:\s*"([\s\S]*?)"/);
    const descriptionMatch = content.match(/description:\s*(.*)/);
    const tools: Record<string, boolean> = {};
    const toolLines = content.match(/tools:[\s\S]*/);
    if (toolLines) {
      const toolBlock = toolLines[0];
      const toolMatches = toolBlock.match(/\s([a-zA-Z_]+):\s*(true|false)/g) || [];
      for (const m of toolMatches) {
        const parts = m.trim().split(':');
        tools[parts[0]] = parts[1].trim() === 'true';
      }
    }
    const name = nameMatch ? nameMatch[1].trim() : path.basename(file, '.md');
    const instructions = instructionsMatch ? instructionsMatch[1] : '';
    const description = descriptionMatch ? descriptionMatch[1].trim() : undefined;
    if (instructions) {
      agents.push({ name, description, instructions, tools });
    }
  }
  return agents;
}

async function injectIntoCopilotChat(text: string) {
  // Best-effort: insert into active chat input or open a new chat editor
  await vscode.commands.executeCommand('workbench.action.openChatView');
  await vscode.commands.executeCommand('interactive.open', { viewId: 'workbench.panel.chat' });
  await vscode.commands.executeCommand('copilot.openChat');
  // Fallback: show input box and copy to clipboard
  await vscode.env.clipboard.writeText(text);
  vscode.window.showInformationMessage('Agent prompt copied to clipboard. Paste into Copilot Chat.');
}

export function activate(context: vscode.ExtensionContext) {
  const workspaceFolders = vscode.workspace.workspaceFolders;
  const root = workspaceFolders && workspaceFolders.length > 0 ? workspaceFolders[0].uri.fsPath : '';
  const agents = loadAgents(root);

  const selectAgent = vscode.commands.registerCommand('eshopAgents.selectAgent', async () => {
    if (agents.length === 0) {
      vscode.window.showWarningMessage('No agents found in .github/chatmodes.');
      return;
    }
    const pick = await vscode.window.showQuickPick(agents.map(a => ({ label: a.name, description: a.description })), { placeHolder: 'Select AI Agent' });
    if (!pick) return;
    const agent = agents.find(a => a.name === pick.label)!;

    const banner = `Agent: ${agent.name}\n\n` + agent.instructions + '\n';
    await injectIntoCopilotChat(banner);
  });

  context.subscriptions.push(selectAgent);
}

export function deactivate() {}
