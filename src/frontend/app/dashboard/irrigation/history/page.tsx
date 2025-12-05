'use client';

import React from 'react';
import { CheckCircle2, AlertTriangle, XCircle, Clock, Search } from 'lucide-react';

export default function HistoryPage() {
  const history = [
    { id: 'h1', date: 'Today, 08:00', program: 'P1 - Morning Ramp', duration: '45m', volume: '120 gal', status: 'success' },
    { id: 'h2', date: 'Yesterday, 16:00', program: 'P3 - Dryback', duration: '30m', volume: '80 gal', status: 'success' },
    { id: 'h3', date: 'Yesterday, 12:00', program: 'P2 - Maintenance', duration: '5m', volume: '10 gal', status: 'aborted', reason: 'Tank Low Level' },
    { id: 'h4', date: 'Oct 24, 08:00', program: 'P1 - Morning Ramp', duration: '45m', volume: '118 gal', status: 'success' },
    { id: 'h5', date: 'Oct 24, 04:00', program: 'P3 - Dryback', duration: '30m', volume: '82 gal', status: 'warning', reason: 'Flow Variance' },
  ];

  const StatusIcon = ({ status }: { status: string }) => {
    switch (status) {
      case 'success': return <CheckCircle2 className="w-4 h-4 text-emerald-500" />;
      case 'warning': return <AlertTriangle className="w-4 h-4 text-amber-500" />;
      case 'aborted': return <XCircle className="w-4 h-4 text-rose-500" />;
      default: return <Clock className="w-4 h-4 text-muted-foreground" />;
    }
  };

  return (
    <div className="max-w-6xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-lg font-bold text-foreground">Run History</h2>
          <p className="text-sm text-muted-foreground">Audit log of all irrigation events and interlocks</p>
        </div>
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
          <input 
            type="text" 
            placeholder="Search logs..." 
            className="pl-9 pr-4 py-2 bg-surface/50 border border-border rounded-lg text-sm text-foreground focus:outline-none focus:border-cyan-500/50 w-64 transition-colors"
          />
        </div>
      </div>

      <div className="bg-surface/50 border border-border rounded-xl overflow-hidden">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="border-b border-border bg-surface/80 text-xs uppercase text-muted-foreground font-medium">
              <th className="p-4 w-12"></th>
              <th className="p-4">Date & Time</th>
              <th className="p-4">Program</th>
              <th className="p-4">Duration</th>
              <th className="p-4">Volume Delivered</th>
              <th className="p-4">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {history.map(run => (
              <tr key={run.id} className="hover:bg-muted/30 transition-colors group cursor-pointer">
                <td className="p-4 text-center">
                  <StatusIcon status={run.status} />
                </td>
                <td className="p-4 font-mono text-sm text-foreground/70">{run.date}</td>
                <td className="p-4 font-medium text-foreground">{run.program}</td>
                <td className="p-4 text-sm text-muted-foreground">{run.duration}</td>
                <td className="p-4 text-sm text-muted-foreground">{run.volume}</td>
                <td className="p-4">
                  {run.reason ? (
                    <span className="text-xs px-2 py-1 rounded bg-muted border border-border text-foreground/70 inline-flex items-center gap-1">
                      {run.reason}
                    </span>
                  ) : (
                    <span className="text-xs text-muted-foreground">Completed</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}



