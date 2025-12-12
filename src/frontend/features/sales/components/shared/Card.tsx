'use client';

import { cn } from '@/lib/utils';

interface CardProps {
  children: React.ReactNode;
  className?: string;
  padding?: 'none' | 'sm' | 'md' | 'lg';
}

export function Card({ children, className, padding = 'md' }: CardProps) {
  const paddingClasses = {
    none: '',
    sm: 'p-3',
    md: 'p-4',
    lg: 'p-6',
  };

  return (
    <div
      className={cn(
        'bg-surface border border-border rounded-xl',
        paddingClasses[padding],
        className
      )}
    >
      {children}
    </div>
  );
}

interface CardHeaderProps {
  title: string;
  subtitle?: string;
  action?: React.ReactNode;
  className?: string;
}

export function CardHeader({ title, subtitle, action, className }: CardHeaderProps) {
  return (
    <div className={cn('flex items-center justify-between mb-4', className)}>
      <div>
        <h3 className="text-sm font-semibold text-foreground">{title}</h3>
        {subtitle && (
          <p className="text-xs text-muted-foreground mt-0.5">{subtitle}</p>
        )}
      </div>
      {action && <div>{action}</div>}
    </div>
  );
}

interface KPICardProps {
  label: string;
  value: string | number;
  change?: {
    value: number;
    trend: 'up' | 'down' | 'neutral';
  };
  icon?: React.ElementType;
  iconColor?: string;
  onClick?: () => void;
}

export function KPICard({
  label,
  value,
  change,
  icon: Icon,
  iconColor = 'text-amber-400',
  onClick,
}: KPICardProps) {
  const Wrapper = onClick ? 'button' : 'div';

  return (
    <Wrapper
      onClick={onClick}
      className={cn(
        'p-4 rounded-xl bg-surface border border-border text-left',
        onClick && 'hover:border-amber-500/30 hover:bg-muted/30 transition-all cursor-pointer'
      )}
    >
      <div className="flex items-start justify-between">
        <div>
          <div className="text-xs text-muted-foreground mb-1">{label}</div>
          <div className="text-2xl font-semibold text-foreground tabular-nums">
            {value}
          </div>
          {change && (
            <div
              className={cn(
                'text-xs mt-1 font-medium',
                change.trend === 'up' && 'text-emerald-400',
                change.trend === 'down' && 'text-rose-400',
                change.trend === 'neutral' && 'text-muted-foreground'
              )}
            >
              {change.trend === 'up' && '↑'}
              {change.trend === 'down' && '↓'}
              {change.value}%
            </div>
          )}
        </div>
        {Icon && (
          <div className="w-10 h-10 rounded-lg bg-muted/50 flex items-center justify-center">
            <Icon className={cn('w-5 h-5', iconColor)} />
          </div>
        )}
      </div>
    </Wrapper>
  );
}
