// src/frontend/app/api/v1/sites/[siteId]/locations/route.ts

import { NextRequest, NextResponse } from 'next/server';
import { simulationDataStore } from '@/lib/simulation-store';

interface RouteParams {
  params: Promise<{ siteId: string }>;
}

export async function GET(request: NextRequest, { params }: RouteParams) {
  try {
    const { siteId } = await params;
    const { searchParams } = new URL(request.url);
    const roomId = searchParams.get('roomId') || undefined;
    
    const locations = simulationDataStore.getLocations(siteId, roomId);
    return NextResponse.json(locations);
  } catch (error) {
    console.error('Error fetching locations:', error);
    return NextResponse.json({ error: 'Failed to fetch locations' }, { status: 500 });
  }
}

export async function POST(request: NextRequest, { params }: RouteParams) {
  try {
    const { siteId } = await params;
    const body = await request.json();

    if (!body.name || typeof body.name !== 'string' || body.name.trim() === '') {
      return NextResponse.json(
        { error: 'Location name is required' },
        { status: 400 }
      );
    }

    if (!body.locationType) {
      return NextResponse.json(
        { error: 'Location type is required' },
        { status: 400 }
      );
    }

    const location = simulationDataStore.createLocation({
      siteId,
      roomId: body.roomId,
      parentLocationId: body.parentLocationId,
      locationType: body.locationType,
      code: body.code || body.name.toUpperCase().replace(/\s+/g, '-'),
      name: body.name.trim()
    });

    return NextResponse.json(location, { status: 201 });
  } catch (error) {
    console.error('Error creating location:', error);
    return NextResponse.json(
      { error: 'Failed to create location' },
      { status: 500 }
    );
  }
}



