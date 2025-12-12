'use client';

import { useEffect, useRef, useState } from 'react';

export function ScannerInput({
  label = 'Scan package label',
  placeholder = 'Scan or paste label then press Enter…',
  disabled,
  onScan,
}: {
  label?: string;
  placeholder?: string;
  disabled?: boolean;
  onScan: (value: string) => void;
}) {
  const [value, setValue] = useState('');
  const inputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    if (!disabled) {
      inputRef.current?.focus();
    }
  }, [disabled]);

  function submit() {
    const v = value.trim();
    if (!v) return;
    onScan(v);
    setValue('');
  }

  return (
    <div className="space-y-1.5">
      <div className="text-xs font-medium text-muted-foreground">{label}</div>
      <input
        ref={inputRef}
        value={value}
        disabled={disabled}
        onChange={(e) => setValue(e.target.value)}
        onKeyDown={(e) => {
          if (e.key === 'Enter') {
            e.preventDefault();
            submit();
          }
        }}
        placeholder={placeholder}
        className="w-full h-10 px-3 rounded-lg bg-muted border border-border text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-violet-500/30 disabled:opacity-60"
      />
    </div>
  );
}

