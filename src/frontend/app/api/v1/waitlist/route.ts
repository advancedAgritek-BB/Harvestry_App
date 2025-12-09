// src/frontend/app/api/v1/waitlist/route.ts
// Waitlist registration API endpoint

import { NextRequest, NextResponse } from 'next/server';
import { createClient } from '@supabase/supabase-js';

interface WaitlistEntry {
  email: string;
  name?: string;
  company?: string;
  facility_size?: string;
  source?: string;
}

// Initialize Supabase client
const supabaseUrl = process.env.NEXT_PUBLIC_SUPABASE_URL || '';
const supabaseServiceKey = process.env.SUPABASE_SERVICE_ROLE_KEY || '';

export async function POST(request: NextRequest) {
  try {
    const body: WaitlistEntry = await request.json();
    
    // Validate email
    if (!body.email || !isValidEmail(body.email)) {
      return NextResponse.json(
        { success: false, error: 'Valid email address is required' },
        { status: 400 }
      );
    }

    // Store in database
    const dbResult = await storeWaitlistEntry(body);
    
    // Send notification email
    await sendNotificationEmail(body);

    return NextResponse.json({
      success: true,
      message: 'Successfully joined the waitlist!',
      id: dbResult?.id
    });
  } catch (error) {
    console.error('Waitlist registration error:', error);
    return NextResponse.json(
      { success: false, error: 'Failed to register. Please try again.' },
      { status: 500 }
    );
  }
}

function isValidEmail(email: string): boolean {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

async function storeWaitlistEntry(entry: WaitlistEntry) {
  // If Supabase is configured, store in database
  if (supabaseUrl && supabaseServiceKey) {
    const supabase = createClient(supabaseUrl, supabaseServiceKey);
    
    const { data, error } = await supabase
      .from('waitlist_entries')
      .insert({
        email: entry.email.toLowerCase().trim(),
        name: entry.name?.trim() || null,
        company: entry.company?.trim() || null,
        facility_size: entry.facility_size || null,
        source: entry.source || 'landing_page',
        created_at: new Date().toISOString(),
      })
      .select('id')
      .single();

    if (error) {
      // If duplicate email, that's okay - just log it
      if (error.code === '23505') {
        console.log('Duplicate waitlist entry:', entry.email);
        return { id: 'existing' };
      }
      console.error('Database error:', error);
      throw error;
    }

    return data;
  }
  
  // Fallback: log to console if no database
  console.log('Waitlist entry (no DB configured):', entry);
  return { id: 'logged' };
}

async function sendNotificationEmail(entry: WaitlistEntry) {
  // Send email notification to register@harvestry.io
  const emailContent = `
New Waitlist Registration

Email: ${entry.email}
Name: ${entry.name || 'Not provided'}
Company: ${entry.company || 'Not provided'}
Facility Size: ${entry.facility_size || 'Not provided'}
Source: ${entry.source || 'landing_page'}
Timestamp: ${new Date().toISOString()}
  `.trim();

  // If using a transactional email service (e.g., Resend, SendGrid)
  const resendApiKey = process.env.RESEND_API_KEY;
  
  if (resendApiKey) {
    try {
      const response = await fetch('https://api.resend.com/emails', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${resendApiKey}`,
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          from: 'Harvestry Waitlist <noreply@harvestry.io>',
          to: ['register@harvestry.io'],
          subject: `New Waitlist Signup: ${entry.email}`,
          text: emailContent,
        }),
      });

      if (!response.ok) {
        console.error('Email send failed:', await response.text());
      }
    } catch (emailError) {
      console.error('Email notification error:', emailError);
      // Don't throw - email failure shouldn't break registration
    }
  } else {
    // Log email content if no email service configured
    console.log('Email notification (no service configured):', emailContent);
  }
}

