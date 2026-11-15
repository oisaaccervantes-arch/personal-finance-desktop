using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.ViewModels;
public sealed class RelayCommand(Action<object?> execute, Func<object?,bool>? canExecute=null) : ICommand
{
    public event EventHandler? CanExecuteChanged { add {CommandManager.RequerySuggested+=value;} remove {CommandManager.RequerySuggested-=value;} }
    public bool CanExecute(object? parameter)=>canExecute?.Invoke(parameter)??true;
    public void Execute(object? parameter){if(CanExecute(parameter))execute(parameter);}
}
public sealed class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? Refreshed;
    private FinanceStore Store { get; } = new();
    public void Save<T>(T entity,int id) where T:class => Store.Save(entity,id);
    public void Delete<T>(int id) where T:class => Store.Delete<T>(id);
    public void Backup(string path)=>Store.Backup(path);
    public void Restore(string path)=>Store.Restore(path);
    public void SaveRecurring(RecurringRule rule)=>Store.SaveRecurring(rule);
    public AppearanceSettings Appearance=>Store.Appearance();
    public void SaveAppearance(string mode,string color)=>Store.SaveAppearance(mode,color);
    public void SaveAppearance(AppearanceSettings settings)=>Store.SaveAppearance(settings);
    public List<Account> Accounts { get; private set; }=[];
    public List<Category> Categories { get; private set; }=[];
    public List<Movement> Movements { get; private set; }=[];
    public List<Budget> Budgets { get; private set; }=[];
    public List<Transfer> Transfers {get;private set;}=[];
    public List<RecurringRule> RecurringRules {get;private set;}=[];
    public string Page { get; private set; }="Dashboard";
    public DateTime Month { get; private set; }=new(DateTime.Today.Year,DateTime.Today.Month,1);
    public string MonthLabel=>Month.ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-MX"));
    public long Balance=>Accounts.Sum(x=>x.OpeningCents)+Movements.Sum(Signed);
    public long Income=>Current.Where(x=>x.Category.Type=="Ingreso").Sum(x=>x.Cents);
    public long Expense=>Current.Where(x=>x.Category.Type=="Gasto").Sum(x=>x.Cents);
    public IEnumerable<Movement> Current=>Movements.Where(x=>x.Date>=Month&&x.Date<Month.AddMonths(1));
    public long AccountBalance(Account a)=>a.OpeningCents+Movements.Where(x=>x.AccountId==a.Id).Sum(Signed)+Transfers.Where(x=>x.ToAccountId==a.Id).Sum(x=>x.Cents)-Transfers.Where(x=>x.FromAccountId==a.Id).Sum(x=>x.Cents);
    public long ProjectedBalance(DateTime date)=>Balance+RecurringRules.Where(x=>x.Active).Sum(x=>FinanceStore.Occurrences(x,date).Count(d=>d>DateTime.Today)*(x.Type=="Ingreso"?x.Cents:-x.Cents));
    public static long Signed(Movement m)=>m.Category.Type=="Ingreso"?m.Cents:-m.Cents;
    public static string Money(long cents)=>(cents/100m).ToString("C2",System.Globalization.CultureInfo.GetCultureInfo("es-MX"));
    public ICommand Navigate { get; }
    public ICommand ChangeMonth { get; }
    private int? selectedCardId;
    public List<Account> CardAccounts=>Accounts.Where(x=>x.Type is "Tarjeta de débito" or "Tarjeta de crédito" or "Cuenta bancaria").ToList();
    public Account? SelectedCard=>CardAccounts.FirstOrDefault(x=>x.Id==selectedCardId)??CardAccounts.FirstOrDefault();
    public bool HasCard=>SelectedCard!=null;
    public string CardIssuer=>SelectedCard?.Bank??"";
    public string CardCustomText=>SelectedCard?.CardText??"";
    public System.Windows.Media.Brush CardPaint=>CardPresentation.Paint(SelectedCard?.CardColor??"#625081");
    public System.Windows.Media.Brush CardInk=>ThemeService.InkFor(SelectedCard?.CardColor??"#625081");
    public string CardAccountName=>SelectedCard?.Name??"Tu tarjeta personalizada";
    public string CardBalance=>SelectedCard is {} account?Money(AccountBalance(account)):"—";
    public string CardBalanceLabel=>SelectedCard?.Type=="Tarjeta de crédito"?"SALDO REGISTRADO · MXN":"BALANCE DISPONIBLE · MXN";
    public string CardPosition=>HasCard?$"{CardAccounts.FindIndex(x=>x.Id==SelectedCard!.Id)+1} de {CardAccounts.Count}":"Sin tarjetas";
    public string CardHint=>HasCard?"Saldo calculado con tus movimientos locales.":"Configura tu tarjeta desde Cuentas";
    public ICommand NextCard { get; }
    public MainViewModel()
    {
        Navigate=new RelayCommand(p=>{Page=(string)p!; Notify(); Refreshed?.Invoke();});
        ChangeMonth=new RelayCommand(p=>{Month=Month.AddMonths(Convert.ToInt32(p)); Notify(); Refreshed?.Invoke();});
        NextCard=new RelayCommand(_=>
        {
            var cards=CardAccounts;
            int index=cards.FindIndex(x=>x.Id==SelectedCard?.Id);
            selectedCardId=cards[(index+1)%cards.Count].Id;
            Notify();
        },_=>CardAccounts.Count>1);
        Reload();
    }
    public void Reload() { Store.ProcessDue();Accounts=Store.Accounts(); Categories=Store.Categories(); Movements=Store.Movements(); Budgets=Store.Budgets(); Transfers=Store.Transfers();RecurringRules=Store.RecurringRules();Notify(); Refreshed?.Invoke(); }
    private void Notify(){PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(null));CommandManager.InvalidateRequerySuggested();}
}
