'use client';

import React, { useMemo, useState, useRef, useEffect } from 'react';
import { Sparkles, Send, Bot, User, Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';
import { AssistantMessage, useAssistantChat } from './useAssistantChat';
import { useCurrentSector } from './useCurrentSector';
import { getSectorConfig, AssistantSector, SectorSuggestion } from './sectorConfig';

type AssistantPanelProps = {
  roomId?: string;
  phase?: string;
  /** Override automatic sector detection */
  sectorOverride?: AssistantSector;
};

export function AssistantPanel({ roomId, phase, sectorOverride }: AssistantPanelProps) {
  const detectedSector = useCurrentSector();
  const sector = sectorOverride ?? detectedSector;
  const sectorConfig = getSectorConfig(sector);
  
  const { messages, sendMessage, isLoading, error } = useAssistantChat({ 
    roomId, 
    phase,
    sector,
  });
  const [input, setInput] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const sortedMessages = useMemo(
    () => [...messages].sort((a, b) => a.timestamp - b.timestamp),
    [messages]
  );

  // Auto-scroll to bottom on new messages
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [sortedMessages]);

  const handleSend = async (message?: string) => {
    const toSend = message ?? input;
    if (!toSend.trim() || isLoading) return;
    if (!message) setInput('');
    await sendMessage(toSend);
    inputRef.current?.focus();
  };

  return (
    <div className="flex flex-col h-full bg-surface/50 border border-border rounded-xl overflow-hidden">
      {/* Header */}
      <div className="px-4 py-3 border-b border-border/50 bg-gradient-to-r from-violet-500/5 to-transparent">
        <div className="flex items-center gap-3">
          <div className="icon-glow-violet p-2 rounded-lg">
            <Sparkles className="w-4 h-4 text-violet-400" />
          </div>
          <div className="flex-1 min-w-0">
            <h3 className="text-sm font-semibold text-foreground tracking-tight">
              Assistant
            </h3>
            <p className="text-[11px] text-muted-foreground truncate">
              {phase ? `${phase} room insights` : sectorConfig.title}
            </p>
          </div>
          {isLoading && (
            <div className="flex items-center gap-1.5 px-2 py-1 rounded-full bg-violet-500/10">
              <Loader2 className="w-3 h-3 text-violet-400 animate-spin" />
              <span className="text-[10px] font-medium text-violet-300">Thinking</span>
            </div>
          )}
        </div>
      </div>

      {/* Messages Area */}
      <div className="flex-1 overflow-y-auto px-3 py-3 space-y-3 custom-scrollbar min-h-0">
        {sortedMessages.length === 0 && (
          <EmptyState 
            sectorConfig={sectorConfig} 
            onSuggestionClick={(question) => handleSend(question)} 
          />
        )}
        {sortedMessages.map((message, index) => (
          <MessageBubble key={index} message={message} />
        ))}
        <div ref={messagesEndRef} />
      </div>

      {/* Input Area */}
      <div className="p-3 border-t border-border/50 bg-gradient-to-t from-background/50 to-transparent">
        {error && (
          <div className="mb-2 px-2 py-1.5 rounded-md bg-amber-500/10 border border-amber-500/20">
            <p className="text-[11px] text-amber-400">{error}</p>
          </div>
        )}
        <div className="flex gap-2 items-center">
          <input
            ref={inputRef}
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                handleSend();
              }
            }}
            className={cn(
              "flex-1 rounded-lg border border-border/60 bg-muted/30 px-3 py-2 text-sm",
              "placeholder:text-muted-foreground/50 outline-none",
              "focus:border-violet-500/50 focus:ring-2 focus:ring-violet-500/20",
              "transition-all duration-200"
            )}
            placeholder={sectorConfig.placeholder}
            disabled={isLoading}
          />
          <button
            type="button"
            onClick={() => handleSend()}
            disabled={isLoading || !input.trim()}
            className={cn(
              "p-2.5 rounded-lg transition-all duration-200",
              "bg-violet-500/20 text-violet-300 border border-violet-500/30",
              "hover:bg-violet-500/30 hover:text-violet-200 hover:border-violet-500/50",
              "disabled:opacity-40 disabled:cursor-not-allowed disabled:hover:bg-violet-500/20",
              "focus:outline-none focus:ring-2 focus:ring-violet-500/30"
            )}
            aria-label="Send message"
          >
            <Send className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  );
}

type EmptyStateProps = {
  sectorConfig: ReturnType<typeof getSectorConfig>;
  onSuggestionClick: (question: string) => void;
};

function EmptyState({ sectorConfig, onSuggestionClick }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center h-full py-6 px-4 text-center">
      <div className="w-12 h-12 rounded-full bg-gradient-to-br from-violet-500/20 to-violet-500/5 flex items-center justify-center mb-3">
        <Bot className="w-6 h-6 text-violet-400/70" />
      </div>
      <p className="text-xs text-muted-foreground mb-3 leading-relaxed max-w-[180px]">
        {sectorConfig.description}
      </p>
      <div className="flex flex-wrap justify-center gap-1.5">
        {sectorConfig.suggestions.map((item: SectorSuggestion) => (
          <button
            key={item.label}
            onClick={() => onSuggestionClick(item.question)}
            className={cn(
              "px-2 py-0.5 text-[10px] font-medium rounded-full",
              "bg-muted/50 text-muted-foreground/80 border border-border/50",
              "hover:bg-violet-500/10 hover:text-violet-300 hover:border-violet-500/30",
              "transition-colors duration-150"
            )}
          >
            {item.label}
          </button>
        ))}
      </div>
    </div>
  );
}

function MessageBubble({ message }: { message: AssistantMessage }) {
  const isAssistant = message.role === 'assistant';

  return (
    <div className={cn(
      "flex gap-2",
      isAssistant ? "justify-start" : "justify-end"
    )}>
      {isAssistant && (
        <div className="flex-shrink-0 w-6 h-6 rounded-full bg-violet-500/20 flex items-center justify-center mt-0.5">
          <Bot className="w-3.5 h-3.5 text-violet-400" />
        </div>
      )}
      
      <div className={cn(
        "max-w-[85%] rounded-xl px-3 py-2 text-sm",
        isAssistant 
          ? "bg-muted/40 text-foreground border border-border/30 rounded-tl-sm" 
          : "bg-violet-500/20 text-violet-100 border border-violet-500/20 rounded-tr-sm"
      )}>
        <p className="whitespace-pre-wrap leading-relaxed">{message.content}</p>
        {message.meta?.source && (
          <p className="text-[10px] mt-1.5 pt-1.5 border-t border-border/30 text-muted-foreground/70 flex items-center gap-1">
            <span className="opacity-60">Source:</span> {message.meta.source}
          </p>
        )}
      </div>

      {!isAssistant && (
        <div className="flex-shrink-0 w-6 h-6 rounded-full bg-violet-500/30 flex items-center justify-center mt-0.5">
          <User className="w-3.5 h-3.5 text-violet-300" />
        </div>
      )}
    </div>
  );
}




