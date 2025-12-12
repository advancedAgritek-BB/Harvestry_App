'use client';

import { useMemo, useState } from 'react';
import { AssistantSector, getSectorConfig } from './sectorConfig';

export type AssistantMessage = {
  role: 'user' | 'assistant' | 'system';
  content: string;
  timestamp: number;
  meta?: Record<string, string>;
};

export type AssistantContext = {
  roomId?: string;
  phase?: string;
  sector?: AssistantSector;
};

type ChatRequest = {
  message: string;
  context: AssistantContext & {
    /** System context string from sector config for LLM prompt */
    systemContext?: string;
  };
};

export function useAssistantChat(initialContext: AssistantContext = {}) {
  const [messages, setMessages] = useState<AssistantMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const context = useMemo(() => initialContext, [initialContext]);

  const sendMessage = async (message: string) => {
    if (!message.trim()) {
      return;
    }

    const userMessage: AssistantMessage = {
      role: 'user',
      content: message,
      timestamp: Date.now(),
    };

    setMessages((prev) => [...prev, userMessage]);
    setIsLoading(true);
    setError(null);

    try {
      // Get the system context string for the current sector
      const sectorConfig = getSectorConfig(context.sector);
      
      const payload: ChatRequest = { 
        message, 
        context: {
          ...context,
          systemContext: sectorConfig.systemContext,
        },
      };
      
      const response = await fetch('/api/assistant/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });

      if (!response.ok) {
        throw new Error(`Assistant call failed (${response.status})`);
      }

      const data = await response.json();
      const assistantMessage: AssistantMessage = {
        role: 'assistant',
        content: data?.content ?? 'I could not find an answer.',
        timestamp: Date.now(),
        meta: data?.meta,
      };

      setMessages((prev) => [...prev, assistantMessage]);
    } catch (err) {
      const fallback: AssistantMessage = {
        role: 'assistant',
        content:
          'I could not reach the assistant service. Please retry. If this persists, check connectivity or configuration.',
        timestamp: Date.now(),
      };
      setMessages((prev) => [...prev, fallback]);
      setError(err instanceof Error ? err.message : 'Assistant unavailable');
    } finally {
      setIsLoading(false);
    }
  };

  return { messages, sendMessage, isLoading, error };
}




