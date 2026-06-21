namespace InvestmentApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // "User" or "Admin"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Asset> Assets { get; set; } = new List<Asset>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
