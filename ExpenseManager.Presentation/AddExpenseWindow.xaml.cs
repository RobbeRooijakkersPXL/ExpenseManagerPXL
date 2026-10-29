using System.Windows;

namespace ExpenseManager.Presentation
{
    public partial class AddExpenseWindow : Window
    {
        public string ExpenseDescription { get; private set; } = "";
        public decimal ExpenseAmount { get; private set; }
        public DateTime ExpenseDate { get; private set; }
        public string ExpenseCategory { get; private set; } = "";

        public AddExpenseWindow()
        {
            InitializeComponent();

            datePicker.SelectedDate = DateTime.Today;
            categoryComboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(amountTextBox.Text, out decimal amount))
            {
                MessageBox.Show("Geef een geldig bedrag in, bv. 12,50.");
                return;
            }

            this.ExpenseDescription = descriptionTextBox.Text;
            this.ExpenseAmount = amount;
            this.ExpenseDate = datePicker.SelectedDate!.Value;
            this.ExpenseCategory = categoryComboBox.Text; // tekst van het geselecteerde item

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
