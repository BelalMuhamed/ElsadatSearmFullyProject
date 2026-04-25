/**
 * Static country data for the PhoneInputComponent.
 *
 * Curated list — GCC + MENA are at the top (the business's primary market),
 * followed by major global countries. Each entry has:
 *   iso2      — lowercase ISO-3166-1 alpha-2 (e.g. 'eg')
 *   name      — English name
 *   nameAr    — Arabic name (shown in the dropdown since the app is Arabic-first)
 *   dialCode  — international dial code WITHOUT the '+' (e.g. '20')
 *   maxLength — max expected length of the national part (digits only),
 *               used for a soft client-side cap. Backend validates authoritatively.
 *
 * Easy to extend: append more entries. No build step needed.
 */

export interface Country {
  iso2: string;
  name: string;
  nameAr: string;
  dialCode: string;
  maxLength: number;
}

export const COUNTRIES: readonly Country[] = [
  // ----- GCC (preferred / top of the list) -----
  { iso2: 'eg', name: 'Egypt',                 nameAr: 'مصر',             dialCode: '20',   maxLength: 10 },
  { iso2: 'sa', name: 'Saudi Arabia',          nameAr: 'السعودية',        dialCode: '966',  maxLength: 9  },
  { iso2: 'ae', name: 'United Arab Emirates',  nameAr: 'الإمارات',        dialCode: '971',  maxLength: 9  },
  { iso2: 'kw', name: 'Kuwait',                nameAr: 'الكويت',          dialCode: '965',  maxLength: 8  },
  { iso2: 'qa', name: 'Qatar',                 nameAr: 'قطر',             dialCode: '974',  maxLength: 8  },
  { iso2: 'bh', name: 'Bahrain',               nameAr: 'البحرين',         dialCode: '973',  maxLength: 8  },
  { iso2: 'om', name: 'Oman',                  nameAr: 'عُمان',           dialCode: '968',  maxLength: 8  },

  // ----- Wider MENA -----
  { iso2: 'jo', name: 'Jordan',                nameAr: 'الأردن',          dialCode: '962',  maxLength: 9  },
  { iso2: 'lb', name: 'Lebanon',               nameAr: 'لبنان',           dialCode: '961',  maxLength: 8  },
  { iso2: 'sy', name: 'Syria',                 nameAr: 'سوريا',           dialCode: '963',  maxLength: 9  },
  { iso2: 'iq', name: 'Iraq',                  nameAr: 'العراق',          dialCode: '964',  maxLength: 10 },
  { iso2: 'ye', name: 'Yemen',                 nameAr: 'اليمن',           dialCode: '967',  maxLength: 9  },
  { iso2: 'ps', name: 'Palestine',             nameAr: 'فلسطين',          dialCode: '970',  maxLength: 9  },
  { iso2: 'sd', name: 'Sudan',                 nameAr: 'السودان',         dialCode: '249',  maxLength: 9  },
  { iso2: 'ly', name: 'Libya',                 nameAr: 'ليبيا',           dialCode: '218',  maxLength: 9  },
  { iso2: 'tn', name: 'Tunisia',               nameAr: 'تونس',            dialCode: '216',  maxLength: 8  },
  { iso2: 'dz', name: 'Algeria',               nameAr: 'الجزائر',         dialCode: '213',  maxLength: 9  },
  { iso2: 'ma', name: 'Morocco',               nameAr: 'المغرب',          dialCode: '212',  maxLength: 9  },
  { iso2: 'mr', name: 'Mauritania',            nameAr: 'موريتانيا',       dialCode: '222',  maxLength: 8  },
  { iso2: 'so', name: 'Somalia',               nameAr: 'الصومال',         dialCode: '252',  maxLength: 9  },
  { iso2: 'dj', name: 'Djibouti',              nameAr: 'جيبوتي',          dialCode: '253',  maxLength: 8  },
  { iso2: 'km', name: 'Comoros',               nameAr: 'جزر القمر',       dialCode: '269',  maxLength: 7  },

  // ----- Neighbors / trade partners -----
  { iso2: 'tr', name: 'Turkey',                nameAr: 'تركيا',           dialCode: '90',   maxLength: 10 },
  { iso2: 'ir', name: 'Iran',                  nameAr: 'إيران',           dialCode: '98',   maxLength: 10 },
  { iso2: 'il', name: 'Israel',                nameAr: 'إسرائيل',         dialCode: '972',  maxLength: 9  },

  // ----- Europe -----
  { iso2: 'gb', name: 'United Kingdom',        nameAr: 'المملكة المتحدة', dialCode: '44',   maxLength: 10 },
  { iso2: 'de', name: 'Germany',               nameAr: 'ألمانيا',         dialCode: '49',   maxLength: 11 },
  { iso2: 'fr', name: 'France',                nameAr: 'فرنسا',           dialCode: '33',   maxLength: 9  },
  { iso2: 'it', name: 'Italy',                 nameAr: 'إيطاليا',         dialCode: '39',   maxLength: 10 },
  { iso2: 'es', name: 'Spain',                 nameAr: 'إسبانيا',         dialCode: '34',   maxLength: 9  },
  { iso2: 'nl', name: 'Netherlands',           nameAr: 'هولندا',          dialCode: '31',   maxLength: 9  },
  { iso2: 'be', name: 'Belgium',               nameAr: 'بلجيكا',          dialCode: '32',   maxLength: 9  },
  { iso2: 'ch', name: 'Switzerland',           nameAr: 'سويسرا',          dialCode: '41',   maxLength: 9  },
  { iso2: 'at', name: 'Austria',               nameAr: 'النمسا',          dialCode: '43',   maxLength: 11 },
  { iso2: 'se', name: 'Sweden',                nameAr: 'السويد',          dialCode: '46',   maxLength: 9  },
  { iso2: 'no', name: 'Norway',                nameAr: 'النرويج',         dialCode: '47',   maxLength: 8  },
  { iso2: 'dk', name: 'Denmark',               nameAr: 'الدنمارك',        dialCode: '45',   maxLength: 8  },
  { iso2: 'fi', name: 'Finland',               nameAr: 'فنلندا',          dialCode: '358',  maxLength: 10 },
  { iso2: 'pl', name: 'Poland',                nameAr: 'بولندا',          dialCode: '48',   maxLength: 9  },
  { iso2: 'gr', name: 'Greece',                nameAr: 'اليونان',         dialCode: '30',   maxLength: 10 },
  { iso2: 'ie', name: 'Ireland',               nameAr: 'أيرلندا',         dialCode: '353',  maxLength: 9  },
  { iso2: 'pt', name: 'Portugal',              nameAr: 'البرتغال',        dialCode: '351',  maxLength: 9  },
  { iso2: 'ru', name: 'Russia',                nameAr: 'روسيا',           dialCode: '7',    maxLength: 10 },
  { iso2: 'ua', name: 'Ukraine',               nameAr: 'أوكرانيا',        dialCode: '380',  maxLength: 9  },
  { iso2: 'ro', name: 'Romania',               nameAr: 'رومانيا',         dialCode: '40',   maxLength: 9  },

  // ----- Americas -----
  { iso2: 'us', name: 'United States',         nameAr: 'الولايات المتحدة',dialCode: '1',    maxLength: 10 },
  { iso2: 'ca', name: 'Canada',                nameAr: 'كندا',            dialCode: '1',    maxLength: 10 },
  { iso2: 'mx', name: 'Mexico',                nameAr: 'المكسيك',         dialCode: '52',   maxLength: 10 },
  { iso2: 'br', name: 'Brazil',                nameAr: 'البرازيل',        dialCode: '55',   maxLength: 11 },
  { iso2: 'ar', name: 'Argentina',             nameAr: 'الأرجنتين',       dialCode: '54',   maxLength: 10 },

  // ----- Asia & Oceania -----
  { iso2: 'cn', name: 'China',                 nameAr: 'الصين',           dialCode: '86',   maxLength: 11 },
  { iso2: 'jp', name: 'Japan',                 nameAr: 'اليابان',         dialCode: '81',   maxLength: 10 },
  { iso2: 'kr', name: 'South Korea',           nameAr: 'كوريا الجنوبية',  dialCode: '82',   maxLength: 10 },
  { iso2: 'in', name: 'India',                 nameAr: 'الهند',           dialCode: '91',   maxLength: 10 },
  { iso2: 'pk', name: 'Pakistan',              nameAr: 'باكستان',         dialCode: '92',   maxLength: 10 },
  { iso2: 'bd', name: 'Bangladesh',            nameAr: 'بنغلاديش',        dialCode: '880',  maxLength: 10 },
  { iso2: 'id', name: 'Indonesia',             nameAr: 'إندونيسيا',       dialCode: '62',   maxLength: 11 },
  { iso2: 'my', name: 'Malaysia',              nameAr: 'ماليزيا',         dialCode: '60',   maxLength: 10 },
  { iso2: 'sg', name: 'Singapore',             nameAr: 'سنغافورة',        dialCode: '65',   maxLength: 8  },
  { iso2: 'th', name: 'Thailand',              nameAr: 'تايلاند',         dialCode: '66',   maxLength: 9  },
  { iso2: 'ph', name: 'Philippines',           nameAr: 'الفلبين',         dialCode: '63',   maxLength: 10 },
  { iso2: 'au', name: 'Australia',             nameAr: 'أستراليا',        dialCode: '61',   maxLength: 9  },
  { iso2: 'nz', name: 'New Zealand',           nameAr: 'نيوزيلندا',       dialCode: '64',   maxLength: 10 },

  // ----- Africa (other) -----
  { iso2: 'ng', name: 'Nigeria',               nameAr: 'نيجيريا',         dialCode: '234',  maxLength: 10 },
  { iso2: 'ke', name: 'Kenya',                 nameAr: 'كينيا',           dialCode: '254',  maxLength: 9  },
  { iso2: 'et', name: 'Ethiopia',              nameAr: 'إثيوبيا',         dialCode: '251',  maxLength: 9  },
  { iso2: 'za', name: 'South Africa',          nameAr: 'جنوب أفريقيا',    dialCode: '27',   maxLength: 9  }
];

