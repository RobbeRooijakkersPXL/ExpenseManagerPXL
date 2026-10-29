using System.Windows;
using System.Windows.Controls;
using ExpenseManager.Application.Services;
using ExpenseManager.Domain.Models;

namespace ExpenseManager.Presentation
{
    public partial class ExpenseListWindow : Window
    {
        private readonly ExpenseService _expenseService;

        public ExpenseListWindow()
        {
            InitializeComponent();

            _expenseService = new ExpenseService();
            RefreshExpenses();
        }

        private void RefreshExpenses()
        {
            expensesListBox.Items.Clear();

            List<Expense> expenses = _expenseService.GetExpenses();

            foreach (Expense expense in expenses)
            {
                expensesListBox.Items.Add(expense); // relies on Expense.ToString()
            }

            decimal total = _expenseService.GetTotalForMonth(DateTime.Today.Year, DateTime.Today.Month);
            monthTotalTextBlock.Text = $"This month: € {total:0.00} / € {ExpenseService.MonthlyBudget:0.00}";

            ClearDetails();
        }

        private void ClearDetails()
        {
            descriptionTextBlock.Text = "";
            amountTextBlock.Text = "";
            dateTextBlock.Text = "";
            categoryTextBlock.Text = "";
            reimbursedTextBlock.Text = "";
            reimbursedAtTextBlock.Text = "";
        }

        private void ExpensesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Expense? selectedExpense = expensesListBox.SelectedItem as Expense;
            if (selectedExpense is null)
            {
                ClearDetails();
                return;
            }

            descriptionTextBlock.Text = selectedExpense.Description;
            amountTextBlock.Text = $"€ {selectedExpense.Amount:0.00}";
            dateTextBlock.Text = selectedExpense.Date.ToLongDateString();
            categoryTextBlock.Text = selectedExpense.Category;
            reimbursedTextBlock.Text = selectedExpense.IsReimbursed ? "yes" : "no";
            reimbursedAtTextBlock.Text = selectedExpense.ReimbursedAt?.ToString() ?? "-";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddExpenseWindow addWindow = new AddExpenseWindow();
            addWindow.Owner = this;

            bool? result = addWindow.ShowDialog();
            if (result != true)
            {
                return;
            }

            try
            {
                _expenseService.AddExpense(addWindow.ExpenseDescription, addWindow.ExpenseAmount,
                                           addWindow.ExpenseDate, addWindow.ExpenseCategory);
                RefreshExpenses();
            }
            catch (ArgumentException ex) // domeinvalidatie (vangt ook ArgumentOutOfRangeException)
            {
                MessageBox.Show(ex.Message);
            }
            catch (InvalidOperationException ex) // business rules
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ReimburseButton_Click(object sender, RoutedEventArgs e)
        {
            Expense? selectedExpense = expensesListBox.SelectedItem as Expense;
            if (selectedExpense is null)
            {
                MessageBox.Show("Geen uitgave geselecteerd.");
                return;
            }

            try
            {
                _expenseService.ReimburseExpense(selectedExpense);
                RefreshExpenses();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Expense? selectedExpense = expensesListBox.SelectedItem as Expense;
            if (selectedExpense is null)
            {
                MessageBox.Show("Geen uitgave geselecteerd.");
                return;
            }

            MessageBoxResult answer = MessageBox.Show($"'{selectedExpense.Description}' verwijderen?",
                                                      "Bevestigen", MessageBoxButton.YesNo);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _expenseService.DeleteExpense(selectedExpense);
                RefreshExpenses();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshExpenses();
        }
    }
}
