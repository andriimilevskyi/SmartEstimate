import { useQuery } from '@tanstack/react-query';

import {
  getConstructionWorks,
  getKnowledgeCategories,
  getKnowledgeMaterial,
  getKnowledgeMaterials,
  getKnowledgeUnits,
  type KnowledgeQueryOptions,
} from '@/entities/knowledge/api/knowledge-api';

export const knowledgeQueryKeys = {
  all: ['knowledge'] as const,
  categories: (options?: KnowledgeQueryOptions) =>
    [...knowledgeQueryKeys.all, 'categories', options ?? {}] as const,
  constructionWorks: (categoryId?: string, options?: KnowledgeQueryOptions) =>
    [...knowledgeQueryKeys.all, 'construction-works', categoryId ?? 'all', options ?? {}] as const,
  material: (id: string) => [...knowledgeQueryKeys.all, 'materials', 'detail', id] as const,
  materials: (categoryId?: string, options?: KnowledgeQueryOptions) =>
    [...knowledgeQueryKeys.all, 'materials', categoryId ?? 'all', options ?? {}] as const,
  units: (options?: KnowledgeQueryOptions) =>
    [...knowledgeQueryKeys.all, 'units', options ?? {}] as const,
};

export function useKnowledgeCategoriesQuery(
  enabled = true,
  options: KnowledgeQueryOptions = { pageSize: 100 },
) {
  return useQuery({
    enabled,
    queryFn: ({ signal }) => getKnowledgeCategories(signal, options),
    queryKey: knowledgeQueryKeys.categories(options),
  });
}

export function useConstructionWorksQuery(categoryId?: string, enabled = true, search?: string) {
  const options = { pageSize: 100, search };

  return useQuery({
    enabled,
    queryFn: ({ signal }) => getConstructionWorks(categoryId, signal, options),
    queryKey: knowledgeQueryKeys.constructionWorks(categoryId, options),
  });
}

export function useKnowledgeMaterialQuery(id: string, enabled = true) {
  return useQuery({
    enabled: enabled && id.length > 0,
    queryFn: ({ signal }) => getKnowledgeMaterial(id, signal),
    queryKey: knowledgeQueryKeys.material(id),
  });
}

export function useKnowledgeMaterialsQuery(
  categoryId?: string,
  enabled = true,
  search?: string,
  options: KnowledgeQueryOptions = {},
) {
  const queryOptions = { pageSize: 100, search, ...options };

  return useQuery({
    enabled,
    queryFn: ({ signal }) => getKnowledgeMaterials(categoryId, signal, queryOptions),
    queryKey: knowledgeQueryKeys.materials(categoryId, queryOptions),
  });
}

export function useKnowledgeUnitsQuery(options: KnowledgeQueryOptions = { pageSize: 100 }) {
  return useQuery({
    queryFn: ({ signal }) => getKnowledgeUnits(signal, options),
    queryKey: knowledgeQueryKeys.units(options),
  });
}
