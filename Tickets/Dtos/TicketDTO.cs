namespace Tickets.Dtos
{
    public class TicketDTO
    {
        public int? CategoryId { get; set; }

        public int? StatusId { get; set; }

        public string Name { get; set; }

        public string Department { get; set; }

        public string Affair { get; set; }

        public string ProblemDescription { get; set; }
    }
}