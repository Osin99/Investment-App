export enum AssetCategory {
  Crypto = 0,
  ETF = 1,
  Stocks = 2,
  Other = 3
}

export const CATEGORY_COLORS: Record<AssetCategory, string> = {
  [AssetCategory.Crypto]: '#a78bfa',  // fiolet
  [AssetCategory.ETF]: '#4ade80',     // zielony
  [AssetCategory.Stocks]: '#60a5fa',  // niebieski
  [AssetCategory.Other]: '#fbbf24'    // amber
};

export const CATEGORY_NAMES: Record<AssetCategory, string> = {
  [AssetCategory.Crypto]: 'Krypto',
  [AssetCategory.ETF]: 'ETF',
  [AssetCategory.Stocks]: 'Akcje',
  [AssetCategory.Other]: 'Inne'
};
