/**
 * Team Service
 * API operations for teams and team member management
 */

import type {
  Team,
  TeamDetail,
  AssignableMembersResponse,
  CreateTeamRequest,
  UpdateTeamRequest,
  AddTeamMemberRequest,
  SetTeamLeadRequest,
  TeamMember,
} from '../types/team.types';

const API_BASE = '/api/v1';
const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_AUTH === 'true';

// Mock data for development - using let so we can mutate during the session
let mockTeams: Team[] = [
  {
    id: 'team-1',
    siteId: 'site-1',
    name: 'Day Shift',
    description: 'Morning cultivation team',
    status: 'Active',
    memberCount: 5,
    teamLeadCount: 1,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
  {
    id: 'team-2',
    siteId: 'site-1',
    name: 'Flower Room Team',
    description: 'Specialized flowering stage team',
    status: 'Active',
    memberCount: 3,
    teamLeadCount: 1,
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  },
];

let mockMembers: TeamMember[] = [
  { id: 'tm-1', userId: 'u1', firstName: 'Marcus', lastName: 'Johnson', role: 'Grower', isTeamLead: true, joinedAt: new Date().toISOString() },
  { id: 'tm-2', userId: 'u2', firstName: 'Sarah', lastName: 'Chen', role: 'Lead Grower', isTeamLead: false, joinedAt: new Date().toISOString() },
  { id: 'tm-3', userId: 'u3', firstName: 'David', lastName: 'Martinez', role: 'Technician', isTeamLead: false, joinedAt: new Date().toISOString() },
  { id: 'tm-4', userId: 'u4', firstName: 'Emily', lastName: 'Rodriguez', role: 'Grower', isTeamLead: false, joinedAt: new Date().toISOString() },
  { id: 'tm-5', userId: 'u5', firstName: 'James', lastName: 'Wilson', role: 'Grower', isTeamLead: false, joinedAt: new Date().toISOString() },

];

// Map team members to teams for mock data
const mockTeamMemberships: Record<string, string[]> = {
  'team-1': ['u1', 'u2', 'u3'],
  'team-2': ['u4', 'u5'],
};

/**
 * Get all teams for a site
 */
export async function getTeams(siteId: string): Promise<Team[]> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    // Return active teams only (filter out archived)
    return mockTeams.filter(t => 
      (t.siteId === siteId || t.siteId === 'site-1') && 
      t.status === 'Active'
    );
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/teams`);
  if (!response.ok) throw new Error('Failed to fetch teams');
  return response.json();
}

/**
 * Get team details with members
 */
export async function getTeamDetail(siteId: string, teamId: string): Promise<TeamDetail> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    const team = mockTeams.find(t => t.id === teamId);
    if (!team) throw new Error('Team not found');
    
    // Get members for this team
    const memberUserIds = mockTeamMemberships[teamId] || [];
    const members = mockMembers.filter(m => memberUserIds.includes(m.userId));
    
    return {
      ...team,
      members,
    };
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/teams/${teamId}`);
  if (!response.ok) throw new Error('Failed to fetch team detail');
  return response.json();
}

/**
 * Get teams the current user can manage
 */
export async function getManagedTeams(siteId: string): Promise<Team[]> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    return mockTeams;
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/teams/managed`);
  if (!response.ok) throw new Error('Failed to fetch managed teams');
  return response.json();
}

/**
 * Get all members the current user can assign to tasks
 * Returns members grouped by team
 */
export async function getAssignableMembers(siteId: string): Promise<AssignableMembersResponse> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    
    // Build teams response from current mock data
    const teams = mockTeams
      .filter(t => t.status === 'Active')
      .map(team => {
        const memberUserIds = mockTeamMemberships[team.id] || [];
        const members = mockMembers
          .filter(m => memberUserIds.includes(m.userId))
          .map(m => ({
            userId: m.userId,
            firstName: m.firstName,
            lastName: m.lastName,
            fullName: `${m.firstName} ${m.lastName}`,
            avatarUrl: m.avatarUrl,
            role: m.role,
            teamId: team.id,
            teamName: team.name,
          }));
        return {
          teamId: team.id,
          teamName: team.name,
          members,
        };
      })
      .filter(t => t.members.length > 0);

    return { teams };
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/teams/assignable-members`);
  if (!response.ok) throw new Error('Failed to fetch assignable members');
  return response.json();
}

/**
 * Create a new team
 */
