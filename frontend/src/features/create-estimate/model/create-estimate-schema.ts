import { z } from 'zod';

export const createEstimateSchema = z.object({
  currency: z
    .string()
    .trim()
    .regex(/^[a-zA-Z]{3}$/),
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
  totalArea: z.coerce.number().positive().optional().or(z.literal('')),
});

export type CreateEstimateFormValues = z.infer<typeof createEstimateSchema>;
