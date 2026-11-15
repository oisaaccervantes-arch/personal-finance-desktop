namespace PersonalFinance.Models;

public class Account
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Efectivo";
    public long OpeningCents { get; set; }
    public string Bank { get; set; } = "";
    public string CardText { get; set; } = "";
    public string CardColor { get; set; } = "#625081";
    public override string ToString() => Name;
}
public class AppearanceSettings
{
    public int Id { get; set; } = 1;
    public string ThemeName {get;set;} = "Personalizado";
    public string Mode { get; set; } = "Oscuro";
    public string BackgroundColor { get; set; } = "#625081";
    public string TextColor {get;set;} = "";
    public string AccentColor {get;set;} = "";
    public string FontChoice {get;set;} = "Space Grotesk";
}
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "Gasto";
    public string Icon { get; set; } = "◇";
    public string Color { get; set; } = "#BAC8D5";
    public override string ToString() => Name;
}
public class Movement
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public long Cents { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int? RecurringRuleId { get; set; }
    public DateTime? OccurrenceDate { get; set; }
}
public class Transfer
{
    public int Id { get; set; }
    public int FromAccountId { get; set; }
    public Account FromAccount { get; set; } = null!;
    public int ToAccountId { get; set; }
    public Account ToAccount { get; set; } = null!;
    public long Cents { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Description { get; set; } = "";
}
public class RecurringRule
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public long Cents { get; set; }
    public string Type { get; set; } = "Gasto";
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public bool Active { get; set; } = true;
}
public class Budget
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public DateTime Month { get; set; }
    public long LimitCents { get; set; }
}
