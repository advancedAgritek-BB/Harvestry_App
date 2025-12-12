'use client';

import React from 'react';
import { cn } from '@/lib/utils';
import { ChevronUp, ChevronDown, MoreHorizontal, Search } from 'lucide-react';

function getColumnWidthClass(width?: string): string | undefined {
  if (!width) return undefined;
  const trimmed = width.trim();

  // If caller already provided a Tailwind class, keep as-is.
  if (
    trimmed.startsWith('w-') ||
    trimmed.startsWith('min-w-') ||
    trimmed.startsWith('max-w-') ||
    trimmed.startsWith('basis-')
  ) {
    return trimmed;
  }

  // If caller provided a CSS length (common in this codebase: "80px", "120px"),
  // convert to Tailwind arbitrary width to avoid inline styles.
  const isCssLength = /^-?\d+(\.\d+)?(px|rem|em|%|vw|vh|ch)$/.test(trimmed);
  if (isCssLength) return `w-[${trimmed}]`;

  // Fallback: treat as class string to avoid breaking unknown formats.
  return trimmed;
}

interface Column<T> {
  /**
   * Column key.
   * - If it matches a property on the row, we'll display that value by default.
   * - For computed columns, provide a stable string key and a `render` function.
   */
  key: keyof T | string;
  header: string;
  /**
   * Column width.
   * Prefer CSS lengths (e.g. "80px") or Tailwind width classes (e.g. "w-24", "w-[120px]").
   */
  width?: string;
  sortable?: boolean;
  render?: (item: T, index: number) => React.ReactNode;
}

interface AdminTableProps<T> {
  columns: Column<T>[];
  data: T[];
  keyField: keyof T;
  onRowClick?: (item: T) => void;
  emptyMessage?: string;
  className?: string;
  sortConfig?: { key: string; direction: 'asc' | 'desc' } | null;
  onSort?: (key: string) => void;
}

export function AdminTable<T extends object>({
  columns,
  data,
  keyField,
  onRowClick,
  emptyMessage = 'No data available',
  className,
  sortConfig,
  onSort,
}: AdminTableProps<T>) {
  return (
    <div className={cn('overflow-hidden rounded-lg border border-border', className)}>
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="bg-surface">
              {columns.map((column) => (
                <th
                  key={column.key.toString()}
                  className={cn(
                    'px-4 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider',
                    column.sortable && 'cursor-pointer hover:text-foreground',
                    getColumnWidthClass(column.width)
                  )}
                  onClick={() => column.sortable && onSort?.(column.key.toString())}
                >
                  <div className="flex items-center gap-1">
                    {column.header}
                    {column.sortable && sortConfig?.key === column.key.toString() && (
                      sortConfig.direction === 'asc' ? (
                        <ChevronUp className="w-3 h-3" />
                      ) : (
                        <ChevronDown className="w-3 h-3" />
                      )
                    )}
                  </div>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {data.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length}
                  className="px-4 py-12 text-center text-sm text-muted-foreground"
                >
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              data.map((item, index) => (
                <tr
                  key={String(item[keyField])}
                  className={cn(
                    'bg-background transition-colors',
                    onRowClick && 'cursor-pointer hover:bg-muted/50'
                  )}
                  onClick={() => onRowClick?.(item)}
                >
                  {columns.map((column) => (
                    <td
                      key={column.key.toString()}
                      className="px-4 py-3 text-sm text-foreground"
                    >
                      {column.render
                        ? column.render(item, index)
                        : String((item as any)[column.key as any] ?? '')}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

interface TableActionsProps {
  children: React.ReactNode;
}

export function TableActions({ children }: TableActionsProps) {
  return (
    <div className="flex items-center gap-1" onClick={(e) => e.stopPropagation()}>
      {children}
    </div>
  );
}

interface TableActionButtonProps {
  onClick: () => void;
  children: React.ReactNode;
  variant?: 'default' | 'danger';
  title?: string;
  disabled?: boolean;
}

export function TableActionButton({
  onClick,
  children,
  variant = 'default',
  title,
  disabled = false,
}: TableActionButtonProps) {
  return (
    <button
      onClick={onClick}
      title={title}
      disabled={disabled}
      className={cn(
        'p-1.5 rounded-md transition-colors',
        variant === 'default'
          ? 'text-muted-foreground hover:text-foreground hover:bg-muted/50'
          : 'text-rose-400 hover:text-rose-300 hover:bg-rose-500/10',
        disabled && 'opacity-50 cursor-not-allowed'
      )}
    >
      {children}
    </button>
  );
}

interface TableSearchProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
}

export function TableSearch({
  value,
  onChange,
  placeholder = 'Search...',
}: TableSearchProps) {
  return (
    <div className="relative">
      <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
      <input
        type="text"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-64 h-9 pl-9 pr-3 bg-elevated border border-border rounded-lg text-sm text-foreground placeholder:text-muted-foreground/50 focus:outline-none focus:ring-2 focus:ring-violet-500/30 focus:border-violet-500/50"
      />
    </div>
  );
}

interface StatusBadgeProps {
  status: 'active' | 'inactive' | 'pending' | 'error' | 'warning';
  label?: string;
}

export function StatusBadge({ status, label }: StatusBadgeProps) {
  const styles = {
    active: 'bg-emerald-500/10 text-emerald-400 ring-emerald-500/20',
    inactive: 'bg-white/5 text-muted-foreground ring-white/10',
    pending: 'bg-amber-500/10 text-amber-400 ring-amber-500/20',
    error: 'bg-rose-500/10 text-rose-400 ring-rose-500/20',
    warning: 'bg-amber-500/10 text-amber-400 ring-amber-500/20',
  };

  const defaultLabels = {
    active: 'Active',
    inactive: 'Inactive',
    pending: 'Pending',
    error: 'Error',
    warning: 'Warning',
  };

  return (
    <span
      className={cn(
        'inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ring-1',
        styles[status]
      )}
    >
      <span className="w-1.5 h-1.5 rounded-full bg-current mr-1.5" />
      {label ?? defaultLabels[status]}
    </span>
  );
}

