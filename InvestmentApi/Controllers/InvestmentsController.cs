using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvestmentApi.Data;
using InvestmentApi.Models;

namespace InvestmentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvestmentsController : ControllerBase
    {

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInvestment(int id)
        {
            var investment = await _context.Investments.FindAsync(id);
            if (investment == null)
            {
                return NotFound();
            }
            _context.Investments.Remove(investment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost] // ← To znaczy: ta metoda obsługuje żądania POST
        public async Task<ActionResult<Investment>> PostInvestment(Investment investment)
        {
            _context.Investments.Add(investment);        // ← dodaj nowy obiekt Investment do bazy
            await _context.SaveChangesAsync();           // ← zapisz zmiany (czyli wykonaj INSERT w bazie)

            return CreatedAtAction(nameof(GetInvestments), new { id = investment.Id }, investment);
            // ← zwróć odpowiedź 201 Created + dodany obiekt + lokalizację zasobu
        }

        private readonly InvestmentContext _context;

        public InvestmentsController(InvestmentContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Investment>>> GetInvestments()
        {
            return await _context.Investments.ToListAsync();
        }



        [HttpGet("{id}")]
        public async Task<ActionResult<Investment>> GetInvestment(int id)
        {
            var investment = await _context.Investments.FindAsync(id);

            if (investment == null)
            {
                return NotFound();
            }

            return investment;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvestment(int id, Investment updateInvestment)
        {
            if (id != updateInvestment.Id)
            {
                return BadRequest("Nieprawidłowe ID"); // Sprawdzenie, czy ID z URL zgadza się z tym z ciała żądania
            }
            var investment = await _context.Investments.FindAsync(id);
            if (investment == null)
            {
                return NotFound();// Brak takiego ID
            }
            investment.Symbol = updateInvestment.Symbol;
            investment.Amount = updateInvestment.Amount;
            investment.BuyPrice = updateInvestment.BuyPrice;
            investment.BuyDate = updateInvestment.BuyDate;

            await _context.SaveChangesAsync();// Zapisujemy do bazy

            return NoContent();// Sukces, ale nic nie zwracamy
        }
  [HttpGet("summary")]
public async Task<ActionResult<IEnumerable<InvestmentSummaryDto>>> GetInvestmentSummary()
{
    var summary = await _context.Investments
        .GroupBy(i => i.Symbol)
        .Select(g => new InvestmentSummaryDto
        {
            Symbol = g.Key,
            TotalAmount = g.Sum(i => i.Amount),
            TotalInvested = g.Sum(i => i.Amount * i.BuyPrice)
        }).ToListAsync();

    return Ok(summary);
}
    }
    
}
   