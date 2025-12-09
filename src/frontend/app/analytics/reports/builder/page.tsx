'use client';

import Link from 'next/link';
import React from 'react';
import { ArrowLeft } from 'lucide-react';

export default function ReportBuilderPage() {
  return (
    <div className="p-6 bg-white min-h-screen">
      <div className="mb-6">
        <Link href="/analytics/reports" className="inline-flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-4">
          <ArrowLeft className="h-4 w-4" />
          Back to Reports
        </Link>
        <h1 className="text-2xl font-bold text-gray-900">Report Builder</h1>
      </div>

      <div className="text-center py-16 bg-gray-50 rounded-lg border border-dashed border-gray-300">
        <h2 className="text-xl font-semibold text-gray-700 mb-2">Coming Soon</h2>
        <p className="text-gray-500">Custom report builder is under development.</p>
        <p className="text-sm text-gray-400 mt-4">Connect your backend to enable this feature.</p>
      </div>
    </div>
  );
}
