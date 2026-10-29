using ExpenseManager.Domain.Models;
using ExpenseManager.Infrastructure.Repositories;

namespace ExpenseManager.Application.Services
{
    public class ExpenseService
    {
        public const decimal MonthlyBudget = 1000m;
        public const int MaxDaysToDelete = 30;

        private readonly ExpenseRepository _repository;

        public ExpenseService()
        {
            _repository = new ExpenseRepository();
        }

        public List<Expense> GetExpenses()
        {
            return _repository.GetAll()
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        public decimal GetTotalForMonth(int year, int month)
        {
            return _repository.GetAll()
                .Where(e => e.Date.Year == year && e.Date.Month == month)
                .Sum(e => e.Amount);
        }

        public void AddExpense(string description, decimal amount, DateTime date, string category)
        {
            // Eerst het object aanmaken: zo valideert de Domain layer de invoer meteen.
            Expense expense = new Expense(description, amount, date, category);

            Expense? duplicate = _repository.GetAll().Find(e =>
                e.Date == expense.Date &&
                e.Description.Equals(expense.Description, StringComparison.OrdinalIgnoreCase));

            if (duplicate != null)
                throw new InvalidOperationException("Deze uitgave bestaat reeds.");

            decimal total = GetTotalForMonth(expense.Date.Year, expense.Date.Month);
            if (total + expense.Amount > MonthlyBudget)
                throw new InvalidOperationException("Het maandbudget wordt overschreden.");

            _repository.Add(expense);
        }

        public void ReimburseExpense(Expense expense)
        {
            if (!expense.Category.Equals("Work", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Enkel uitgaven in de categorie Work kunnen terugbetaald worden.");

            Expense? existing = _repository.Get(expense.Id);
            if (existing is null)
                throw new InvalidOperationException("Deze uitgave bestaat niet.");

            existing.MarkAsReimbursed();
        }

        public void DeleteExpense(Expense expense)
        {
            if (expense.IsReimbursed)
                throw new InvalidOperationException("Een terugbetaalde uitgave mag niet verwijderd worden.");

            if (expense.Date < DateTime.Today.AddDays(-MaxDaysToDelete))
                throw new InvalidOperationException($"Uitgaven ouder dan {MaxDaysToDelete} dagen mogen niet verwijderd worden.");

            _repository.Remove(expense);
        }
    }
}
