namespace InvestmentApi.Services
{
    public interface IPriceService
    {
        Task<Dictionary<string, decimal>> GetCurrentPricesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default);
        Task<Dictionary<string, Dictionary<string, decimal>>> GetHistoricalPricesAsync(
            IEnumerable<string> symbols,
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);
    }
}
