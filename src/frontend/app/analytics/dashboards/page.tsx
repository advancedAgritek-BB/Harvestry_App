'use client';

// Skip static generation - requires QueryClient at runtime
export const dynamic = 'force-dynamic';

import { useDashboards } from '@/features/analytics/hooks/useDashboards';
import Link from 'next/link';
import React from 'react';

export default function DashboardsListPage() {
  const { data: dashboards, isLoading, error } = useDashboards();

  if (isLoading) return <div className="p-8">Loading dashboards...</div>;
  if (error) return <div className="p-8 text-red-600">Error loading dashboards</div>;

  return (
    <div className="p-8 space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold text-gray-900">Analytics Dashboards</h1>
        <Link href="/analytics/dashboards/builder" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
          Create New Dashboard
        </Link>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {dashboards?.map((dashboard) => (
          <div key={dashboard.id} className="bg-white p-6 rounded-lg shadow hover:shadow-md transition-shadow border border-gray-100">
            <h3 className="text-xl font-semibold mb-2 text-gray-800">{dashboard.name}</h3>
            <p className="text-gray-600 mb-4 line-clamp-2">{dashboard.description || 'No description'}</p>
            <div className="flex justify-between items-center text-sm text-gray-500 mt-4 border-t pt-4">
              <span>Updated: {new Date(dashboard.updatedAt).toLocaleDateString()}</span>
              <span className={`px-2 py-0.5 rounded text-xs ${dashboard.isPublic ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'}`}>
                {dashboard.isPublic ? 'Public' : 'Private'}
              </span>
            </div>
          </div>
        ))}
        {(!dashboards || dashboards.length === 0) && (
          <div className="col-span-full text-center text-gray-500 py-12 bg-gray-50 rounded-lg border border-dashed border-gray-300">
            <p className="text-lg">No dashboards found.</p>
            <p className="text-sm">Create your first dashboard to get started.</p>
          </div>
        )}
      </div>
    </div>
  );
}

