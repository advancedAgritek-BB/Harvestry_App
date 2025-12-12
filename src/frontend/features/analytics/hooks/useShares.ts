import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/client';

export interface Share {
  id: string;
  resourceType: string;
  resourceId: string;
  sharedWithId: string;
  sharedWithType: 'user' | 'role';
  permissionLevel: 'view' | 'edit';
}

export const useShares = (resourceType: string, resourceId: string) => {
  return useQuery({
    queryKey: ['shares', resourceType, resourceId],
    queryFn: () => api.get<Share[]>(`/analytics/shares/${resourceType}/${resourceId}`),
    enabled: !!resourceId,
  });
};

export const useAddShare = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: Omit<Share, 'id'>) => api.post<Share>('/analytics/shares', data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['shares', variables.resourceType, variables.resourceId] });
    },
  });
};

export const useRemoveShare = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.delete(`/analytics/shares/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['shares'] });
    },
  });
};





