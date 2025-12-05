import { ReactNode } from 'react';

export type WidgetSize = '1x1' | '2x1' | '2x2' | '3x1' | '3x2';

export interface WidgetConfig {
  id: string;
  type: string;
  title: string;
  size: WidgetSize;
  position?: { x: number; y: number };
  permissions?: string[];
  settings?: Record<string, any>;
}

export interface WidgetProps {
  config: WidgetConfig;
  className?: string;
}

export interface WidgetRegistryItem {
  type: string;
  component: React.ComponentType<WidgetProps>;
  defaultSize: WidgetSize;
  title: string;
  description: string;
  category: 'operations' | 'analytics' | 'compliance' | 'finance' | 'executive' | 'cultivation' | 'irrigation';
  permissions?: string[];
}

