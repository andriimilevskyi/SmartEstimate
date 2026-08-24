import { Download, Eye, FileText, LoaderCircle } from 'lucide-react';
import { useEffect, useState } from 'react';
import { toast } from 'sonner';

import {
  getEstimatePdfDocument,
  getEstimatePdfDocumentUrl,
} from '@/entities/estimate/api/estimate-document-api';
import { useEstimateDocumentTemplatesQuery } from '@/entities/estimate/api/estimate-queries';
import type { Estimate, EstimateDocumentTemplate } from '@/entities/estimate/model/types';
import { useTranslation } from '@/shared/i18n/use-translation';
import { Button } from '@/shared/ui/button';

interface EstimateDocumentsPanelProps {
  estimate: Estimate;
}

const createPdfUrl = (blob: Blob) =>
  URL.createObjectURL(new Blob([blob], { type: 'application/pdf' }));
const emptyTemplates: EstimateDocumentTemplate[] = [];

const buildFileName = (estimateNumber: string, templateCode: string) => {
  const safeNumber = estimateNumber.replace(/[^a-zA-Z0-9а-яА-ЯіїєґІЇЄҐ_-]+/g, '-');

  return `smartestimate-${safeNumber}-${templateCode}.pdf`;
};

export function EstimateDocumentsPanel({ estimate }: EstimateDocumentsPanelProps) {
  const { locale, t } = useTranslation();
  const templatesQuery = useEstimateDocumentTemplatesQuery(locale);
  const templates = templatesQuery.data ?? emptyTemplates;
  const [selectedTemplateCode, setSelectedTemplateCode] = useState('');
  const [isPreviewLoading, setIsPreviewLoading] = useState(false);
  const [isDownloadLoading, setIsDownloadLoading] = useState(false);

  useEffect(() => {
    if (!selectedTemplateCode && templates[0]) {
      setSelectedTemplateCode(templates[0].code);
    }
  }, [selectedTemplateCode, templates]);

  const selectedTemplate = templates.find((template) => template.code === selectedTemplateCode);

  const openPreview = async () => {
    if (!selectedTemplate) {
      return;
    }

    setIsPreviewLoading(true);
    const previewUrl = getEstimatePdfDocumentUrl(estimate.id, selectedTemplate.code, locale);
    const previewWindow = window.open(previewUrl, '_blank', 'noopener,noreferrer');
    if (!previewWindow) {
      toast.error(t('estimateDocuments.messages.previewError'));
    }
    setIsPreviewLoading(false);
  };

  const downloadPdf = async () => {
    if (!selectedTemplate) {
      return;
    }

    setIsDownloadLoading(true);
    try {
      const blob = await getEstimatePdfDocument(estimate.id, selectedTemplate.code, locale);
      const url = createPdfUrl(blob);
      const link = document.createElement('a');

      link.href = url;
      link.download = buildFileName(estimate.estimateNumber, selectedTemplate.code);
      link.click();
      URL.revokeObjectURL(url);
    } catch {
      toast.error(t('estimateDocuments.messages.downloadError'));
    } finally {
      setIsDownloadLoading(false);
    }
  };

  return (
    <section
      aria-labelledby="estimate-documents-title"
      className="rounded-xl border border-border bg-card p-5 shadow-sm"
    >
      <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-start">
        <div className="flex items-start gap-3">
          <div className="rounded-lg bg-primary/10 p-2 text-primary">
            <FileText aria-hidden="true" className="size-5" />
          </div>
          <div>
            <h2 className="font-semibold" id="estimate-documents-title">
              {t('estimateDocuments.title')}
            </h2>
            <p className="mt-1 max-w-2xl text-sm leading-6 text-muted-foreground">
              {t('estimateDocuments.description')}
            </p>
          </div>
        </div>

        <div className="grid gap-3 sm:grid-cols-[minmax(15rem,1fr)_auto_auto]">
          <label className="sr-only" htmlFor="estimate-document-template">
            {t('estimateDocuments.template')}
          </label>
          <select
            className="h-10 rounded-md border border-input bg-background px-3 text-sm shadow-sm outline-none transition-colors focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            disabled={templatesQuery.isPending || templates.length === 0}
            id="estimate-document-template"
            onChange={(event) => setSelectedTemplateCode(event.target.value)}
            value={selectedTemplateCode}
          >
            {templatesQuery.isPending ? (
              <option value="">{t('estimateDocuments.loading')}</option>
            ) : null}
            {templates.map((template) => (
              <option key={template.code} value={template.code}>
                {template.name}
              </option>
            ))}
          </select>
          <Button
            disabled={!selectedTemplate || isPreviewLoading}
            onClick={() => void openPreview()}
            type="button"
            variant="outline"
          >
            {isPreviewLoading ? (
              <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
            ) : (
              <Eye aria-hidden="true" className="size-4" />
            )}
            {t('estimateDocuments.preview')}
          </Button>
          <Button
            disabled={!selectedTemplate || isDownloadLoading}
            onClick={() => void downloadPdf()}
            type="button"
          >
            {isDownloadLoading ? (
              <LoaderCircle aria-hidden="true" className="size-4 animate-spin" />
            ) : (
              <Download aria-hidden="true" className="size-4" />
            )}
            {t('estimateDocuments.download')}
          </Button>
        </div>
      </div>

      {selectedTemplate ? (
        <p className="mt-4 text-sm text-muted-foreground">{selectedTemplate.description}</p>
      ) : null}

      {templatesQuery.isError ? (
        <p className="mt-4 text-sm text-destructive">{t('estimateDocuments.unavailable')}</p>
      ) : null}
    </section>
  );
}
