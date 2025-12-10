// src/frontend/app/api/v1/sites/[siteId]/equipment/route.ts

import { NextRequest, NextResponse } from 'next/server';
import { simulationDataStore } from '@/lib/simulation-store';

interface RouteParams {
  params: Promise<{ siteId: string }>;
}

export async function GET(request: NextRequest, { params }: RouteParams) {
  try {
    const { siteId } = await params;
    const equipment = simulationDataStore.getEquipment(siteId);
    return NextResponse.json({ items: equipment });
  } catch (error) {
    console.error('Error fetching equipment:', error);
    return NextResponse.json({ error: 'Failed to fetch equipment' }, { status: 500 });
  }
}

export async function POST(request: NextRequest, { params }: RouteParams) {
  try {
    const { siteId } = await params;
    const body = await request.json();

    if (!body.code || typeof body.code !== 'string' || body.code.trim() === '') {
      return NextResponse.json(
        { error: 'Equipment code is required' },
        { status: 400 }
      );
    }

    if (!body.coreType) {
      return NextResponse.json(
        { error: 'Equipment core type is required' },
        { status: 400 }
      );
    }

    const equipment = simulationDataStore.createEquipment({
      siteId,
      locationId: body.locationId,
      code: body.code.trim(),
      typeCode: body.typeCode || body.coreType,
      coreType: body.coreType,
      manufacturer: body.manufacturer,
      model: body.model,
      serialNumber: body.serialNumber,
      firmwareVersion: body.firmwareVersion
    });

    return NextResponse.json(equipment, { status: 201 });
  } catch (error) {
    console.error('Error creating equipment:', error);
    return NextResponse.json(
      { error: 'Failed to create equipment' },
      { status: 500 }
    );
  }
}








