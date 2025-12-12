'use client';

import { useState } from 'react';
import {
  BarChart3,
  Download,
  Calendar,
  FileText,
  Users,
  Shield,
  TrendingUp,
} from 'lucide-react';
import { Card, CardHeader, EmptyState, DemoModeBanner } from '@/features/sales/components/shared';

interface ReportCard {
  id: string;
  title: string;
  description: string;
  icon: React.ElementType;
  iconColor: string;
  category: 'sales' | 'compliance';
}

const AVAILABLE_REPORTS: ReportCard[] = [
  {
    id: 'sales-by-customer',
    title: 'Sales by Customer',
    description: 'Order volume and revenue breakdown by customer account',
    icon: Users,
    iconColor: 'text-violet-400',
    category: 'sales',
  },
  {
    id: 'sales-by-period',
    title: 'Sales by Period',
    description: 'Daily, weekly, and monthly sales trends',
    icon: TrendingUp,
    iconColor: 'text-emerald-400',
    category: 'sales',
  },
  {
    id: 'order-fulfillment',
    title: 'Order Fulfillment',
    description: 'Order lifecycle metrics and fulfillment rates',
    icon: FileText,
    iconColor: 'text-blue-400',
    category: 'sales',
  },
  {
    id: 'license-verification',
    title: 'License Verification Audit',
    description: 'Customer license verification history and status changes',
    icon: Shield,
    iconColor: 'text-amber-400',
    category: 'compliance',
  },
  {
    id: 'metrc-submissions',
    title: 'METRC Submissions',
    description: 'Transfer submission history, success rates, and failures',
    icon: BarChart3,
    iconColor: 'text-cyan-400',
    category: 'compliance',
  },
];

export default function SalesReportsPage() {
  const [isDemoMode] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState<'all' | 'sales' | 'compliance'>('all');
  const [dateRange, setDateRange] = useState('30d');

  const filteredReports = AVAILABLE_REPORTS.filter(
    (r) => selectedCategory === 'all' || r.category === selectedCategory
  );

  function handleRunReport(reportId: string) {
    // Placeholder - would generate/download report
    console.log('Running report:', reportId, 'for date range:', dateRange);
    alert(`Report "${reportId}" would be generated here.\n\nThis feature is coming soon!`);
  }

  return (
    <div className="p-6 space-y-6">
      {/* Demo Mode Banner */}
      {isDemoMode && <DemoModeBanner />}

      {/* Controls */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        {/* Category Tabs */}
        <div className="flex items-center gap-2 p-1 bg-muted/30 rounded-lg">
          {(['all', 'sales', 'compliance'] as const).map((category) => (
            <button
              key={category}
              onClick={() => setSelectedCategory(category)}
              className={`px-4 py-2 rounded-md text-sm font-medium transition-colors ${
                selectedCategory === category
                  ? 'bg-amber-500/20 text-amber-400'
                  : 'text-muted-foreground hover:text-foreground'
              }`}
            >
              {category === 'all' ? 'All Reports' : category.charAt(0).toUpperCase() + category.slice(1)}
            </button>
          ))}
        </div>

        {/* Date Range Selector */}
        <div className="flex items-center gap-3">
          <Calendar className="w-4 h-4 text-muted-foreground" />
          <select
            value={dateRange}
            onChange={(e) => setDateRange(e.target.value)}
            aria-label="Select date range"
            className="h-10 px-3 rounded-lg bg-muted/30 border border-border text-sm text-foreground focus:outline-none focus:border-amber-500/30"
          >
            <option value="7d">Last 7 days</option>
            <option value="30d">Last 30 days</option>
            <option value="90d">Last 90 days</option>
            <option value="ytd">Year to date</option>
            <option value="custom">Custom range</option>
          </select>
        </div>
      </div>

      {/* Report Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {filteredReports.map((report) => {
          const Icon = report.icon;
          return (
            <Card key={report.id} className="hover:border-amber-500/30 transition-colors">
              <div className="flex items-start gap-4">
                <div className="w-12 h-12 rounded-xl bg-muted/50 flex items-center justify-center flex-shrink-0">
                  <Icon className={`w-6 h-6 ${report.iconColor}`} />
                </div>
                <div className="flex-1 min-w-0">
                  <h3 className="text-sm font-medium text-foreground mb-1">{report.title}</h3>
                  <p className="text-xs text-muted-foreground mb-3">{report.description}</p>
                  <button
                    onClick={() => handleRunReport(report.id)}
                    className="flex items-center gap-1.5 text-xs text-amber-400 hover:text-amber-300 font-medium transition-colors"
                  >
                    <Download className="w-3.5 h-3.5" />
                    Generate Report
                  </button>
                </div>
              </div>
            </Card>
          );
        })}
      </div>

      {filteredReports.length === 0 && (
        <Card>
          <EmptyState
            icon={BarChart3}
            title="No reports available"
            description="Select a different category to view available reports"
          />
        </Card>
      )}

      {/* Placeholder for report output */}
      <Card>
        <CardHeader
          title="Report Output"
          subtitle="Generated reports will appear here"
        />
        <div className="border-2 border-dashed border-border rounded-lg p-8 text-center">
          <BarChart3 className="w-8 h-8 text-muted-foreground mx-auto mb-3" />
          <p className="text-sm text-muted-foreground">
            Select a report above to generate and view results
          </p>
        </div>
      </Card>
    </div>
  );
}
