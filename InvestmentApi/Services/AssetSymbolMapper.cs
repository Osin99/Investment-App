namespace InvestmentApi.Services
{
    public static class AssetSymbolMapper
    {
        private static readonly Dictionary<string, (string CoinGeckoId, string Name)> KnownAssets =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["BTC"] = ("bitcoin", "Bitcoin"),
                ["ETH"] = ("ethereum", "Ethereum"),
                ["SOL"] = ("solana", "Solana"),
                ["DOGE"] = ("dogecoin", "Dogecoin"),
                ["SHIB"] = ("shiba-inu", "Shiba Inu"),
                ["XRP"] = ("ripple", "XRP"),
                ["ADA"] = ("cardano", "Cardano"),
                ["LINK"] = ("chainlink", "Chainlink"),
                ["USDT"] = ("tether", "Tether")
            };

        public static string NormalizeSymbol(string symbol) =>
            symbol.Trim().ToUpperInvariant();

        public static string ToCoinGeckoId(string symbol)
        {
            var normalized = NormalizeSymbol(symbol);
            return KnownAssets.TryGetValue(normalized, out var asset)
                ? asset.CoinGeckoId
                : normalized.ToLowerInvariant();
        }

        public static string GetDisplayName(string symbol)
        {
            var normalized = NormalizeSymbol(symbol);
            return KnownAssets.TryGetValue(normalized, out var asset)
                ? asset.Name
                : normalized;
        }

        public static IEnumerable<(string Symbol, string CoinGeckoId, string Name)> GetSeedAssets() =>
            KnownAssets.Select(pair => (pair.Key, pair.Value.CoinGeckoId, pair.Value.Name));
    }
}
