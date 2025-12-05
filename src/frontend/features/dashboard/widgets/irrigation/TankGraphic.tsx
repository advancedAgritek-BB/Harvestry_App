import React from 'react';
import { cn } from '@/lib/utils';

interface TankGraphicProps {
  fillPercentage: number; // 0 to 100
  status?: 'mixing' | 'feeding' | 'filling' | 'evacuating' | 'empty' | 'error' | 'idle';
  className?: string;
}

export function TankGraphic({ fillPercentage, status = 'idle', className }: TankGraphicProps) {
  // Clamping fill percentage
  const fill = Math.min(100, Math.max(0, fillPercentage));
  
  // Dynamic colors based on status
  const isError = status === 'error';
  const isFeeding = status === 'feeding';
  const isMixing = status === 'mixing';
  
  return (
    <div className={cn("relative w-full h-full flex items-center justify-center", className)}>
      <svg 
        viewBox="0 0 100 160" 
        className="w-full h-full drop-shadow-xl"
        preserveAspectRatio="xMidYMid meet"
      >
        <defs>
          {/* Liquid Gradient */}
          <linearGradient id="liquidGradient" x1="0%" y1="0%" x2="0%" y2="100%">
            <stop offset="0%" stopColor="#06b6d4" stopOpacity="0.9" />  {/* cyan-500 */}
            <stop offset="100%" stopColor="#1d4ed8" stopOpacity="0.95" /> {/* blue-700 */}
          </linearGradient>

          {/* Reflection Gradient */}
          <linearGradient id="reflectionGradient" x1="0%" y1="0%" x2="100%" y2="0%">
            <stop offset="0%" stopColor="white" stopOpacity="0.05" />
            <stop offset="20%" stopColor="white" stopOpacity="0.2" />
            <stop offset="40%" stopColor="white" stopOpacity="0.05" />
            <stop offset="100%" stopColor="white" stopOpacity="0.02" />
          </linearGradient>

          {/* Clip path for the tank body shape */}
          <clipPath id="tankClip">
             {/* Cylinder top to cone bottom transition */}
             <path d="M15,10 L15,110 L35,150 L65,150 L85,110 L85,10 Z" />
          </clipPath>
          
          {/* Bubbles Pattern for Mixing */}
          <pattern id="bubbles" x="0" y="0" width="20" height="20" patternUnits="userSpaceOnUse">
             <circle cx="2" cy="2" r="1" fill="rgba(255,255,255,0.3)">
               <animate attributeName="cy" from="20" to="-5" dur="2s" repeatCount="indefinite" />
             </circle>
             <circle cx="12" cy="12" r="1.5" fill="rgba(255,255,255,0.2)">
               <animate attributeName="cy" from="20" to="-5" dur="3s" repeatCount="indefinite" begin="1s"/>
             </circle>
          </pattern>
        </defs>

        {/* --- TANK BODY (Outline) --- */}
        <path 
          d="M15,10 L15,110 L35,150 L65,150 L85,110 L85,10 Z" 
          fill="#0f172a" 
          stroke={isError ? "#f43f5e" : (isFeeding ? "#10b981" : "#334155")}
          strokeWidth="2"
          className={cn("transition-colors duration-500", isFeeding && "animate-pulse")}
        />

        {/* --- LIQUID FILL --- */}
        <g clipPath="url(#tankClip)">
          {/* The liquid rect moves up/down based on fill % */}
          <rect 
            x="0" 
            y={160 - (1.6 * fill)} // Map 0-100 to coordinate space roughly
            width="100" 
            height="160" 
            fill="url(#liquidGradient)"
            className="transition-all duration-1000 ease-in-out"
          />
          
          {/* Mixing Bubbles Overlay */}
          {isMixing && (
            <rect x="0" y="0" width="100" height="160" fill="url(#bubbles)" className="opacity-70" />
          )}

          {/* Surface Line (optional shimmer) */}
          <line 
             x1="0" y1={160 - (1.6 * fill)} 
             x2="100" y2={160 - (1.6 * fill)} 
             stroke="rgba(255,255,255,0.5)" 
             strokeWidth="1"
          />
        </g>

        {/* --- GLASS REFLECTION OVERLAY --- */}
        <path 
          d="M15,10 L15,110 L35,150 L65,150 L85,110 L85,10 Z" 
          fill="url(#reflectionGradient)" 
          className="pointer-events-none"
        />

        {/* --- MEASUREMENT LINES (Ticks) --- */}
        <g stroke="rgba(255,255,255,0.2)" strokeWidth="1">
           <line x1="85" y1="35" x2="75" y2="35" />
           <line x1="85" y1="60" x2="75" y2="60" />
           <line x1="85" y1="85" x2="75" y2="85" />
           <line x1="85" y1="110" x2="75" y2="110" />
        </g>

        {/* --- STATUS GLOW (Underlay) --- */}
        {isMixing && (
           <ellipse cx="50" cy="150" rx="30" ry="5" fill="#a78bfa" filter="blur(8px)" opacity="0.6" className="animate-pulse" />
        )}
      </svg>
      
      {/* Percentage Text Overlay (Optional) */}
      <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
         <span className="text-xs font-bold text-foreground/80 drop-shadow-md translate-y-4">
           {Math.round(fill)}%
         </span>
      </div>
    </div>
  );
}

