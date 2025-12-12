'use client';

/**
 * TeamMemberPicker Component
 * A dropdown component for selecting team members to assign to tasks
 * Groups members by team and supports search filtering
 */

import { useState, useEffect, useRef, useMemo } from 'react';
import { Search, User, Users, ChevronDown, X, Check } from 'lucide-react';
import type { AssignableMember, AssignableMembersResponse } from '../../types/team.types';
import { getAssignableMembers } from '../../services/team.service';

interface TeamMemberPickerProps {
  siteId: string;
  selectedUserId?: string;
  onSelect: (member: AssignableMember | null) => void;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
}

export function TeamMemberPicker({
  siteId,
  selectedUserId,
  onSelect,
  placeholder = 'Select team member...',
  disabled = false,
  className = '',
}: TeamMemberPickerProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [search, setSearch] = useState('');
  const [data, setData] = useState<AssignableMembersResponse | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // Load assignable members when component mounts
  useEffect(() => {
    const loadMembers = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const response = await getAssignableMembers(siteId);
        setData(response);
      } catch (err) {
        setError('Failed to load team members');
        console.error('Error loading assignable members:', err);
      } finally {
        setIsLoading(false);
      }
    };

    loadMembers();
  }, [siteId]);

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  // Get flat list of all members for finding selected
  const allMembers = useMemo(() => {
    if (!data) return [];
    return data.teams.flatMap(team => team.members);
  }, [data]);

  // Find currently selected member
  const selectedMember = useMemo(() => {
    if (!selectedUserId) return null;
    return allMembers.find(m => m.userId === selectedUserId) || null;
  }, [selectedUserId, allMembers]);

  // Filter members based on search
  const filteredTeams = useMemo(() => {
    if (!data) return [];
    if (!search.trim()) return data.teams;

    const searchLower = search.toLowerCase();
    return data.teams
      .map(team => ({
        ...team,
        members: team.members.filter(member =>
          member.fullName.toLowerCase().includes(searchLower) ||
          member.role?.toLowerCase().includes(searchLower)
        ),
      }))
      .filter(team => team.members.length > 0);
  }, [data, search]);

  const handleSelect = (member: AssignableMember) => {
    onSelect(member);
    setIsOpen(false);
    setSearch('');
  };

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onSelect(null);
  };

  const handleOpen = () => {
    if (!disabled) {
      setIsOpen(true);
      setTimeout(() => inputRef.current?.focus(), 0);
    }
  };

  return (
    <div ref={dropdownRef} className={`relative ${className}`}>
      {/* Selected Value Display / Trigger */}
      <button
        type="button"
        onClick={handleOpen}
        disabled={disabled}
        className={`
          w-full flex items-center justify-between gap-2 px-3 py-2
          rounded-lg border border-[var(--border)] 
          bg-[var(--bg-elevated)] text-left
          focus:outline-none focus:border-[var(--accent-cyan)]
          ${disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer hover:border-[var(--accent-cyan)]/50'}
          transition-colors
        `}
      >
        {selectedMember ? (
          <div className="flex items-center gap-2 flex-1 min-w-0">
            {selectedMember.avatarUrl ? (
              <img
                src={selectedMember.avatarUrl}
                alt=""
                className="w-6 h-6 rounded-full flex-shrink-0"
              />
            ) : (
              <div className="w-6 h-6 rounded-full bg-[var(--accent-cyan)]/20 flex items-center justify-center flex-shrink-0">
                <span className="text-xs text-[var(--accent-cyan)] font-medium">
                  {selectedMember.firstName[0]}
                </span>
              </div>
            )}
            <div className="flex-1 min-w-0">
              <span className="text-sm text-[var(--text-primary)] truncate block">
                {selectedMember.fullName}
              </span>
              {selectedMember.role && (
                <span className="text-xs text-[var(--text-subtle)] truncate block">
                  {selectedMember.teamName} · {selectedMember.role}
                </span>
              )}
            </div>
          </div>
        ) : (
          <span className="text-sm text-[var(--text-subtle)]">{placeholder}</span>
        )}
        
        <div className="flex items-center gap-1 flex-shrink-0">
          {selectedMember && !disabled && (
            <button
              type="button"
              onClick={handleClear}
              aria-label="Clear selection"
              className="p-0.5 rounded hover:bg-[var(--bg-tile)] text-[var(--text-muted)] hover:text-[var(--text-primary)]"
            >
              <X className="w-4 h-4" />
            </button>
          )}
          <ChevronDown className={`w-4 h-4 text-[var(--text-muted)] transition-transform ${isOpen ? 'rotate-180' : ''}`} />
        </div>
      </button>

      {/* Dropdown */}
      {isOpen && (
        <div className="absolute z-50 w-full mt-1 rounded-lg border border-[var(--border)] bg-[var(--bg-elevated)] shadow-lg overflow-hidden">
          {/* Search Input */}
          <div className="p-2 border-b border-[var(--border)]">
            <div className="relative">
              <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-[var(--text-muted)]" />
              <input
                ref={inputRef}
                type="text"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Search team members..."
                className="w-full pl-8 pr-3 py-1.5 text-sm rounded-md border border-[var(--border)] bg-[var(--bg-surface)] text-[var(--text-primary)] placeholder:text-[var(--text-subtle)] focus:outline-none focus:border-[var(--accent-cyan)]"
              />
            </div>
          </div>

          {/* Loading State */}
          {isLoading && (
            <div className="p-4 text-center">
              <div className="animate-spin w-5 h-5 border-2 border-[var(--accent-cyan)] border-t-transparent rounded-full mx-auto" />
              <p className="text-xs text-[var(--text-muted)] mt-2">Loading team members...</p>
            </div>
          )}

          {/* Error State */}
          {error && (
            <div className="p-4 text-center">
              <p className="text-sm text-rose-400">{error}</p>
            </div>
          )}

          {/* Members List */}
          {!isLoading && !error && (
            <div className="max-h-64 overflow-y-auto">
              {/* Unassigned Option */}
              <button
                type="button"
                aria-label="Clear selection - unassign task"
                onClick={() => {
                  onSelect(null);
                  setIsOpen(false);
                  setSearch('');
                }}
                className={`
                  w-full flex items-center gap-3 px-3 py-2 text-left
                  hover:bg-[var(--bg-tile)] transition-colors
                  ${!selectedUserId ? 'bg-[var(--accent-cyan)]/5' : ''}
                `}
              >
                <div className="w-6 h-6 rounded-full bg-[var(--bg-tile)] flex items-center justify-center">
                  <User className="w-3.5 h-3.5 text-[var(--text-muted)]" />
                </div>
                <span className="text-sm text-[var(--text-muted)] italic">Unassigned</span>
                {!selectedUserId && (
                  <Check className="w-4 h-4 text-[var(--accent-cyan)] ml-auto" />
                )}
              </button>

              {/* Teams and Members */}
              {filteredTeams.map(team => (
                <div key={team.teamId}>
                  {/* Team Header */}
                  <div className="px-3 py-1.5 bg-[var(--bg-surface)] border-y border-[var(--border)]">
                    <div className="flex items-center gap-2">
                      <Users className="w-3.5 h-3.5 text-[var(--text-subtle)]" />
                      <span className="text-xs font-medium text-[var(--text-muted)] uppercase tracking-wide">
                        {team.teamName}
                      </span>
                      <span className="text-xs text-[var(--text-subtle)]">
                        ({team.members.length})
                      </span>
                    </div>
                  </div>

                  {/* Team Members */}
                  {team.members.map(member => (
                    <button
                      key={member.userId}
                      type="button"
                      onClick={() => handleSelect(member)}
                      className={`
                        w-full flex items-center gap-3 px-3 py-2 text-left
                        hover:bg-[var(--bg-tile)] transition-colors
                        ${selectedUserId === member.userId ? 'bg-[var(--accent-cyan)]/5' : ''}
                      `}
                    >
                      {member.avatarUrl ? (
                        <img
                          src={member.avatarUrl}
                          alt=""
                          className="w-6 h-6 rounded-full"
                        />
                      ) : (
                        <div className="w-6 h-6 rounded-full bg-[var(--accent-cyan)]/20 flex items-center justify-center">
                          <span className="text-xs text-[var(--accent-cyan)] font-medium">
                            {member.firstName[0]}
                          </span>
                        </div>
                      )}
                      <div className="flex-1 min-w-0">
                        <span className="text-sm text-[var(--text-primary)] truncate block">
                          {member.fullName}
                        </span>
                        {member.role && (
                          <span className="text-xs text-[var(--text-subtle)]">
                            {member.role}
                          </span>
                        )}
                      </div>
                      {selectedUserId === member.userId && (
                        <Check className="w-4 h-4 text-[var(--accent-cyan)] flex-shrink-0" />
                      )}
                    </button>
                  ))}
                </div>
              ))}

              {/* Empty State */}
              {filteredTeams.length === 0 && !isLoading && (
                <div className="p-4 text-center">
                  <User className="w-8 h-8 text-[var(--text-subtle)] mx-auto mb-2" />
                  <p className="text-sm text-[var(--text-muted)]">
                    {search ? 'No matching team members' : 'No team members available'}
                  </p>
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
