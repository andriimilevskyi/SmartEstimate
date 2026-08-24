import { apiRequest } from '@/shared/api/api-client';

import type {
  CategoryWriteRequest,
  ConstructionWork,
  ConstructionWorkWriteRequest,
  KnowledgeCategory,
  KnowledgeMaterial,
  KnowledgeStatus,
  KnowledgeUnit,
  MaterialWriteRequest,
  PagedKnowledgeResponse,
  UnitWriteRequest,
} from '@/entities/knowledge/model/types';

const knowledgePath = '/v1/knowledge';

export interface KnowledgeQueryOptions {
  activeOnly?: boolean;
  categoryId?: string;
  page?: number;
  pageSize?: number;
  search?: string;
  sort?: string;
  status?: KnowledgeStatus;
}

const getCatalogPath = (resource: string, options: KnowledgeQueryOptions = {}) => {
  const query = new URLSearchParams(
    `page=${options.page ?? 1}&pageSize=${options.pageSize ?? 100}`,
  );

  if (options.categoryId) {
    query.set('categoryId', options.categoryId);
  }
  if (options.search) {
    query.set('search', options.search);
  }
  if (options.sort) {
    query.set('sort', options.sort);
  }
  if (options.status) {
    query.set('status', options.status);
  }
  if (options.activeOnly === false) {
    query.set('activeOnly', 'false');
  }

  return `${knowledgePath}/${resource}?${query.toString()}`;
};

export function getKnowledgeCategories(signal?: AbortSignal, options?: KnowledgeQueryOptions) {
  return apiRequest<PagedKnowledgeResponse<KnowledgeCategory>>(
    getCatalogPath('categories', options),
    {
      signal,
    },
  );
}

export function getConstructionWorks(
  categoryId?: string,
  signal?: AbortSignal,
  options?: KnowledgeQueryOptions,
) {
  return apiRequest<PagedKnowledgeResponse<ConstructionWork>>(
    getCatalogPath('construction-works', { ...options, categoryId }),
    { signal },
  );
}

export function getKnowledgeMaterials(
  categoryId?: string,
  signal?: AbortSignal,
  options?: KnowledgeQueryOptions,
) {
  return apiRequest<PagedKnowledgeResponse<KnowledgeMaterial>>(
    getCatalogPath('materials', { ...options, categoryId }),
    { signal },
  );
}

export function getKnowledgeMaterial(id: string, signal?: AbortSignal) {
  return apiRequest<KnowledgeMaterial>(`${knowledgePath}/materials/${id}`, { signal });
}

export function getKnowledgeUnits(signal?: AbortSignal, options?: KnowledgeQueryOptions) {
  return apiRequest<PagedKnowledgeResponse<KnowledgeUnit>>(getCatalogPath('units', options), {
    signal,
  });
}

export function createKnowledgeCategory(request: CategoryWriteRequest) {
  return apiRequest<KnowledgeCategory>(`${knowledgePath}/categories`, {
    body: JSON.stringify(request),
    method: 'POST',
  });
}

export function updateKnowledgeCategory(id: string, request: CategoryWriteRequest) {
  return apiRequest<KnowledgeCategory>(`${knowledgePath}/categories/${id}`, {
    body: JSON.stringify(request),
    method: 'PUT',
  });
}

export function createConstructionWork(request: ConstructionWorkWriteRequest) {
  return apiRequest<ConstructionWork>(`${knowledgePath}/construction-works`, {
    body: JSON.stringify(request),
    method: 'POST',
  });
}

export function updateConstructionWork(id: string, request: ConstructionWorkWriteRequest) {
  return apiRequest<ConstructionWork>(`${knowledgePath}/construction-works/${id}`, {
    body: JSON.stringify(request),
    method: 'PUT',
  });
}

export function createKnowledgeMaterial(request: MaterialWriteRequest) {
  return apiRequest<KnowledgeMaterial>(`${knowledgePath}/materials`, {
    body: JSON.stringify(request),
    method: 'POST',
  });
}

export function updateKnowledgeMaterial(id: string, request: MaterialWriteRequest) {
  return apiRequest<KnowledgeMaterial>(`${knowledgePath}/materials/${id}`, {
    body: JSON.stringify(request),
    method: 'PUT',
  });
}

export function createKnowledgeUnit(request: UnitWriteRequest) {
  return apiRequest<KnowledgeUnit>(`${knowledgePath}/units`, {
    body: JSON.stringify(request),
    method: 'POST',
  });
}

export function updateKnowledgeUnit(id: string, request: UnitWriteRequest) {
  return apiRequest<KnowledgeUnit>(`${knowledgePath}/units/${id}`, {
    body: JSON.stringify(request),
    method: 'PUT',
  });
}

export function archiveKnowledge(
  resource: 'categories' | 'construction-works' | 'materials' | 'units',
  id: string,
) {
  return apiRequest<void>(`${knowledgePath}/${resource}/${id}`, { method: 'DELETE' });
}
