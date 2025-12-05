// src/frontend/app/api/v1/simulation/route.ts
// Base simulation endpoint (for completeness)

import { NextResponse } from 'next/server';

export async function GET() {
  return NextResponse.json({ 
    status: 'ok',
    message: 'Simulation API active' 
  });
}




