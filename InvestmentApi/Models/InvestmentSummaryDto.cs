namespace InvestmentApi.Models
{
    public class InvestmentSummaryDto
    {
        public string ?Symbol{ get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalInvested { get; set; }
    }
}
