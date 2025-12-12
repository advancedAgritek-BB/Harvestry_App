// src/frontend/app/api/v1/simulation/config/route.ts
// Update simulation profile/configuration

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
    const profile = await request.json();
    
    const result = simulationStateStore.updateProfile(streamType, profile);
    return NextResponse.json({ message: result });
  } catch (error) {
    console.error('Error updating simulation config:', error);
    return NextResponse.json(
      { error: 'Failed to update simulation config' },
      { status: 500 }
    );
  }
}









