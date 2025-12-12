'use client';

import React, { useState, useRef, useEffect, useMemo } from 'react';
import { X, Send, Bot, User, Loader2, Minimize2 } from 'lucide-react';
import { cn } from '@/lib/utils';
import { AssistantMessage, useAssistantChat } from './useAssistantChat';
import { useCurrentSector } from './useCurrentSector';
import { getSectorConfig, SectorSuggestion, AssistantSector } from './sectorConfig';

type FloatingAssistantProps = {
  /** Override automatic sector detection */
  sectorOverride?: AssistantSector;
  /** Default context for the assistant */
  defaultContext?: {
    roomId?: string;
    phase?: string;
  };
};

export function FloatingAssistant({ sectorOverride, defaultContext }: FloatingAssistantProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [isMinimized, setIsMinimized] = useState(false);
  const detectedSector = useCurrentSector();
  const sector = sectorOverride ?? detectedSector;

  return (
    <>
      {/* Floating Button */}
      {!isOpen && (
        <button
          onClick={() => setIsOpen(true)}
          className={cn(
            "fixed bottom-12 right-6 z-50",
            "w-14 h-14 rounded-full",
            "bg-surface border border-emerald-500/50",
            "shadow-lg",
            "flex items-center justify-center",
            "hover:border-emerald-400 hover:bg-emerald-500/10",
            "active:scale-95",
            "transition-all duration-200 ease-out"
          )}
          aria-label="Open Assistant"
        >
          <Bot className="w-7 h-7 text-emerald-400" />
        </button>
      )}

      {/* Chat Panel */}
      {isOpen && (
        <ChatPanel
          isMinimized={isMinimized}
          onMinimize={() => setIsMinimized(!isMinimized)}
          onClose={() => {
            setIsOpen(false);
            setIsMinimized(false);
          }}
          sector={sector}
          defaultContext={defaultContext}
        />
      )}
    </>
  );
}

type ChatPanelProps = {
  isMinimized: boolean;
  onMinimize: () => void;
  onClose: () => void;
  sector: AssistantSector;
  defaultContext?: FloatingAssistantProps['defaultContext'];
};

