import fs from 'node:fs';
import path from 'node:path';
import process from 'node:process';

import i18next from 'i18next';

const root = process.cwd();
const sourceRoot = path.join(root, 'src');
const localesRoot = path.join(sourceRoot, 'shared/i18n/locales');
const locales = ['uk', 'en', 'de'];
const defaultLocale = 'uk';
const namespaces = ['common', 'navigation', 'estimate', 'knowledge', 'validation'];
const fallbackNamespaces = namespaces.filter((namespace) => namespace !== 'common');
const pluralSuffixes = ['zero', 'one', 'two', 'few', 'many', 'other'];
const keyPropNames = ['labelKey', 'titleKey', 'descriptionKey', 'messageKey', 'fallbackKey'];
const keyRoots = [
  'actions',
  'customers',
  'errors',
  'estimateDetails',
  'estimateDocuments',
  'estimateEditor',
  'estimates',
  'knowledgeStudio',
  'language',
  'materials',
  'navigation',
  'objects',
  'overview',
  'placeholders',
  'pricing',
  'shell',
  'validation',
];
const ignoredHardcodedPaths = [
  'src/features/add-estimate-item/ui/AddEstimateItemForm.tsx',
  'src/features/estimate-documents/ui/EstimateDocumentsPanel.tsx',
];
const strictHardcodedUiPaths = [
  'src/entities/pricing/',
  'src/pages/materials/',
  'src/pages/pricing/',
  'src/shared/ui/',
];

function readText(filePath) {
  return fs.readFileSync(filePath, 'utf8');
}

function readJson(filePath) {
  return JSON.parse(readText(filePath));
}

function flattenKeys(value, prefix = '') {
  return Object.entries(value).flatMap(([key, child]) => {
    const nextPrefix = prefix ? `${prefix}.${key}` : key;

    if (child && typeof child === 'object' && !Array.isArray(child)) {
      return flattenKeys(child, nextPrefix);
    }

    return [nextPrefix];
  });
}

function flattenEntries(value, prefix = '') {
  return Object.entries(value).flatMap(([key, child]) => {
    const nextPrefix = prefix ? `${prefix}.${key}` : key;

    if (child && typeof child === 'object' && !Array.isArray(child)) {
      return flattenEntries(child, nextPrefix);
    }

    return [{ key: nextPrefix, value: child }];
  });
}

function walkFiles(directory, matcher) {
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const entryPath = path.join(directory, entry.name);

    if (entry.isDirectory()) {
      return walkFiles(entryPath, matcher);
    }

    return matcher(entry.name) ? [entryPath] : [];
  });
}

function readStringToken(text, start) {
  const quote = text[start];
  let value = '';
  let index = start + 1;

  while (index < text.length) {
    const char = text[index];

    if (char === '\\') {
      value += text.slice(index, index + 2);
      index += 2;
      continue;
    }

    if (char === quote) {
      return { end: index + 1, value };
    }

    value += char;
    index += 1;
  }

  return { end: text.length, value };
}

function nextNonWhitespace(text, start) {
  let index = start;

  while (/\s/.test(text[index] ?? '')) {
    index += 1;
  }

  return { char: text[index], index };
}

function findDuplicateJsonKeys(filePath) {
  const text = readText(filePath);
  const duplicates = [];
  const stack = [];
  let index = 0;

  while (index < text.length) {
    const char = text[index];

    if (char === '{') {
      stack.push({ keys: new Map(), type: 'object' });
      index += 1;
      continue;
    }

    if (char === '[') {
      stack.push({ type: 'array' });
      index += 1;
      continue;
    }

    if (char === '}' || char === ']') {
      stack.pop();
      index += 1;
      continue;
    }

    if (char === '"') {
      const token = readStringToken(text, index);
      const next = nextNonWhitespace(text, token.end);
      const current = stack.at(-1);

      if (current?.type === 'object' && next.char === ':') {
        const count = current.keys.get(token.value) ?? 0;
        current.keys.set(token.value, count + 1);

        if (count > 0) {
          duplicates.push(`${path.relative(root, filePath)}: duplicate key "${token.value}"`);
        }
      }

      index = token.end;
      continue;
    }

    index += 1;
  }

  return duplicates;
}

