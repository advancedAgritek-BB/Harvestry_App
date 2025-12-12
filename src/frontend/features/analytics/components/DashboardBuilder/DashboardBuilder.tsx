'use client';

import React, { useState } from 'react';
import { DashboardWidget } from '../../types';
import { useReports } from '../../hooks/useReports';
import { useCreateDashboard } from '../../hooks/useDashboards';

export const DashboardBuilder = () => {
  const [name, setName] = useState('');
  const [widgets, setWidgets] = useState<DashboardWidget[]>([]);
  const { data: reports } = useReports();
  const createDashboard = useCreateDashboard();

  const addWidget = (reportId: string) => {
    const report = reports?.find(r => r.id === reportId);
    if (!report) return;
    
    setWidgets([...widgets, {
      id: crypto.randomUUID(),
      reportId,
      title: report.name,
      x: 0, y: 0, w: 4, h: 4,
      visualizationType: 'table'
    }]);
  };

  const handleSave = () => {
    if (!name) return;
    createDashboard.mutate({
      name,
      layoutConfig: widgets,
      isPublic: false
    });
  };
  
  return (
    <div className="p-6 bg-white min-h-screen">
       {/* Toolbar */}
       <div className="flex justify-between mb-6 border-b pb-4 items-center">
         <div className="flex-1 mr-4">
            <input 
                value={name} 
                onChange={e => setName(e.target.value)} 
                placeholder="Dashboard Name" 
                className="text-2xl font-bold p-2 w-full border-b focus:outline-none focus:border-blue-500" 
            />
         </div>
         <div className="flex gap-2 items-center">
            <select 
                onChange={e => {
                    if (e.target.value) {
                        addWidget(e.target.value);
                        e.target.value = "";
                    }
                }} 
                className="border p-2 rounded bg-white"
            >
              <option value="">+ Add Widget...</option>
              {reports?.map(r => <option key={r.id} value={r.id}>{r.name}</option>)}
            </select>
            <button 
                onClick={handleSave}
                disabled={createDashboard.isPending || !name} 
                className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 disabled:opacity-50"
            >
                {createDashboard.isPending ? 'Saving...' : 'Save Dashboard'}
            </button>
         </div>
       </div>
       
       {/* Grid Area */}
       <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 bg-gray-50 min-h-[500px] p-4 rounded border border-dashed border-gray-300">
          {widgets.length === 0 && (
             <div className="col-span-full flex items-center justify-center text-gray-400">
                Add widgets to build your dashboard
             </div>
          )}
          {widgets.map(w => (
            <div key={w.id} className="bg-white p-4 rounded shadow border relative group min-h-[200px] flex flex-col">
               <div className="flex justify-between items-start mb-2">
                   <h3 className="font-bold text-gray-800 truncate pr-6">{w.title}</h3>
                   <button 
                    className="text-gray-400 hover:text-red-500 absolute top-2 right-2 p-1" 
                    onClick={() => setWidgets(widgets.filter(x => x.id !== w.id))}
                   >
                    ✕
                   </button>
               </div>
               <div className="flex-1 bg-gray-50 rounded flex items-center justify-center text-sm text-gray-400">
                  {w.visualizationType} preview
               </div>
            </div>
          ))}
       </div>
    </div>
  );
};





