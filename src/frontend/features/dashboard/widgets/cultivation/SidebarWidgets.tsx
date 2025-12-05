import React, { useState } from 'react';
import { cn } from '@/lib/utils';
import { Bell, CheckCircle2, User, Zap, Target, AlertOctagon } from 'lucide-react';

// --- Active Alerts Widget ---
const MOCK_ALERTS = [
  { id: 1, title: 'Zone B pressure low', desc: 'Sensor: 0-612 — Flow pulses below threshold', severity: 'critical', time: '2m ago' },
  { id: 2, title: 'Controller link loss', desc: 'Rina recovered - Confirm sensors', severity: 'warning', time: '15m ago' },
];

export function ActiveAlertsListWidget() {
  return (
    <div className="flex flex-col h-full min-h-[200px] bg-surface/50 border border-border rounded-xl p-3">
       <div className="flex items-center justify-between mb-3">
         <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider flex items-center gap-2">
           <Bell className="w-4 h-4 text-rose-500" />
           Active Alerts
         </h3>
         <span className="px-2 py-0.5 bg-muted rounded-full text-xs font-bold text-foreground">{MOCK_ALERTS.length}</span>
       </div>

       <div className="flex-1 overflow-y-auto space-y-2 custom-scrollbar">
         {MOCK_ALERTS.map(alert => (
           <div key={alert.id} className="group p-3 rounded-lg bg-muted/30 border border-border hover:bg-muted/50 transition-colors">
             <div className="flex items-center gap-2 mb-1.5">
                <span className={cn(
                  "px-1.5 py-0.5 text-[10px] font-bold uppercase rounded tracking-wide",
                  alert.severity === 'critical' ? "bg-rose-500 text-foreground" : "bg-amber-500 text-black"
                )}>
                  {alert.severity}
                </span>
                <span className="text-xs text-muted-foreground ml-auto">{alert.time}</span>
             </div>
             <h4 className="text-sm font-bold text-foreground leading-tight mb-1 truncate">{alert.title}</h4>
             <p className="text-xs text-muted-foreground leading-snug mb-2 line-clamp-2">{alert.desc}</p>
             
             {/* Actions */}
             <div className="flex items-center gap-2 opacity-60 group-hover:opacity-100 transition-opacity">
                <button className="flex-1 flex items-center justify-center gap-1 px-2 py-1 rounded bg-muted hover:bg-emerald-500/20 hover:text-emerald-400 text-xs text-foreground/70 transition-colors">
                  <CheckCircle2 className="w-3 h-3" /> Ack
                </button>
                <button className="flex-1 flex items-center justify-center gap-1 px-2 py-1 rounded bg-muted hover:bg-blue-500/20 hover:text-blue-400 text-xs text-foreground/70 transition-colors">
                  <User className="w-3 h-3" /> Delegate
                </button>
             </div>
           </div>
         ))}
       </div>
    </div>
  );
}

// --- Targets vs Current Widget ---
const TARGETS = [
  { metric: 'Temp', current: '75.4 °F', target: '74–78', range: [74, 78], val: 75.4 },
  { metric: 'RH', current: '57 %', target: '55–60', range: [55, 60], val: 57 },
  { metric: 'CO₂', current: '1050', target: '900–1.2k', range: [900, 1200], val: 1050 }, 
  { metric: 'PPFD', current: '900', target: '850–950', range: [850, 950], val: 900 },
  { metric: 'EC', current: '2.2', target: '2.0–2.4', range: [2.0, 2.4], val: 2.2 },
];

