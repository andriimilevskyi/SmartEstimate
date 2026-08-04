import { z } from 'zod';

export const addEstimateItemSchema = z.object({
  catalogItemId: z.string().min(1),
  notes: z.string().max(2000),
  quantity: z.coerce.number().positive(),
  unitPrice: z.coerce.number().min(0),
});

export type AddEstimateItemFormValues = z.infer<typeof addEstimateItemSchema>;
