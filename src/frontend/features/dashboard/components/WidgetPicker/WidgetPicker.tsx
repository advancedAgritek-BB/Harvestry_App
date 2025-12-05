'use client';

import React from 'react';
import { X, Plus, LayoutGrid } from 'lucide-react';
import { widgetRegistry } from '../../widgets/registry';
import { useDashboardStore } from '../../stores/dashboardStore';
import { cn } from '@/lib/utils';

interface WidgetPickerProps {
  isOpen: boolean;
  onClose: () => void;
}

export function WidgetPicker({ isOpen, onClose }: WidgetPickerProps) {
  const addWidget = useDashboardStore((state) => state.addWidget);

  if (!isOpen) return null;

  const categories = Array.from(new Set(Object.values(widgetRegistry).map(w => w.category)));

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
      <div className="bg-surface border border-border rounded-xl shadow-2xl w-full max-w-3xl max-h-[80vh] flex flex-col animate-in fade-in zoom-in-95 duration-200">
        
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b border-border">
          <div className="flex items-center gap-2">
            <LayoutGrid className="w-5 h-5 text-cyan-500" />
            <h2 className="text-lg font-semibold text-foreground">Add Widget</h2>
          </div>
          <button onClick={onClose} className="p-1 hover:bg-background rounded-md text-muted-foreground transition-colors">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Body */}
        <div className="flex-1 overflow-y-auto p-4">
          {categories.map((category) => (
            <div key={category} className="mb-6 last:mb-0">
              <h3 className="text-xs font-bold text-muted-foreground uppercase tracking-wider mb-3">
                {category}
              </h3>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                {Object.values(widgetRegistry)
                  .filter((w) => w.category === category)
                  .map((widget) => (
                    <button
                      key={widget.type}
                      onClick={() => {
                        addWidget(widget.type, widget.defaultSize);
                        onClose();
                      }}
                      className="flex flex-col items-start text-left p-3 rounded-lg border border-border/50 bg-background/50 hover:bg-background hover:border-cyan-500/50 hover:shadow-lg transition-all group"
                    >
                      <div className="flex items-center justify-between w-full mb-2">
                        <span className="font-medium text-sm text-foreground">{widget.title}</span>
                        <Plus className="w-4 h-4 text-cyan-500 opacity-0 group-hover:opacity-100 transition-opacity" />
                      </div>
                      <p className="text-xs text-muted-foreground line-clamp-2">
                        {widget.description}
                      </p>
                      <div className="mt-2 text-[10px] text-muted-foreground bg-surface px-1.5 py-0.5 rounded border border-border">
                        Default: {widget.defaultSize}
                      </div>
                    </button>
                  ))}
              </div>
            </div>
          ))}
        </div>

      </div>
    </div>
  );
}

