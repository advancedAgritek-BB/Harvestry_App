// src/frontend/app/api/v1/sites/route.ts
// Sites endpoint - fallback for when backend is not running

import { NextRequest, NextResponse } from 'next/server';
import { simulationDataStore } from '@/lib/simulation-store';

export async function GET() {
  try {
    const sites = simulationDataStore.getSites();
    return NextResponse.json(sites);
  } catch (error) {
    console.error('Error fetching sites:', error);
    return NextResponse.json({ error: 'Failed to fetch sites' }, { status: 500 });
  }
}

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    
    if (!body.name || typeof body.name !== 'string' || body.name.trim() === '') {
      return NextResponse.json(
        { error: 'Site name is required' },
        { status: 400 }
      );
    }

    const site = simulationDataStore.createSite({
      name: body.name.trim(),
      organizationId: body.organizationId
    });

    return NextResponse.json(site, { status: 201 });
  } catch (error) {
    console.error('Error creating site:', error);
    return NextResponse.json(
      { error: 'Failed to create site' },
      { status: 500 }
    );
  }
}








