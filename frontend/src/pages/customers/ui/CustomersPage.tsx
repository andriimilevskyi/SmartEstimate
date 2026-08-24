import { Archive, LoaderCircle, Mail, Phone, Plus, Search, UserRound } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';

import { archiveCustomer, createCustomer, restoreCustomer } from '@/entities/business/api/business-api';
import { businessQueryKeys, useCustomersQuery } from '@/entities/business/api/business-queries';
import type { BusinessRecordStatus, Customer } from '@/entities/business/model/types';
import { useTranslation } from '@/shared/i18n/use-translation';
import { formatDate } from '@/shared/lib/formatters';
import { Button } from '@/shared/ui/button';

const fieldClassName =
  'flex h-10 w-full rounded-md border border-input bg-background px-3 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring';

export function CustomersPage() {
  const { locale, t } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [searchParams, setSearchParams] = useSearchParams();
  const statusParam = searchParams.get('status');
  const status: BusinessRecordStatus =
    statusParam === 'archived' || statusParam === 'all' ? statusParam : 'active';
  const [search, setSearch] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(searchParams.get('create') === 'customer');
  const [form, setForm] = useState({ email: '', name: '', note: '', phone: '' });
  const customersQuery = useCustomersQuery(search || undefined, status);
  const createMutation = useMutation({
    mutationFn: () =>
      createCustomer({
        email: form.email.trim() || null,
        name: form.name.trim(),
        note: form.note.trim() || null,
        phone: form.phone.trim() || null,
      }),
    onError: () => toast.error(t('customers.messages.createError')),
    onSuccess: (customer: Customer) => {
      toast.success(t('customers.messages.created'));
      setForm({ email: '', name: '', note: '', phone: '' });
      setIsCreateOpen(false);
      setSearchParams({});
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
      navigate(`/customers/${customer.id}`);
    },
  });
  const archiveMutation = useMutation({
    mutationFn: (customerId: string) => archiveCustomer(customerId),
    onError: () => toast.error(t('customers.messages.archiveError')),
    onSuccess: () => {
      toast.success(t('customers.messages.archived'));
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });
  const restoreMutation = useMutation({
    mutationFn: (customerId: string) => restoreCustomer(customerId),
    onError: () => toast.error(t('customers.messages.restoreError')),
    onSuccess: () => {
      toast.success(t('customers.messages.restored'));
      void queryClient.invalidateQueries({ queryKey: businessQueryKeys.all });
    },
  });

  useEffect(() => {
    setIsCreateOpen(searchParams.get('create') === 'customer');
  }, [searchParams]);

  const setStatus = (nextStatus: BusinessRecordStatus) => {
    const nextParams = new URLSearchParams(searchParams);
    nextParams.set('status', nextStatus);
    setSearchParams(nextParams);
  };

  const emptyTitle =
    status === 'archived'
      ? t('customers.states.emptyArchivedTitle')
      : status === 'all'
        ? t('customers.states.emptyAllTitle')
        : t('customers.states.emptyTitle');
  const emptyDescription =
    status === 'archived'
      ? t('customers.states.emptyArchivedDescription')
      : status === 'all'
        ? t('customers.states.emptyAllDescription')
        : t('customers.states.emptyDescription');

  return (
    <section aria-labelledby="customers-title" className="space-y-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
        <div className="space-y-2">
          <p className="text-sm font-medium text-primary">{t('customers.eyebrow')}</p>
          <h1 className="text-3xl font-semibold tracking-tight" id="customers-title">
            {t('customers.title')}
          </h1>
          <p className="max-w-2xl text-base leading-7 text-muted-foreground">
            {t('customers.description')}
          </p>
        </div>
        <Button onClick={() => setIsCreateOpen(true)}>
          <Plus aria-hidden="true" className="size-4" />
          {t('customers.create.action')}
        </Button>
      </div>

      {isCreateOpen ? (
        <section className="rounded-xl border border-border bg-card p-5 shadow-sm">
          <h2 className="font-semibold">{t('customers.create.title')}</h2>
          <form
            className="mt-4 grid gap-4 lg:grid-cols-2"
            onSubmit={(event) => {
              event.preventDefault();
              if (!form.name.trim()) {
                toast.error(t('customers.validation.name'));
                return;
              }
              createMutation.mutate();
            }}
          >
            <input
              className={fieldClassName}
              onChange={(event) => setForm((current) => ({ ...current, name: event.target.value }))}
              placeholder={t('customers.form.name')}
              value={form.name}
            />
            <input
              className={fieldClassName}
              onChange={(event) => setForm((current) => ({ ...current, phone: event.target.value }))}
              placeholder={t('customers.form.phone')}
              value={form.phone}
            />
            <input
              className={fieldClassName}
              onChange={(event) => setForm((current) => ({ ...current, email: event.target.value }))}
              placeholder={t('customers.form.email')}
              value={form.email}
            />
            <input
              className={fieldClassName}
              onChange={(event) => setForm((current) => ({ ...current, note: event.target.value }))}
              placeholder={t('customers.form.note')}
              value={form.note}
            />
            <div className="flex justify-end gap-2 lg:col-span-2">
              <Button onClick={() => setIsCreateOpen(false)} type="button" variant="ghost">
                {t('customers.create.cancel')}
              </Button>
              <Button disabled={createMutation.isPending} type="submit">
                {createMutation.isPending ? (
                  <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
                ) : null}
                {t('customers.create.submit')}
              </Button>
            </div>
          </form>
        </section>
      ) : null}

      <section className="overflow-hidden rounded-xl border border-border bg-card shadow-sm">
        <div className="border-b border-border p-5">
          <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
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
                  {t(`customers.filters.${value}`)}
                </button>
              ))}
            </div>
            <div className="relative max-w-xl flex-1 lg:max-w-xl">
              <Search aria-hidden="true" className="pointer-events-none absolute left-3 top-3 size-4 text-muted-foreground" />
              <input
                className={`${fieldClassName} pl-9`}
                onChange={(event) => setSearch(event.target.value)}
                placeholder={t('customers.search')}
                value={search}
              />
            </div>
          </div>
        </div>

        {customersQuery.isPending ? (
          <div className="flex min-h-56 items-center justify-center gap-3 text-sm text-muted-foreground">
            <LoaderCircle aria-hidden="true" className="size-5 animate-spin" />
            {t('customers.states.loading')}
          </div>
        ) : null}

        {customersQuery.isError ? (
          <div className="grid min-h-56 place-items-center p-6 text-center text-sm text-muted-foreground">
            {t('customers.states.error')}
          </div>
        ) : null}

        {!customersQuery.isPending && !customersQuery.isError && customersQuery.data?.items.length === 0 ? (
          <div className="grid min-h-56 place-items-center p-6 text-center">
            <div className="space-y-3">
              <UserRound aria-hidden="true" className="mx-auto size-8 text-muted-foreground" />
              <p className="font-medium">{emptyTitle}</p>
              <p className="text-sm text-muted-foreground">{emptyDescription}</p>
            </div>
          </div>
        ) : null}

        <ul className="divide-y divide-border">
          {(customersQuery.data?.items ?? []).map((customer) => (
            <li className="px-5 py-4" key={customer.id}>
              <div className="flex items-start justify-between gap-3">
                <Link className="block min-w-0 flex-1 rounded-md hover:text-primary" to={`/customers/${customer.id}`}>
                  <div className="flex flex-wrap items-center justify-between gap-3">
                    <div className="flex flex-wrap items-center gap-2">
                      <span className="font-medium">{customer.name}</span>
                      {customer.isArchived ? (
                        <span className="inline-flex rounded-full border border-border bg-card px-2 py-0.5 text-xs font-medium text-muted-foreground">
                          {t('customers.actions.archived')}
                        </span>
                      ) : null}
                    </div>
                    <span className="text-sm text-muted-foreground">{formatDate(customer.updatedAt, locale)}</span>
                  </div>
                  <div className="mt-1 flex flex-wrap gap-x-4 gap-y-1 text-sm text-muted-foreground">
                    {customer.phone ? (
                      <span className="inline-flex items-center gap-1">
                        <Phone aria-hidden="true" className="size-4" />
                        {customer.phone}
                      </span>
                    ) : null}
                    {customer.email ? (
                      <span className="inline-flex items-center gap-1">
                        <Mail aria-hidden="true" className="size-4" />
                        {customer.email}
                      </span>
                    ) : null}
                  </div>
                </Link>
                <Button
                  aria-label={customer.isArchived ? t('customers.actions.restore') : t('customers.actions.archive')}
                  disabled={archiveMutation.isPending || restoreMutation.isPending}
                  onClick={() =>
                    customer.isArchived
                      ? restoreMutation.mutate(customer.id)
                      : archiveMutation.mutate(customer.id)
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
