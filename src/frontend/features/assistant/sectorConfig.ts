/**
 * Sector configuration for the AI Assistant.
 * Defines sector-specific UI text, suggestions, and context for each application area.
 */

export type AssistantSector =
  | 'overview'
  | 'planner'
  | 'tasks'
  | 'cultivation'
  | 'irrigation'
  | 'library'
  | 'inventory'
  | 'analytics'
  | 'admin'
  | 'recipes'
  | 'general';

export type SectorSuggestion = {
  label: string;
  question: string;
};

export type SectorConfig = {
  /** Display title shown in header subtitle */
  title: string;
  /** Short description for empty state */
  description: string;
  /** Placeholder text for input field */
  placeholder: string;
  /** Quick suggestion buttons */
  suggestions: SectorSuggestion[];
  /** Context string sent to LLM for focused responses */
  systemContext: string;
};

/**
 * Configuration for each sector the assistant can operate in.
 * Each sector has customized UI and context for relevant assistance.
 */
export const SECTOR_CONFIGS: Record<AssistantSector, SectorConfig> = {
  overview: {
    title: 'Ask about your facility',
    description: 'Get insights across your entire operation. I can help with facility-wide questions.',
    placeholder: 'Ask about your facility...',
    suggestions: [
      { label: 'Summary', question: "What's the overall status of my facility?" },
      { label: 'Alerts', question: 'Are there any urgent alerts I should know about?' },
      { label: 'Today', question: "What's on the schedule for today?" },
      { label: 'Tasks', question: 'What tasks need my attention?' },
    ],
    systemContext: 'facility overview and cross-functional operations',
  },

  planner: {
    title: 'Ask about production planning',
    description: 'Help with scheduling, batches, and production timelines.',
    placeholder: 'Ask about planning...',
    suggestions: [
      { label: 'Schedule', question: "What's the upcoming production schedule?" },
      { label: 'Batches', question: 'How are current batches progressing?' },
      { label: 'Capacity', question: 'What is our current capacity utilization?' },
      { label: 'Timeline', question: 'Are any batches behind schedule?' },
    ],
    systemContext: 'production planning, scheduling, and batch management',
  },

  tasks: {
    title: 'Ask about tasks',
    description: 'Get help with task management, SOPs, and workflow assignments.',
    placeholder: 'Ask about tasks...',
    suggestions: [
      { label: 'Pending', question: 'What tasks are pending for today?' },
      { label: 'Overdue', question: 'Are there any overdue tasks?' },
      { label: 'SOPs', question: 'What SOPs are assigned to me?' },
      { label: 'Priority', question: 'Which tasks are highest priority?' },
    ],
    systemContext: 'task management, SOPs, and workflow assignments',
  },

  cultivation: {
    title: 'Ask about cultivation',
    description: 'Ask about environment, irrigation, harvest QA, or IPM. I\'ll cite my sources.',
    placeholder: 'Ask about cultivation...',
    suggestions: [
      { label: 'Environment', question: "How's the environment looking?" },
      { label: 'Harvest QA', question: 'Any issues with current harvest?' },
      { label: 'Irrigation', question: 'When is the next irrigation?' },
      { label: 'IPM', question: 'Any pest alerts I should know about?' },
    ],
    systemContext: 'cultivation operations including environment, plants, harvest QA, and IPM',
  },

  irrigation: {
    title: 'Ask about irrigation',
    description: 'Help with irrigation schedules, programs, and zone management.',
    placeholder: 'Ask about irrigation...',
    suggestions: [
      { label: 'Schedule', question: "What's the irrigation schedule today?" },
      { label: 'Programs', question: 'Which irrigation programs are active?' },
      { label: 'Zones', question: 'How are the irrigation zones configured?' },
      { label: 'History', question: 'Show me recent irrigation events.' },
    ],
    systemContext: 'irrigation schedules, programs, zones, and fertigation',
  },

  library: {
    title: 'Ask about your library',
    description: 'Search strains, recipes, SOPs, and knowledge base resources.',
    placeholder: 'Ask about library...',
    suggestions: [
      { label: 'Strains', question: 'What strains are in my library?' },
      { label: 'Recipes', question: 'What recipes are available?' },
      { label: 'SOPs', question: 'Find SOPs for harvest procedures.' },
      { label: 'Search', question: 'Search the knowledge base for...' },
    ],
    systemContext: 'strain library, recipes, SOPs, and knowledge base',
  },

  inventory: {
    title: 'Ask about inventory',
    description: 'Track packages, items, and inventory levels across your facility.',
    placeholder: 'Ask about inventory...',
    suggestions: [
      { label: 'Levels', question: 'What items are running low?' },
      { label: 'Packages', question: 'How many active packages do we have?' },
      { label: 'Compliance', question: 'Are there any compliance issues?' },
      { label: 'Movement', question: "What's been transferred recently?" },
    ],
    systemContext: 'inventory management, packages, items, and compliance tracking',
  },

  analytics: {
    title: 'Ask about analytics',
    description: 'Get insights from your data, reports, and performance metrics.',
    placeholder: 'Ask about analytics...',
    suggestions: [
      { label: 'Performance', question: 'How is yield performance trending?' },
      { label: 'Reports', question: 'What reports are available?' },
      { label: 'Trends', question: 'Show me environmental trends.' },
      { label: 'Compare', question: 'Compare this batch to previous ones.' },
    ],
    systemContext: 'analytics, reporting, and performance metrics',
  },

  recipes: {
    title: 'Ask about recipes',
    description: 'Help with environment, fertigation, and lighting recipes.',
    placeholder: 'Ask about recipes...',
    suggestions: [
      { label: 'Environment', question: 'What environment recipes exist?' },
      { label: 'Fertigation', question: 'Show fertigation recipe details.' },
      { label: 'Lighting', question: 'What lighting schedules are set up?' },
      { label: 'Create', question: 'Help me create a new recipe.' },
    ],
    systemContext: 'environment recipes, fertigation recipes, and lighting schedules',
  },

  admin: {
    title: 'Ask about administration',
    description: 'Help with users, permissions, integrations, and system configuration.',
    placeholder: 'Ask about admin...',
    suggestions: [
      { label: 'Users', question: 'How many users are in the system?' },
      { label: 'Permissions', question: 'What roles are configured?' },
      { label: 'Integrations', question: 'What integrations are active?' },
      { label: 'Settings', question: 'How do I configure settings?' },
    ],
    systemContext: 'administration, users, permissions, and system configuration',
  },

  general: {
    title: 'Ask anything',
    description: 'I can help with questions across your entire Harvestry platform.',
    placeholder: 'Ask a question...',
    suggestions: [
      { label: 'Overview', question: "What's the status of my facility?" },
      { label: 'Help', question: 'What can you help me with?' },
      { label: 'Tasks', question: 'What needs my attention today?' },
      { label: 'Alerts', question: 'Are there any alerts?' },
    ],
    systemContext: 'general facility operations and cross-functional questions',
  },
};

/**
 * Get sector config with fallback to general
 */
export function getSectorConfig(sector: AssistantSector | undefined): SectorConfig {
  return SECTOR_CONFIGS[sector ?? 'general'];
}




