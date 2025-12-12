import React from 'react';

interface Props {
  data: any[] | undefined;
  isLoading: boolean;
  error: any;
}

export const Visualizer: React.FC<Props> = ({ data, isLoading, error }) => {
  if (isLoading) return <div className="flex items-center justify-center h-full text-gray-500">Loading...</div>;
  if (error) return <div className="text-red-600 p-4">Error: {error.message || 'Unknown error'}</div>;
  if (!data || data.length === 0) return <div className="flex items-center justify-center h-full text-gray-400">No data available to preview</div>;

  const columns = Object.keys(data[0]);

  return (
    <div className="flex-1 overflow-auto w-full bg-white shadow rounded">
      <table className="min-w-full divide-y divide-gray-200">
        <thead className="bg-gray-50 sticky top-0">
          <tr>
            {columns.map(col => (
              <th key={col} className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                {col}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-gray-200">
          {data.map((row, i) => (
            <tr key={i} className="hover:bg-gray-50">
              {columns.map(col => (
                <td key={col} className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {row[col] === null ? '-' : (typeof row[col] === 'object' ? JSON.stringify(row[col]) : row[col].toString())}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};





