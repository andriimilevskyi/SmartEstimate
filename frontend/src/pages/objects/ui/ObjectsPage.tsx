import { Archive, Building2, LoaderCircle, MapPin, Plus, Search } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';

import { archiveObject, createObject, restoreObject } from '@/entities/business/api/business-api';
import {
  businessQueryKeys,
  useCustomersQuery,
  useObjectsQuery,
} from '@/entities/business/api/business-queries';
import type { BusinessRecordStatus, EstimateObject } from '@/entities/business/model/types';
import type { EstimateObjectType } from '@/entities/estimate/model/types';
import { useTranslation } from '@/shared/i18n/use-translation';
import { Button } from '@/shared/ui/button';

const objectTypes: EstimateObjectType[] = [
  'Apartment',
  'PrivateHouse',
  'CommercialSpace',
  'Office',
  'IndustrialSpace',
  'Other',
];

const fieldClassName =
  'flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring';

export function ObjectsPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [searchParams, setSearchParams] = useSearchParams();
  const statusParam = searchParams.get('status');
  const status: BusinessRecordStatus =
    statusParam === 'archived' || statusParam === 'all' ? statusParam : 'active';
  const [search, setSearch] = useState('');
  const [customerId, setCustomerId] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(searchParams.get('create') === 'object');
  const [form, setForm] = useState({
    address: '',
    customerId: '',
    description: '',
    name: '',
    objectType: 'Apartment' as EstimateObjectType,
    totalArea: '',
  });
  const objectsQuery = useObjectsQuery(customerId || undefined, search || undefined, status);
  const customersQuery = useCustomersQuery(undefined, 'all');
  const createMutation = useMutation({
    mutationFn: () =>
      createObject({
        address: form.address.trim() || null,
        customerId: form.customerId,
        description: form.description.trim() || null,
        name: form.name.trim(),
        objectType: form.objectType,
        totalArea: form.totalArea === '' ? null : Number(form.totalArea),
      }),
    onError: () => toast.error(t('objects.messages.createError')),
    onSuccess: (estimateObject: EstimateObject) => {
      toast.success(t('objects.messages.created'));
      setForm({
        address: '',
        customerId: '',
        description: '',
        name: '',
        objectType: 'Apartment',
        totalArea: '',
      });
      setIsCreateOpen(false);
      setSearchParams({});
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
      navigate(`/objects/${estimateObject.id}`);
    },
  });
  const archiveMutation = useMutation({
    mutationFn: (objectId: string) => archiveObject(objectId),
    onError: () => toast.error(t('objects.messages.archiveError')),
    onSuccess: () => {
      toast.success(t('objects.messages.archived'));
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });
  const restoreMutation = useMutation({
    mutationFn: (objectId: string) => restoreObject(objectId),
    onError: () => toast.error(t('objects.messages.restoreError')),
    onSuccess: () => {
      toast.success(t('objects.messages.restored'));
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });

  useEffect(() => {
    setIsCreateOpen(searchParams.get('create') === 'object');
  }, [searchParams]);

  const setStatus = (nextStatus: BusinessRecordStatus) => {
    const nextParams = new URLSearchParams(searchParams);
    nextParams.set('status', nextStatus);
    setSearchParams(nextParams);
  };

  const emptyTitle =
    status === 'archived'
      ? t('objects.states.emptyArchivedTitle')
      : status === 'all'
        ? t('objects.states.emptyAllTitle')
        : t('objects.states.emptyTitle');
  const emptyDescription =
    status === 'archived'
      ? t('objects.states.emptyArchivedDescription')
      : status === 'all'
        ? t('objects.states.emptyAllDescription')
        : t('objects.states.emptyDescription');

  return (
    <section aria-labelledby="objects-title" className="space-y-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div className="space-y-2">
          <p className="text-sm font-medium text-primary">{t('objects.eyebrow')}</p>
          <h1 className="text-3xl font-semibold tracking-tight" id="objects-title">
            {t('objects.title')}
          </h1>
          <p className="max-w-2xl text-base leading-7 text-muted-foreground">
            {t('objects.description')}
          </p>
        </div>
        <Button onClick={() => setIsCreateOpen(true)}>
          <Plus aria-hidden="true" className="size-4" />
          {t('objects.create.action')}
        </Button>
      </div>

      {isCreateOpen ? (
        <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
          <h2 className="font-semibold">{t('objects.create.title')}</h2>
          <form
            className="mt-4 grid gap-4 lg:grid-cols-2"
            onSubmit={(event) => {
              event.preventDefault();
              if (!form.customerId || !form.name.trim()) {
                toast.error(t('objects.validation.required'));
                return;
              }
              createMutation.mutate();
            }}
          >
            <select
              className={fieldClassName}
              onChange={(event) => setForm((current) => ({ ...current, customerId: event.target.value }))}
              value={form.customerId}
            >
              <option value="">{t('objects.form.customer')}</option>
              {(customersQuery.data?.items ?? []).map((customer) => (
                <option key={customer.id} value={customer.id}>
                  {customer.name}
                </option>
              ))}
            </select>
            <input
              className={fieldClassName}
              onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
              placeholder={t('objects.form.name')}
              value={form.name}
            />
            <select
              className={fieldClassName}
              onChange={(event) =>
                setForm((current) => ({ ...current, objectType: event.target.value as EstimateObjectType }))
              }
              value={form.objectType}
            >
              {objectTypes.map((type) => (
                <option key={type} value={type}>
                  {t(`estimates.objectTypes.${type}`)}
                </option>
              ))}
            </select>
            <input
              className={fieldClassName}
              onChange={(event) => setForm((current) => ({ ...current, address: event.target.value }))}
              placeholder={t('objects.form.address')}
              value={form.address}
            />
            <input
              className={fieldClassName}
              inputMode="decimal"
              min="0.01"
              onChange={(event) => setForm((current) => ({ ...current, totalArea: event.target.value }))}
              placeholder={t('objects.form.totalArea')}
              step="0.01"
              type="number"
              value={form.totalArea}
            />
            <input
              className={fieldClassName}
              onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
              placeholder={t('objects.form.description')}
              value={form.description}
            />
            <div className="flex justify-end gap-2 lg:col-span-2">
              <Button onClick={() => setIsCreateOpen(false)} type="button" variant="ghost">
                {t('objects.create.cancel')}
              </Button>
              <Button disabled={createMutation.isPending} type="submit">
                {createMutation.isPending ? (
                  <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
                ) : null}
                {t('objects.create.submit')}
              </Button>
            </div>
          </form>
        </section>
      ) : null}

      <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
        <div className="grid gap-3 border-b border-border p-5 lg:grid-cols-[minmax(0,1fr)_18rem]">
          <div className="space-y-3 lg:col-span-2">
            <div className="inline-flex rounded-md border border-border bg-muted/40 p-1">
              {(['active', 'archived', 'all'] as const).map((value) => (
                <button
                  className={`rounded-sm px-3 py-1.5 text-sm font-medium transition ${
                    status === value
                      ? 'bg-background text-foreground shadow-sm'
                      : 'text-muted-foreground hover:text-foreground'
                  }`}
                  key={value}
                  onClick={() => setStatus(value)}
                  type="button"
                >
                  {t(`objects.filters.${value}`)}
                </button>
              ))}
            </div>
            <div className="grid gap-3 lg:grid-cols-[minmax(0,1fr)_18rem]">
              <div className="relative">
                <Search aria-hidden="true" className="pointer-events-none absolute left-3 top-3 size-4 text-muted-foreground" />
                <input
                  className={`${fieldClassName} pl-9`}
                  onChange={(event) => setSearch(event.target.value)}
                  placeholder={t('objects.search')}
                  value={search}
                />
              </div>
              <select
                className={fieldClassName}
                disabled={customersQuery.isPending}
                onChange={(event) => setCustomerId(event.target.value)}
                value={customerId}
              >
                <option value="">{t('objects.filters.allCustomers')}</option>
                {(customersQuery.data?.items ?? []).map((customer) => (
                  <option key={customer.id} value={customer.id}>
                    {customer.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {objectsQuery.isPending ? (
          <div className="flex min-h-56 items-center justify-center gap-3 text-sm text-muted-foreground">
            <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
            {t('objects.states.loading')}
          </div>
        ) : null}

        {objectsQuery.isError ? (
          <div className="grid min-h-56 place-items-center p-6 text-center text-sm text-muted-foreground">
            {t('objects.states.error')}
          </div>
        ) : null}

        {!objectsQuery.isPending && !objectsQuery.isError && objectsQuery.data?.items.length === 0 ? (
          <div className="grid min-h-56 place-items-center p-6 text-center">
            <div className="space-y-3">
              <Building2 aria-hidden="true" className="mx-auto size-8 text-muted-foreground" />
              <p className="font-medium">{emptyTitle}</p>
              <p className="text-sm text-muted-foreground">{emptyDescription}</p>
            </div>
          </div>
        ) : null}

        <ul className="divide-y divide-border">
          {(objectsQuery.data?.items ?? []).map((estimateObject) => (
            <li className="px-5 py-4" key={estimateObject.id}>
              <div className="flex items-start justify-between gap-3">
                <Link className="block min-w-0 flex-1 rounded-md hover:text-primary" to={`/objects/${estimateObject.id}`}>
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-medium">{estimateObject.name}</span>
                      {estimateObject.isArchived ? (
                        <span className="inline-flex rounded-full border border-border bg-card px-2 py-0.5 text-xs font-medium text-muted-foreground">
                          {t('objects.actions.archived')}
                        </span>
                      ) : null}
                    </div>
                    <span className="text-sm text-muted-foreground">
                      {t(`estimates.objectTypes.${estimateObject.objectType}`)}
                    </span>
                  </div>
                  <div className="mt-1 flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted-foreground">
                    {estimateObject.address ? (
                      <span className="inline-flex items-center gap-1">
                        <MapPin aria-hidden="true" className="size-4" />
                        {estimateObject.address}
                      </span>
                    ) : null}
                  </div>
                </Link>
                <Button
                  aria-label={estimateObject.isArchived ? t('objects.actions.restore') : t('objects.actions.archive')}
                  disabled={archiveMutation.isPending || restoreMutation.isPending}
                  onClick={() =>
                    estimateObject.isArchived
                      ? restoreMutation.mutate(estimateObject.id)
                      : archiveMutation.mutate(estimateObject.id)
                  }
                  size="icon"
                  type="button"
                  variant="ghost"
                >
                  <Archive aria-hidden="true" className="size-4" />
                </Button>
              </div>
            </li>
          ))}
        </ul>
      </section>
    </section>
  );
}
