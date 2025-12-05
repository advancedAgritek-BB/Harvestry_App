/**
 * Task Recommendation Service
 * Reviews strain blueprints and provides task recommendations for the next 2 days.
 * Notifies users of unassigned expected tasks.
 */

import {
  TaskRecommendation,
  UnassignedTaskAlert,
  TaskRecommendationSummary,
  StrainBlueprint,
  BlueprintTask,
  TaskAssignee,
  Task,
  GrowthPhase,
} from '../types/task.types';

interface ActiveBatch {
  id: string;
  name: string;
  strainId: string;
  strainName: string;
  currentPhase: GrowthPhase;
  phaseStartDate: Date;
  roomId: string;
}

interface TeamMember extends TaskAssignee {
  skills: string[];
  currentWorkload: number;
  availability: boolean;
}

export class TaskRecommendationService {
  private blueprints: Map<string, StrainBlueprint> = new Map();
  private teamMembers: Map<string, TeamMember> = new Map();
  private existingTasks: Map<string, Task> = new Map();
  
  constructor() {
    this.initializeMockData();
  }

  /**
   * Get task recommendations for the next N days
   */
  getRecommendations(daysAhead: number = 2): TaskRecommendationSummary {
    const now = new Date();
    const recommendations: TaskRecommendation[] = [];
    const unassignedAlerts: UnassignedTaskAlert[] = [];
    
    const activeBatches = this.getActiveBatches();
    
    for (const batch of activeBatches) {
      const blueprint = this.blueprints.get(batch.strainId);
      if (!blueprint) continue;
      
      const phaseConfig = blueprint.phases[batch.currentPhase];
      if (!phaseConfig) continue;
      
      const dayInPhase = this.calculateDayInPhase(batch.phaseStartDate, now);
      
      for (const blueprintTask of phaseConfig.tasks) {
        const taskDate = this.calculateTaskDate(batch.phaseStartDate, blueprintTask.dayInPhase);
        const daysUntilDue = this.calculateDaysUntil(now, taskDate);
        
        // Check if task is within our recommendation window
        if (daysUntilDue <= daysAhead && daysUntilDue >= -1) {
          const existingTask = this.findExistingTaskForBlueprint(batch.id, blueprintTask.id);
          
          if (!existingTask) {
            // Task should exist but doesn't - recommend creating it
            const recommendation = this.createRecommendation(
              batch,
              blueprintTask,
              taskDate,
              daysUntilDue
            );
            recommendations.push(recommendation);
          } else if (!existingTask.assignee) {
            // Task exists but is unassigned
            const alert = this.createUnassignedAlert(existingTask, batch, taskDate);
            unassignedAlerts.push(alert);
          }
        }
      }
    }
    
    // Sort recommendations by urgency
    recommendations.sort((a, b) => a.daysUntilDue - b.daysUntilDue);
    
    // Sort alerts by severity
    unassignedAlerts.sort((a, b) => {
      const severityOrder = { critical: 0, warning: 1, info: 2 };
      return severityOrder[a.severity] - severityOrder[b.severity];
    });
    
    return {
      recommendations,
      unassignedAlerts,
      upcomingCount: recommendations.length,
      overdueCount: recommendations.filter(r => r.isOverdue).length,
      lastUpdated: now,
    };
  }

  /**
   * Get suggested assignees for a task based on skills and workload
   */
  getSuggestedAssignees(blueprintTask: BlueprintTask): TaskAssignee[] {
    const availableMembers = Array.from(this.teamMembers.values())
      .filter(member => member.availability)
      .filter(member => {
        if (!blueprintTask.requiredRole) return true;
        return member.skills.includes(blueprintTask.requiredRole);
      })
      .sort((a, b) => a.currentWorkload - b.currentWorkload)
      .slice(0, 3);
    
    return availableMembers.map(({ skills, currentWorkload, availability, ...assignee }) => assignee);
  }

  /**
   * Check if any expected tasks haven't been assigned
   */
  getUnassignedTaskAlerts(): UnassignedTaskAlert[] {
    const summary = this.getRecommendations(2);
    return summary.unassignedAlerts;
  }

