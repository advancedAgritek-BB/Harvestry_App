import { NextResponse } from 'next/server';
import { cookies } from 'next/headers';

type ChatRequest = {
  message: string;
  context?: {
    roomId?: string;
    phase?: string;
    sector?: string;
    systemContext?: string;
  };
};

type UserContext = {
  userId?: string;
  organizationId?: string;
  siteId?: string;
  role?: string;
};

const OPENAI_URL = 'https://api.openai.com/v1/chat/completions';
const FALLBACK = {
  content:
    'Assistant is unavailable. Verify your OPENAI_API_KEY or backend LLM gateway configuration.',
};

export async function POST(request: Request) {
  const body = (await request.json()) as ChatRequest;
  const apiKey = process.env.OPENAI_API_KEY;

  if (!apiKey) {
    return NextResponse.json(FALLBACK, { status: 200 });
  }

  // Extract user context for data scoping (RLS preparation)
  const userContext = await extractUserContext();
  const systemPrompt = buildSystemPrompt(body.context, userContext);

  try {
    const response = await fetch(OPENAI_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${apiKey}`,
      },
      body: JSON.stringify({
        model: 'gpt-4o-mini',
        messages: [
          { role: 'system', content: systemPrompt },
          { role: 'user', content: body.message },
        ],
        temperature: 0.2,
        max_tokens: 400,
      }),
    });

    if (!response.ok) {
      return NextResponse.json(FALLBACK, { status: 200 });
    }

    const data = await response.json();
    const content = data?.choices?.[0]?.message?.content ?? FALLBACK.content;

    return NextResponse.json({ 
      content,
      meta: {
        sector: body.context?.sector,
      },
    });
  } catch (error) {
    console.error('Assistant call failed', error);
    return NextResponse.json(FALLBACK, { status: 200 });
  }
}

/**
 * Extract user context from auth session for data scoping.
 * This enables RLS-aware responses by identifying the user's organization and site.
 */
async function extractUserContext(): Promise<UserContext> {
  try {
    const cookieStore = await cookies();
    
    // Try to get auth session data from cookies
    // These would be set by your auth system (Supabase, NextAuth, etc.)
    const authSession = cookieStore.get('auth-session')?.value;
    
    if (authSession) {
      try {
        const session = JSON.parse(authSession);
        return {
          userId: session.userId,
          organizationId: session.organizationId,
          siteId: session.siteId,
          role: session.role,
        };
      } catch {
        // Invalid session JSON, continue without user context
      }
    }

    // Alternative: Check for Supabase session
    const supabaseAuth = cookieStore.get('sb-access-token')?.value;
    if (supabaseAuth) {
      // In production, you would decode the JWT to extract claims
      // For now, return empty context to indicate authenticated but no details
      return {};
    }

    return {};
  } catch {
    return {};
  }
}

function buildSystemPrompt(
  context?: ChatRequest['context'],
  userContext?: UserContext
): string {
  const sector = context?.sector ?? 'general';
  const systemContext = context?.systemContext ?? 'general facility operations';
  const room = context?.roomId ?? 'facility-wide';
  const phase = context?.phase;

  const promptParts = [
    'You are the Harvestry AI assistant for cannabis cultivation operations.',
    '',
    '## Your Role',
    `You are currently assisting with: ${systemContext}.`,
    'Provide concise, actionable answers with bullet points when appropriate.',
    'Always cite data sources when referencing specific metrics or readings.',
    '',
    '## Constraints',
    '- Use only the provided context and available data; never fabricate data.',
    '- Refuse any direct actuation or control commands; provide guidance only.',
    '- If you don\'t have enough information, ask clarifying questions.',
    '- Keep responses focused on the current sector unless explicitly asked otherwise.',
    '',
    '## Current Context',
    `- Sector: ${sector}`,
    `- Scope: ${room}`,
  ];

  if (phase) {
    promptParts.push(`- Growth Phase: ${phase}`);
  }

  // Add user context for data scoping awareness
  if (userContext?.organizationId) {
    promptParts.push('', '## Data Scope');
    promptParts.push('Responses should be limited to data the user has access to.');
    if (userContext.siteId) {
      promptParts.push(`- Site-scoped data access`);
    }
    if (userContext.role) {
      promptParts.push(`- User role: ${userContext.role}`);
    }
  }

  return promptParts.join('\n');
}



