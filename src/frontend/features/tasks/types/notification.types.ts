/**
 * Notification Types for Task Management
 */

export type NotificationType = 
  | 'taskCreated'
  | 'taskAssigned'
  | 'taskStarted'
  | 'taskCompleted'
  | 'taskBlocked'
  | 'taskOverdue'
  | 'conversationMention'
  | 'slackMention'
  | 'slackReaction';

export interface UserNotification {
  id: string;
  siteId: string;
  notificationType: NotificationType;
  title: string;
  message?: string;
  relatedEntityType?: string;
  relatedEntityId?: string;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

export interface NotificationCountResponse {
  unreadCount: number;
}

export interface NotificationListResponse {
  notifications: UserNotification[];
  total: number;
}

