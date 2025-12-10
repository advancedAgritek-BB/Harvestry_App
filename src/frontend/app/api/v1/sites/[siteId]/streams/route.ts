// src/frontend/app/api/v1/sites/[siteId]/streams/route.ts

import { NextRequest, NextResponse } from 'next/server';
import { simulationDataStore } from '@/lib/simulation-store';

interface RouteParams {
  params: Promise<{ siteId: string }>;
}

export async function GET(request: NextRequest, { params }: RouteParams) {
  try {
    const { siteId } = await params;
    const streams = simulationDataStore.getStreams(siteId);
    return NextResponse.json(streams);
  } catch (error) {
    console.error('Error fetching streams:', error);
    return NextResponse.json({ error: 'Failed to fetch streams' }, { status: 500 });
  }
}

export async function POST(request: NextRequest, { params }: RouteParams) {
  try {
    const { siteId } = await params;
    const body = await request.json();

    if (!body.equipmentId) {
      return NextResponse.json(
        { error: 'Equipment ID is required' },
        { status: 400 }
      );
    }

    if (!body.displayName || typeof body.displayName !== 'string' || body.displayName.trim() === '') {
      return NextResponse.json(
        { error: 'Display name is required' },
        { status: 400 }
      );
    }

    if (body.streamType === undefined || body.streamType === null) {
      return NextResponse.json(
        { error: 'Stream type is required' },
        { status: 400 }
      );
    }

    if (body.unit === undefined || body.unit === null) {
      return NextResponse.json(
        { error: 'Unit is required' },
        { status: 400 }
      );
    }

    const stream = simulationDataStore.createStream({
      siteId,
      equipmentId: body.equipmentId,
      equipmentChannelId: body.equipmentChannelId,
      streamType: body.streamType,
      unit: body.unit,
      displayName: body.displayName.trim(),
      locationId: body.locationId,
      roomId: body.roomId,
      zoneId: body.zoneId
    });

    return NextResponse.json(stream, { status: 201 });
  } catch (error) {
    console.error('Error creating stream:', error);
    return NextResponse.json(
      { error: 'Failed to create stream' },
      { status: 500 }
    );
  }
}








