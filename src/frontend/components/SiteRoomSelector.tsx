'use client';

import React from 'react';
import { ChevronDown, Building2, DoorOpen } from 'lucide-react';
import { cn } from '@/lib/utils';
import { 
  useSiteRoomStore, 
  useSelectedSite, 
  useSelectedRoom,
  useAvailableSites,
  useAvailableRooms,
} from '@/stores/siteRoomStore';

interface SelectorDropdownProps {
  label: string;
  icon: React.ReactNode;
  value: string;
  options: { id: string; name: string }[];
  onChange: (id: string) => void;
  className?: string;
}

function SelectorDropdown({ 
  label, 
  icon, 
  value, 
  options, 
  onChange,
  className 
}: SelectorDropdownProps) {
  const [isOpen, setIsOpen] = React.useState(false);
  const dropdownRef = React.useRef<HTMLDivElement>(null);

  // Close on outside click
  React.useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div ref={dropdownRef} className={cn("relative", className)}>
      <button
        type="button"
        onClick={() => setIsOpen(!isOpen)}
        className={cn(
          "flex items-center gap-2 px-3 py-1.5 rounded-lg transition-colors",
          "bg-muted/50 hover:bg-muted border border-border/50",
          "text-sm text-foreground"
        )}
      >
        <span className="text-muted-foreground">{icon}</span>
        <span className="font-medium">{value}</span>
        <ChevronDown className={cn(
          "w-4 h-4 text-muted-foreground transition-transform",
          isOpen && "rotate-180"
        )} />
      </button>

      {isOpen && (
        <div className="absolute top-full left-0 mt-1 w-48 bg-surface border border-border rounded-lg shadow-xl z-50 py-1 max-h-64 overflow-y-auto">
          <div className="px-3 py-1.5 text-xs font-semibold text-muted-foreground uppercase tracking-wider">
            {label}
          </div>
          {options.map((option) => (
            <button
              key={option.id}
              type="button"
              onClick={() => {
                onChange(option.id);
                setIsOpen(false);
              }}
              className={cn(
                "w-full text-left px-3 py-2 text-sm transition-colors",
                option.name === value
                  ? "bg-cyan-500/10 text-cyan-400"
                  : "text-foreground hover:bg-muted"
              )}
            >
              {option.name}
            </button>
          ))}
          {options.length === 0 && (
            <div className="px-3 py-2 text-sm text-muted-foreground">
              No options available
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export function SiteRoomSelector() {
  const selectedSite = useSelectedSite();
  const selectedRoom = useSelectedRoom();
  const sites = useAvailableSites();
  const rooms = useAvailableRooms();
  const setSelectedSite = useSiteRoomStore(state => state.setSelectedSite);
  const setSelectedRoom = useSiteRoomStore(state => state.setSelectedRoom);

  return (
    <div className="flex items-center gap-2">
      <SelectorDropdown
        label="Select Site"
        icon={<Building2 className="w-4 h-4" />}
        value={selectedSite?.name || 'Select Site'}
        options={sites}
        onChange={setSelectedSite}
      />
      <SelectorDropdown
        label="Select Room"
        icon={<DoorOpen className="w-4 h-4" />}
        value={selectedRoom?.name || 'Select Room'}
        options={rooms}
        onChange={setSelectedRoom}
      />
    </div>
  );
}

// Site-only selector for the main header
export function SiteSelector() {
  const [isMounted, setIsMounted] = React.useState(false);
  const selectedSite = useSelectedSite();
  const sites = useAvailableSites();
  const setSelectedSite = useSiteRoomStore(state => state.setSelectedSite);

  React.useEffect(() => {
    setIsMounted(true);
  }, []);

  // Prevent hydration mismatch by not rendering until client-side
  if (!isMounted) {
    return (
      <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-muted/50 border border-border/50 text-sm">
        <Building2 className="w-4 h-4 text-muted-foreground" />
        <span className="font-medium text-foreground">Loading...</span>
        <ChevronDown className="w-4 h-4 text-muted-foreground" />
      </div>
    );
  }

  return (
    <SelectorDropdown
      label="Select Site"
      icon={<Building2 className="w-4 h-4" />}
      value={selectedSite?.name || 'Select Site'}
      options={sites}
      onChange={setSelectedSite}
    />
  );
}

// Room-only selector for cultivation-specific contexts
export function RoomSelector() {
  const [isMounted, setIsMounted] = React.useState(false);
  const selectedRoom = useSelectedRoom();
  const rooms = useAvailableRooms();
  const setSelectedRoom = useSiteRoomStore(state => state.setSelectedRoom);

  React.useEffect(() => {
    setIsMounted(true);
  }, []);

  if (!isMounted) {
    return (
      <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-muted/50 border border-border/50 text-sm">
        <DoorOpen className="w-4 h-4 text-muted-foreground" />
        <span className="font-medium text-foreground">Loading...</span>
        <ChevronDown className="w-4 h-4 text-muted-foreground" />
      </div>
    );
  }

  return (
    <SelectorDropdown
      label="Select Room"
      icon={<DoorOpen className="w-4 h-4" />}
      value={selectedRoom?.name || 'Select Room'}
      options={rooms}
      onChange={setSelectedRoom}
    />
  );
}

// Compact room selector for smaller spaces
export function RoomSelectorCompact() {
  const [isMounted, setIsMounted] = React.useState(false);
  const selectedRoom = useSelectedRoom();
  const rooms = useAvailableRooms();
  const setSelectedRoom = useSiteRoomStore(state => state.setSelectedRoom);

  React.useEffect(() => {
    setIsMounted(true);
  }, []);

  if (!isMounted) {
    return <span className="text-sm text-muted-foreground">Loading...</span>;
  }

  return (
    <div className="relative group">
      <select
        value={selectedRoom?.id || ''}
        onChange={(e) => setSelectedRoom(e.target.value)}
        aria-label="Select room"
        className="appearance-none bg-transparent border-none text-foreground font-medium cursor-pointer pr-5 focus:outline-none focus:ring-0 text-sm"
      >
        {rooms.map((room) => (
          <option key={room.id} value={room.id} className="bg-surface text-foreground">
            {room.name}
          </option>
        ))}
      </select>
      <ChevronDown className="absolute right-0 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
    </div>
  );
}

// Compact version for smaller spaces (legacy - combines site and room)
export function SiteRoomSelectorCompact() {
  const selectedSite = useSelectedSite();
  const selectedRoom = useSelectedRoom();
  const sites = useAvailableSites();
  const rooms = useAvailableRooms();
  const setSelectedSite = useSiteRoomStore(state => state.setSelectedSite);
  const setSelectedRoom = useSiteRoomStore(state => state.setSelectedRoom);

  return (
    <div className="flex items-center gap-3 text-sm">
      <div className="relative group">
        <select
          value={selectedSite?.id || ''}
          onChange={(e) => setSelectedSite(e.target.value)}
          aria-label="Select site"
          className="appearance-none bg-transparent border-none text-foreground font-medium cursor-pointer pr-5 focus:outline-none focus:ring-0"
        >
          {sites.map((site) => (
            <option key={site.id} value={site.id} className="bg-surface text-foreground">
              {site.name}
            </option>
          ))}
        </select>
        <ChevronDown className="absolute right-0 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
      </div>
      <span className="text-muted-foreground">•</span>
      <div className="relative group">
        <select
          value={selectedRoom?.id || ''}
          onChange={(e) => setSelectedRoom(e.target.value)}
          aria-label="Select room"
          className="appearance-none bg-transparent border-none text-foreground font-medium cursor-pointer pr-5 focus:outline-none focus:ring-0"
        >
          {rooms.map((room) => (
            <option key={room.id} value={room.id} className="bg-surface text-foreground">
              {room.name}
            </option>
          ))}
        </select>
        <ChevronDown className="absolute right-0 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground pointer-events-none" />
      </div>
    </div>
  );
}



