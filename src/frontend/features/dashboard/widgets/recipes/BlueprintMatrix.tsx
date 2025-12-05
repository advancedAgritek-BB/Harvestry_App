import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { Book, Leaf, Sun, ChevronDown, Plus } from 'lucide-react';

interface BlueprintCell {
  fertigationId?: string;
  environmentId?: string;
  lightingId?: string;
}

interface BlueprintRow {
  strainId: string;
  strainName: string;
  assignments: Record<string, BlueprintCell>; // key = phaseId
}

export function BlueprintMatrix() {
  const phases = ['Clone', 'Veg W1-2', 'Veg W3-4', 'Flower W1-4', 'Flower W5-8', 'Cure'];
  
  const [data, setData] = useState<BlueprintRow[]>([
    {
      strainId: 's1',
      strainName: 'OG Kush',
      assignments: {
        'Clone': { fertigationId: 'f5', environmentId: 'e1', lightingId: 'l3' },
        'Veg W1-2': { fertigationId: 'f1', environmentId: 'e1', lightingId: 'l1' },
        'Veg W3-4': { fertigationId: 'f2', environmentId: 'e1', lightingId: 'l1' },
        'Flower W1-4': { fertigationId: 'f3', environmentId: 'e2', lightingId: 'l2' },
        'Flower W5-8': { fertigationId: 'f4', environmentId: 'e3', lightingId: 'l2' },
        'Cure': { environmentId: 'e4' }
      }
    },
    {
      strainId: 's2',
      strainName: 'Blue Dream',
      assignments: {
        'Clone': { fertigationId: 'f5', environmentId: 'e1', lightingId: 'l3' },
        'Veg W1-2': { fertigationId: 'f1', environmentId: 'e1', lightingId: 'l1' },
        'Veg W3-4': { fertigationId: 'f2', environmentId: 'e1', lightingId: 'l1' },
        'Flower W1-4': { fertigationId: 'f3', environmentId: 'e2', lightingId: 'l2' },
        'Flower W5-8': { fertigationId: 'f4', environmentId: 'e3', lightingId: 'l2' },
        'Cure': { environmentId: 'e4' }
      }
    }
  ]);

  const CellContent = ({ cell }: { cell?: BlueprintCell }) => {
    if (!cell) return <div className="text-muted-foreground/60 text-[10px] italic">Empty</div>;

    return (
      <div className="flex flex-col gap-1 w-full">
        {cell.fertigationId && (
          <div className="flex items-center gap-1.5 px-1.5 py-0.5 rounded bg-purple-500/10 border border-purple-500/20 text-[10px] text-purple-300">
            <Book className="w-2.5 h-2.5" />
            <span className="truncate">Fert</span>
          </div>
        )}
        {cell.environmentId && (
          <div className="flex items-center gap-1.5 px-1.5 py-0.5 rounded bg-emerald-500/10 border border-emerald-500/20 text-[10px] text-emerald-300">
            <Leaf className="w-2.5 h-2.5" />
            <span className="truncate">Env</span>
          </div>
        )}
        {cell.lightingId && (
          <div className="flex items-center gap-1.5 px-1.5 py-0.5 rounded bg-amber-500/10 border border-amber-500/20 text-[10px] text-amber-300">
            <Sun className="w-2.5 h-2.5" />
            <span className="truncate">Light</span>
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="w-full overflow-x-auto rounded-xl border border-border bg-surface/50">
      <table className="w-full min-w-[1000px] border-collapse">
        <thead>
          <tr>
            <th className="sticky left-0 z-10 bg-surface border-b border-r border-border p-4 text-left text-xs font-bold text-muted-foreground uppercase tracking-wider w-[200px]">
              Strain / Phase
            </th>
            {phases.map(phase => (
              <th key={phase} className="bg-surface/80 border-b border-border p-3 text-left text-xs font-bold text-foreground/70 min-w-[140px]">
                {phase}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {data.map((row) => (
            <tr key={row.strainId} className="group hover:bg-muted/30">
              {/* Row Header (Sticky) */}
              <td className="sticky left-0 z-10 bg-surface group-hover:bg-surface border-r border-b border-border p-4 font-medium text-foreground">
                <div className="flex items-center justify-between">
                  <span>{row.strainName}</span>
                  <button className="opacity-0 group-hover:opacity-100 p-1 hover:bg-muted rounded">
                    <ChevronDown className="w-3 h-3 text-muted-foreground" />
                  </button>
                </div>
              </td>
              
              {/* Cells */}
              {phases.map(phase => (
                <td key={phase} className="border-b border-border p-2 align-top h-[80px]">
                  <button className="w-full h-full rounded hover:bg-muted/50 flex items-start justify-center p-1 transition-colors text-left">
                    <CellContent cell={row.assignments[phase]} />
                  </button>
                </td>
              ))}
            </tr>
          ))}

          {/* Add Row Button */}
          <tr>
            <td colSpan={phases.length + 1} className="p-2 bg-surface/30">
              <button className="flex items-center gap-2 px-4 py-2 text-sm font-medium text-muted-foreground hover:text-foreground hover:bg-muted rounded transition-colors w-full justify-center border border-dashed border-border hover:border-border">
                <Plus className="w-4 h-4" />
                Add Strain to Matrix
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  );
}






