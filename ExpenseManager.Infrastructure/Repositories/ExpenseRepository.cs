using ExpenseManager.Domain.Models;

namespace ExpenseManager.Infrastructure.Repositories
{
    public class ExpenseRepository
    {
        private readonly List<Expense> _expenses;

        public ExpenseRepository()
        {
            _expenses = new List<Expense>();
        }

        public List<Expense> GetAll()
        {
            return new List<Expense>(_expenses);
        }

        public Expense? Get(int id)
        {
            return _expenses.Find(expense => expense.Id == id);
        }

        public void Add(Expense expense)
        {
            expense.Id = _expenses.Count + 1;
            _expenses.Add(expense);
        }

        public bool Remove(Expense expense)
        {
            return _expenses.Remove(expense);
        }
    }
}
