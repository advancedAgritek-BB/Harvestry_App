'use client';

import React from 'react';
import { Plus, X, AlertCircle } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { SchedulePredicate, PredicateMetric, PredicateOperator } from './types';
import {
  PREDICATE_METRICS,
  PREDICATE_OPERATORS,
  createDefaultPredicate,
} from './types';

interface PredicateBuilderProps {
  predicates: SchedulePredicate[];
  onChange: (predicates: SchedulePredicate[]) => void;
  label: string;
  description?: string;
  emptyMessage?: string;
  required?: boolean;
}

export function PredicateBuilder({
  predicates,
  onChange,
  label,
  description,
  emptyMessage = 'No conditions configured',
  required = false,
}: PredicateBuilderProps) {
  const handleAddPredicate = () => {
    onChange([...predicates, createDefaultPredicate()]);
  };

  const handleRemovePredicate = (id: string) => {
    onChange(predicates.filter((p) => p.id !== id));
  };

  const handleUpdatePredicate = (
    id: string,
    field: keyof SchedulePredicate,
    value: string | number
  ) => {
    onChange(
      predicates.map((p) => {
        if (p.id !== id) return p;

        // When changing metric, reset value to the default for that metric
        if (field === 'metric') {
          const newMetric = value as PredicateMetric;
          const config = PREDICATE_METRICS[newMetric];
          return { ...p, metric: newMetric, value: config.defaultValue };
        }

        return { ...p, [field]: value };
      })
    );
  };

  return (
    <div className="space-y-3">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <label className="text-sm font-medium text-foreground">
            {label}
            {required && <span className="text-rose-400 ml-1">*</span>}
          </label>
          {description && (
            <p className="text-xs text-muted-foreground mt-0.5">{description}</p>
          )}
        </div>
        <button
          type="button"
          onClick={handleAddPredicate}
          className={cn(
            'flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-colors',
            'bg-cyan-500/10 text-cyan-400 hover:bg-cyan-500/20 border border-cyan-500/20'
          )}
        >
          <Plus className="w-3.5 h-3.5" />
          Add Condition
        </button>
      </div>

      {/* Predicate List */}
      {predicates.length === 0 ? (
        <div className="flex items-center gap-2 p-3 rounded-xl bg-muted/30 border border-border text-muted-foreground">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span className="text-xs">{emptyMessage}</span>
        </div>
      ) : (
        <div className="space-y-2">
          {predicates.map((predicate, index) => (
            <PredicateRow
              key={predicate.id}
              predicate={predicate}
              index={index}
              onUpdate={(field, value) =>
                handleUpdatePredicate(predicate.id, field, value)
              }
              onRemove={() => handleRemovePredicate(predicate.id)}
            />
          ))}
        </div>
      )}
    </div>
  );
}

interface PredicateRowProps {
  predicate: SchedulePredicate;
  index: number;
  onUpdate: (field: keyof SchedulePredicate, value: string | number) => void;
  onRemove: () => void;
}

function PredicateRow({ predicate, index, onUpdate, onRemove }: PredicateRowProps) {
  const metricConfig = PREDICATE_METRICS[predicate.metric];

  return (
    <div className="flex items-center gap-2 p-2 rounded-xl bg-white/5 border border-border group">
      {/* Condition number badge */}
      <div className="w-6 h-6 rounded-full bg-muted flex items-center justify-center text-xs font-bold text-muted-foreground shrink-0">
        {index + 1}
      </div>

      {/* Metric selector */}
      <select
        value={predicate.metric}
        onChange={(e) => onUpdate('metric', e.target.value)}
        className={cn(
          'h-9 px-2 rounded-lg text-sm font-medium',
          'bg-muted border border-border text-foreground',
          'focus:outline-none focus:border-cyan-500/30'
        )}
      >
        {Object.entries(PREDICATE_METRICS).map(([key, config]) => (
          <option key={key} value={key}>
            {config.label}
          </option>
        ))}
      </select>

      {/* Operator selector */}
      <select
        value={predicate.operator}
        onChange={(e) => onUpdate('operator', e.target.value as PredicateOperator)}
        className={cn(
          'h-9 px-2 rounded-lg text-sm',
          'bg-muted border border-border text-foreground',
          'focus:outline-none focus:border-cyan-500/30'
        )}
      >
        {PREDICATE_OPERATORS.map((op) => (
          <option key={op.value} value={op.value}>
            {op.label}
          </option>
        ))}
      </select>

      {/* Value input */}
      <div className="flex items-center gap-1.5 flex-1">
        <input
          type="number"
          value={predicate.value}
          onChange={(e) => onUpdate('value', parseFloat(e.target.value) || 0)}
          min={metricConfig.min}
          max={metricConfig.max}
          step={metricConfig.step}
          className={cn(
            'w-20 h-9 px-2 rounded-lg text-sm tabular-nums',
            'bg-muted border border-border text-foreground',
            'focus:outline-none focus:border-cyan-500/30'
          )}
        />
        {metricConfig.unit && (
          <span className="text-xs text-muted-foreground shrink-0">
            {metricConfig.unit}
          </span>
        )}
      </div>

      {/* Remove button */}
      <button
        type="button"
        onClick={onRemove}
        className={cn(
          'p-1.5 rounded-lg transition-colors shrink-0',
          'text-muted-foreground hover:text-rose-400 hover:bg-rose-500/10',
          'opacity-0 group-hover:opacity-100'
        )}
        title="Remove condition"
      >
        <X className="w-4 h-4" />
      </button>
    </div>
  );
}

export default PredicateBuilder;








