using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tickets.Dtos;
using Tickets.Models;

namespace Tickets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketFormController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketFormController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.TkCategories
                                    .AsNoTracking()
                                    .ToListAsync();

            return Ok(categories);
        }

        [HttpGet]
        [Route("GetTickets")]
        public async Task<IActionResult> GetTickets()
        {
            var tickets = await _context.Tickets
                                .Select(t => new
                                {
                                    t.Id,
                                    t.Name,
                                    t.Affair,
                                    Date = t.RegistrationDate!.Value.ToString("dd/MM/yyyy"),
                                    Category = t.Category.Name,
                                    Status = t.Status.Name,
                                    t.StatusId,
                                    t.CategoryId,
                                    t.Department,
                                    t.ProblemDescription
                                })
                                .AsNoTracking()
                                .ToListAsync();
            return Ok(tickets);
        }

        [HttpPost]
        [Route("RegisterTicket")]
        public async Task<IActionResult> RegisterTicket([FromBody] TicketDTO ticket)
        {
            if (ticket == null)
            {
                return BadRequest("Ticket data is null.");
            }

            var newTicket = new Ticket
            {
                Name = ticket.Name,
                Department = ticket.Department,
                Affair = ticket.Affair,
                ProblemDescription = ticket.ProblemDescription,
                CategoryId = ticket.CategoryId,
                StatusId = 1
            };

            _context.Tickets.Add(newTicket);

            await _context.SaveChangesAsync();
            return Ok(new
            {
                success = true,
                message = "Ticket registered successfully",
                ticketId = newTicket.Id
            });
        }

        [HttpPut]
        [Route("Update/{id:int}")]
        public async Task<IActionResult> Update([FromBody] TicketDTO ticketDTO, int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return BadRequest("Ticket not found");

            ticket.Name = ticketDTO.Name;
            ticket.Department = ticketDTO.Department;
            ticket.Affair = ticketDTO.Affair;
            ticket.ProblemDescription = ticketDTO.ProblemDescription;
            ticket.CategoryId = ticketDTO.CategoryId;
            ticket.StatusId = ticketDTO.StatusId;

            await _context.SaveChangesAsync();

            return Ok(new 
            {
                success = true,
                message = "Ticket update successfully" 
            });
        }

        [HttpDelete]
        [Route("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);

            if(ticket == null) return NotFound("Ticket not found");

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return Ok(new 
            {
                success = true,
                message = "Ticket deleted successfully"           
            });
        }
    }
}