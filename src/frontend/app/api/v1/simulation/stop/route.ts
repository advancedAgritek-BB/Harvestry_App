// src/frontend/app/api/v1/simulation/stop/route.ts
// Stop simulation for a stream type

import { NextRequest, NextResponse } from 'next/server';
import { simulationStateStore } from '@/lib/simulation-state-store';

export async function POST(request: NextRequest) {
  try {
    const { searchParams } = new URL(request.url);
    const typeParam = searchParams.get('type');
    
    if (!typeParam) {
      return NextResponse.json(
        { error: 'Stream type is required' },
        { status: 400 }
      );
    }

    const streamType = parseInt(typeParam, 10);
    const result = simulationStateStore.stopByType(streamType);
    return NextResponse.json({ message: result });
  } catch (error) {
    console.error('Error stopping simulation:', error);
    return NextResponse.json(
      { error: 'Failed to stop simulation' },
      { status: 500 }
    );
  }
}



