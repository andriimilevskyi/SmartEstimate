import { useQuery } from '@tanstack/react-query';

import {
  getConstructionWorks,
  getKnowledgeCategories,
  getKnowledgeMaterials,
  getKnowledgeUnits,
} from '@/entities/knowledge/api/knowledge-api';

export const knowledgeQueryKeys = {
  all: ['knowledge'] as const,
  categories: () => [...knowledgeQueryKeys.all, 'categories'] as const,
  constructionWorks: (categoryId?: string, search?: string) =>
    [...knowledgeQueryKeys.all, 'construction-works', categoryId ?? 'all', search ?? ''] as const,
  materials: (categoryId?: string, search?: string) =>
    [...knowledgeQueryKeys.all, 'materials', categoryId ?? 'all', search ?? ''] as const,
  units: () => [...knowledgeQueryKeys.all, 'units'] as const,
};

export function useKnowledgeCategoriesQuery(enabled = true) {
  return useQuery({
    enabled,
    queryFn: ({ signal }) => getKnowledgeCategories(signal),
    queryKey: knowledgeQueryKeys.categories(),
  });
}

export function useConstructionWorksQuery(categoryId?: string, enabled = true, search?: string) {
  return useQuery({
    enabled,
    queryFn: ({ signal }) => getConstructionWorks(categoryId, signal, { pageSize: 100, search }),
    queryKey: knowledgeQueryKeys.constructionWorks(categoryId, search),
  });
}

export function useKnowledgeMaterialsQuery(categoryId?: string, enabled = true, search?: string) {
  return useQuery({
    enabled,
    queryFn: ({ signal }) => getKnowledgeMaterials(categoryId, signal, { pageSize: 100, search }),
    queryKey: knowledgeQueryKeys.materials(categoryId, search),
  });
}

export function useKnowledgeUnitsQuery() {
  return useQuery({
    queryFn: ({ signal }) => getKnowledgeUnits(signal),
    queryKey: knowledgeQueryKeys.units(),
  });
}
