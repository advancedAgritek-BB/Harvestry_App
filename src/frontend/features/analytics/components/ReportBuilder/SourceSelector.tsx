import React from 'react';

interface Props {
  value: string;
  onChange: (val: string) => void;
}

export const SourceSelector: React.FC<Props> = ({ value, onChange }) => {
  return (
    <div className="space-y-2">
      <label className="font-semibold text-sm text-gray-700">Data Source</label>
      <select 
        value={value} 
        onChange={(e) => onChange(e.target.value)}
        className="w-full border p-2 rounded bg-white text-gray-900"
      >
        <option value="">Select Source...</option>
        <option value="harvests">Harvests</option>
        <option value="tasks">Tasks</option>
      </select>
    </div>
  );
};





