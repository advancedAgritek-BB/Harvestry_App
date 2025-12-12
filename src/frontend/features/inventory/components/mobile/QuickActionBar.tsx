'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import {
  QrCode,
  ArrowLeftRight,
  Plus,
  Search,
} from 'lucide-react';

interface QuickAction {
  id: string;
  label: string;
  icon: React.ReactNode;
  onClick: () => void;
  variant?: 'default' | 'primary' | 'outline';
}

interface QuickActionBarProps {
  actions?: QuickAction[];
  onScan?: () => void;
  onMove?: () => void;
  onReceive?: () => void;
  onSearch?: () => void;
  className?: string;
}

const defaultActions = (props: QuickActionBarProps): QuickAction[] => [
  {
    id: 'scan',
    label: 'Scan',
    icon: <QrCode className="h-5 w-5" />,
    onClick: props.onScan ?? (() => {}),
    variant: 'primary',
  },
  {
    id: 'move',
    label: 'Move',
    icon: <ArrowLeftRight className="h-5 w-5" />,
    onClick: props.onMove ?? (() => {}),
  },
  {
    id: 'receive',
    label: 'Receive',
    icon: <Plus className="h-5 w-5" />,
    onClick: props.onReceive ?? (() => {}),
  },
  {
    id: 'search',
    label: 'Search',
    icon: <Search className="h-5 w-5" />,
    onClick: props.onSearch ?? (() => {}),
  },
];

export function QuickActionBar(props: QuickActionBarProps) {
  const { actions, className } = props;
  const displayActions = actions ?? defaultActions(props);

  return (
    <div className={cn('grid grid-cols-4 gap-2 p-4 bg-muted/50 rounded-xl', className)}>
      {displayActions.map((action) => (
        <button
          key={action.id}
          onClick={action.onClick}
          className={cn(
            'flex flex-col items-center justify-center h-auto py-3 gap-1 rounded-lg border transition-colors touch-manipulation',
            action.variant === 'primary'
              ? 'bg-primary text-primary-foreground border-primary hover:bg-primary/90'
              : 'bg-background text-foreground border-border hover:bg-muted'
          )}
        >
          {action.icon}
          <span className="text-xs font-medium">{action.label}</span>
        </button>
      ))}
    </div>
  );
}

export default QuickActionBar;




