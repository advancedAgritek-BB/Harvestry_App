// src/frontend/app/api/v1/simulation/toggle/[streamId]/route.ts
// Toggle simulation for a specific stream

import { NextRequest, NextResponse } from 'next/server';
import { simulationStateStore } from '@/lib/simulation-state-store';

interface RouteParams {
  params: Promise<{ streamId: string }>;
}

export async function POST(request: NextRequest, { params }: RouteParams) {
  try {
    const { streamId } = await params;
    const result = simulationStateStore.toggle(streamId);
    return NextResponse.json({ message: result });
  } catch (error) {
    console.error('Error toggling simulation:', error);
    return NextResponse.json(
      { error: 'Failed to toggle simulation' },
      { status: 500 }
    );
  }
}



