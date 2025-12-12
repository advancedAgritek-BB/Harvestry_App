// src/frontend/app/api/v1/simulation/active/route.ts
// Returns active simulation states

import { NextResponse } from 'next/server';
import { simulationStateStore } from '@/lib/simulation-state-store';

export async function GET() {
  try {
    const activeSimulations = simulationStateStore.getActive();
    return NextResponse.json(activeSimulations);
  } catch (error) {
    console.error('Error fetching active simulations:', error);
    return NextResponse.json([], { status: 200 });
  }
}









