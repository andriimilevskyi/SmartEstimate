export interface LocalizedText {
  de: string;
  en: string;
  uk: string;
}

export interface KnowledgeCategory {
  description?: string | null;
  id: string;
  name: LocalizedText;
  parentCategoryId?: string | null;
  status: KnowledgeStatus;
  version: number;
  createdAt: string;
  updatedAt: string;
  createdBy?: string | null;
  updatedBy?: string | null;
}

export type KnowledgeStatus = 'Draft' | 'Active' | 'Archived';

export interface ConstructionWork {
  categoryId: string;
  description?: string | null;
  id: string;
  name: LocalizedText;
  tags: string[];
  unitId: string;
  status: KnowledgeStatus;
  version: number;
  createdAt: string;
  updatedAt: string;
  createdBy?: string | null;
  updatedBy?: string | null;
}

export interface KnowledgeMaterial {
  categoryId?: string | null;
  description?: string | null;
  id: string;
  name: LocalizedText;
  tags: string[];
  unitId: string;
  status: KnowledgeStatus;
  version: number;
  createdAt: string;
  updatedAt: string;
  createdBy?: string | null;
  updatedBy?: string | null;
}

export interface KnowledgeUnit {
  id: string;
  name: LocalizedText;
  symbol: string;
  status: KnowledgeStatus;
  version: number;
  createdAt: string;
  updatedAt: string;
  createdBy?: string | null;
  updatedBy?: string | null;
}

export interface PagedKnowledgeResponse<TItem> {
  items: TItem[];
  page: number;
  pageSize: number;
  totalCount: number;
}

export interface LocalizedTextInput {
  de?: string;
  en?: string;
  uk: string;
}

export interface CategoryWriteRequest {
  description?: string;
  name: LocalizedTextInput;
  parentCategoryId?: string | null;
  status: KnowledgeStatus;
}

export interface ConstructionWorkWriteRequest {
  categoryId: string;
  description?: string;
  name: LocalizedTextInput;
  status: KnowledgeStatus;
  tags?: string[];
  unitId: string;
}

export interface MaterialWriteRequest {
  categoryId?: string | null;
  description?: string;
  name: LocalizedTextInput;
  status: KnowledgeStatus;
  tags?: string[];
  unitId: string;
}

export interface UnitWriteRequest {
  name: LocalizedTextInput;
  status: KnowledgeStatus;
  symbol: string;
}
