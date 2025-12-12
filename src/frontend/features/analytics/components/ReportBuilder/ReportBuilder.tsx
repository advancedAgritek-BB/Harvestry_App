'use client';

import React, { useState } from 'react';
import { ReportConfig } from '../../types';
import { usePreviewQuery, useCreateReport } from '../../hooks/useReports';
import { SourceSelector } from './SourceSelector';
import { FieldPicker } from './FieldPicker';
import { Visualizer } from './Visualizer';

export const ReportBuilder = () => {
  const [config, setConfig] = useState<ReportConfig>({
    source: '',
    columns: [],
    filters: [],
    sorts: [],
  });
  const [reportName, setReportName] = useState('');
  
  const previewQuery = usePreviewQuery();
  const createReport = useCreateReport();

  const handleSourceChange = (source: string) => {
    setConfig({ ...config, source, columns: [], filters: [] });
  };

  const handlePreview = () => {
    if (!config.source) return;
    previewQuery.mutate(config);
  };

  const handleSave = () => {
    if (!reportName) return;
    createReport.mutate({
      name: reportName,
      config,
      isPublic: false,
    });
  };

  return (
    <div className="flex flex-col h-full space-y-4 p-6 bg-white min-h-screen">
      <div className="flex justify-between items-center border-b pb-4">
        <h1 className="text-2xl font-bold text-gray-800">New Report</h1>
        <div className="space-x-2 flex items-center">
          <input 
            type="text" 
            placeholder="Report Name" 
            className="border p-2 rounded text-gray-800"
            value={reportName}
            onChange={(e) => setReportName(e.target.value)}
          />
          <button 
            onClick={handleSave} 
            className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 disabled:opacity-50"
            disabled={createReport.isPending || !reportName}
          >
            {createReport.isPending ? 'Saving...' : 'Save Report'}
          </button>
        </div>
      </div>

      <div className="grid grid-cols-12 gap-6 flex-1">
        <div className="col-span-3 border-r pr-4 space-y-6 overflow-y-auto">
          <SourceSelector value={config.source} onChange={handleSourceChange} />
          
          {config.source && (
            <FieldPicker 
              source={config.source} 
              config={config} 
              onChange={setConfig} 
            />
          )}
          
          <button 
            onClick={handlePreview} 
            className="w-full bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50"
            disabled={previewQuery.isPending || !config.source}
          >
            {previewQuery.isPending ? 'Loading...' : 'Run Query'}
          </button>
        </div>

        <div className="col-span-9 bg-gray-50 p-4 rounded-lg overflow-hidden flex flex-col">
          <h2 className="text-sm font-semibold text-gray-500 mb-2">Preview</h2>
          <Visualizer 
            data={previewQuery.data} 
            isLoading={previewQuery.isPending} 
            error={previewQuery.error} 
          />
        </div>
      </div>
    </div>
  );
};





