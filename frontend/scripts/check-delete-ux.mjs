import fs from 'node:fs';
import path from 'node:path';
import process from 'node:process';

const root = process.cwd();

const readFile = (relativePath) => fs.readFileSync(path.join(root, relativePath), 'utf8');

const customersList = readFile('src/pages/customers/ui/CustomersPage.tsx');
const customerDetails = readFile('src/pages/customers/ui/CustomerDetailsPage.tsx');
const estimatesList = readFile('src/pages/estimates/ui/EstimatesPage.tsx');
const estimateDetails = readFile('src/pages/estimate-details/ui/EstimateDetailsPage.tsx');
const permanentEstimateDelete = readFile('src/features/delete-estimate/ui/PermanentDeleteEstimateButton.tsx');
const objectsList = readFile('src/pages/objects/ui/ObjectsPage.tsx');
const objectDetails = readFile('src/pages/object-details/ui/ObjectDetailsPage.tsx');

const failures = [];

if (customersList.includes('deleteCustomerPermanently') || customersList.includes('deletePermanently')) {
  failures.push('Customers list must not expose permanent delete.');
}

if (objectsList.includes('deleteObjectPermanently') || objectsList.includes('deletePermanently')) {
  failures.push('Objects list must not expose permanent delete.');
}

if (!customerDetails.includes('deleteCustomerPermanently') || !customerDetails.includes('customers.actions.deletePermanently')) {
  failures.push('Customer details must expose permanent delete.');
}

if (!objectDetails.includes('deleteObjectPermanently') || !objectDetails.includes('objects.actions.deletePermanently')) {
  failures.push('Object details must expose permanent delete.');
}

if (estimatesList.includes('deleteEstimatePermanently') || estimatesList.includes('PermanentDeleteEstimateButton')) {
  failures.push('Estimates list must not expose permanent delete.');
}

if (
  !estimateDetails.includes('PermanentDeleteEstimateButton')
  || !estimateDetails.includes('estimate.isDeleted')
  || !permanentEstimateDelete.includes("t('estimates.permanentDelete.action')")
) {
  failures.push('Estimate details must expose permanent delete only for soft-deleted estimates.');
}

if (
  !estimateDetails.includes('DeleteEstimateButton')
  || !estimateDetails.includes("!estimate.isDeleted ?")
) {
  failures.push('Estimate details must keep soft delete for active estimates.');
}

if (
  !customerDetails.includes('ApiClientError')
  || !customerDetails.includes("toast.success(t('customers.messages.deleted'))")
  || !customerDetails.includes("toast.error(error.message)")
  || !customerDetails.includes("toast.error(t('customers.messages.deleteError'))")
  || !customerDetails.includes("navigate('/customers')")
) {
  failures.push('Customer details must handle delete success, conflict messaging, and fallback errors.');
}

if (
  !objectDetails.includes('ApiClientError')
  || !objectDetails.includes("toast.success(t('objects.messages.deleted'))")
  || !objectDetails.includes("toast.error(error.message)")
  || !objectDetails.includes("toast.error(t('objects.messages.deleteError'))")
  || !objectDetails.includes("navigate('/objects')")
) {
  failures.push('Object details must handle delete success, conflict messaging, and fallback errors.');
}

if (
  !estimateDetails.includes("navigate('/estimates')")
  || !permanentEstimateDelete.includes("t('estimates.messages.permanentlyDeleted')")
  || !permanentEstimateDelete.includes("t('estimates.messages.permanentDeleteError')")
  || !permanentEstimateDelete.includes('toast.error(error.message)')
) {
  failures.push('Estimate details must handle permanent delete success and fallback errors.');
}

if (failures.length > 0) {
  console.error(failures.join('\n'));
  process.exitCode = 1;
} else {
  console.log('Delete UX contract verified for list/detail pages.');
}
