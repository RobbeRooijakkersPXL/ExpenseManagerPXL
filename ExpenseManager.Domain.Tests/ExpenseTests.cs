using ExpenseManager.Domain.Models;

namespace ExpenseManager.Domain.Tests
{
    public class ExpenseTests
    {
        [Fact]
        public void Constructor_EmptyDescription_ThrowsArgumentException()
        {
            // Arrange
            string description = "";
            decimal amount = 10m;
            DateTime date = DateTime.Today;

            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
            {
                Expense expense = new Expense(description, amount, date, "Food");
            });
        }

        [Fact]
        public void Constructor_WhitespaceDescription_ThrowsArgumentException()
        {
            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
            {
                Expense expense = new Expense("   ", 10m, DateTime.Today, "Food");
            });
        }

        [Fact]
        public void Constructor_EmptyCategory_ThrowsArgumentException()
        {
            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
            {
                Expense expense = new Expense("Lunch", 10m, DateTime.Today, "");
            });
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Constructor_AmountZeroOrNegative_ThrowsArgumentOutOfRangeException(decimal amount)
        {
            // Act + Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Expense expense = new Expense("Lunch", amount, DateTime.Today, "Food");
            });
        }

        [Fact]
        public void Constructor_FutureDate_ThrowsArgumentException()
        {
            // Act + Assert
            Assert.Throws<ArgumentException>(() =>
            {
                Expense expense = new Expense("Concert", 45m, DateTime.Today.AddDays(1), "Leisure");
            });
        }

        [Fact]
        public void MarkAsReimbursed_SetsIsReimbursedAndReimbursedAt()
        {
            // Arrange
            Expense expense = new Expense("Laptop bag", 60m, DateTime.Today, "Work");

            // Act
            expense.MarkAsReimbursed();

            // Assert
            Assert.True(expense.IsReimbursed);
            Assert.NotNull(expense.ReimbursedAt);
        }

        [Fact]
        public void MarkAsReimbursed_Twice_KeepsFirstReimbursedAt()
        {
            // Arrange
            Expense expense = new Expense("Laptop bag", 60m, DateTime.Today, "Work");
            expense.MarkAsReimbursed();
            DateTime? first = expense.ReimbursedAt;

            // Act
            expense.MarkAsReimbursed();

            // Assert
            Assert.Equal(first, expense.ReimbursedAt);
        }
    }
}
