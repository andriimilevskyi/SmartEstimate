import { zodResolver } from '@hookform/resolvers/zod';
import { ArrowLeft, ArrowRight, Building2, LoaderCircle, Minus, Plus, X } from 'lucide-react';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { toast } from 'sonner';

import type { Estimate, EstimateObjectType } from '@/entities/estimate/model/types';
import {
  createEstimateSchema,
  type CreateEstimateFormValues,
} from '@/features/create-estimate/model/create-estimate-schema';
import { useCreateEstimate } from '@/features/create-estimate/model/use-create-estimate';
import { useTranslation } from '@/shared/i18n/use-translation';
import { Button } from '@/shared/ui/button';

interface CreateEstimateFormProps {
  onCancel: () => void;
  onCreated: (estimate: Estimate) => void;
}

type RepairScope = 'full' | 'partial';

interface ZoneTemplate {
  key: string;
  recommended: number;
}

const zoneTemplates: Record<EstimateObjectType, ZoneTemplate[]> = {
  Apartment: [
    { key: 'kitchen', recommended: 1 },
    { key: 'bathroom', recommended: 1 },
    { key: 'toilet', recommended: 1 },
    { key: 'livingRoom', recommended: 1 },
    { key: 'bedroom', recommended: 2 },
    { key: 'cabinet', recommended: 0 },
    { key: 'balcony', recommended: 1 },
    { key: 'wardrobe', recommended: 0 },
    { key: 'hallway', recommended: 1 },
  ],
  PrivateHouse: [
    { key: 'kitchen', recommended: 1 },
    { key: 'livingRoom', recommended: 1 },
    { key: 'bedroom', recommended: 3 },
    { key: 'bathroom', recommended: 2 },
    { key: 'boilerRoom', recommended: 1 },
    { key: 'stairs', recommended: 1 },
    { key: 'garage', recommended: 1 },
    { key: 'terrace', recommended: 1 },
    { key: 'facade', recommended: 1 },
  ],
  CommercialSpace: [
    { key: 'salesArea', recommended: 1 },
    { key: 'cashDesk', recommended: 1 },
    { key: 'storage', recommended: 1 },
    { key: 'staffRoom', recommended: 1 },
    { key: 'bathroom', recommended: 1 },
    { key: 'entranceGroup', recommended: 1 },
    { key: 'utilityRoom', recommended: 1 },
  ],
  Office: [
    { key: 'openSpace', recommended: 1 },
    { key: 'meetingRoom', recommended: 2 },
    { key: 'managerOffice', recommended: 1 },
    { key: 'kitchenette', recommended: 1 },
    { key: 'bathroom', recommended: 2 },
    { key: 'serverRoom', recommended: 1 },
    { key: 'reception', recommended: 1 },
  ],
  IndustrialSpace: [
    { key: 'productionHall', recommended: 1 },
    { key: 'warehouse', recommended: 1 },
    { key: 'technicalRoom', recommended: 1 },
    { key: 'staffRoom', recommended: 1 },
    { key: 'lockerRoom', recommended: 1 },
    { key: 'bathroom', recommended: 2 },
    { key: 'officeArea', recommended: 1 },
  ],
  Other: [
    { key: 'mainArea', recommended: 1 },
    { key: 'bathroom', recommended: 1 },
    { key: 'technicalRoom', recommended: 0 },
    { key: 'storage', recommended: 0 },
  ],
};

const fieldClassName =
  'flex h-11 w-full rounded-md border border-input bg-background px-3 text-sm shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

const createZoneCounts = (objectType: EstimateObjectType, repairScope: RepairScope) =>
  Object.fromEntries(
    zoneTemplates[objectType].map((zone) => [
      zone.key,
      repairScope === 'full' ? zone.recommended : 0,
    ]),
  );

