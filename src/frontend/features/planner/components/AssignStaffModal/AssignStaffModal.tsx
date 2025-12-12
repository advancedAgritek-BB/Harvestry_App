'use client';

/**
 * AssignStaffModal Component
 * Modal for quickly assigning team members to production tasks
 * Uses TeamMemberPicker instead of redirecting to Shift Board
 */

import { useState } from 'react';
import { X, UserPlus, Users, Check } from 'lucide-react';
import { cn } from '@/lib/utils';
import { TeamMemberPicker } from '@/features/labor/components/TeamMemberPicker';
import type { AssignableMember } from '@/features/labor/types/team.types';
import type { ProductionTask } from '../../types';

interface AssignStaffModalProps {
  isOpen: boolean;
  onClose: () => void;
  task: ProductionTask | null;
  siteId: string;
  onAssign: (taskId: string, members: AssignableMember[]) => Promise<void>;
}

export function AssignStaffModal({
  isOpen,
  onClose,
  task,
  siteId,
  onAssign,
}: AssignStaffModalProps) {
  const [selectedMembers, setSelectedMembers] = useState<AssignableMember[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen || !task) return null;

  const shortfall = task.requiredCount - task.assignedCount;

  const handleSelectMember = (member: AssignableMember | null) => {
    if (!member) return;
    
    // Check if already selected
    if (selectedMembers.find(m => m.userId === member.userId)) {
      return;
    }

    setSelectedMembers(prev => [...prev, member]);
  };

  const handleRemoveMember = (userId: string) => {
    setSelectedMembers(prev => prev.filter(m => m.userId !== userId));
  };

  const handleSubmit = async () => {
    if (selectedMembers.length === 0) {
      setError('Please select at least one team member');
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      await onAssign(task.id, selectedMembers);
      setSelectedMembers([]);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to assign staff');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    setSelectedMembers([]);
    setError(null);
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      {/* Backdrop */}
      <div 
        className="absolute inset-0 bg-black/60 backdrop-blur-sm"
        onClick={handleClose}
      />
      
      {/* Modal */}
      <div className="relative bg-surface border border-border rounded-xl shadow-2xl w-full max-w-md overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-border">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-emerald-500/10">
              <UserPlus className="w-5 h-5 text-emerald-400" />
            </div>
            <div>
              <h2 className="text-lg font-semibold text-foreground">Assign Staff</h2>
              <p className="text-xs text-muted-foreground">{task.name}</p>
            </div>
          </div>
          <button
            onClick={handleClose}
            aria-label="Close modal"
            className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-muted/50 rounded-lg transition-colors"
          >
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Content */}
        <div className="p-5 space-y-5">
          {/* Staffing Status */}
          <div className="flex items-center justify-between p-3 rounded-lg bg-muted/30 border border-border">
            <div className="flex items-center gap-2">
              <Users className="w-4 h-4 text-muted-foreground" />
              <span className="text-sm text-muted-foreground">Current staffing</span>
            </div>
            <div className="text-sm">
              <span className="font-medium text-foreground">{task.assignedCount}</span>
              <span className="text-muted-foreground"> / {task.requiredCount}</span>
              {shortfall > 0 && (
                <span className="ml-2 text-amber-400">({shortfall} needed)</span>
              )}
            </div>
          </div>

          {/* Team Member Picker */}
          <div>
            <label className="block text-sm font-medium text-muted-foreground mb-2">
              Select team member to add
            </label>
            <TeamMemberPicker
              siteId={siteId}
              onSelect={handleSelectMember}
              placeholder="Search and select team member..."
            />
          </div>

          {/* Selected Members */}
          {selectedMembers.length > 0 && (
            <div>
              <label className="block text-sm font-medium text-muted-foreground mb-2">
                Selected ({selectedMembers.length})
              </label>
              <div className="space-y-2 max-h-40 overflow-y-auto">
                {selectedMembers.map(member => (
                  <div
                    key={member.userId}
                    className="flex items-center justify-between p-2.5 rounded-lg bg-emerald-500/5 border border-emerald-500/20"
                  >
                    <div className="flex items-center gap-2">
                      {member.avatarUrl ? (
                        <img
                          src={member.avatarUrl}
                          alt=""
                          className="w-7 h-7 rounded-full"
                        />
                      ) : (
                        <div className="w-7 h-7 rounded-full bg-emerald-500/20 flex items-center justify-center">
                          <span className="text-xs text-emerald-400 font-medium">
                            {member.firstName[0]}
                          </span>
                        </div>
                      )}
                      <div>
                        <p className="text-sm font-medium text-foreground">{member.fullName}</p>
                        <p className="text-xs text-muted-foreground">
                          {member.teamName}{member.role && ` · ${member.role}`}
                        </p>
                      </div>
                    </div>
                    <button
                      onClick={() => handleRemoveMember(member.userId)}
                      aria-label={`Remove ${member.fullName} from selection`}
                      className="p-1 text-muted-foreground hover:text-rose-400 transition-colors"
                    >
                      <X className="w-4 h-4" />
                    </button>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Error */}
          {error && (
            <div className="text-rose-400 text-sm bg-rose-500/10 border border-rose-500/20 rounded-lg px-3 py-2">
              {error}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex items-center justify-end gap-3 px-5 py-4 border-t border-border bg-muted/20">
          <button
            onClick={handleClose}
            className="px-4 py-2 text-sm font-medium text-muted-foreground hover:text-foreground border border-border rounded-lg hover:bg-muted/50 transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={selectedMembers.length === 0 || isSubmitting}
            className={cn(
              'flex items-center gap-2 px-4 py-2 text-sm font-medium rounded-lg transition-all',
              selectedMembers.length > 0
                ? 'text-white bg-emerald-500 hover:bg-emerald-400'
                : 'text-muted-foreground bg-muted cursor-not-allowed'
            )}
          >
            {isSubmitting ? (
              <>
                <div className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                Assigning...
              </>
            ) : (
              <>
                <Check className="w-4 h-4" />
                Assign {selectedMembers.length > 0 ? `(${selectedMembers.length})` : ''}
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
