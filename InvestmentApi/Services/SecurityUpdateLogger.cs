using System;
using System.IO;

namespace InvestmentApi.Services
{
    public class SecurityUpdateLogger
    {
        private readonly string _logsDirectory;

        public SecurityUpdateLogger()
        {
            _logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            
            // Utwórz folder Logs jeśli nie istnieje
            if (!Directory.Exists(_logsDirectory))
            {
                Directory.CreateDirectory(_logsDirectory);
            }
        }

        public void Log(string message)
        {
            try
            {
                var logFile = Path.Combine(_logsDirectory, 
                    $"SecurityUpdate_{DateTime.Now:yyyy-MM-dd}.txt");
                
                var logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(logFile, logMessage + Environment.NewLine);
                
                // Również loguj do konsoli
                Console.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas logowania: {ex.Message}");
            }
        }

        public void LogSection(string sectionName)
        {
            Log($"\n=== {sectionName} ===");
        }

        public void LogSuccess(string message)
        {
            Log($"✓ {message}");
        }

        public void LogError(string message)
        {
            Log($"❌ BŁĄD: {message}");
        }

        public void LogWarning(string message)
        {
            Log($"⚠️ OSTRZEŻENIE: {message}");
        }

        public void LogSummary(int totalSecurities, int updated, int added, TimeSpan duration, string status)
        {
            LogSection("PODSUMOWANIE");
            Log($"Czas trwania: {duration.Minutes} minut {duration.Seconds} sekund");
            Log($"Razem papierów: {totalSecurities}");
            Log($"Razem zaktualizowano: {updated}");
            Log($"Razem dodano: {added}");
            Log($"Status: {status}");
            Log("---");
        }
    }
}
