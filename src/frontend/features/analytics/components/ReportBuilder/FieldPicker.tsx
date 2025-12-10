import React from 'react';
import { ReportConfig } from '../../types';

interface Props {
  source: string;
  config: ReportConfig;
  onChange: (config: ReportConfig) => void;
}

const SCHEMAS: Record<string, string[]> = {
  harvests: ['harvest_id', 'site_name', 'harvest_name', 'total_wet_weight', 'status', 'created_at', 'created_by_name'],
  tasks: ['task_id', 'title', 'status', 'priority', 'due_date', 'assigned_to_name']
};

export const FieldPicker: React.FC<Props> = ({ source, config, onChange }) => {
  const fields = SCHEMAS[source] || [];

  const toggleField = (field: string) => {
    const exists = config.columns.find(c => c.field === field);
    let newColumns;
    if (exists) {
      newColumns = config.columns.filter(c => c.field !== field);
    } else {
      newColumns = [...config.columns, { field }];
    }
    onChange({ ...config, columns: newColumns });
  };

  return (
    <div className="space-y-2">
      <label className="font-semibold text-sm text-gray-700">Columns</label>
      <div className="space-y-1">
        {fields.map(field => (
          <div key={field} className="flex items-center space-x-2">
            <input 
              type="checkbox" 
              id={`field-${field}`}
              checked={!!config.columns.find(c => c.field === field)}
              onChange={() => toggleField(field)}
              className="rounded text-blue-600 focus:ring-blue-500"
            />
            <label htmlFor={`field-${field}`} className="text-sm cursor-pointer text-gray-700">{field}</label>
          </div>
        ))}
      </div>
    </div>
  );
};




