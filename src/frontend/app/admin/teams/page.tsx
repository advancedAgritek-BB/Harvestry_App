'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  Users,
  UserPlus,
  Plus,
  Edit2,
  Trash2,
  Crown,
  Search,
  MoreHorizontal,
  AlertCircle,
  X,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminTable,
  StatusBadge,
  TableActions,
  TableActionButton,
  TableSearch,
  Button,
  AdminModal,
  FormField,
  Input,
} from '@/components/admin';
import type { Team, TeamDetail, TeamMember } from '@/features/labor/types/team.types';
import * as TeamService from '@/features/labor/services/team.service';

// Mock site ID - in production this would come from auth context
const CURRENT_SITE_ID = 'site-1';

export default function TeamsAdminPage() {
  const router = useRouter();
  const [teams, setTeams] = useState<Team[]>([]);
  const [selectedTeam, setSelectedTeam] = useState<TeamDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isMembersModalOpen, setIsMembersModalOpen] = useState(false);
  const [editingTeam, setEditingTeam] = useState<Team | null>(null);
  
  // Form state
  const [teamName, setTeamName] = useState('');
  const [teamDescription, setTeamDescription] = useState('');

  // Load teams on mount
  useEffect(() => {
    loadTeams();
  }, []);

  const loadTeams = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await TeamService.getTeams(CURRENT_SITE_ID);
      setTeams(data);
    } catch (err) {
      console.error('Failed to load teams:', err);
      setError('Failed to load teams. Please check your connection and try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateTeam = async () => {
    if (!teamName.trim()) return;
    
    setIsSubmitting(true);
    setError(null);
    try {
      await TeamService.createTeam(CURRENT_SITE_ID, {
        name: teamName,
        description: teamDescription || undefined,
      });
      setIsCreateModalOpen(false);
      setTeamName('');
      setTeamDescription('');
      loadTeams();
    } catch (err) {
      console.error('Failed to create team:', err);
      setError('Failed to create team. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleEditTeam = async () => {
    if (!editingTeam || !teamName.trim()) return;
    
    setIsSubmitting(true);
    setError(null);
    try {
      await TeamService.updateTeam(CURRENT_SITE_ID, editingTeam.id, {
        name: teamName,
        description: teamDescription || undefined,
      });
      setIsEditModalOpen(false);
      setEditingTeam(null);
      setTeamName('');
      setTeamDescription('');
      loadTeams();
    } catch (err) {
      console.error('Failed to update team:', err);
      setError('Failed to update team. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteTeam = async (teamId: string) => {
    if (!confirm('Are you sure you want to archive this team?')) return;
    
    setError(null);
    try {
      await TeamService.deleteTeam(CURRENT_SITE_ID, teamId);
      loadTeams();
    } catch (err) {
      console.error('Failed to delete team:', err);
      setError('Failed to archive team. Please try again.');
    }
  };

  const handleViewMembers = async (team: Team) => {
    setError(null);
    try {
      const detail = await TeamService.getTeamDetail(CURRENT_SITE_ID, team.id);
      setSelectedTeam(detail);
      setIsMembersModalOpen(true);
    } catch (err) {
      console.error('Failed to load team members:', err);
      setError('Failed to load team members. Please try again.');
    }
  };

  const handleToggleTeamLead = async (teamId: string, userId: string, isLead: boolean) => {
    setError(null);
    try {
      await TeamService.setTeamLead(CURRENT_SITE_ID, teamId, userId, { isTeamLead: !isLead });
      // Refresh the team detail
      const detail = await TeamService.getTeamDetail(CURRENT_SITE_ID, teamId);
      setSelectedTeam(detail);
    } catch (err) {
      console.error('Failed to update team lead:', err);
      setError('Failed to update team lead status. Please try again.');
    }
  };

  const handleRemoveMember = async (teamId: string, userId: string) => {
    if (!confirm('Remove this member from the team?')) return;
    
    setError(null);
    try {
      await TeamService.removeTeamMember(CURRENT_SITE_ID, teamId, userId);
      const detail = await TeamService.getTeamDetail(CURRENT_SITE_ID, teamId);
      setSelectedTeam(detail);
      loadTeams();
    } catch (err) {
      console.error('Failed to remove member:', err);
      setError('Failed to remove team member. Please try again.');
    }
  };

  const openEditModal = (team: Team) => {
    setEditingTeam(team);
    setTeamName(team.name);
    setTeamDescription(team.description || '');
    setIsEditModalOpen(true);
  };

  // Filter teams by search
  const filteredTeams = teams.filter(team =>
    team.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    team.description?.toLowerCase().includes(searchQuery.toLowerCase())
  );

  // Team columns
  const teamColumns = [
    {
      key: 'name',
      header: 'Team Name',
      sortable: true,
      render: (item: Team) => (
        <div>
          <div className="font-medium text-foreground">{item.name}</div>
          {item.description && (
            <div className="text-xs text-muted-foreground mt-0.5">{item.description}</div>
          )}
        </div>
      ),
    },
    {
      key: 'members',
      header: 'Members',
      render: (item: Team) => (
        <div className="flex items-center gap-2">
          <Users className="w-4 h-4 text-muted-foreground" />
          <span>{item.memberCount}</span>
        </div>
      ),
    },
    {
      key: 'leads',
      header: 'Team Leads',
      render: (item: Team) => (
        <div className="flex items-center gap-2">
          <Crown className="w-4 h-4 text-amber-400" />
          <span>{item.teamLeadCount}</span>
        </div>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: Team) => (
        <StatusBadge status={item.status === 'Active' ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '120px',
      render: (item: Team) => (
        <TableActions>
          <TableActionButton onClick={() => handleViewMembers(item)} title="View Members">
            <Users className="w-4 h-4" />
          </TableActionButton>
          <TableActionButton onClick={() => openEditModal(item)} title="Edit">
            <Edit2 className="w-4 h-4" />
          </TableActionButton>
          <TableActionButton onClick={() => handleDeleteTeam(item.id)} variant="danger" title="Archive">
            <Trash2 className="w-4 h-4" />
          </TableActionButton>
        </TableActions>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      {/* Error Banner */}
      {error && (
        <div className="flex items-center justify-between gap-3 px-4 py-3 bg-rose-500/10 border border-rose-500/30 rounded-lg">
          <div className="flex items-center gap-3">
            <AlertCircle className="w-5 h-5 text-rose-400 flex-shrink-0" />
            <span className="text-sm text-rose-300">{error}</span>
          </div>
          <button 
            onClick={() => setError(null)}
            className="p-1 hover:bg-rose-500/20 rounded transition-colors"
            aria-label="Dismiss error"
          >
            <X className="w-4 h-4 text-rose-400" />
          </button>
        </div>
      )}

      <AdminSection 
        title="Team Management" 
        description="Create and manage teams, assign members, and designate team leads"
      >
        <AdminCard
          title="Teams"
          icon={Users}
          actions={
            <div className="flex items-center gap-3">
              <TableSearch 
                value={searchQuery} 
                onChange={setSearchQuery} 
                placeholder="Search teams..." 
              />
              <Button onClick={() => setIsCreateModalOpen(true)}>
                <Plus className="w-4 h-4" />
                Create Team
              </Button>
            </div>
          }
        >
          {isLoading ? (
            <div className="flex items-center justify-center py-12">
              <div className="animate-spin w-6 h-6 border-2 border-cyan-500 border-t-transparent rounded-full" />
            </div>
          ) : (
            <AdminTable columns={teamColumns} data={filteredTeams} keyField="id" />
          )}
        </AdminCard>
      </AdminSection>

      {/* Create Team Modal */}
      <AdminModal
        isOpen={isCreateModalOpen}
        onClose={() => {
          if (!isSubmitting) {
            setIsCreateModalOpen(false);
            setTeamName('');
            setTeamDescription('');
          }
        }}
        title="Create Team"
        description="Create a new team for organizing employees"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsCreateModalOpen(false)} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button onClick={handleCreateTeam} disabled={!teamName.trim() || isSubmitting}>
              {isSubmitting ? 'Creating...' : 'Create Team'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <FormField label="Team Name" required>
            <Input
              value={teamName}
              onChange={(e) => setTeamName(e.target.value)}
              placeholder="e.g., Day Shift, Flower Room Team"
            />
          </FormField>
          <FormField label="Description">
            <Input
              value={teamDescription}
              onChange={(e) => setTeamDescription(e.target.value)}
              placeholder="Brief description of the team's purpose"
            />
          </FormField>
        </div>
      </AdminModal>

      {/* Edit Team Modal */}
      <AdminModal
        isOpen={isEditModalOpen}
        onClose={() => {
          if (!isSubmitting) {
            setIsEditModalOpen(false);
            setEditingTeam(null);
            setTeamName('');
            setTeamDescription('');
          }
        }}
        title="Edit Team"
        description="Update team details"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsEditModalOpen(false)} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button onClick={handleEditTeam} disabled={!teamName.trim() || isSubmitting}>
              {isSubmitting ? 'Saving...' : 'Save Changes'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <FormField label="Team Name" required>
            <Input
              value={teamName}
              onChange={(e) => setTeamName(e.target.value)}
              placeholder="e.g., Day Shift, Flower Room Team"
            />
          </FormField>
          <FormField label="Description">
            <Input
              value={teamDescription}
              onChange={(e) => setTeamDescription(e.target.value)}
              placeholder="Brief description of the team's purpose"
            />
          </FormField>
        </div>
      </AdminModal>

      {/* Team Members Modal */}
      <AdminModal
        isOpen={isMembersModalOpen}
        onClose={() => {
          setIsMembersModalOpen(false);
          setSelectedTeam(null);
        }}
        title={`${selectedTeam?.name || 'Team'} Members`}
        description="Manage team members and designate team leads"
        size="lg"
        footer={
          <Button variant="ghost" onClick={() => setIsMembersModalOpen(false)}>
            Close
          </Button>
        }
      >
        {selectedTeam && (
          <div className="space-y-4">
            {/* Add Member Button */}
            <div className="flex justify-end">
              <Button size="sm" variant="secondary">
                <UserPlus className="w-4 h-4" />
                Add Member
              </Button>
            </div>

            {/* Members List */}
            {selectedTeam.members.length === 0 ? (
              <div className="text-center py-8">
                <Users className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
                <p className="text-muted-foreground">No members in this team yet</p>
              </div>
            ) : (
              <div className="space-y-2">
                {selectedTeam.members.map((member) => (
                  <div
                    key={member.id}
                    className="flex items-center justify-between p-3 bg-white/[0.02] border border-white/[0.06] rounded-lg"
                  >
                    <div className="flex items-center gap-3">
                      {member.avatarUrl ? (
                        <img src={member.avatarUrl} alt="" className="w-10 h-10 rounded-full" />
                      ) : (
                        <div className="w-10 h-10 rounded-full bg-gradient-to-br from-cyan-400 to-blue-600 flex items-center justify-center text-white text-sm font-bold">
                          {member.firstName[0]}{member.lastName[0]}
                        </div>
                      )}
                      <div>
                        <div className="flex items-center gap-2">
                          <span className="font-medium text-foreground">
                            {member.firstName} {member.lastName}
                          </span>
                          {member.isTeamLead && (
                            <span className="flex items-center gap-1 text-xs text-amber-400 bg-amber-500/10 px-2 py-0.5 rounded-full">
                              <Crown className="w-3 h-3" />
                              Lead
                            </span>
                          )}
                        </div>
                        <span className="text-xs text-muted-foreground">{member.role || 'No role'}</span>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => router.push(`/admin/identity/users/${member.userId}`)}
                        className="p-1.5 text-muted-foreground hover:text-foreground hover:bg-white/[0.08] rounded-lg transition-colors"
                        title="Edit user profile"
                        aria-label={`Edit ${member.firstName} ${member.lastName}'s profile`}
                      >
                        <Edit2 className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleToggleTeamLead(selectedTeam.id, member.userId, member.isTeamLead)}
                        className={`px-3 py-1.5 text-xs font-medium rounded-lg transition-colors ${
                          member.isTeamLead
                            ? 'bg-amber-500/10 text-amber-400 hover:bg-amber-500/20'
                            : 'bg-white/[0.04] text-muted-foreground hover:bg-white/[0.08] hover:text-foreground'
                        }`}
                      >
                        {member.isTeamLead ? 'Remove Lead' : 'Make Lead'}
                      </button>
                      <button
                        onClick={() => handleRemoveMember(selectedTeam.id, member.userId)}
                        aria-label={`Remove ${member.firstName} ${member.lastName} from team`}
                        className="p-1.5 text-muted-foreground hover:text-rose-400 hover:bg-rose-500/10 rounded-lg transition-colors"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </AdminModal>
    </div>
  );
}
