import {
  Archive,
  ArrowLeft,
  Building2,
  LoaderCircle,
  Mail,
  Pencil,
  Phone,
  Trash2,
} from 'lucide-react';
import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';

import { getApiErrorMessage } from '@/shared/api/api-client';
import {
  archiveCustomer,
  deleteCustomerPermanently,
  restoreCustomer,
  updateCustomer,
} from '@/entities/business/api/business-api';
import {
  businessQueryKeys,
  useCustomerQuery,
  useObjectsQuery,
} from '@/entities/business/api/business-queries';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatDate } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';
import { ConfirmationDialog } from '@/shared/ui/confirmation-dialog';

export function CustomerDetailsPage() {
  const { locale, t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { customerId } = useParams();
  const customerQuery = useCustomerQuery(customerId ?? '');
  const objectsQuery = useObjectsQuery(customerId || undefined);
  const customer = customerQuery.data;
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [form, setForm] = useState({ email: '', name: '', note: '', phone: '' });
  const objects = objectsQuery.data?.items ?? [];
  const updateMutation = useMutation({
    mutationFn: () => {
      if (!customer) {
        throw new Error('Customer is not loaded.');
      }

      return updateCustomer(customer.id, {
        email: form.email.trim() || null,
        name: form.name.trim(),
        note: form.note.trim() || null,
        phone: form.phone.trim() || null,
      });
    },
    onError: () => toast.error(t('customers.messages.updateError')),
    onSuccess: () => {
      toast.success(t('customers.messages.updated'));
      setIsEditOpen(false);
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });
  const archiveMutation = useMutation({
    mutationFn: () => {
      if (!customer) {
        throw new Error('Customer is not loaded.');
      }

      return archiveCustomer(customer.id);
    },
    onError: () => toast.error(t('customers.messages.archiveError')),
    onSuccess: () => {
      toast.success(t('customers.messages.archived'));
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });
  const restoreMutation = useMutation({
    mutationFn: () => {
      if (!customer) {
        throw new Error('Customer is not loaded.');
      }

      return restoreCustomer(customer.id);
    },
    onError: () => toast.error(t('customers.messages.restoreError')),
    onSuccess: () => {
      toast.success(t('customers.messages.restored'));
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });
  const deleteMutation = useMutation({
    mutationFn: () => {
      if (!customer) {
        throw new Error('Customer is not loaded.');
      }

      return deleteCustomerPermanently(customer.id);
    },
    onError: (error: unknown) => {
      toast.error(getApiErrorMessage(error, t, 'customers.messages.deleteError'));
    },
    onSuccess: () => {
      toast.success(t('customers.messages.deleted'));
      setIsDeleteOpen(false);
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
      navigate('/customers');
    },
  });

  if (!customerId) {
    return null;
  }

  if (customerQuery.isPending) {
    return (
      <div className="flex min-h-64 items-center justify-center gap-3 text-sm text-muted-foreground">
        <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
        {t('customers.states.loading')}
      </div>
    );
  }

  if (customerQuery.isError || !customer) {
    return (
      <section className="grid min-h-[50vh] place-items-center text-center">
        <div className="max-w-md space-y-4">
          <h1 className="text-2xl font-semibold">{t('customers.states.unavailableTitle')}</h1>
          <p className="text-sm leading-6 text-muted-foreground">
            {t('customers.states.unavailableDescription')}
          </p>
          <Button asChild>
            <Link to="/customers">{t('customers.backToList')}</Link>
          </Button>
        </div>
      </section>
    );
  }

  const openEdit = () => {
    setForm({
      email: customer.email ?? '',
      name: customer.name,
      note: customer.note ?? '',
      phone: customer.phone ?? '',
    });
    setIsEditOpen(true);
  };

  return (
    <section aria-labelledby="customer-details-title" className="space-y-6">
      <Link
        className="inline-flex items-center gap-2 rounded-md text-sm font-medium text-muted-foreground hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        to="/customers"
      >
        <ArrowLeft aria-hidden="true" className="size-4" />
        {t('customers.backToList')}
      </Link>

      <div className="space-y-2">
        <p className="text-sm font-medium text-primary">{t('customers.details.eyebrow')}</p>
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div className="space-y-2">
            <h1 className="text-3xl font-semibold tracking-tight" id="customer-details-title">
              {customer.name}
            </h1>
            {customer.isArchived ? (
              <span className="inline-flex rounded-full border border-border bg-card px-3 py-1 text-xs font-medium text-muted-foreground">
                {t('customers.actions.archived')}
              </span>
            ) : null}
          </div>
          <div className="flex flex-wrap gap-2">
            {!customer.isArchived ? (
              <>
                <Button onClick={openEdit} type="button" variant="outline">
                  <Pencil aria-hidden="true" className="size-4" />
                  {t('customers.actions.edit')}
                </Button>
                <Button
                  disabled={archiveMutation.isPending}
                  onClick={() => archiveMutation.mutate()}
                  type="button"
                  variant="outline"
                >
                  <Archive aria-hidden="true" className="size-4" />
                  {t('customers.actions.archive')}
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
                  {t('customers.actions.restore')}
                </Button>
                <Button
                  disabled={deleteMutation.isPending}
                  onClick={() => setIsDeleteOpen(true)}
                  type="button"
                  variant="ghost"
                >
                  <Trash2 aria-hidden="true" className="size-4" />
                  {t('customers.actions.deletePermanently')}
                </Button>
              </>
            )}
          </div>
        </div>
        <div className="flex flex-wrap gap-2 text-sm text-muted-foreground">
          {customer.phone ? (
            <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1">
              <Phone aria-hidden="true" className="size-4" />
              {customer.phone}
            </span>
          ) : null}
          {customer.email ? (
            <span className="inline-flex items-center gap-2 rounded-full border border-border bg-card px-3 py-1">
              <Mail aria-hidden="true" className="size-4" />
              {customer.email}
            </span>
          ) : null}
        </div>
      </div>

      {isEditOpen ? (
        <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
          <h2 className="font-semibold">{t('customers.edit.title')}</h2>
          <form
            className="mt-4 grid gap-4 lg:grid-cols-2"
            onSubmit={(event) => {
              event.preventDefault();
              if (!form.name.trim()) {
                toast.error(t('customers.validation.name'));
                return;
              }
              updateMutation.mutate();
            }}
          >
            <input
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
              onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
              placeholder={t('customers.form.name')}
              value={form.name}
            />
            <input
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
              onChange={(event) =>
                setForm((current) => ({ ...current, phone: event.target.value }))
              }
              placeholder={t('customers.form.phone')}
              value={form.phone}
            />
            <input
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
              onChange={(event) =>
                setForm((current) => ({ ...current, email: event.target.value }))
              }
              placeholder={t('customers.form.email')}
              value={form.email}
            />
            <input
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
              onChange={(event) => setForm((current) => ({ ...current, note: event.target.value }))}
              placeholder={t('customers.form.note')}
              value={form.note}
            />
            <div className="flex justify-end gap-2 lg:col-span-2">
              <Button onClick={() => setIsEditOpen(false)} type="button" variant="ghost">
                {t('customers.create.cancel')}
              </Button>
              <Button disabled={updateMutation.isPending} type="submit">
                {updateMutation.isPending ? (
                  <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
                ) : null}
                {t('customers.edit.submit')}
              </Button>
            </div>
          </form>
        </section>
      ) : null}

      <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
        <h2 className="font-semibold">{t('customers.details.info')}</h2>
        <dl className="mt-4 grid gap-4 text-sm sm:grid-cols-2">
          <div>
            <dt className="text-muted-foreground">{t('customers.details.createdAt')}</dt>
            <dd className="mt-1 font-medium">{formatDate(customer.createdAt, locale)}</dd>
          </div>
          <div>
            <dt className="text-muted-foreground">{t('customers.details.updatedAt')}</dt>
            <dd className="mt-1 font-medium">{formatDate(customer.updatedAt, locale)}</dd>
          </div>
        </dl>
        {customer.note ? (
          <p className="mt-4 whitespace-pre-wrap text-sm leading-6 text-muted-foreground">
            {customer.note}
          </p>
        ) : null}
      </section>

      <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h2 className="font-semibold">{t('customers.details.objects')}</h2>
          <span className="text-sm text-muted-foreground">
            {objectsQuery.data?.totalCount ?? 0}
          </span>
        </div>
        {objectsQuery.isPending ? (
          <p className="p-5 text-sm text-muted-foreground">{t('objects.states.loading')}</p>
        ) : null}
        {!objectsQuery.isPending && objects.length === 0 ? (
          <p className="p-5 text-sm text-muted-foreground">{t('customers.details.noObjects')}</p>
        ) : null}
        <ul className="divide-y divide-border">
          {objects.map((estimateObject) => (
            <li className="px-5 py-4" key={estimateObject.id}>
              <Link
                className="block rounded-md hover:text-primary"
                to={`/objects/${estimateObject.id}`}
              >
                <div className="flex flex-wrap items-center gap-3">
                  <Building2 aria-hidden="true" className="size-4 text-muted-foreground" />
                  <span className="font-medium">{estimateObject.name}</span>
                  <span className="text-sm text-muted-foreground">
                    {t(`estimates.objectTypes.${estimateObject.objectType}`)}
                  </span>
                </div>
                {estimateObject.address ? (
                  <p className="mt-1 text-sm text-muted-foreground">{estimateObject.address}</p>
                ) : null}
              </Link>
            </li>
          ))}
        </ul>
      </section>
      <ConfirmationDialog
        cancelLabel={t('actions.cancel')}
        confirmLabel={t('customers.actions.deletePermanently')}
        description={t('customers.delete.confirmation')}
        isLoading={deleteMutation.isPending}
        isOpen={isDeleteOpen}
        onCancel={() => setIsDeleteOpen(false)}
        onConfirm={() => deleteMutation.mutate()}
        title={t('customers.delete.title')}
        variant="destructive"
      />
    </section>
  );
}
