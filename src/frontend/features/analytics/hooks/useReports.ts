import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api/client';
import { Report, CreateReportDto, UpdateReportDto, ReportConfig } from '../types';

export const useReports = () => {
  return useQuery({
    queryKey: ['reports'],
    queryFn: () => api.get<Report[]>('/analytics/reports'),
  });
};

export const useReport = (id: string) => {
  return useQuery({
    queryKey: ['reports', id],
    queryFn: () => api.get<Report>(`/analytics/reports/${id}`),
    enabled: !!id,
  });
};

export const useCreateReport = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateReportDto) => api.post<Report>('/analytics/reports', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reports'] });
    },
  });
};

export const useUpdateReport = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateReportDto }) => 
      api.put(`/analytics/reports/${id}`, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: ['reports'] });
      queryClient.invalidateQueries({ queryKey: ['reports', id] });
    },
  });
};

export const useDeleteReport = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => api.delete(`/analytics/reports/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reports'] });
    },
  });
};

export const usePreviewQuery = () => {
  return useMutation({
    mutationFn: (config: ReportConfig) => api.post<any[]>('/analytics/reports/query', config),
  });
};