export async function createTeam(siteId: string, request: CreateTeamRequest): Promise<Team> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    const newTeam: Team = {
      id: `team-${Date.now()}`,
      siteId,
      name: request.name,
      description: request.description,
      status: 'Active',
      memberCount: 0,
      teamLeadCount: 0,
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
    };
    // Add to mock data so it persists during the session
    mockTeams.push(newTeam);
    mockTeamMemberships[newTeam.id] = [];
    return newTeam;
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/teams`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to create team');
  return response.json();
}

/**
 * Update a team
 */
export async function updateTeam(
  siteId: string,
  teamId: string,
  request: UpdateTeamRequest
): Promise<Team> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    const teamIndex = mockTeams.findIndex(t => t.id === teamId);
    if (teamIndex === -1) throw new Error('Team not found');
    
    const updatedTeam = { 
      ...mockTeams[teamIndex], 
      ...request, 
      updatedAt: new Date().toISOString() 
    };
    mockTeams[teamIndex] = updatedTeam;
    return updatedTeam;
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/teams/${teamId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to update team');
  return response.json();
}

/**
 * Delete (archive) a team
 */
export async function deleteTeam(siteId: string, teamId: string): Promise<void> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    // Archive the team (set status to Archived)
    const teamIndex = mockTeams.findIndex(t => t.id === teamId);
    if (teamIndex !== -1) {
      mockTeams[teamIndex] = { 
        ...mockTeams[teamIndex], 
        status: 'Archived',
        updatedAt: new Date().toISOString() 
      };
    }
    return;
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/teams/${teamId}`, {
    method: 'DELETE',
  });
  if (!response.ok) throw new Error('Failed to delete team');
}

/**
 * Add a member to a team
 */
export async function addTeamMember(
  siteId: string,
  teamId: string,
  request: AddTeamMemberRequest
): Promise<TeamMember> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    
    // Add user to team membership
    if (!mockTeamMemberships[teamId]) {
      mockTeamMemberships[teamId] = [];
    }
    if (!mockTeamMemberships[teamId].includes(request.userId)) {
      mockTeamMemberships[teamId].push(request.userId);
    }
    
    // Update team member count
    const teamIndex = mockTeams.findIndex(t => t.id === teamId);
    if (teamIndex !== -1) {
      mockTeams[teamIndex].memberCount = mockTeamMemberships[teamId].length;
      if (request.isTeamLead) {
        mockTeams[teamIndex].teamLeadCount++;
      }
    }
    
    // Find or create the member
    let member = mockMembers.find(m => m.userId === request.userId);
    if (!member) {
      member = {
        id: `tm-${Date.now()}`,
        userId: request.userId,
        firstName: 'New',
        lastName: 'Member',
        isTeamLead: request.isTeamLead || false,
        joinedAt: new Date().toISOString(),
      };
      mockMembers.push(member);
    }
    
    return member;
  }

  const response = await fetch(`${API_BASE}/sites/${siteId}/teams/${teamId}/members`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error('Failed to add team member');
  return response.json();
}

/**
 * Remove a member from a team
 */
export async function removeTeamMember(
  siteId: string,
  teamId: string,
  userId: string
): Promise<void> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    
    // Remove user from team membership
    if (mockTeamMemberships[teamId]) {
      mockTeamMemberships[teamId] = mockTeamMemberships[teamId].filter(id => id !== userId);
      
      // Update team member count
      const teamIndex = mockTeams.findIndex(t => t.id === teamId);
      if (teamIndex !== -1) {
        mockTeams[teamIndex].memberCount = mockTeamMemberships[teamId].length;
      }
    }
    return;
  }

  const response = await fetch(
    `${API_BASE}/sites/${siteId}/teams/${teamId}/members/${userId}`,
    { method: 'DELETE' }
  );
  if (!response.ok) throw new Error('Failed to remove team member');
}

/**
 * Set or remove team lead status for a member
 */
export async function setTeamLead(
  siteId: string,
  teamId: string,
  userId: string,
  request: SetTeamLeadRequest
): Promise<void> {
  if (USE_MOCK) {
    await new Promise(resolve => setTimeout(resolve, 300));
    
    // Update the member's team lead status
    const memberIndex = mockMembers.findIndex(m => m.userId === userId);
    if (memberIndex !== -1) {
      const wasTeamLead = mockMembers[memberIndex].isTeamLead;
      mockMembers[memberIndex].isTeamLead = request.isTeamLead;
      
      // Update team lead count
      const teamIndex = mockTeams.findIndex(t => t.id === teamId);
      if (teamIndex !== -1) {
        if (request.isTeamLead && !wasTeamLead) {
          mockTeams[teamIndex].teamLeadCount++;
        } else if (!request.isTeamLead && wasTeamLead) {
          mockTeams[teamIndex].teamLeadCount = Math.max(0, mockTeams[teamIndex].teamLeadCount - 1);
        }
      }
    }
    return;
  }

  const response = await fetch(
    `${API_BASE}/sites/${siteId}/teams/${teamId}/members/${userId}/lead`,
    {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request),
    }
  );
  if (!response.ok) throw new Error('Failed to update team lead status');
}
