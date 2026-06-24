using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using InvestmentApi.Data;

namespace InvestmentApi.Services
{
    public class SecurityUpdateBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SecurityUpdateLogger _logger;
        private Timer? _securityUpdateTimer;
        private Timer? _cryptoPriceTimer;
        private Timer? _stockPriceTimer;

        public SecurityUpdateBackgroundService(IServiceProvider serviceProvider, SecurityUpdateLogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Log("SecurityUpdateBackgroundService uruchomiony");

            // Uruchom aktualizację papierów zaraz po starcie
            await UpdateSecuritiesAsync();

            // Timer 1: Aktualizacja papierów wartościowych - codziennie o 2:00 AM
            var now = DateTime.Now;
            var scheduledTime = now.Date.AddHours(2);
            if (now > scheduledTime)
                scheduledTime = scheduledTime.AddDays(1);

            var timeUntilScheduled = scheduledTime - now;
            _logger.Log($"Następna aktualizacja papierów zaplanowana na: {scheduledTime:yyyy-MM-dd HH:mm:ss}");

            _securityUpdateTimer = new Timer(
                async _ => await UpdateSecuritiesAsync(),
                null,
                timeUntilScheduled,
                TimeSpan.FromHours(24)
            );

            // Timer 2: Pobieranie cen kryptowalut - co 5 minut
            _logger.Log("Uruchamianie timera pobierania cen kryptowalut (co 5 minut)");
            _cryptoPriceTimer = new Timer(
                async _ => await UpdateCryptoPricesAsync(),
                null,
                TimeSpan.FromSeconds(30), // Poczekaj 30s po starcie
                TimeSpan.FromMinutes(5)
            );

            // Timer 3: Pobieranie cen akcji/ETF - codziennie o 2:00 AM (razem z krypto)
            _logger.Log("Uruchamianie timera pobierania cen akcji/ETF (codziennie o 2:00 AM)");
            _stockPriceTimer = new Timer(
                async _ => await UpdateStockPricesAsync(),
                null,
                TimeSpan.FromSeconds(60), // Poczekaj 60s po starcie
                TimeSpan.FromHours(24)
            );

            await Task.CompletedTask;
        }

        private async Task UpdateSecuritiesAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var updateService = scope.ServiceProvider.GetRequiredService<SecurityUpdateService>();
                await updateService.UpdateSecuritiesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Błąd w SecurityUpdateBackgroundService: {ex.Message}");
            }
        }

        private async Task UpdateCryptoPricesAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<InvestmentContext>();
                var priceService = scope.ServiceProvider.GetRequiredService<IPriceService>();

                // Pobierz symbole kryptowalut z portfela (przez transakcje)
                var cryptoSymbols = await context.Assets
                    .Where(a => context.Securities.Any(s => s.Symbol == a.Symbol && s.Type == "Crypto"))
                    .Select(a => a.Symbol)
                    .Distinct()
                    .ToListAsync();

                if (cryptoSymbols.Count > 0)
                {
                    _logger.Log($"[CRYPTO] Pobieranie cen dla {cryptoSymbols.Count} kryptowalut...");
                    var prices = await priceService.GetCurrentPricesAsync(cryptoSymbols);
                    _logger.Log($"[CRYPTO] Pobrano ceny dla {prices.Count(p => p.Value > 0)} kryptowalut");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[CRYPTO] Błąd pobierania cen: {ex.Message}");
            }
        }

        private async Task UpdateStockPricesAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<InvestmentContext>();
                var priceService = scope.ServiceProvider.GetRequiredService<IPriceService>();

                // Pobierz tylko symbole akcji/ETF które są w portfelu (przez transakcje)
                var stockSymbols = await context.Transactions
                    .AsNoTracking()
                    .Select(t => t.Asset.Symbol)
                    .Distinct()
                    .ToListAsync();

                // Filtruj tylko akcje/ETF (nie krypto)
                var filteredSymbols = new List<string>();
                foreach (var symbol in stockSymbols)
                {
                    var security = await context.Securities
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Symbol == symbol);

                    if (security?.Type == "Stock" || security?.Type == "ETF")
                        filteredSymbols.Add(symbol);
                }

                if (filteredSymbols.Count > 0)
                {
                    _logger.Log($"[STOCK] Pobieranie cen dla {filteredSymbols.Count} akcji/ETF z portfela...");
                    var prices = await priceService.GetCurrentPricesAsync(filteredSymbols);
                    _logger.Log($"[STOCK] Pobrano ceny dla {prices.Count(p => p.Value > 0)} akcji/ETF");
                }
                else
                {
                    _logger.Log("[STOCK] Brak akcji/ETF w portfelu - pomijam aktualizację cen");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"[STOCK] Błąd pobierania cen: {ex.Message}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Log("SecurityUpdateBackgroundService zatrzymywany");
            _securityUpdateTimer?.Dispose();
            _cryptoPriceTimer?.Dispose();
            _stockPriceTimer?.Dispose();
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _securityUpdateTimer?.Dispose();
            _cryptoPriceTimer?.Dispose();
            _stockPriceTimer?.Dispose();
            base.Dispose();
        }
    }
}
