'use client';

export function SectionCard({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="bg-surface border border-border rounded-xl p-4 space-y-3">
      <div className="text-sm font-medium text-foreground">{title}</div>
      {children}
    </div>
  );
}

