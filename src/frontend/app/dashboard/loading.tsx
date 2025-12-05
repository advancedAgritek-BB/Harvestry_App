import { DashboardGrid } from '@/features/dashboard/components/DashboardGrid';
import { cn } from '@/lib/utils';

export default function DashboardLoading() {
  return (
    <div className="flex flex-col h-full animate-pulse">
       {/* Fake Toolbar */}
       <div className="h-[50px] w-full border-b border-border/50 mb-4" />
       
       <DashboardGrid>
          {/* Simulate 6-8 random widgets loading */}
          {[1, 2, 3, 4, 5, 6].map((i) => (
             <div 
               key={i} 
               className={cn(
                 "rounded-xl border border-border/30 bg-surface/50 skeleton-shine",
                 i === 1 ? "col-span-1 md:col-span-2 lg:col-span-8 lg:row-span-1 h-[200px]" : "col-span-1 h-[250px]"
               )}
             />
          ))}
       </DashboardGrid>
    </div>
  );
}