function ChatPanel({ isMinimized, onMinimize, onClose, sector, defaultContext }: ChatPanelProps) {
  const sectorConfig = getSectorConfig(sector);
  const { messages, sendMessage, isLoading, error } = useAssistantChat({
    ...defaultContext,
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

  // Focus input when opened
  useEffect(() => {
    if (!isMinimized) {
      inputRef.current?.focus();
    }
  }, [isMinimized]);

  const handleSend = async (message?: string) => {
    const toSend = message ?? input;
    if (!toSend.trim() || isLoading) return;
    if (!message) setInput('');
    await sendMessage(toSend);
    inputRef.current?.focus();
  };

  return (
    <div
      className={cn(
        "fixed bottom-12 right-6 z-50",
        "w-[380px] bg-surface border border-border rounded-xl",
        "shadow-2xl shadow-black/20",
        "flex flex-col overflow-hidden",
        "animate-in slide-in-from-bottom-4 fade-in duration-300",
        isMinimized ? "h-[52px]" : "h-[480px] max-h-[80vh]"
      )}
    >
      {/* Header */}
      <div 
        className={cn(
          "px-4 py-3 border-b border-border/50",
          "flex items-center gap-3 cursor-pointer select-none shrink-0"
        )}
        onClick={onMinimize}
      >
        <div className="w-8 h-8 rounded-full bg-emerald-500/15 border border-emerald-500/30 flex items-center justify-center">
          <Bot className="w-4 h-4 text-emerald-400" />
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-sm font-semibold text-foreground tracking-tight">
            Assistant
          </h3>
          <p className="text-[11px] text-muted-foreground truncate">
            {isLoading ? 'Thinking...' : sectorConfig.title}
          </p>
        </div>
        <div className="flex items-center gap-1">
          {isLoading && (
            <div className="px-2 py-1 rounded-full bg-emerald-500/10 mr-1">
              <Loader2 className="w-3 h-3 text-emerald-400 animate-spin" />
            </div>
          )}
          <button
            onClick={(e) => {
              e.stopPropagation();
              onMinimize();
            }}
            className="p-1.5 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
            aria-label={isMinimized ? "Expand" : "Minimize"}
          >
            <Minimize2 className="w-4 h-4" />
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation();
              onClose();
            }}
            className="p-1.5 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
            aria-label="Close"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Chat Content - Hidden when minimized */}
      {!isMinimized && (
        <>
          {/* Messages Area */}
          <div className="flex-1 overflow-y-auto px-4 py-4 space-y-4 custom-scrollbar min-h-0">
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
          <div className="p-3 border-t border-border/50 shrink-0">
            {error && (
              <div className="mb-2 px-3 py-2 rounded-lg bg-amber-500/10 border border-amber-500/20">
                <p className="text-xs text-amber-400">{error}</p>
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
                  "flex-1 rounded-lg border border-border bg-muted/30 px-3 py-2 text-sm",
                  "placeholder:text-muted-foreground/50 outline-none",
                  "focus:border-emerald-500/50 focus:ring-1 focus:ring-emerald-500/20",
                  "transition-colors duration-150"
                )}
                placeholder={sectorConfig.placeholder}
                disabled={isLoading}
              />
              <button
                type="button"
                onClick={() => handleSend()}
                disabled={isLoading || !input.trim()}
                className={cn(
                  "p-2 rounded-lg transition-colors duration-150",
                  "bg-emerald-500/15 text-emerald-400 border border-emerald-500/30",
                  "hover:bg-emerald-500/25 hover:text-emerald-300",
                  "disabled:opacity-40 disabled:cursor-not-allowed",
                  "focus:outline-none focus:ring-1 focus:ring-emerald-500/30"
                )}
                aria-label="Send message"
              >
                <Send className="w-4 h-4" />
              </button>
            </div>
          </div>
        </>
      )}
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
      <div className="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center mb-3">
        <Bot className="w-6 h-6 text-emerald-400/70" />
      </div>
      <p className="text-xs text-muted-foreground mb-4 leading-relaxed max-w-[240px]">
        {sectorConfig.description}
      </p>
      <div className="w-full space-y-2">
        <p className="text-[10px] uppercase tracking-wider text-muted-foreground/50 font-medium">
          Try asking about
        </p>
        <div className="grid grid-cols-2 gap-1.5">
          {sectorConfig.suggestions.map((item: SectorSuggestion) => (
            <button
              key={item.label}
              onClick={() => onSuggestionClick(item.question)}
              className={cn(
                "px-2.5 py-1.5 text-[11px] font-medium rounded-md text-center",
                "bg-muted/40 text-muted-foreground",
                "hover:bg-emerald-500/10 hover:text-emerald-300",
                "transition-colors duration-150"
              )}
            >
              {item.label}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}

function MessageBubble({ message }: { message: AssistantMessage }) {
  const isAssistant = message.role === 'assistant';

  return (
    <div className={cn(
      "flex gap-2.5",
      isAssistant ? "justify-start" : "justify-end"
    )}>
      {isAssistant && (
        <div className="flex-shrink-0 w-6 h-6 rounded-md bg-emerald-500/15 flex items-center justify-center mt-0.5">
          <Bot className="w-3.5 h-3.5 text-emerald-400" />
        </div>
      )}
      
      <div className={cn(
        "max-w-[80%] rounded-lg px-3 py-2 text-sm",
        isAssistant 
          ? "bg-muted/50 text-foreground" 
          : "bg-emerald-500/15 text-foreground border border-emerald-500/20"
      )}>
        <p className="whitespace-pre-wrap leading-relaxed">{message.content}</p>
        {message.meta?.source && (
          <p className="text-[10px] mt-1.5 pt-1.5 border-t border-border/30 text-muted-foreground/60">
            Source: {message.meta.source}
          </p>
        )}
      </div>

      {!isAssistant && (
        <div className="flex-shrink-0 w-6 h-6 rounded-md bg-muted/50 flex items-center justify-center mt-0.5">
          <User className="w-3.5 h-3.5 text-muted-foreground" />
        </div>
      )}
    </div>
  );
}