export function TargetsVsCurrentWidget() {
  return (
    <div className="flex flex-col h-full min-h-[200px] bg-surface/50 border border-border rounded-xl p-3">
      <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider mb-3 flex items-center gap-2">
         <Target className="w-4 h-4 text-cyan-500" />
         Targets
      </h3>
      
      <div className="space-y-1">
         <div className="grid grid-cols-[60px_1fr_1fr] gap-2 px-2 py-1 text-[10px] font-semibold text-muted-foreground uppercase tracking-wider">
            <span>Metric</span>
            <span className="text-right">Current</span>
            <span className="text-right">Target</span>
         </div>
         {TARGETS.map((row, i) => {
           const isOutOfSpec = row.val < row.range[0] || row.val > row.range[1];
           
           return (
             <div 
               key={i} 
               className={cn(
                 "grid grid-cols-[60px_1fr_1fr] gap-2 py-2.5 px-2 border-b border-border/50 last:border-0 transition-all cursor-pointer rounded items-center",
                 isOutOfSpec 
                   ? "bg-amber-500/10 border-amber-500/20 hover:bg-amber-500/15" 
                   : "hover:bg-muted/50"
               )}
               onClick={() => console.log("Edit target", row.metric)}
             >
               <span className={cn("text-xs font-bold", isOutOfSpec ? "text-amber-200" : "text-muted-foreground")}>{row.metric}</span>
               <span className={cn("text-sm font-bold text-right", isOutOfSpec ? "text-amber-100" : "text-foreground")}>{row.current}</span>
               <span className={cn("text-xs font-medium text-right font-mono", isOutOfSpec ? "text-amber-300/70" : "text-muted-foreground")}>{row.target}</span>
             </div>
           );
         })}
      </div>
    </div>
  );
}

// --- Quick Actions Widget ---
export function QuickActionsWidget() {
  const [showPauseConfirm, setShowPauseConfirm] = useState(false);
  
  const handleNudge = (amount: number) => {
    console.log(`Nudging EC target by ${amount} (Temporary Override)`);
    // Api call here
  };

  return (
    <div className="flex flex-col h-full min-h-[150px] bg-surface/50 border border-border rounded-xl p-3">
      <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider mb-3 flex items-center gap-2">
        <Zap className="w-4 h-4 text-amber-500" />
        Actions
      </h3>
      
      <div className="flex flex-col gap-2">
        <div className="grid grid-cols-2 gap-2">
          <button 
             onClick={() => handleNudge(0.1)}
             className="py-2 text-xs font-medium text-foreground/70 bg-muted hover:bg-muted/80 rounded border border-border transition-colors text-center"
          >
            EC +0.1
          </button>
          <button 
             onClick={() => handleNudge(-0.1)}
             className="py-2 text-xs font-medium text-foreground/70 bg-muted hover:bg-muted/80 rounded border border-border transition-colors text-center"
          >
            EC -0.1
          </button>
        </div>

        <button 
           onClick={() => setShowPauseConfirm(true)}
           className="w-full py-2 text-xs font-bold text-rose-300 bg-rose-500/10 hover:bg-rose-500/20 border border-rose-500/30 rounded transition-colors flex items-center justify-center gap-2"
        >
           <AlertOctagon className="w-4 h-4" />
           Pause Irrigation
        </button>
        
        <button className="w-full py-1 text-xs font-medium text-muted-foreground hover:text-foreground hover:underline text-center">
          Open Trial Manager
        </button>
      </div>

      {/* Pause Confirmation Modal */}
      {showPauseConfirm && (
        <div className="absolute inset-0 z-50 flex items-center justify-center bg-background/80 backdrop-blur-sm rounded-xl">
          <div className="bg-surface border border-border p-4 rounded-xl shadow-2xl max-w-[200px] w-full text-center">
             <AlertOctagon className="w-6 h-6 text-rose-500 mx-auto mb-2" />
             <h4 className="text-xs font-bold text-foreground mb-1">Emergency Stop?</h4>
             <div className="flex gap-2 mt-3">
               <button onClick={() => setShowPauseConfirm(false)} className="flex-1 py-1 text-[10px] bg-muted hover:bg-muted/80 rounded text-foreground/70">Cancel</button>
               <button onClick={() => setShowPauseConfirm(false)} className="flex-1 py-1 text-[10px] bg-rose-600 hover:bg-rose-500 rounded text-foreground font-bold">PAUSE</button>
             </div>
          </div>
        </div>
      )}
    </div>
  );
}
