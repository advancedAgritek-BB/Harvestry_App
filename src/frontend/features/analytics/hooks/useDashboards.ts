import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/client';
import { Dashboard, CreateDashboardDto, UpdateDashboardDto } from '../types';

export const useDashboards = () => {
  return useQuery({
    queryKey: ['dashboards'],
    queryFn: () => api.get<Dashboard[]>('/analytics/dashboards'),
  });
};

export const useDashboard = (id: string) => {
  return useQuery({
    queryKey: ['dashboards', id],
    queryFn: () => api.get<Dashboard>(`/analytics/dashboards/${id}`),
    enabled: !!id,
  });
};

export const useCreateDashboard = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateDashboardDto) => api.post<Dashboard>('/analytics/dashboards', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dashboards'] });
    },
  });
};

export const useUpdateDashboard = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateDashboardDto }) => 
      api.put(`/analytics/dashboards/${id}`, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['dashboards'] });
      queryClient.invalidateQueries({ queryKey: ['dashboards', id] });
    },
  });
};

export const useDeleteDashboard = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.delete(`/analytics/dashboards/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dashboards'] });
    },
  });
};
