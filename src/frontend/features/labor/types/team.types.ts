/**
 * Team Types
 * Types for team management and member assignment
 */

export interface Team {
  id: string;
  siteId: string;
  name: string;
  description?: string;
  status: TeamStatus;
  memberCount: number;
  teamLeadCount: number;
  createdAt: string;
  updatedAt: string;
}

export type TeamStatus = 'Active' | 'Inactive' | 'Archived';

export interface TeamDetail extends Team {
  members: TeamMember[];
}

export interface TeamMember {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  avatarUrl?: string;
  role?: string;
  isTeamLead: boolean;
  joinedAt: string;
}

/**
 * Simplified member for assignment picker
 */
export interface AssignableMember {
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  avatarUrl?: string;
  role?: string;
  teamId: string;
  teamName: string;
}

export interface TeamWithMembers {
  teamId: string;
  teamName: string;
  members: AssignableMember[];
}

export interface AssignableMembersResponse {
  teams: TeamWithMembers[];
}

// Request types
export interface CreateTeamRequest {
  name: string;
  description?: string;
}

export interface UpdateTeamRequest {
  name: string;
  description?: string;
}

export interface AddTeamMemberRequest {
  userId: string;
  isTeamLead?: boolean;
}

export interface SetTeamLeadRequest {
  isTeamLead: boolean;
}
