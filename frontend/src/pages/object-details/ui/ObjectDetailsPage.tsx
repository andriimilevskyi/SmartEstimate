import {
  Archive,
  ArrowLeft,
  Building2,
  LoaderCircle,
  Mail,
  MapPin,
  Pencil,
  Phone,
  Ruler,
  Trash2,
} from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';

import { getApiErrorMessage } from '@/shared/api/api-client';
import {
  archiveObject,
  deleteObjectPermanently,
  restoreObject,
  updateObject,
} from '@/entities/business/api/business-api';
import { useObjectQuery } from '@/entities/business/api/business-queries';
import { businessQueryKeys } from '@/entities/business/api/business-queries';
import { useEstimatesQuery } from '@/entities/estimate/api/estimate-queries';
import type { EstimateObjectType } from '@/entities/estimate/model/types';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatDate, formatMoney } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';
import { ConfirmationDialog } from '@/shared/ui/confirmation-dialog';

const objectTypes: EstimateObjectType[] = [
  'Apartment',
  'PrivateHouse',
  'CommercialSpace',
  'Office',
  'IndustrialSpace',
  'Other',
];

export function ObjectDetailsPage() {
  const { locale, t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { objectId } = useParams();
  const objectQuery = useObjectQuery(objectId ?? '');
  const estimatesQuery = useEstimatesQuery({ objectId: objectId || undefined });
  const estimateObject = objectQuery.data;
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [form, setForm] = useState({
    address: '',
    description: '',
    name: '',
    objectType: 'Apartment' as EstimateObjectType,
    totalArea: '',
  });
  const estimates = estimatesQuery.data?.items ?? [];
  const updateMutation = useMutation({
    mutationFn: () => {
      if (!estimateObject) {
        throw new Error('Estimate object is not loaded.');
      }

      return updateObject(estimateObject.id, {
        address: form.address.trim() || null,
        customerId: estimateObject.customer.id,
        description: form.description.trim() || null,
        name: form.name.trim(),
        objectType: form.objectType,
        totalArea: form.totalArea === '' ? null : Number(form.totalArea),
      });
    },
    onError: () => toast.error(t('objects.messages.updateError')),
    onSuccess: () => {
      toast.success(t('objects.messages.updated'));
      setIsEditOpen(false);
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });
  const archiveMutation = useMutation({
    mutationFn: () => {
      if (!estimateObject) {
        throw new Error('Estimate object is not loaded.');
      }

      return archiveObject(estimateObject.id);
    },
    onError: () => toast.error(t('objects.messages.archiveError')),
    onSuccess: () => {
      toast.success(t('objects.messages.archived'));
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });
  const restoreMutation = useMutation({
    mutationFn: () => {
      if (!estimateObject) {
        throw new Error('Estimate object is not loaded.');
      }

      return restoreObject(estimateObject.id);
    },
    onError: () => toast.error(t('objects.messages.restoreError')),
    onSuccess: () => {
      toast.success(t('objects.messages.restored'));
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });
  const deleteMutation = useMutation({
    mutationFn: () => {
      if (!estimateObject) {
        throw new Error('Estimate object is not loaded.');
      }

      return deleteObjectPermanently(estimateObject.id);
    },
    onError: (error: unknown) => {
      toast.error(getApiErrorMessage(error, t, 'objects.messages.deleteError'));
    },
    onSuccess: () => {
      toast.success(t('objects.messages.deleted'));
      setIsDeleteOpen(false);
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
      navigate('/objects');
    },
  });

  if (!objectId) {
    return null;
  }

  if (objectQuery.isPending) {
    return (
      <div className="flex min-h-64 items-center justify-center gap-3 text-sm text-muted-foreground">
        <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
        {t('objects.states.loading')}
      </div>
    );
  }

  if (objectQuery.isError || !estimateObject) {
    return (
      <section className="grid min-h-[50vh] place-items-center text-center">
        <div className="max-w-md space-y-4">
          <h1 className="text-2xl font-semibold">{t('objects.states.unavailableTitle')}</h1>
          <p className="text-sm leading-6 text-muted-foreground">
            {t('objects.states.unavailableDescription')}
          </p>
          <Button asChild>
            <Link to="/objects">{t('objects.backToList')}</Link>
          </Button>
        </div>
      </section>
    );
  }
  const formattedArea =
    estimateObject.totalArea == null
      ? null
      : t('estimateDetails.objectArea', {
          area: new Intl.NumberFormat(locale, { maximumFractionDigits: 2 }).format(
            estimateObject.totalArea,
          ),
        });

  const openEdit = () => {
    setForm({
      address: estimateObject.address ?? '',
      description: estimateObject.description ?? '',
      name: estimateObject.name,
      objectType: estimateObject.objectType,
      totalArea: estimateObject.totalArea == null ? '' : String(estimateObject.totalArea),
    });
    setIsEditOpen(true);
  };

  return (
    <section aria-labelledby="object-details-title" className="space-y-6">
      <Link
        className="inline-flex items-center gap-2 rounded-md text-sm font-medium text-muted-foreground hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        to="/objects"
      >
        <ArrowLeft aria-hidden="true" className="size-4" />
        {t('objects.backToList')}
      </Link>

      <div className="space-y-2">
        <p className="text-sm font-medium text-primary">{t('objects.eyebrow')}</p>
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="space-y-2">
            <h1 className="text-3xl font-semibold tracking-tight" id="object-details-title">
              {estimateObject.name}
            </h1>
            {estimateObject.isArchived ? (
              <span className="inline-flex rounded-full border border-border bg-card px-3 py-1 text-xs font-medium text-muted-foreground">
                {t('objects.actions.archived')}
              </span>
            ) : null}
          </div>
          <div className="flex flex-wrap gap-2">
            {!estimateObject.isArchived ? (
              <>
                <Button onClick={openEdit} type="button" variant="outline">
                  <Pencil aria-hidden="true" className="size-4" />
                  {t('objects.actions.edit')}
                </Button>
                <Button
                  disabled={archiveMutation.isPending}
                  onClick={() => archiveMutation.mutate()}
                  type="button"
                  variant="outline"
                >
                  <Archive aria-hidden="true" className="size-4" />
                  {t('objects.actions.archive')}
                </Button>
              </>
            ) : (
              <>
                <Button
                  disabled={restoreMutation.isPending}
                  onClick={() => restoreMutation.mutate()}
                  type="button"
                  variant="outline"
                >
                  <Archive aria-hidden="true" className="size-4" />
                  {t('objects.actions.restore')}
                </Button>
                <Button
                  disabled={deleteMutation.isPending}
                  onClick={() => setIsDeleteOpen(true)}
                  type="button"
                  variant="ghost"
                >
                  <Trash2 aria-hidden="true" className="size-4" />
                  {t('objects.actions.deletePermanently')}
                </Button>
              </>
            )}
          </div>
        </div>
        <div className="flex flex-wrap gap-2 text-sm text-muted-foreground">
          <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1">
            <Building2 aria-hidden="true" className="size-4" />
            {t(`estimates.objectTypes.${estimateObject.objectType}`)}
          </span>
          {estimateObject.address ? (
            <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1">
              <MapPin aria-hidden="true" className="size-4" />
              {estimateObject.address}
            </span>
          ) : null}
          {formattedArea ? (
            <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1">
              <Ruler aria-hidden="true" className="size-4" />
              {formattedArea}
            </span>
          ) : null}
        </div>
      </div>

      {isEditOpen ? (
        <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
          <h2 className="font-semibold">{t('objects.edit.title')}</h2>
          <form
            className="mt-4 grid gap-4 lg:grid-cols-2"
            onSubmit={(event) => {
              event.preventDefault();
              if (!form.name.trim()) {
                toast.error(t('objects.validation.required'));
                return;
              }
              updateMutation.mutate();
            }}
          >
            <input
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
              onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
              placeholder={t('objects.form.name')}
              value={form.name}
            />
            <select
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  objectType: event.target.value as EstimateObjectType,
                }))
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
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
              onChange={(event) =>
                setForm((current) => ({ ...current, address: event.target.value }))
              }
              placeholder={t('objects.form.address')}
              value={form.address}
            />
            <input
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
              inputMode="decimal"
              min="0.01"
              onChange={(event) =>
                setForm((current) => ({ ...current, totalArea: event.target.value }))
              }
              placeholder={t('objects.form.totalArea')}
              step="0.01"
              type="number"
              value={form.totalArea}
            />
            <input
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring lg:col-span-2"
              onChange={(event) =>
                setForm((current) => ({ ...current, description: event.target.value }))
              }
              placeholder={t('objects.form.description')}
              value={form.description}
            />
            <div className="flex justify-end gap-2 lg:col-span-2">
              <Button onClick={() => setIsEditOpen(false)} type="button" variant="ghost">
                {t('objects.create.cancel')}
              </Button>
              <Button disabled={updateMutation.isPending} type="submit">
                {updateMutation.isPending ? (
                  <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
                ) : null}
                {t('objects.edit.submit')}
              </Button>
            </div>
          </form>
        </section>
      ) : null}

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_minmax(18rem,0.45fr)]">
        <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
          <h2 className="font-semibold">{t('objects.objectInfo')}</h2>
          <dl className="mt-4 grid gap-4 text-sm sm:grid-cols-2">
            <div>
              <dt className="text-muted-foreground">{t('objects.createdAt')}</dt>
              <dd className="mt-1 font-medium">{formatDate(estimateObject.createdAt, locale)}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">{t('objects.updatedAt')}</dt>
              <dd className="mt-1 font-medium">{formatDate(estimateObject.updatedAt, locale)}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">{t('objects.estimateCount')}</dt>
              <dd className="mt-1 font-medium">{estimateObject.estimateCount}</dd>
            </div>
          </dl>
          {estimateObject.description ? (
            <p className="mt-4 whitespace-pre-wrap text-sm leading-6 text-muted-foreground">
              {estimateObject.description}
            </p>
          ) : null}
        </section>

        <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
          <h2 className="font-semibold">{t('objects.customerInfo')}</h2>
          <Link
            className="mt-4 inline-block rounded-md font-medium hover:text-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            to={`/customers/${estimateObject.customer.id}`}
          >
            {estimateObject.customer.name}
          </Link>
          <div className="mt-3 space-y-2 text-sm text-muted-foreground">
            {estimateObject.customer.phone ? (
              <p className="flex items-center gap-2">
                <Phone aria-hidden="true" className="size-4" />
                {estimateObject.customer.phone}
              </p>
            ) : null}
            {estimateObject.customer.email ? (
              <p className="flex items-center gap-2">
                <Mail aria-hidden="true" className="size-4" />
                {estimateObject.customer.email}
              </p>
            ) : null}
            {estimateObject.customer.note ? <p>{estimateObject.customer.note}</p> : null}
          </div>
        </section>
      </div>

      <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h2 className="font-semibold">{t('objects.estimates')}</h2>
          <span className="text-sm text-muted-foreground">{estimateObject.estimateCount}</span>
        </div>
        {estimatesQuery.isPending ? (
          <p className="p-5 text-sm text-muted-foreground">{t('estimates.states.loading')}</p>
        ) : null}
        {!estimatesQuery.isPending && estimates.length === 0 ? (
          <p className="p-5 text-sm text-muted-foreground">{t('objects.noEstimates')}</p>
        ) : null}
        <ul className="divide-y divide-border">
          {estimates.map((estimate) => (
            <li className="px-5 py-4" key={estimate.id}>
              <Link
                className="block rounded-md hover:text-primary"
                to={`/estimates/${estimate.id}`}
              >
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <span className="font-medium">{estimate.estimateNumber}</span>
                  <span className="font-semibold tabular-nums">
                    {formatMoney(estimate.grandTotal, estimate.currency, locale)}
                  </span>
                </div>
                <div className="mt-1 flex flex-wrap gap-x-3 gap-y-1 text-sm text-muted-foreground">
                  <span>{t(`estimates.status.${estimate.status}`)}</span>
                  <span>{formatDate(estimate.updatedAt, locale)}</span>
                </div>
              </Link>
            </li>
          ))}
        </ul>
      </section>
      <ConfirmationDialog
        cancelLabel={t('actions.cancel')}
        confirmLabel={t('objects.actions.deletePermanently')}
        description={t('objects.delete.confirmation')}
        isLoading={deleteMutation.isPending}
        isOpen={isDeleteOpen}
        onCancel={() => setIsDeleteOpen(false)}
        onConfirm={() => deleteMutation.mutate()}
        title={t('objects.delete.title')}
        variant="destructive"
      />
    </section>
  );
}