/** Default country ISO2 — Egypt, matching the business's primary market. */
export const DEFAULT_COUNTRY_ISO: string = 'eg';

/**
 * Looks up a country by ISO2 (case-insensitive). Returns Egypt as a safe fallback.
 */
export function findCountryByIso(iso2: string | null | undefined): Country {
  if (!iso2) return COUNTRIES.find(c => c.iso2 === DEFAULT_COUNTRY_ISO)!;
  const key = iso2.toLowerCase();
  return COUNTRIES.find(c => c.iso2 === key)
      ?? COUNTRIES.find(c => c.iso2 === DEFAULT_COUNTRY_ISO)!;
}

/**
 * Given a stored E.164 number (e.g. "+201012345678"), returns the country it
 * belongs to by matching the longest dial code. Falls back to Egypt if no match.
 *
 * Greedy longest-match matters because country codes overlap:
 *   "1"  → US/CA
 *   "20" → Egypt
 *   "966" → Saudi Arabia
 *   "971" → UAE  (shares the leading "9" / "97" prefix space with others)
 */
export function findCountryByE164(e164: string | null | undefined): Country {
  if (!e164) return findCountryByIso(DEFAULT_COUNTRY_ISO);

  const digitsOnly = e164.replace(/[^\d]/g, '');
  if (!digitsOnly) return findCountryByIso(DEFAULT_COUNTRY_ISO);

  // Sort candidates by dial-code length descending — longest match wins.
  const sorted = [...COUNTRIES].sort((a, b) => b.dialCode.length - a.dialCode.length);
  const hit = sorted.find(c => digitsOnly.startsWith(c.dialCode));
  return hit ?? findCountryByIso(DEFAULT_COUNTRY_ISO);
}
