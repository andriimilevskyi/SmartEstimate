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

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, 'utf8'));
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

function walkFiles(directory) {
  return fs.readdirSync(directory, { withFileTypes: true }).flatMap((entry) => {
    const entryPath = path.join(directory, entry.name);

    if (entry.isDirectory()) {
      return walkFiles(entryPath);
    }

    return /\.(tsx?|jsx?)$/.test(entry.name) ? [entryPath] : [];
  });
}

function collectStaticTranslationKeys() {
  const patterns = [/\bt\(\s*["']([^"']+)["']/g, /i18n\.t\(\s*["']([^"']+)["']/g];
  const keys = new Map();

  for (const filePath of walkFiles(sourceRoot)) {
    const text = fs.readFileSync(filePath, 'utf8');

    for (const pattern of patterns) {
      let match;
      while ((match = pattern.exec(text))) {
        const key = match[1];

        if (!key.includes('${')) {
          keys.set(key, path.relative(root, filePath));
        }
      }
    }
  }

  return [...keys.entries()].map(([key, filePath]) => ({ key, filePath }));
}

const resources = {};
const flattenedResources = {};

for (const locale of locales) {
  resources[locale] = {};
  flattenedResources[locale] = new Set();

  for (const namespace of namespaces) {
    const filePath = path.join(localesRoot, locale, `${namespace}.json`);

    if (!fs.existsSync(filePath)) {
      throw new Error(`Missing locale file: ${path.relative(root, filePath)}`);
    }

    resources[locale][namespace] = readJson(filePath);

    for (const key of flattenKeys(resources[locale][namespace])) {
      flattenedResources[locale].add(key);
    }
  }
}

const usedKeys = collectStaticTranslationKeys();
for (const locale of locales) {
  usedKeys.push({
    filePath: 'src/features/change-language/ui/LanguageSwitcher.tsx',
    key: `language.options.${locale}`,
  });
}
const missingKeys = [];

for (const { key, filePath } of usedKeys) {
  for (const locale of locales) {
    if (!flattenedResources[locale].has(key)) {
      missingKeys.push(`${locale}: ${key} (${filePath})`);
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

for (const { key, filePath } of usedKeys) {
  for (const locale of locales) {
    const value = i18next.t(key, { count: 2, date: '2026-08-03', lng: locale });

    if (value === key) {
      unresolvedKeys.push(`${locale}: ${key} (${filePath})`);
    }
  }
}

if (missingKeys.length || unresolvedKeys.length) {
  if (missingKeys.length) {
    console.error(`Missing translation keys:\n${missingKeys.join('\n')}`);
  }

  if (unresolvedKeys.length) {
    console.error(`Unresolved i18next lookups:\n${unresolvedKeys.join('\n')}`);
  }

  process.exitCode = 1;
} else {
  console.log(`Validated ${usedKeys.length} translation keys across ${locales.length} locales.`);
}