export function CreateEstimateForm({ onCancel, onCreated }: CreateEstimateFormProps) {
  const { t } = useTranslation();
  const createMutation = useCreateEstimate();
  const [step, setStep] = useState<1 | 2>(1);
  const [repairScope, setRepairScope] = useState<RepairScope>('full');
  const [zoneCounts, setZoneCounts] = useState<Record<string, number>>({});
  const [zoneDraftObjectType, setZoneDraftObjectType] = useState<EstimateObjectType | null>(null);
  const {
    formState: { errors },
    handleSubmit,
    register,
    watch,
  } = useForm<CreateEstimateFormValues>({
    defaultValues: {
      currency: 'UAH',
      estimateNumber: '',
      notes: '',
      objectAddress: '',
      objectType: 'Apartment',
      totalArea: '',
    },
    resolver: zodResolver(createEstimateSchema),
  });
  const objectType = watch('objectType') as EstimateObjectType;
  const activeTemplate = zoneTemplates[objectType];
  const configuredZones = useMemo(
    () =>
      activeTemplate.flatMap((zone) => {
        const count = zoneCounts[zone.key] ?? 0;
        const label = t(`estimates.zones.${zone.key}`);

        if (count === 1) {
          return [label];
        }

        return Array.from({ length: count }, (_, index) => `${label} ${index + 1}`);
      }),
    [activeTemplate, t, zoneCounts],
  );

  const resetZoneDraft = () => {
    setZoneDraftObjectType(null);
    setZoneCounts({});
  };

  const openZoneConfigurator = (values: CreateEstimateFormValues) => {
    if (zoneDraftObjectType !== values.objectType) {
      setRepairScope('full');
      setZoneCounts(createZoneCounts(values.objectType, 'full'));
      setZoneDraftObjectType(values.objectType);
    }

    setStep(2);
  };

  const setScope = (nextScope: RepairScope) => {
    setRepairScope(nextScope);
    setZoneCounts(createZoneCounts(objectType, nextScope));
  };

  const changeZoneCount = (zoneKey: string, delta: number) => {
    setZoneCounts((current) => ({
      ...current,
      [zoneKey]: Math.max(0, Math.min(20, (current[zoneKey] ?? 0) + delta)),
    }));
  };

  const onSubmit = (values: CreateEstimateFormValues) => {
    if (configuredZones.length === 0) {
      toast.error(t('estimates.validation.zones'));
      return;
    }

    createMutation.mutate(
      {
        currency: values.currency.trim().toUpperCase(),
        estimateNumber: values.estimateNumber.trim(),
        materialItems: [],
        notes: values.notes.trim() || undefined,
        objectAddress: values.objectAddress.trim() || null,
        objectType: values.objectType,
        totalArea: values.totalArea === '' ? null : Number(values.totalArea),
        workItems: [],
        zones: configuredZones,
      },
      {
        onError: () => {
          toast.error(t('estimates.messages.createError'));
        },
        onSuccess: (estimate) => {
          toast.success(t('estimates.messages.created'));
          onCreated(estimate);
        },
      },
    );
  };

  return (
    <section
      aria-labelledby="create-estimate-title"
      className="rounded-xl border border-border bg-card p-5 shadow-sm"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex items-start gap-3">
          <div className="rounded-lg bg-primary/10 p-2 text-primary">
            <Building2 aria-hidden="true" className="size-5" />
          </div>
          <div>
            <h2 className="text-lg font-semibold tracking-tight" id="create-estimate-title">
              {t('estimates.create.title')}
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              {step === 1 ? t('estimates.create.stepObject') : t('estimates.create.stepZones')}
            </p>
          </div>
        </div>
        <Button
          aria-label={t('estimates.create.cancel')}
          onClick={onCancel}
          size="sm"
          type="button"
          variant="ghost"
        >
          <X aria-hidden="true" className="size-4" />
        </Button>
      </div>

      <form className="mt-5 space-y-5" noValidate onSubmit={handleSubmit(onSubmit)}>
        {step === 1 ? (
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <label className="text-sm font-medium" htmlFor="estimate-number">
                {t('estimates.form.number')}
              </label>
              <input
                aria-invalid={Boolean(errors.estimateNumber)}
                autoComplete="off"
                className={fieldClassName}
                id="estimate-number"
                placeholder={t('estimates.form.numberPlaceholder')}
                {...register('estimateNumber')}
              />
              {errors.estimateNumber ? (
                <p className="text-sm text-destructive" role="alert">
                  {t('estimates.validation.number')}
                </p>
              ) : null}
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium" htmlFor="estimate-object-type">
                {t('estimates.form.objectType')}
              </label>
              <select
                className={fieldClassName}
                id="estimate-object-type"
                {...register('objectType', {
                  onChange: resetZoneDraft,
                })}
              >
                {Object.keys(zoneTemplates).map((type) => (
                  <option key={type} value={type}>
                    {t(`estimates.objectTypes.${type}`)}
                  </option>
                ))}
              </select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium" htmlFor="estimate-address">
                {t('estimates.form.objectAddress')}
              </label>
              <input
                aria-invalid={Boolean(errors.objectAddress)}
                className={fieldClassName}
                id="estimate-address"
                placeholder={t('estimates.form.objectAddressPlaceholder')}
                {...register('objectAddress')}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium" htmlFor="estimate-area">
                {t('estimates.form.totalArea')}
              </label>
              <input
                aria-invalid={Boolean(errors.totalArea)}
                className={fieldClassName}
                id="estimate-area"
                inputMode="decimal"
                min="0.01"
                placeholder={t('estimates.form.totalAreaPlaceholder')}
                step="0.01"
                type="number"
                {...register('totalArea')}
              />
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium" htmlFor="estimate-currency">
                {t('estimates.form.currency')}
              </label>
              <input
                aria-invalid={Boolean(errors.currency)}
                autoCapitalize="characters"
                autoComplete="off"
                className={`${fieldClassName} uppercase`}
                id="estimate-currency"
                maxLength={3}
                placeholder={t('estimates.form.currencyPlaceholder')}
                {...register('currency')}
              />
            </div>

            <div className="space-y-2 md:col-span-2">
              <label className="text-sm font-medium" htmlFor="estimate-notes">
                {t('estimates.form.notes')}
              </label>
              <textarea
                aria-invalid={Boolean(errors.notes)}
                className="min-h-24 w-full rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm outline-none transition-colors placeholder:text-muted-foreground focus-visible:ring-2 focus-visible:ring-ring"
                id="estimate-notes"
                placeholder={t('estimates.form.notesPlaceholder')}
                {...register('notes')}
              />
            </div>
          </div>
        ) : (
          <div className="space-y-5">
            <div className="rounded-lg border border-dashed border-primary/30 bg-primary/5 p-4 text-sm leading-6 text-muted-foreground">
              {t('estimates.create.zoneDraftNotice')}
            </div>

            <div className="grid gap-2 sm:grid-cols-2" role="group">
              <Button
                aria-pressed={repairScope === 'full'}
                onClick={() => setScope('full')}
                type="button"
                variant={repairScope === 'full' ? 'secondary' : 'outline'}
              >
                {t('estimates.scope.full')}
              </Button>
              <Button
                aria-pressed={repairScope === 'partial'}
                onClick={() => setScope('partial')}
                type="button"
                variant={repairScope === 'partial' ? 'secondary' : 'outline'}
              >
                {t('estimates.scope.partial')}
              </Button>
            </div>

            <div className="divide-y divide-border rounded-lg border border-border">
              {activeTemplate.map((zone) => (
                <div className="flex items-center justify-between gap-4 px-4 py-3" key={zone.key}>
                  <span className="font-medium">{t(`estimates.zones.${zone.key}`)}</span>
                  <div className="flex items-center gap-2">
                    <Button
                      aria-label={t('estimates.create.decreaseZone')}
                      disabled={(zoneCounts[zone.key] ?? 0) === 0}
                      onClick={() => changeZoneCount(zone.key, -1)}
                      size="icon"
                      type="button"
                      variant="outline"
                    >
                      <Minus aria-hidden="true" className="size-4" />
                    </Button>
                    <span className="w-8 text-center font-semibold tabular-nums">
                      {zoneCounts[zone.key] ?? 0}
                    </span>
                    <Button
                      aria-label={t('estimates.create.increaseZone')}
                      onClick={() => changeZoneCount(zone.key, 1)}
                      size="icon"
                      type="button"
                      variant="outline"
                    >
                      <Plus aria-hidden="true" className="size-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>

            <div className="rounded-lg bg-muted p-3 text-sm text-muted-foreground">
              <p className="font-medium text-foreground">
                {t('estimates.create.futureZones', { count: configuredZones.length })}
              </p>
              <p className="mt-1">
                {configuredZones.length > 0
                  ? configuredZones.join(', ')
                  : t('estimates.create.noZones')}
              </p>
            </div>
          </div>
        )}

        <div className="flex flex-wrap justify-between gap-3">
          <Button
            onClick={step === 1 ? onCancel : () => setStep(1)}
            type="button"
            variant="outline"
          >
            {step === 1 ? null : <ArrowLeft aria-hidden="true" className="size-4" />}
            {step === 1 ? t('estimates.create.cancel') : t('estimates.create.back')}
          </Button>
          {step === 1 ? (
            <Button onClick={handleSubmit(openZoneConfigurator)} type="button">
              {t('estimates.create.next')}
              <ArrowRight aria-hidden="true" className="size-4" />
            </Button>
          ) : (
            <Button disabled={createMutation.isPending || configuredZones.length === 0} type="submit">
              {createMutation.isPending ? (
                <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
              ) : null}
              {t('estimates.create.submit')}
            </Button>
          )}
        </div>
      </form>
    </section>
  );
}