  private createRecommendation(
    batch: ActiveBatch,
    blueprintTask: BlueprintTask,
    taskDate: Date,
    daysUntilDue: number
  ): TaskRecommendation {
    const isOverdue = daysUntilDue < 0;
    
    let reason: string;
    if (isOverdue) {
      reason = `Task is ${Math.abs(daysUntilDue)} day(s) overdue based on ${batch.strainName} blueprint`;
    } else if (daysUntilDue === 0) {
      reason = `Task is due today according to ${batch.strainName} blueprint`;
    } else {
      reason = `Task is due in ${daysUntilDue} day(s) based on ${batch.strainName} blueprint`;
    }
    
    return {
      id: `rec-${batch.id}-${blueprintTask.id}`,
      blueprintTask,
      batchId: batch.id,
      batchName: batch.name,
      strainName: batch.strainName,
      suggestedDate: taskDate,
      suggestedAssignees: this.getSuggestedAssignees(blueprintTask),
      priority: isOverdue ? 'critical' : blueprintTask.priority,
      isOverdue,
      daysUntilDue,
      reason,
    };
  }

  private createUnassignedAlert(
    task: Task,
    batch: ActiveBatch,
    dueDate: Date
  ): UnassignedTaskAlert {
    const now = new Date();
    const hoursUntilDue = (dueDate.getTime() - now.getTime()) / (1000 * 60 * 60);
    
    let severity: 'info' | 'warning' | 'critical';
    if (hoursUntilDue <= 0) {
      severity = 'critical';
    } else if (hoursUntilDue <= 24) {
      severity = 'warning';
    } else {
      severity = 'info';
    }
    
    const blueprintTask = this.findBlueprintTask(batch.strainId, task.blueprintTaskId);
    
    return {
      taskId: task.id,
      taskTitle: task.title,
      batchName: batch.name,
      dueAt: dueDate,
      hoursUntilDue: Math.max(0, hoursUntilDue),
      suggestedAssignees: blueprintTask 
        ? this.getSuggestedAssignees(blueprintTask)
        : [],
      severity,
    };
  }

  private calculateDayInPhase(phaseStartDate: Date, currentDate: Date): number {
    const diffTime = currentDate.getTime() - phaseStartDate.getTime();
    return Math.floor(diffTime / (1000 * 60 * 60 * 24));
  }

  private calculateTaskDate(phaseStartDate: Date, dayInPhase: number): Date {
    const taskDate = new Date(phaseStartDate);
    taskDate.setDate(taskDate.getDate() + dayInPhase);
    return taskDate;
  }

