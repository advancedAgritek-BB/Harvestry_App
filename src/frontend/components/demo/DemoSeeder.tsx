'use client';

import { useEffect } from 'react';
import { usePlannerStore } from '@/features/planner/stores/plannerStore';
import { useInventoryStore } from '@/features/inventory/stores/inventoryStore';
import { useAlertsStore, Alert } from '@/stores/alertsStore';
import { useOverridesStore, Override } from '@/stores/overridesStore';
import { MOCK_BATCHES, MOCK_LOTS } from '@/lib/demo-data';

const MOCK_ALERTS: Alert[] = [
  {
    id: '1',
    title: 'HVAC-01 Malfunction',
    source: 'Environment',
    severity: 'critical',
    timestamp: new Date(Date.now() - 1000 * 60 * 30).toISOString(), // 30 mins ago
    dismissed: false,
  },
  {
    id: '2',
    title: 'Humidity High (72%)',
    source: 'Room B',
    severity: 'warning',
    timestamp: new Date(Date.now() - 1000 * 60 * 45).toISOString(), // 45 mins ago
    dismissed: false,
  },
  {
    id: '3',
    title: 'Water Tank Refilled',
    source: 'Irrigation',
    severity: 'info',
    timestamp: new Date(Date.now() - 1000 * 60 * 60 * 2).toISOString(), // 2 hours ago
    dismissed: false,
  },
];

// Calculate expiry time (e.g., 18:00 today or tomorrow if past)
const getExpiryTime = (hour: number): string => {
  const now = new Date();
  const expiry = new Date();
  expiry.setHours(hour, 0, 0, 0);
  
  // If the time has passed today, set it for tomorrow
  if (expiry <= now) {
    expiry.setDate(expiry.getDate() + 1);
  }
  
  return expiry.toISOString();
};

const MOCK_OVERRIDES: Override[] = [
  {
    id: 'override-co2-1',
    type: 'manual',
    label: 'CO₂ Override',
    details: '1100ppm until 18:00',
    severity: 'critical',
    metric: 'co2',
    targetValue: 1100,
    unit: 'ppm',
    expiresAt: getExpiryTime(18),
    createdAt: new Date(Date.now() - 1000 * 60 * 60).toISOString(), // 1 hour ago
    roomId: 'f1',
    isActive: true,
  },
  {
    id: 'override-recipe-1',
    type: 'recipe',
    label: 'Recipe: Flower v2',
    details: 'EC 2.5, pH 5.8, PPFD 900',
    severity: 'info',
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24).toISOString(), // 1 day ago
    roomId: 'f1',
    isActive: true,
  },
];

export function DemoSeeder() {
  const setBatches = usePlannerStore((state) => state.setBatches);
  const batches = usePlannerStore((state) => state.batches);
  
  const setLots = useInventoryStore((state) => state.setLots);
  const lots = useInventoryStore((state) => state.lots);

  const setAlerts = useAlertsStore((state) => state.setAlerts);
  const alerts = useAlertsStore((state) => state.alerts);

  const setOverrides = useOverridesStore((state) => state.setOverrides);
  const overrides = useOverridesStore((state) => state.overrides);

  useEffect(() => {
    // Only run in mock/demo mode
    if (process.env.NEXT_PUBLIC_USE_MOCK_AUTH !== 'true') return;

    // Seed Planner Batches if empty
    if (batches.length === 0) {
      console.log('🌱 Seeding demo batches...');
      setBatches(MOCK_BATCHES);
    }

    // Seed Inventory Lots if empty
    if (lots.length === 0) {
      console.log('🌱 Seeding demo inventory...');
      setLots(MOCK_LOTS, MOCK_LOTS.length);
    }

    // Seed Alerts if empty
    if (alerts.length === 0) {
      console.log('🌱 Seeding demo alerts...');
      setAlerts(MOCK_ALERTS);
    }

    // Seed Overrides if empty
    if (overrides.length === 0) {
      console.log('🌱 Seeding demo overrides...');
      setOverrides(MOCK_OVERRIDES);
    }
  }, [batches.length, lots.length, alerts.length, overrides.length, setBatches, setLots, setAlerts, setOverrides]);

  return null;
}
