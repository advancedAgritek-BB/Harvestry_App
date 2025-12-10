// src/frontend/app/api/v1/simulation/start/route.ts
// Start simulation for a stream type

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
    const result = simulationStateStore.startByType(streamType);
    return NextResponse.json({ message: result });
  } catch (error) {
    console.error('Error starting simulation:', error);
    return NextResponse.json(
      { error: 'Failed to start simulation' },
      { status: 500 }
    );
  }
}








