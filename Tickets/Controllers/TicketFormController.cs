using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tickets.Dtos;
using Tickets.Models;
using Tickets.Services;

namespace Tickets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketFormController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public TicketFormController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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
                                .OrderByDescending(t => t.Id)
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

        [HttpGet]
        [Route("SearchTicketByName")]
        public async Task<IActionResult> SearchTicketByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("El parámetro 'name' es obligatorio.");

            var ticket = await _context.Tickets
                .Where(t => EF.Functions.Like(t.Name, $"%{name}%"))
                .OrderByDescending(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Department,
                    t.Affair,
                    t.ProblemDescription,
                    t.CategoryId,
                    t.StatusId,
                    RegistrationDate = t.RegistrationDate!.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (ticket == null)
                return NotFound(new { success = false, message = "No se encontró ningún ticket con ese nombre." });

            return Ok(new { success = true, data = ticket });
        }

        [HttpPost]
        [Route("DownloadReport")]
        public async Task<IActionResult> DownloadReport()
        {
            var tickts = await _context.Tickets.Select(t => new
            {
                t.Id,
                t.Name,
                Category = t.Category.Name,
                Status = t.Status.Name,
                t.RegistrationDate,
                t.ResolutionDate,
            })
            .AsNoTracking()
            .ToListAsync();

            using (var workBook = new XLWorkbook())
            {
                var workSheet = workBook.Worksheets.Add("Tickets");

                workSheet.Cell(1, 1).Value = "ID";
                workSheet.Cell(1, 2).Value = "Nombre del solicitante";
                workSheet.Cell(1, 3).Value = "Tipo de ticket";
                workSheet.Cell(1, 4).Value = "Estatus del ticket";
                workSheet.Cell(1, 5).Value = "Fecha de la solicitud";
                workSheet.Cell(1, 6).Value = "Fecha de resolución";

                var headerRange = workSheet.Range("A1:F1");
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0071ab");
                headerRange.Style.Font.FontColor = XLColor.White;
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                for (int i = 0; i < tickts.Count; i++)
                {
                    var rowNumber = i + 2;
                    var currentRow = workSheet.Row(rowNumber);

                    workSheet.Cell(rowNumber, 1).Value = tickts[i].Id;
                    workSheet.Cell(rowNumber, 2).Value = tickts[i].Name;
                    workSheet.Cell(rowNumber, 3).Value = tickts[i].Category;
                    workSheet.Cell(rowNumber, 4).Value = tickts[i].Status;

                    workSheet.Cell(rowNumber, 5).Value = tickts[i].RegistrationDate.HasValue
                        ? tickts[i].RegistrationDate.Value.ToString("dd/MM/yyyy")
                        : "N/A";

                    workSheet.Cell(rowNumber, 6).Value = tickts[i].ResolutionDate.HasValue
                        ? tickts[i].ResolutionDate.Value.ToString("dd/MM/yyyy")
                        : " ";

                    if (rowNumber % 2 == 0)
                    {
                        currentRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#F4F4F4");
                    }
                }
 
                var statusColumn = workSheet.Range($"D2:D{tickts.Count + 1}");

                statusColumn.AddConditionalFormat().WhenContains("Resuelto")
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#C6EFCE"))
                    .Font.SetFontColor(XLColor.FromHtml("#006100"));

                statusColumn.AddConditionalFormat().WhenContains("En progreso")
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#BDD7EE"))
                    .Font.SetFontColor(XLColor.FromHtml("#1A4E8A"));

                statusColumn.AddConditionalFormat().WhenContains("Pendiente")
                    .Fill.SetBackgroundColor(XLColor.FromHtml("#FFEB9C"))
                    .Font.SetFontColor(XLColor.FromHtml("#9C6500")); 

                workSheet.Columns().AdjustToContents();

                var stream = new MemoryStream();
                workBook.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"ReporteDeTickets_{DateTime.Now:ddMMyyyy}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
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
                StatusId = 1,
                ResolutionDate = null
            };

            _context.Tickets.Add(newTicket);

            await _context.SaveChangesAsync();

            try
            {
                await SendEmailTickets(newTicket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enviando correo: {ex.Message}");
            }

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

            if (ticketDTO.StatusId == 3 && ticket.StatusId != 3)
            {
                ticket.ResolutionDate = DateTime.UtcNow;
            }
            else if (ticketDTO.StatusId != 3 && ticket.StatusId == 3)
            {
                ticket.ResolutionDate = null;
            }

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

        private async Task SendEmailTickets(Ticket ticket)
        {
            var subject = "Nuevo ticket registrado";

            var body = $@"
                <h3>¡Hola!</h3>
                <p>Se ha registrado un nuevo ticket que requiere tu atención.</p>
                <ul>
                    <li><strong>Nombre:</strong> {ticket.Name}</li>
                    <li><strong>Departamento:</strong> {ticket.Department ?? "N/A"}</li>
                    <li><strong>Asunto:</strong> {ticket.Affair}</li>
                    <li><strong>Categoría:</strong> {ticket.Category?.Name ?? "Sin categoría"}</li>
                </ul>
                <p><em>Por favor, revísalo a la brevedad.</em></p>
                <br/>
                <p>Saludos,<br/>Equipo de Soporte</p>
            ";

            var recipients = new List<string>
            {
                "juan.poblano@mesa.ms",
                "ulises.gonzalez@mesa.ms"
            };

            await _emailService.SendEmailAsync(recipients, subject, body);
        }
    }
}