  private calculateDaysUntil(from: Date, to: Date): number {
    const diffTime = to.getTime() - from.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  private findExistingTaskForBlueprint(batchId: string, blueprintTaskId: string): Task | undefined {
    return Array.from(this.existingTasks.values()).find(
      task => task.batchId === batchId && task.blueprintTaskId === blueprintTaskId
    );
  }

  private findBlueprintTask(strainId: string, blueprintTaskId?: string): BlueprintTask | undefined {
    if (!blueprintTaskId) return undefined;
    const blueprint = this.blueprints.get(strainId);
    if (!blueprint) return undefined;
    
    for (const phase of Object.values(blueprint.phases)) {
      if (!phase) continue;
      const task = phase.tasks.find(t => t.id === blueprintTaskId);
      if (task) return task;
    }
    return undefined;
  }

  private getActiveBatches(): ActiveBatch[] {
    // Mock data - in production this would come from the API
    return [
      {
        id: 'b1',
        name: 'B-203-OGK',
        strainId: 's1',
        strainName: 'OG Kush',
        currentPhase: 'flower',
        phaseStartDate: new Date(Date.now() - 20 * 24 * 60 * 60 * 1000), // 20 days ago
        roomId: 'f1',
      },
      {
        id: 'b2',
        name: 'B-204-BDR',
        strainId: 's2',
        strainName: 'Blue Dream',
        currentPhase: 'veg',
        phaseStartDate: new Date(Date.now() - 12 * 24 * 60 * 60 * 1000), // 12 days ago
        roomId: 'v1',
      },
    ];
  }

  private initializeMockData(): void {
    // Mock strain blueprints
    this.blueprints.set('s1', {
      id: 'bp1',
      strainId: 's1',
      strainName: 'OG Kush',
      phases: {
        clone: {
          durationDays: 14,
          tasks: [
            { id: 'c1', name: 'Initial cutting inspection', phase: 'clone', dayInPhase: 1, estimatedDurationMinutes: 30, priority: 'high' },
            { id: 'c2', name: 'Root development check', phase: 'clone', dayInPhase: 7, estimatedDurationMinutes: 20, priority: 'normal' },
          ],
        },
        veg: {
          durationDays: 28,
          tasks: [
            { id: 'v1', name: 'Transplant to veg', phase: 'veg', dayInPhase: 0, estimatedDurationMinutes: 60, priority: 'critical' },
            { id: 'v2', name: 'First top/training', phase: 'veg', dayInPhase: 7, estimatedDurationMinutes: 45, priority: 'high' },
            { id: 'v3', name: 'Second defoliation', phase: 'veg', dayInPhase: 14, estimatedDurationMinutes: 45, priority: 'normal' },
            { id: 'v4', name: 'Pre-flip inspection', phase: 'veg', dayInPhase: 27, estimatedDurationMinutes: 30, priority: 'high' },
          ],
        },
        flower: {
          durationDays: 63,
          tasks: [
            { id: 'f1', name: 'Flip to flower', phase: 'flower', dayInPhase: 0, estimatedDurationMinutes: 60, priority: 'critical' },
            { id: 'f2', name: 'Week 1 stretch management', phase: 'flower', dayInPhase: 7, estimatedDurationMinutes: 45, priority: 'high' },
            { id: 'f3', name: 'Lollipopping/defoliation', phase: 'flower', dayInPhase: 21, estimatedDurationMinutes: 90, priority: 'high' },
            { id: 'f4', name: 'Trichome check', phase: 'flower', dayInPhase: 49, estimatedDurationMinutes: 30, priority: 'normal' },
            { id: 'f5', name: 'Flush initiation', phase: 'flower', dayInPhase: 56, estimatedDurationMinutes: 30, priority: 'high' },
            { id: 'f6', name: 'Harvest readiness check', phase: 'flower', dayInPhase: 62, estimatedDurationMinutes: 45, priority: 'critical' },
          ],
        },
      },
    });

    this.blueprints.set('s2', {
      id: 'bp2',
      strainId: 's2',
      strainName: 'Blue Dream',
      phases: {
        veg: {
          durationDays: 35,
          tasks: [
            { id: 'bd-v1', name: 'Transplant to veg', phase: 'veg', dayInPhase: 0, estimatedDurationMinutes: 60, priority: 'critical' },
            { id: 'bd-v2', name: 'Training/topping', phase: 'veg', dayInPhase: 10, estimatedDurationMinutes: 60, priority: 'high' },
            { id: 'bd-v3', name: 'Second training session', phase: 'veg', dayInPhase: 14, estimatedDurationMinutes: 45, priority: 'normal' },
            { id: 'bd-v4', name: 'Pre-flip defoliation', phase: 'veg', dayInPhase: 34, estimatedDurationMinutes: 60, priority: 'high' },
          ],
        },
        flower: {
          durationDays: 70,
          tasks: [
            { id: 'bd-f1', name: 'Flip to flower', phase: 'flower', dayInPhase: 0, estimatedDurationMinutes: 60, priority: 'critical' },
            { id: 'bd-f2', name: 'Stretch support', phase: 'flower', dayInPhase: 14, estimatedDurationMinutes: 45, priority: 'high' },
          ],
        },
      },
    });

    // Mock team members
    this.teamMembers.set('u1', {
      id: 'u1',
      firstName: 'Marcus',
      lastName: 'Johnson',
      skills: ['cultivation', 'training', 'harvest'],
      currentWorkload: 3,
      availability: true,
    });
    this.teamMembers.set('u2', {
      id: 'u2',
      firstName: 'Sarah',
      lastName: 'Chen',
      skills: ['cultivation', 'cloning', 'IPM'],
      currentWorkload: 5,
      availability: true,
    });
    this.teamMembers.set('u3', {
      id: 'u3',
      firstName: 'David',
      lastName: 'Martinez',
      skills: ['irrigation', 'fertigation', 'maintenance'],
      currentWorkload: 2,
      availability: true,
    });
  }
}

// Singleton instance
let serviceInstance: TaskRecommendationService | null = null;

export function getTaskRecommendationService(): TaskRecommendationService {
  if (!serviceInstance) {
    serviceInstance = new TaskRecommendationService();
  }
  return serviceInstance;
}

