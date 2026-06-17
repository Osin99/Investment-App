using InvestmentApi.Models;

namespace InvestmentApi.Services
{
    public interface IPortfolioHistoryService
    {
        Task<PortfolioHistoryDto> BuildHistoryAsync(CancellationToken cancellationToken = default);
    }
}