function collectTranslationKeys() {
  const files = walkFiles(sourceRoot, (name) => /\.(tsx?|jsx?)$/.test(name));
  const staticPatterns = [
    /\bt\(\s*["']([^"']+)["']/g,
    /i18n\.t\(\s*["']([^"']+)["']/g,
    new RegExp(`(?:${keyPropNames.join('|')})\\s*=\\s*["']([^"']+)["']`, 'g'),
    new RegExp(`(?:${keyPropNames.join('|')})\\s*:\\s*["']([^"']+)["']`, 'g'),
    new RegExp(`["']((${keyRoots.join('|')})\\.[^"'$]+)["']`, 'g'),
  ];
  const dynamicPattern = /\bt\(\s*`([^`$]+)\$\{[^}]+\}[^`]*`/g;
  const staticKeys = new Map();
  const dynamicFamilies = new Map();

  for (const filePath of files) {
    const text = readText(filePath);
    const relativePath = path.relative(root, filePath);

    for (const pattern of staticPatterns) {
      let match;
      while ((match = pattern.exec(text))) {
        staticKeys.set(match[1], relativePath);
      }
    }

    let match;
    while ((match = dynamicPattern.exec(text))) {
      dynamicFamilies.set(match[1], relativePath);
    }
  }

  return {
    dynamicFamilies: [...dynamicFamilies.entries()].map(([prefix, filePath]) => ({
      filePath,
      prefix,
    })),
    staticKeys: [...staticKeys.entries()].map(([key, filePath]) => ({ filePath, key })),
  };
}

function hasKeyOrPluralFamily(locale, key) {
  const keys = flattenedResources[locale];

  if (keys.has(key)) {
    return true;
  }

  return pluralSuffixes.some((suffix) => keys.has(`${key}_${suffix}`));
}

function dynamicFamilyExists(locale, prefix) {
  return [...flattenedResources[locale]].some((key) => key.startsWith(prefix));
}

function markUsedKey(used, key) {
  used.add(key);

  for (const suffix of pluralSuffixes) {
    used.add(`${key}_${suffix}`);
  }
}

function collectHardcodedUiStrings() {
  const issues = [];
  const files = walkFiles(sourceRoot, (name) => /\.(tsx?|jsx?)$/.test(name));
  const jsxTextPattern = />\s*([^<>{}\n]*[А-Яа-яІіЇїЄєҐґ][^<>{}\n]*)\s*</g;
  const propPattern =
    /\b(?:placeholder|aria-label|title|alt)\s*=\s*["']([^"']*[А-Яа-яІіЇїЄєҐґ][^"']*)["']/g;

  for (const filePath of files) {
    const relativePath = path.relative(root, filePath);

    if (ignoredHardcodedPaths.includes(relativePath)) {
      continue;
    }

    const text = readText(filePath);
    for (const pattern of [jsxTextPattern, propPattern]) {
      let match;
      while ((match = pattern.exec(text))) {
        issues.push(`${relativePath}: "${match[1].trim()}"`);
      }
    }
  }

  return issues;
}

function collectStrictHardcodedUiStrings() {
  const issues = [];
  const files = walkFiles(sourceRoot, (name) => /\.(tsx?|jsx?)$/.test(name));
  const cyrillic = '[А-Яа-яІіЇїЄєҐґ]';
  const patterns = [
    new RegExp(`>\\s*([^<>{}\\n]*${cyrillic}[^<>{}\\n]*)\\s*<`, 'g'),
    new RegExp(`\\b(?:placeholder|aria-label|title|alt)\\s*=\\s*["']([^"']*${cyrillic}[^"']*)["']`, 'g'),
    /["'`]([^"'`\n]*[А-Яа-яІіЇїЄєҐґ][^"'`\n]*)["'`]/g,
  ];

  for (const filePath of files) {
    const relativePath = path.relative(root, filePath);

    if (!strictHardcodedUiPaths.some((prefix) => relativePath.startsWith(prefix))) {
      continue;
    }

    const text = readText(filePath);
    for (const pattern of patterns) {
      let match;
      while ((match = pattern.exec(text))) {
        issues.push(`${relativePath}: "${match[1].trim()}"`);
      }
    }
  }

  return [...new Set(issues)];
}

const resources = {};
const flattenedResources = {};
const duplicateKeys = [];

for (const locale of locales) {
  resources[locale] = {};
  flattenedResources[locale] = new Set();

  for (const namespace of namespaces) {
    const filePath = path.join(localesRoot, locale, `${namespace}.json`);

    if (!fs.existsSync(filePath)) {
      throw new Error(`Missing locale file: ${path.relative(root, filePath)}`);
    }

    duplicateKeys.push(...findDuplicateJsonKeys(filePath));
    resources[locale][namespace] = readJson(filePath);

    for (const key of flattenKeys(resources[locale][namespace])) {
      flattenedResources[locale].add(key);
    }
  }
}

const { dynamicFamilies, staticKeys } = collectTranslationKeys();
dynamicFamilies.push({ filePath: 'src/shared/api/api-client.ts', prefix: 'errors.api.' });
for (const locale of locales) {
  staticKeys.push({
    filePath: 'src/features/change-language/ui/LanguageSwitcher.tsx',
    key: `language.options.${locale}`,
  });
}

const missingKeys = [];
const missingDynamicFamilies = [];

for (const { key, filePath } of staticKeys) {
  for (const locale of locales) {
    if (!hasKeyOrPluralFamily(locale, key)) {
      missingKeys.push(`${locale}: ${key} (${filePath})`);
    }
  }
}

for (const { prefix, filePath } of dynamicFamilies) {
  for (const locale of locales) {
    if (!dynamicFamilyExists(locale, prefix)) {
      missingDynamicFamilies.push(`${locale}: ${prefix}* (${filePath})`);
    }
  }
}

await i18next.init({
  defaultNS: 'common',
  fallbackLng: [defaultLocale, 'en'],
  fallbackNS: fallbackNamespaces,
  interpolation: {
    escapeValue: false,
  },
  load: 'languageOnly',
  ns: namespaces,
  resources,
  supportedLngs: locales,
});

const unresolvedKeys = [];

for (const { key, filePath } of staticKeys) {
  for (const locale of locales) {
    const value = i18next.t(key, { count: 2, date: '2026-08-03', lng: locale });

    if (value === key) {
      unresolvedKeys.push(`${locale}: ${key} (${filePath})`);
    }
  }
}

const usedDefaultKeys = new Set();
for (const { key } of staticKeys) {
  markUsedKey(usedDefaultKeys, key);
}
for (const { prefix } of dynamicFamilies) {
  for (const key of flattenedResources[defaultLocale]) {
    if (key.startsWith(prefix)) {
      usedDefaultKeys.add(key);
    }
  }
}

const orphanKeys = [...flattenedResources[defaultLocale]]
  .filter((key) => !usedDefaultKeys.has(key))
  .sort();
const hardcodedUiStrings = collectHardcodedUiStrings();
const strictHardcodedUiStrings = collectStrictHardcodedUiStrings();
const localizedSurfaceCyrillic = [];

for (const locale of ['en', 'de']) {
  for (const namespace of namespaces) {
    for (const { key, value } of flattenEntries(resources[locale][namespace])) {
      if (
        typeof value === 'string' &&
        /^(pricing|materials)\./.test(key) &&
        /[А-Яа-яІіЇїЄєҐґ]/.test(value)
      ) {
        localizedSurfaceCyrillic.push(`${locale}: ${key} = "${value}"`);
      }
    }
  }
}

if (
  duplicateKeys.length ||
  missingKeys.length ||
  missingDynamicFamilies.length ||
  unresolvedKeys.length ||
  strictHardcodedUiStrings.length ||
  localizedSurfaceCyrillic.length
) {
  if (duplicateKeys.length) {
    console.error(`Duplicate translation keys:\n${duplicateKeys.join('\n')}`);
  }

  if (missingKeys.length) {
    console.error(`Missing translation keys:\n${missingKeys.join('\n')}`);
  }

  if (missingDynamicFamilies.length) {
    console.error(
      `Missing dynamic translation key families:\n${missingDynamicFamilies.join('\n')}`,
    );
  }

  if (unresolvedKeys.length) {
    console.error(`Unresolved i18next lookups:\n${unresolvedKeys.join('\n')}`);
  }

  if (strictHardcodedUiStrings.length) {
    console.error(
      `Hardcoded Ukrainian UI strings in localized Pricing/Materials surfaces:\n${strictHardcodedUiStrings.join('\n')}`,
    );
  }

  if (localizedSurfaceCyrillic.length) {
    console.error(
      `Ukrainian text in English/German Pricing/Materials translations:\n${localizedSurfaceCyrillic.join('\n')}`,
    );
  }

  process.exitCode = 1;
} else {
  console.log(
    `Validated ${staticKeys.length} static keys and ${dynamicFamilies.length} dynamic key families across ${locales.length} locales.`,
  );
}

if (orphanKeys.length) {
  console.warn(
    `Orphan translation keys in ${defaultLocale} (first 50):\n${orphanKeys.slice(0, 50).join('\n')}`,
  );
}

if (hardcodedUiStrings.length) {
  console.warn(
    `Potential hardcoded UI strings (first 50):\n${hardcodedUiStrings.slice(0, 50).join('\n')}`,
  );
}
