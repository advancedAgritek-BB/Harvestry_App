import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type {
  CreatePTORequestInput,
  ProfileStateSnapshot,
  PTORequest,
  PTORequestStatus,
} from '@/features/profile/types/profile.types';
import { createMockProfileSnapshot } from '@/features/profile/constants/mockProfileData';

interface ProfileStoreState {
  byUserId: Record<string, ProfileStateSnapshot>;

  seedProfileIfMissing: (params: { userId: string; siteId: string }) => void;

  createPtoRequest: (
    params: { userId: string; input: CreatePTORequestInput }
  ) => { ok: true; requestId: string } | { ok: false; error: string };

  cancelPtoRequest: (
    params: { userId: string; requestId: string }
  ) => { ok: true } | { ok: false; error: string };
}

function parseDateYmd(value: string): Date | null {
  // YYYY-MM-DD
  const m = /^\d{4}-\d{2}-\d{2}$/.exec(value);
  if (!m) return null;
  const d = new Date(`${value}T00:00:00`);
  if (Number.isNaN(d.getTime())) return null;
  return d;
}

function daysInclusive(startYmd: string, endYmd: string): number {
  const start = parseDateYmd(startYmd);
  const end = parseDateYmd(endYmd);
  if (!start || !end) return 0;
  const ms = end.getTime() - start.getTime();
  const days = Math.floor(ms / (24 * 60 * 60 * 1000)) + 1;
  return Math.max(days, 0);
}

function overlaps(aStart: string, aEnd: string, bStart: string, bEnd: string): boolean {
  const aS = parseDateYmd(aStart);
  const aE = parseDateYmd(aEnd);
  const bS = parseDateYmd(bStart);
  const bE = parseDateYmd(bEnd);
  if (!aS || !aE || !bS || !bE) return false;

  return aS.getTime() <= bE.getTime() && bS.getTime() <= aE.getTime();
}

function isActiveStatus(status: PTORequestStatus): boolean {
  return status === 'pending' || status === 'approved';
}

export const useProfileStore = create<ProfileStoreState>()(
  persist(
    (set, get) => ({
      byUserId: {},

      seedProfileIfMissing: ({ userId, siteId }) => {
        const existing = get().byUserId[userId];
        if (existing) return;

        set((state) => ({
          byUserId: {
            ...state.byUserId,
            [userId]: createMockProfileSnapshot({ userId, siteId }),
          },
        }));
      },

      createPtoRequest: ({ userId, input }) => {
        const snapshot = get().byUserId[userId];
        if (!snapshot) {
          return { ok: false, error: 'Profile not initialized' };
        }

        const start = parseDateYmd(input.startDate);
        const end = parseDateYmd(input.endDate);
        if (!start || !end) {
          return { ok: false, error: 'Invalid date format' };
        }
        if (end.getTime() < start.getTime()) {
          return { ok: false, error: 'End date must be on or after start date' };
        }

        const requestedDays = daysInclusive(input.startDate, input.endDate);
        if (requestedDays <= 0) {
          return { ok: false, error: 'Invalid request duration' };
        }
        if (snapshot.ptoBalance.availableDays < requestedDays) {
          return { ok: false, error: 'Insufficient PTO available' };
        }

        const hasOverlap = snapshot.ptoRequests
          .filter((r) => isActiveStatus(r.status))
          .some((r) => overlaps(r.startDate, r.endDate, input.startDate, input.endDate));
        if (hasOverlap) {
          return { ok: false, error: 'Request overlaps an existing PTO request' };
        }

        const requestId = `pto-${userId}-${Date.now()}`;

        const newRequest: PTORequest = {
          id: requestId,
          startDate: input.startDate,
          endDate: input.endDate,
          type: input.type,
          status: 'pending',
          reason: input.reason?.trim() || undefined,
          submittedAt: new Date().toISOString(),
        };

        set((state) => {
          const current = state.byUserId[userId];
          if (!current) return state;

          return {
            byUserId: {
              ...state.byUserId,
              [userId]: {
                ...current,
                ptoBalance: {
                  ...current.ptoBalance,
                  availableDays: Math.max(current.ptoBalance.availableDays - requestedDays, 0),
                  pendingDays: current.ptoBalance.pendingDays + requestedDays,
                },
                ptoRequests: [newRequest, ...current.ptoRequests],
              },
            },
          };
        });

        return { ok: true, requestId };
      },

      cancelPtoRequest: ({ userId, requestId }) => {
        const snapshot = get().byUserId[userId];
        if (!snapshot) {
          return { ok: false, error: 'Profile not initialized' };
        }

        const target = snapshot.ptoRequests.find((r) => r.id === requestId);
        if (!target) {
          return { ok: false, error: 'Request not found' };
        }
        if (target.status !== 'pending') {
          return { ok: false, error: 'Only pending requests can be cancelled' };
        }

        const requestedDays = daysInclusive(target.startDate, target.endDate);

        set((state) => {
          const current = state.byUserId[userId];
          if (!current) return state;

          return {
            byUserId: {
              ...state.byUserId,
              [userId]: {
                ...current,
                ptoBalance: {
                  ...current.ptoBalance,
                  availableDays: current.ptoBalance.availableDays + requestedDays,
                  pendingDays: Math.max(current.ptoBalance.pendingDays - requestedDays, 0),
                },
                ptoRequests: current.ptoRequests.map((r) =>
                  r.id === requestId ? { ...r, status: 'cancelled' as const } : r
                ),
              },
            },
          };
        });

        return { ok: true };
      },
    }),
    {
      name: 'harvestry-profile',
      partialize: (state) => ({ byUserId: state.byUserId }),
    }
  )
);
