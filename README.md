# ExpenseManager

**C# Advanced** · Graduaat programmeren

In deze oefening bouw je een WPF-applicatie waarmee je persoonlijke uitgaven bijhoudt. Je werkt met een vereenvoudigde Clean Architecture: Domain, Infrastructure, Application en een WPF-UI. Daarna test je de businessregels met xUnit.

## Inhoud

1. [Structuur van de solution](#structuur-van-de-solution)
2. [Functionele vereisten](#functionele-vereisten)
3. [Solution en projecten](#solution-en-projecten)
4. [Domain layer (Expense)](#domain-layer-expense)
5. [Infrastructure layer (ExpenseRepository)](#infrastructure-layer-expenserepository)
6. [Application layer (ExpenseService)](#application-layer-expenseservice)
7. [Presentation layer (WPF met 2 windows)](#presentation-layer-wpf-met-2-windows)
8. [Unit tests](#unit-tests)
9. [In het kort](#in-het-kort)

## Structuur van de solution

```
ExpenseManager/
├── ExpenseManager.slnx
├── ExpenseManager.Domain/              Model + domeinvalidatie
│   └── Models/Expense.cs
├── ExpenseManager.Infrastructure/      In-memory opslag
│   └── Repositories/ExpenseRepository.cs
├── ExpenseManager.Application/         Use cases + business rules
│   └── Services/ExpenseService.cs
├── ExpenseManager.Presentation/        WPF-UI
│   ├── App.xaml
│   ├── ExpenseListWindow.xaml(.cs)
│   └── AddExpenseWindow.xaml(.cs)
├── ExpenseManager.Domain.Tests/        xUnit-tests voor Expense
└── ExpenseManager.Application.Tests/   xUnit-tests voor ExpenseService
```

### Starten

- Open `ExpenseManager.slnx` in Visual Studio 2026 (of 2022 17.14+) met .NET 10.
- Stel `ExpenseManager.Presentation` in als startup project en druk op F5.
- Tests uitvoeren: **Test → Run All Tests**, of `dotnet test` in de map van de solution.

> De WPF-app draait enkel op Windows. De class libraries en de tests draaien op elk platform.

---

## Functionele vereisten

### Expense-eigenschappen

Een uitgave bevat:

- Description (verplicht)
- Amount (verplicht, groter dan 0)
- Date (verplicht, niet in de toekomst)
- Category (verplicht: Food, Transport, Housing, Leisure of Work)
- IsReimbursed
- ReimbursedAt (wordt automatisch ingevuld)

### Business rules

Deze regels moeten correct werken:

1. Een uitgave moet altijd een beschrijving hebben (geen lege of whitespace string).
2. Het bedrag moet groter zijn dan 0.
3. De datum mag niet in de toekomst liggen.
4. Een uitgave met dezelfde beschrijving op dezelfde datum mag niet dubbel worden toegevoegd (case-insensitive, en ook met trim).
5. Het totaal van alle uitgaven in één maand mag niet hoger worden dan het maandbudget van € 1000.
6. Enkel uitgaven in de categorie Work kunnen terugbetaald worden.
7. Een terugbetaalde uitgave mag niet verwijderd worden.
8. Een uitgave ouder dan 30 dagen mag niet verwijderd worden.
9. ReimbursedAt wordt automatisch ingevuld op het moment dat een uitgave terugbetaald wordt.

## Solution en projecten

### Solution en projectnamen

Maak een nieuwe solution aan met de naam `ExpenseManager`.

Voeg daarna deze projecten toe:

- `ExpenseManager.Presentation` (WPF App)
- `ExpenseManager.Domain` (Class Library)
- `ExpenseManager.Application` (Class Library)
- `ExpenseManager.Infrastructure` (Class Library)

Let op dat je exact dezelfde namen gebruikt. In deze oefening zijn projectnamen onderdeel van de leerstof.

### Projectreferenties instellen

Leg nu de juiste referenties:

- `ExpenseManager.Presentation` → `ExpenseManager.Application`
- `ExpenseManager.Presentation` → `ExpenseManager.Domain`
- `ExpenseManager.Application` → `ExpenseManager.Domain`
- `ExpenseManager.Application` → `ExpenseManager.Infrastructure`
- `ExpenseManager.Infrastructure` → `ExpenseManager.Domain`

**Controlepunt:** build de volledige solution. Dit mag slagen, ook al staat er nog bijna geen code.

## Domain layer (Expense)

Maak in `ExpenseManager.Domain` een folder `Models` en voeg de klasse `Expense` toe in namespace `ExpenseManager.Domain.Models`.

De categorie van een uitgave bewaar je als een gewone `string`. De gebruiker kiest ze in de UI uit een vaste lijst: Food, Transport, Housing, Leisure of Work.

### Properties

```csharp
public int Id { get; set; }
public string Description { get; }
public decimal Amount { get; }
public DateTime Date { get; }
public string Category { get; }
public bool IsReimbursed { get; private set; }
public DateTime? ReimbursedAt { get; private set; }
```

Stel deze vraag eens aan je favoriete LLM (bv. ChatGPT):

```
Waarom gebruik je in C# best decimal in plaats van double voor geldbedragen?
```

### Constructor met validatie

Maak een constructor met deze signature:

```csharp
public Expense(string description, decimal amount, DateTime date, string category)
{

}
```

Voeg de volgende regels toe:

- Description en Category zijn verplicht: als description of category leeg is of enkel whitespace bevat, gooi je een `ArgumentException`.
- Amount moet groter zijn dan 0: anders gooi je een `ArgumentOutOfRangeException`.
- Date mag niet in de toekomst liggen: anders gooi je een `ArgumentException`. TIP: vergelijk `date.Date` met `DateTime.Today`.
- Sla de beschrijving getrimd op (`description.Trim()`) en de datum zonder tijd (`date.Date`).
- IsReimbursed is initieel `false`.
- ReimbursedAt is initieel `null`.

Dit is domeinvalidatie: deze regels moeten altijd gelden, ongeacht UI of opslag. Daarom horen ze in de Domain layer.

### MarkAsReimbursed()

```csharp
public void MarkAsReimbursed()
{
    // TODO: doe niets als de uitgave al terugbetaald is
    // TODO: zet IsReimbursed op true
    // TODO: zet ReimbursedAt op DateTime.Now
}
```

De eerste regel maakt de methode "idempotent": twee keer oproepen geeft hetzelfde resultaat als één keer. De oorspronkelijke datum van terugbetaling blijft dus behouden.

### Override ToString()

Override `ToString()` zodat een uitgave mooi getoond kan worden in de ListBox.

```csharp
public override string ToString()
{
    string status = IsReimbursed ? "[Reimbursed]" : "[Open]";
    return $"{status} {Date:dd/MM/yyyy} - {Description} ({Category}) € {Amount:0.00}";
}
```

**Controlepunt:** `ExpenseManager.Domain` moet builden.

## Infrastructure layer (ExpenseRepository)

Maak in `ExpenseManager.Infrastructure` een folder `Repositories` en voeg de klasse `ExpenseRepository` toe in namespace `ExpenseManager.Infrastructure.Repositories`.

Deze repository bewaart uitgaven intern in een `List<Expense>` en geeft data enkel vrij via methodes.

### Private lijst + constructor

```csharp
private readonly List<Expense> _expenses;

public ExpenseRepository()
{
    // TODO: initialiseer de lijst
}
```

### GetAll() geeft een kopie van de lijst

```csharp
public List<Expense> GetAll()
{
    return new List<Expense>(_expenses);
}
```

Stel deze vraag eens aan je favoriete LLM:

```
Waarom geeft een repository in C# beter een kopie van de interne lijst terug in plaats van de lijst zelf?
```

### Get() geeft één object van de lijst

```csharp
public Expense? Get(int id)
{
    return _expenses.Find(expense => expense.Id == id);
}
```

### Add()

```csharp
public void Add(Expense expense)
{
    // TODO: bepaal het volgende unieke Id
    // TIP: Count + 1
    // TODO: voeg toe aan _expenses
}
```

### Remove()

```csharp
public bool Remove(Expense expense)
{
    // TODO: verwijder het item en return true/false
}
```

**Controlepunt:** `ExpenseManager.Domain` + `ExpenseManager.Infrastructure` moeten builden.

## Application layer (ExpenseService)

Maak in `ExpenseManager.Application` een folder `Services` en voeg de klasse `ExpenseService` toe in namespace `ExpenseManager.Application.Services`.

Deze service bevat de use cases en business rules.

### Repository en constanten

```csharp
public class ExpenseService
{
    public const decimal MonthlyBudget = 1000m;
    public const int MaxDaysToDelete = 30;

    private readonly ExpenseRepository _repository;

    public ExpenseService()
    {
        // TODO: maak de repository aan
    }
}
```

Stel deze vraag eens aan je favoriete LLM:

```
Wat is het verschil tussen const en readonly in C#?
```

### GetExpenses()

```csharp
public List<Expense> GetExpenses()
{
    // TODO: return alle uitgaven via de repository
    // TIP: sorteer met LINQ op datum, nieuwste eerst: .OrderByDescending(e => e.Date).ToList()
}
```

### GetTotalForMonth()

```csharp
public decimal GetTotalForMonth(int year, int month)
{
    // TODO: filter de uitgaven van dat jaar en die maand
    // TODO: return de som van de bedragen
    // TIP: .Where(e => e.Date.Year == year && e.Date.Month == month).Sum(e => e.Amount)
}
```

### AddExpense() + duplicate rule + budget rule

```csharp
public void AddExpense(string description, decimal amount, DateTime date, string category)
```

Business rules:

- Een uitgave met dezelfde beschrijving op dezelfde datum mag niet dubbel bestaan. Als de repository zo'n uitgave bevat, gooi je een `InvalidOperationException`.
- Het totaal van de maand plus het nieuwe bedrag mag niet hoger zijn dan `MonthlyBudget`. Anders gooi je een `InvalidOperationException`. Precies op het budget uitkomen mag wel.

TIP: maak eerst het `Expense`-object aan. Zo valideert de Domain layer de invoer meteen, en is de beschrijving al getrimd.

```csharp
public void AddExpense(string description, decimal amount, DateTime date, string category)
{
    Expense expense = new Expense(description, amount, date, category);

    // TODO: zoek een duplicate:
    //   _repository.GetAll().Find(e => e.Date == expense.Date &&
    //       e.Description.Equals(expense.Description, StringComparison.OrdinalIgnoreCase));
    // TODO: als duplicate -> throw new InvalidOperationException("Deze uitgave bestaat reeds.");

    // TODO: bereken het totaal van de maand met GetTotalForMonth(...)
    // TODO: als totaal + bedrag > MonthlyBudget -> throw new InvalidOperationException("Het maandbudget wordt overschreden.");

    _repository.Add(expense);
}
```

### ReimburseExpense()

```csharp
public void ReimburseExpense(Expense expense)
{
    // TODO: als expense.Category niet "Work" is -> throw new InvalidOperationException("...");
    // TIP: expense.Category.Equals("Work", StringComparison.OrdinalIgnoreCase)
    // TODO: zoek het item in de repository (TIP: Get(expense.Id))
    // TODO: roep MarkAsReimbursed() aan
}
```

### DeleteExpense() + 2 delete rules

Business rules:

- Een uitgave mag niet verwijderd worden als `IsReimbursed == true`.
- Een uitgave mag niet verwijderd worden als ze ouder is dan 30 dagen.

```csharp
public void DeleteExpense(Expense expense)
{
    if (expense.IsReimbursed)
    {
        throw new InvalidOperationException("...");
    }

    if (expense.Date < DateTime.Today.AddDays(-MaxDaysToDelete))
    {
        throw new InvalidOperationException("...");
    }

    _repository.Remove(expense);
}
```

**Controlepunt:** alle class libraries moeten builden.

## Presentation layer (WPF met 2 windows)

Je voorziet 2 vensters:

- `ExpenseListWindow` toont de lijst, de details, het maandtotaal en de acties.
- `AddExpenseWindow` verzamelt input en geeft data terug via properties + `DialogResult`.

### MainWindow

- Verwijder `MainWindow.xaml` en `MainWindow.xaml.cs`.
- Pas `App.xaml` aan zodat `ExpenseListWindow.xaml` het startup window is: `StartupUri="ExpenseListWindow.xaml"`.

> ⚠️ `x:Class` in elk `.xaml`-bestand, de `namespace` in het `.xaml.cs`-bestand en de klassenaam moeten exact overeenkomen, bv. `ExpenseManager.Presentation.ExpenseListWindow`. Anders krijg je de fout *"The name 'InitializeComponent' does not exist in the current context"*.

### XAML

Neem de XAML over uit deze repository:

- [`ExpenseListWindow.xaml`](ExpenseManager.Presentation/ExpenseListWindow.xaml): bovenaan het maandtotaal (`monthTotalTextBlock`), links de `expensesListBox`, rechts het detailpaneel en onderaan de knoppen Add, Reimburse, Delete en Refresh.
- [`AddExpenseWindow.xaml`](ExpenseManager.Presentation/AddExpenseWindow.xaml): `descriptionTextBox`, `amountTextBox`, `datePicker` en een `categoryComboBox` met vijf vaste `ComboBoxItem`s.

### AddExpenseWindow.xaml.cs

Voorzie 4 properties die van overal gelezen, maar enkel binnen de klasse gewijzigd kunnen worden:

```csharp
public string ExpenseDescription { get; private set; } = "";
public decimal ExpenseAmount { get; private set; }
public DateTime ExpenseDate { get; private set; }
public string ExpenseCategory { get; private set; } = "";
```

- Stel de datum van vandaag in als standaardwaarde van de `datePicker`.
- Selecteer de eerste categorie in de `categoryComboBox` (`SelectedIndex = 0`).
- Zet de tekst uit `amountTextBox` om naar een `decimal` met `decimal.TryParse`. Lukt dat niet, toon dan een MessageBox en laat het venster open.
- Geef de 4 properties de juiste waarde wanneer er op de SaveButton geklikt wordt. De gekozen categorie lees je met `categoryComboBox.Text`.
- Geef de `DialogResult`-property de juiste waarde.

Stel deze vraag eens aan je favoriete LLM:

```
Wat is in C# het verschil tussen decimal.Parse en decimal.TryParse, en wat doet het out-keyword?
```

### ExpenseListWindow.xaml.cs

Maak een private field aan voor de `ExpenseService` en initialiseer deze in de constructor.

```csharp
private readonly ExpenseService _expenseService;

public ExpenseListWindow()
{
    InitializeComponent();

    _expenseService = new ExpenseService();
    // RefreshExpenses();
}
```

Maak deze methodes en eventhandlers:

| Methode | Wat moet ze doen? |
|---|---|
| `RefreshExpenses()` | Alle uitgaven in de `expensesListBox` zetten, het maandtotaal tonen in `monthTotalTextBlock` (bv. `This month: € 245,00 / € 1000,00`) en `ClearDetails()` oproepen. Roep deze methode aan vanuit de constructor. |
| `ClearDetails()` | Elke TextBlock uit het detailgedeelte leegmaken. |
| `ExpensesListBox_SelectionChanged` | Het geselecteerde item casten naar `Expense`. Lukt dat niet: `ClearDetails()` + return. Anders de details tonen. |
| `AddButton_Click` | `AddExpenseWindow` tonen met `ShowDialog()`. Is het resultaat niet `true`: return. Anders `AddExpense(...)` oproepen in een try-catch (vang `ArgumentException` én `InvalidOperationException`) en `RefreshExpenses()`. |
| `ReimburseButton_Click` | Geselecteerde uitgave ophalen (niets geselecteerd: MessageBox + return), `ReimburseExpense(...)` in een try-catch, `RefreshExpenses()`. |
| `DeleteButton_Click` | Zoals Reimburse, maar vraag eerst een bevestiging met `MessageBoxButton.YesNo` en roep `DeleteExpense(...)` op. |
| `RefreshButton_Click` | `RefreshExpenses()` oproepen. |

## Unit tests

We schrijven unit tests voor:

- Domain: validatie in `Expense` en het gedrag van `MarkAsReimbursed()`
- Application: business rules in `ExpenseService` (duplicaten, maandbudget, terugbetalen, delete rules)

We testen niet de WPF UI.

### Testprojecten aanmaken en referenties leggen

Maak in je solution twee extra projecten aan:

- `ExpenseManager.Domain.Tests` (xUnit Test Project)
- `ExpenseManager.Application.Tests` (xUnit Test Project)

Leg daarna de juiste referenties:

- `ExpenseManager.Domain.Tests` → `ExpenseManager.Domain`
- `ExpenseManager.Application.Tests` → `ExpenseManager.Application`
- `ExpenseManager.Application.Tests` → `ExpenseManager.Domain`
- `ExpenseManager.Application.Tests` → `ExpenseManager.Infrastructure`

### Domain tests (ExpenseTests)

```csharp
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
```

Met `[Theory]` en `[InlineData]` voer je dezelfde test uit met verschillende waarden:

```csharp
[Theory]
[InlineData(0)]
[InlineData(-5)]
public void Constructor_AmountZeroOrNegative_ThrowsArgumentOutOfRangeException(decimal amount)
{
    Assert.Throws<ArgumentOutOfRangeException>(() =>
    {
        Expense expense = new Expense("Lunch", amount, DateTime.Today, "Food");
    });
}
```

Schrijf zelf ook:

- `Constructor_WhitespaceDescription_ThrowsArgumentException`
- `Constructor_EmptyCategory_ThrowsArgumentException`
- `Constructor_FutureDate_ThrowsArgumentException`
- `MarkAsReimbursed_SetsIsReimbursedAndReimbursedAt`

Stel deze vraag eens aan je favoriete LLM:

```
Wat is in xUnit het verschil tussen [Fact] en [Theory]?
```

### Application tests (ExpenseServiceTests)

`ExpenseService` maakt zelf een `ExpenseRepository` aan. Daardoor testen we hier de business rules in combinatie met de in-memory repository. Elke test maakt een nieuwe service aan en start dus met een lege lijst.

```csharp
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
```

Schrijf zelf ook:

- `AddExpense_ExceedsMonthlyBudget_ThrowsInvalidOperationException` (900 + 150)
- `ReimburseExpense_NonWorkExpense_ThrowsInvalidOperationException`
- `ReimburseExpense_WorkExpense_SetsReimbursedAt`
- `DeleteExpense_ReimbursedExpense_ThrowsInvalidOperationException`
- `DeleteExpense_OlderThan30Days_ThrowsInvalidOperationException` (datum `DateTime.Today.AddDays(-31)`)

### Extra uitdaging (optioneel)

- `AddExpense()` met een lege beschrijving moet falen (via de domeinvalidatie).
- `AddExpense()` precies op het maandbudget (900 + 100) moet lukken.
- `DeleteExpense()` moet wel werken voor een recente uitgave die niet terugbetaald is.
- `MarkAsReimbursed()` twee keer oproepen verandert `ReimbursedAt` niet.

## In het kort

- Je maakte een solution met 4 projecten volgens een vereenvoudigde Clean Architecture-structuur.
- Je bouwde laag per laag de code op: Domain (model + validatie), Infrastructure (in-memory opslag), Application (use cases + business rules).
- Je koppelde alles aan een WPF UI met twee vensters (`ShowDialog()` + data terug ophalen), een ComboBox met vaste keuzes en invoer die je zelf omzet naar een `decimal`.
- Je testte business rules via exceptions die je in de UI toont met een MessageBox.
- Je voegde twee testprojecten toe: één voor de Domain layer en één voor de Application layer.
