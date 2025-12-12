// src/frontend/app/api/spatial/v1/rooms/route.ts
// Global rooms endpoint for simulation/test environment

import { NextResponse } from 'next/server';
import { simulationDataStore } from '@/lib/simulation-store';

export async function GET() {
  try {
    const rooms = simulationDataStore.getRooms();
    return NextResponse.json(rooms);
  } catch (error) {
    console.error('Error fetching rooms:', error);
    return NextResponse.json({ error: 'Failed to fetch rooms' }, { status: 500 });
  }
}









