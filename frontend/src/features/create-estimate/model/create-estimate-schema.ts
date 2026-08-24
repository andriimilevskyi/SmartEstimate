import { z } from 'zod';

export const createEstimateSchema = z.object({
  currency: z
    .string()
    .trim()
    .regex(/^[a-zA-Z]{3}$/),
  customerEmail: z.string().trim().email().or(z.literal('')),
  customerName: z.string().trim(),
  customerNote: z.string().max(2000),
  customerPhone: z.string().trim().max(64),
  objectDescription: z.string().max(2000),
  objectId: z.string(),
  objectMode: z.enum(['existing', 'new']),
  objectAddress: z.string().trim().max(512),
  objectType: z.enum([
    'Apartment',
    'PrivateHouse',
    'CommercialSpace',
    'Office',
    'IndustrialSpace',
    'Other',
  ]),
  estimateNumber: z.string().trim().min(1).max(64),
  notes: z.string().max(2000),
  objectName: z.string().trim(),
  totalArea: z.coerce.number().positive().optional().or(z.literal('')),
}).superRefine((value, context) => {
  if (value.objectMode === 'existing' && !value.objectId) {
    context.addIssue({
      code: z.ZodIssueCode.custom,
      message: 'Object is required.',
      path: ['objectId'],
    });
  }

  if (value.objectMode === 'new') {
    if (!value.customerName.trim()) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Customer name is required.',
        path: ['customerName'],
      });
    }

    if (!value.objectName.trim()) {
      context.addIssue({
        code: z.ZodIssueCode.custom,
        message: 'Object name is required.',
        path: ['objectName'],
      });
    }
  }
});

export type CreateEstimateFormValues = z.infer<typeof createEstimateSchema>;
