namespace ExpenseManager.Domain.Models
{
    public class Expense
    {
        public Expense(string description, decimal amount, DateTime date, string category)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Een uitgave moet een beschrijving hebben.");
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Een uitgave moet een categorie hebben.");
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Het bedrag moet groter zijn dan 0.");
            }

            if (date.Date > DateTime.Today)
            {
                throw new ArgumentException("De datum mag niet in de toekomst liggen.");
            }

            Description = description.Trim();
            Amount = amount;
            Date = date.Date;
            Category = category.Trim();

            IsReimbursed = false;
            ReimbursedAt = null;
        }

        public int Id { get; set; }
        public string Description { get; }
        public decimal Amount { get; }
        public DateTime Date { get; }
        public string Category { get; }
        public bool IsReimbursed { get; private set; }
        public DateTime? ReimbursedAt { get; private set; }

        public void MarkAsReimbursed()
        {
            if (IsReimbursed) return; // al terugbetaald: niets doen

            IsReimbursed = true;
            ReimbursedAt = DateTime.Now;
        }

        public override string ToString()
        {
            string status = IsReimbursed ? "[Reimbursed]" : "[Open]";
            return $"{status} {Date:dd/MM/yyyy} - {Description} ({Category}) € {Amount:0.00}";
        }
    }
}
