'use client';

import React, { useEffect, useState, useCallback } from 'react';
import { 
  RefreshCw, 
  Shield, 
  AlertTriangle, 
  Activity, 
  Cpu, 
  Zap,
  Radio,
  TrendingUp
} from 'lucide-react';
import { simulationService, SimulationState } from '@/features/telemetry/services/simulation.service';
import { useAuthStore, useIsSuperAdmin } from '@/stores/auth';

// Import modular components
import ProvisioningWizard from './components/ProvisioningWizard';
import StreamBrowser from './components/StreamBrowser';
import GlobalControls from './components/GlobalControls';
import ActiveSimulations from './components/ActiveSimulations';
import SiteFeatureManager from './components/SiteFeatureManager';

export default function SimulationPage() {
  const [activeSimulations, setActiveSimulations] = useState<SimulationState[]>([]);
  const [isRefreshing, setIsRefreshing] = useState(false);
  
  // Auth state - Simulator is Super Admin only
  const isSuperAdmin = useIsSuperAdmin();
  const { user } = useAuthStore();

  // Fetch active simulations
  const fetchActive = useCallback(async () => {
    setIsRefreshing(true);
    try {
      const data = await simulationService.getActive();
      setActiveSimulations(data);
    } catch (err) { 
      console.error('Error fetching active simulations:', err); 
    } finally {
      setIsRefreshing(false);
    }
  }, []);

  // Initial fetch and polling
  useEffect(() => {
    fetchActive();
    const interval = setInterval(fetchActive, 5000);
    return () => clearInterval(interval);
  }, [fetchActive]);

  // Callback when a new stream is created
  const handleStreamCreated = useCallback(() => {
    fetchActive();
  }, [fetchActive]);

  // Non-Super Admin users see access denied
  if (!isSuperAdmin) {
    return (
      <div className="min-h-[80vh] flex items-center justify-center p-8">
        <div className="max-w-lg w-full">
          <div className="text-center mb-8">
            <div className="w-20 h-20 rounded-full bg-amber-500/10 flex items-center justify-center mx-auto mb-6">
              <AlertTriangle className="w-10 h-10 text-amber-400" />
            </div>
            <h1 className="text-2xl font-bold mb-2">Access Restricted</h1>
            <p className="text-muted-foreground">
              Super Admin privileges required
            </p>
          </div>
          
          <div className="p-6 bg-gradient-to-br from-amber-500/5 to-orange-500/5 border border-amber-500/20 rounded-2xl">
            <p className="text-sm text-muted-foreground leading-relaxed">
              The Simulation Control panel allows creating virtual sensor data streams 
              for testing and development. This powerful feature is restricted to 
              Super Admin users to maintain data integrity.
            </p>
            <div className="mt-4 flex items-center gap-2 text-xs text-muted-foreground">
              <Shield className="w-4 h-4" />
              <span>Current role: <span className="text-foreground font-medium">{user?.role || 'Unknown'}</span></span>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Calculate stats
  const totalStreams = activeSimulations.length;
  const uniqueTypes = new Set(activeSimulations.map(s => s.stream.streamType)).size;
  const avgValue = totalStreams > 0 
    ? activeSimulations.reduce((sum, s) => sum + s.lastValue, 0) / totalStreams 
    : 0;

  return (
    <div className="space-y-8">
      {/* Hero Header */}
      <div className="relative overflow-hidden rounded-2xl bg-gradient-to-br from-cyan-500/10 via-teal-500/5 to-emerald-500/10 border border-cyan-500/20 p-8">
        {/* Background Pattern */}
        <div className="absolute inset-0 opacity-30">
          <div className="absolute top-0 right-0 w-96 h-96 bg-cyan-500/20 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2" />
          <div className="absolute bottom-0 left-0 w-64 h-64 bg-teal-500/20 rounded-full blur-3xl translate-y-1/2 -translate-x-1/2" />
        </div>
        
        <div className="relative z-10">
          <div className="flex items-start justify-between">
            <div>
              <div className="flex items-center gap-3 mb-4">
                <div className="p-3 rounded-xl bg-cyan-500/20 ring-1 ring-cyan-500/30">
                  <Radio className="w-6 h-6 text-cyan-400" />
                </div>
                <div>
                  <h1 className="text-2xl font-bold">Simulation Control</h1>
                  <p className="text-sm text-muted-foreground">
                    Create and manage virtual sensor data streams
                  </p>
                </div>
              </div>
              
              {/* Quick Stats */}
              <div className="flex gap-6 mt-6">
                <StatCard 
                  icon={Activity} 
                  label="Active Streams" 
                  value={totalStreams}
                  color="cyan"
                />
                <StatCard 
                  icon={Cpu} 
                  label="Stream Types" 
                  value={uniqueTypes}
                  color="teal"
                />
                <StatCard 
                  icon={TrendingUp} 
                  label="Avg Value" 
                  value={avgValue.toFixed(1)}
                  color="emerald"
                />
              </div>
            </div>
            
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-2 px-4 py-2 rounded-xl bg-violet-500/10 border border-violet-500/30">
                <Shield className="w-4 h-4 text-violet-400" />
                <span className="text-sm font-medium text-violet-300">Super Admin</span>
              </div>
              <button 
                onClick={fetchActive} 
                disabled={isRefreshing}
                className="p-3 rounded-xl bg-white/5 hover:bg-white/10 border border-white/10 transition-all disabled:opacity-50"
                title="Refresh"
              >
                <RefreshCw className={`w-5 h-5 ${isRefreshing ? 'animate-spin text-cyan-400' : ''}`} />
              </button>
            </div>
          </div>
        </div>
        
        {/* Pulse Animation for Active Simulations */}
        {totalStreams > 0 && (
          <div className="absolute top-4 right-4 flex items-center gap-2">
            <span className="relative flex h-3 w-3">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
              <span className="relative inline-flex rounded-full h-3 w-3 bg-green-500"></span>
            </span>
            <span className="text-xs text-green-400 font-medium">Live</span>
          </div>
        )}
      </div>

      {/* Site Feature Manager (Collapsible) */}
      <SiteFeatureManager />

      {/* Active Simulations - Full Width at Top */}
      <ActiveSimulations 
        simulations={activeSimulations} 
        onRefresh={fetchActive}
      />

      {/* Control Panels - Three Column Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <ProvisioningWizard onStreamCreated={handleStreamCreated} />
        <StreamBrowser 
          activeSimulations={activeSimulations} 
          onRefresh={fetchActive} 
        />
        <GlobalControls onRefresh={fetchActive} />
      </div>
    </div>
  );
}

// Stat Card Component
function StatCard({ 
  icon: Icon, 
  label, 
  value, 
  color 
}: { 
  icon: React.ElementType; 
  label: string; 
  value: string | number; 
  color: 'cyan' | 'teal' | 'emerald';
}) {
  const colors = {
    cyan: 'text-cyan-400 bg-cyan-500/10',
    teal: 'text-teal-400 bg-teal-500/10',
    emerald: 'text-emerald-400 bg-emerald-500/10'
  };
  
  return (
    <div className="flex items-center gap-3">
      <div className={`p-2 rounded-lg ${colors[color]}`}>
        <Icon className="w-4 h-4" />
      </div>
      <div>
        <div className="text-xl font-bold">{value}</div>
        <div className="text-xs text-muted-foreground">{label}</div>
      </div>
    </div>
  );
}
