import { 
  Shield, 
  Leaf, 
  CheckSquare, 
  ListTodo, 
  Activity, 
  Droplets, 
  Server, 
  Clock, 
  Lock, 
  Database, 
  FileSpreadsheet, 
  AlertTriangle, 
  RefreshCw,
  LucideIcon
} from 'lucide-react';

// Problem data for the "Old Way" section
export interface Problem {
  icon: LucideIcon;
  title: string;
  description: string;
}

export const PROBLEMS: Problem[] = [
  { 
    icon: FileSpreadsheet, 
    title: 'Disconnected Tools', 
    description: 'Spreadsheets for scheduling, separate apps for climate monitoring, manual processes for compliance.' 
  },
  { 
    icon: AlertTriangle, 
    title: 'Compliance Anxiety', 
    description: 'Manual data entry into METRC/BioTrack, scrambling before inspections, hoping nothing was missed.' 
  },
  { 
    icon: RefreshCw, 
    title: 'Reconciliation Chaos', 
    description: 'Financial data that never quite matches cultivation records, endless manual adjustments.' 
  },
];

// Solutions data for the "Harvestry Way" section
export const SOLUTIONS: string[] = [
  'Single unified platform for all operations',
  'Automatic compliance sync with retry & reconciliation',
  'Blueprint-driven, SLA-monitored workflows',
  'Real-time financial integration with QuickBooks',
  'Safety-first automation with complete explainability',
  'Predictive equipment health monitoring',
  'Out-of-the-box KPIs and dashboards',
];

// Trust badge metrics data
export interface TrustMetric {
  icon: LucideIcon;
  value: string;
  label: string;
  color: string;
}

export const TRUST_METRICS: TrustMetric[] = [
  { icon: Activity, value: '99.9%', label: 'Uptime SLA', color: 'emerald' },
  { icon: Clock, value: '<1s', label: 'Telemetry Ingest', color: 'cyan' },
  { icon: Shield, value: 'SOC 2', label: 'Type II Compliant', color: 'violet' },
  { icon: Lock, value: 'RLS', label: 'Row-Level Security', color: 'amber' },
  { icon: Database, value: '5 min', label: 'Max Data Loss (RPO)', color: 'sky' },
  { icon: Server, value: '30 min', label: 'Recovery Time (RTO)', color: 'rose' },
];

// Trust badge color mappings
export const TRUST_COLORS: Record<string, { bg: string; text: string }> = {
  emerald: { bg: 'bg-accent-emerald/10', text: 'text-accent-emerald' },
  cyan: { bg: 'bg-accent-cyan/10', text: 'text-accent-cyan' },
  violet: { bg: 'bg-accent-violet/10', text: 'text-accent-violet' },
  amber: { bg: 'bg-accent-amber/10', text: 'text-accent-amber' },
  sky: { bg: 'bg-accent-sky/10', text: 'text-accent-sky' },
  rose: { bg: 'bg-accent-rose/10', text: 'text-accent-rose' },
};

// Widget configuration for the interactive dashboard
export interface WidgetInfo {
  label: string;
  description: string;
  icon: LucideIcon;
  color: string;
}

export const WIDGET_INFO: Record<string, WidgetInfo> = {
  rooms: {
    label: 'Spatial Management',
    description: 'Track every room, zone, and growing area in real-time',
    icon: Shield,
    color: 'accent-emerald',
  },
  plants: {
    label: 'Plant Lifecycle',
    description: 'Monitor 847 plants from seed to sale with full traceability',
    icon: Leaf,
    color: 'accent-cyan',
  },
  compliance: {
    label: 'Compliance Engine',
    description: 'Automated METRC sync keeps you audit-ready 24/7',
    icon: CheckSquare,
    color: 'accent-amber',
  },
  tasks: {
    label: 'Task Orchestration',
    description: 'AI-prioritized workflows for your cultivation team',
    icon: ListTodo,
    color: 'accent-violet',
  },
  chart: {
    label: 'Analytics Dashboard',
    description: 'Real-time insights across your entire operation',
    icon: Activity,
    color: 'accent-emerald',
  },
  environment: {
    label: 'Environmental Control',
    description: 'VPD, temperature, and humidity optimization',
    icon: Droplets,
    color: 'accent-cyan',
  },
};

export type WidgetKey = keyof typeof WIDGET_INFO;

export const WIDGET_SEQUENCE: WidgetKey[] = ['rooms', 'plants', 'compliance', 'tasks', 'chart', 'environment'];

// Dashboard screenshot tabs for carousel
export interface DashboardTab {
  id: string;
  label: string;
  image: string;
  description: string;
}

export const DASHBOARD_TABS: DashboardTab[] = [
  { 
    id: 'cultivation', 
    label: 'Cultivation', 
    image: '/images/cultivation-dashboard.png',
    description: 'Real-time environmental metrics and trend analysis'
  },
  { 
    id: 'irrigation', 
    label: 'Irrigation', 
    image: '/images/irrigation-dashboard.png',
    description: 'Tank management and irrigation scheduling'
  },
  { 
    id: 'planner', 
    label: 'Planner', 
    image: '/images/planner-dashboard.png',
    description: 'Visual timeline for batch scheduling'
  },
  { 
    id: 'tasks', 
    label: 'Tasks', 
    image: '/images/tasks-dashboard.png',
    description: 'Kanban workflow and task management'
  },
];





