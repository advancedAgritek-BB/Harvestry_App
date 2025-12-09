'use client';

import Link from 'next/link';
import React from 'react';

export default function DashboardsListPage() {
  return (
    <div className="p-8 space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold text-gray-900">Analytics Dashboards</h1>
        <Link href="/analytics/dashboards/builder" className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700">
          Create New Dashboard
        </Link>
      </div>

      <div className="text-center py-16 bg-gray-50 rounded-lg border border-dashed border-gray-300">
        <h2 className="text-xl font-semibold text-gray-700 mb-2">Coming Soon</h2>
        <p className="text-gray-500">Custom analytics dashboards are under development.</p>
        <p className="text-sm text-gray-400 mt-4">Connect your backend to enable this feature.</p>
      </div>
    </div>
  );
}
