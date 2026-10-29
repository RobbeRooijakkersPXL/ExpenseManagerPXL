using ExpenseManager.Application.Services;
using ExpenseManager.Domain.Models;

namespace ExpenseManager.Application.Tests
{
    public class ExpenseServiceTests
    {
        [Fact]
        public void AddExpense_Duplicate_ThrowsInvalidOperationException()
        {
            // Arrange
            ExpenseService service = new ExpenseService();

            service.AddExpense("Groceries", 35.20m, DateTime.Today, "Food");

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.AddExpense("  GROCERIES  ", 12m, DateTime.Today, "Food");
            });
        }

        [Fact]
        public void AddExpense_ExceedsMonthlyBudget_ThrowsInvalidOperationException()
        {
            // Arrange
            ExpenseService service = new ExpenseService();

            service.AddExpense("Rent", 900m, DateTime.Today, "Housing");

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.AddExpense("New shoes", 150m, DateTime.Today, "Leisure");
            });
        }

        [Fact]
        public void AddExpense_ExactlyMonthlyBudget_IsAllowed()
        {
            // Arrange
            ExpenseService service = new ExpenseService();

            // Act
            service.AddExpense("Rent", 900m, DateTime.Today, "Housing");
            service.AddExpense("Groceries", 100m, DateTime.Today, "Food");

            // Assert
            Assert.Equal(ExpenseService.MonthlyBudget,
                service.GetTotalForMonth(DateTime.Today.Year, DateTime.Today.Month));
        }

        [Fact]
        public void AddExpense_EmptyDescription_ThrowsArgumentException()
        {
            // Arrange
            ExpenseService service = new ExpenseService();

            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
            {
                service.AddExpense("", 10m, DateTime.Today, "Food");
            });
        }

        [Fact]
        public void ReimburseExpense_NonWorkExpense_ThrowsInvalidOperationException()
        {
            // Arrange
            ExpenseService service = new ExpenseService();

            service.AddExpense("Cinema", 12m, DateTime.Today, "Leisure");

            var expenses = service.GetExpenses();
            var expense = expenses[0];

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.ReimburseExpense(expense);
            });
        }

        [Fact]
        public void ReimburseExpense_WorkExpense_SetsReimbursedAt()
        {
            // Arrange
            ExpenseService service = new ExpenseService();

            service.AddExpense("Train to client", 24m, DateTime.Today, "Work");

            var expenses = service.GetExpenses();
            var expense = expenses[0];

            // Act
            service.ReimburseExpense(expense);

            // Assert
            Assert.True(expense.IsReimbursed);
            Assert.NotNull(expense.ReimbursedAt);
        }

        [Fact]
        public void DeleteExpense_ReimbursedExpense_ThrowsInvalidOperationException()
        {
            // Arrange
            ExpenseService service = new ExpenseService();

            service.AddExpense("Train to client", 24m, DateTime.Today, "Work");

            var expenses = service.GetExpenses();
            var expense = expenses[0];

            service.ReimburseExpense(expense);

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.DeleteExpense(expense);
            });
        }

        [Fact]
        public void DeleteExpense_OlderThan30Days_ThrowsInvalidOperationException()
        {
            // Arrange
            ExpenseService service = new ExpenseService();

            service.AddExpense("Old receipt", 10m, DateTime.Today.AddDays(-31), "Food");

            var expenses = service.GetExpenses();
            var expense = expenses[0];

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                service.DeleteExpense(expense);
            });
        }

        [Fact]
        public void DeleteExpense_RecentOpenExpense_IsRemoved()
        {
            // Arrange
            ExpenseService service = new ExpenseService();

            service.AddExpense("Coffee", 3.50m, DateTime.Today, "Food");
            var expense = service.GetExpenses()[0];

            // Act
            service.DeleteExpense(expense);

            // Assert
            Assert.Empty(service.GetExpenses());
        }
    }
}
