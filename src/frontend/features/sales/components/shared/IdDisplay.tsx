'use client';

import { useState } from 'react';
import { Copy, Check } from 'lucide-react';

interface IdDisplayProps {
  id: string;
  /** Number of characters to show before truncating (default: 8) */
  truncateAt?: number;
  /** Show full ID on hover (default: true) */
  showFullOnHover?: boolean;
  /** Label to show before the ID */
  label?: string;
  /** Additional class names */
  className?: string;
}

/**
 * Displays a truncated GUID/ID with copy-to-clipboard functionality.
 * Shows full ID on hover and provides visual feedback on copy.
 */
export function IdDisplay({
  id,
  truncateAt = 8,
  showFullOnHover = true,
  label,
  className = '',
}: IdDisplayProps) {
  const [copied, setCopied] = useState(false);

  const truncatedId = id.length > truncateAt ? `${id.slice(0, truncateAt)}...` : id;

  async function handleCopy() {
    try {
      await navigator.clipboard.writeText(id);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  }

  return (
    <div className={`inline-flex items-center gap-1.5 ${className}`}>
      {label && <span className="text-xs text-muted-foreground">{label}</span>}
      <span
        className="text-xs font-mono text-muted-foreground cursor-default"
        title={showFullOnHover ? id : undefined}
      >
        {truncatedId}
      </span>
      <button
        type="button"
        onClick={handleCopy}
        className="p-0.5 rounded hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
        title="Copy full ID"
      >
        {copied ? (
          <Check className="w-3 h-3 text-emerald-400" />
        ) : (
          <Copy className="w-3 h-3" />
        )}
      </button>
    </div>
  );
}

/**
 * Inline version for use in tables/lists - just the truncated ID with copy
 */
export function IdCopyCell({ id }: { id: string }) {
  const [copied, setCopied] = useState(false);

  async function handleCopy(e: React.MouseEvent) {
    e.stopPropagation();
    try {
      await navigator.clipboard.writeText(id);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy:', err);
    }
  }

  return (
    <span
      className="inline-flex items-center gap-1 cursor-pointer hover:text-foreground transition-colors"
      onClick={handleCopy}
      title={`Click to copy: ${id}`}
    >
      <span className="font-mono text-xs">{id.slice(0, 8)}...</span>
      {copied ? (
        <Check className="w-3 h-3 text-emerald-400" />
      ) : (
        <Copy className="w-3 h-3 opacity-50" />
      )}
    </span>
  );
}
