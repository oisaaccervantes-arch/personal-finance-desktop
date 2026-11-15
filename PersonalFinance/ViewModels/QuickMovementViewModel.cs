using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using PersonalFinance.Models;
using PersonalFinance.Services;

namespace PersonalFinance.ViewModels;
public sealed class QuickMovementViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? Closed;
    public event Action? Saved;
    public event Action? AccountsRequested;
    private readonly FinanceStore store=new();
    public List<Account> Accounts {get;}
    public List<Account> BankAccounts {get;}
    public List<Category> Categories {get;}
    private bool? incoming;
    private bool transfer,withdrawal,cashChoice;
    private string concept="",amount="",error="";
    private Account? account;
    private Category? category;
    private bool saving;
    public string Concept {get=>concept;set{concept=value;Notify();}}
    public string Amount {get=>amount;set{amount=value;Notify();}}
    public Account? Account {get=>account;set{account=value;Notify();}}
    public Category? Category {get=>category;set{category=value;Notify();}}
    public string Error=>error;
    public string Title=>cashChoice?"Mover efectivo":incoming==null&&!transfer?"¿Qué vas a registrar?":transfer?(withdrawal?"Retirar efectivo":"Depositar efectivo"):incoming==true?"Dinero entrante":"Dinero saliente";
    public string Subtitle=>cashChoice?"Entre tu billetera y una cuenta.":incoming==null&&!transfer?"Elige el tipo de movimiento para continuar.":"Se registrará hoy · MXN";
    public string AccountLabel=>incoming==true?"¿A qué cuenta?":"¿De qué cuenta?";
    public string SaveLabel=>transfer?(withdrawal?"Registrar retiro":"Registrar depósito"):incoming==true?"Registrar ingreso":"Registrar gasto";
    public string TransferAccountLabel=>withdrawal?"¿De qué cuenta sale el dinero?":"¿A qué cuenta llegará el dinero?";
    public string CashTransferHint=>withdrawal?"El dinero se sumará a Efectivo.":"El dinero se descontará de Efectivo.";
    public Visibility ChoiceVisibility=>incoming==null&&!transfer&&!cashChoice?Visibility.Visible:Visibility.Collapsed;
    public Visibility CashChoiceVisibility=>cashChoice?Visibility.Visible:Visibility.Collapsed;
    public Visibility StepVisibility=>incoming!=null||transfer||cashChoice?Visibility.Visible:Visibility.Collapsed;
    public Visibility FormVisibility=>(incoming!=null||transfer)&&(transfer?BankAccounts.Count>0:Accounts.Count>0)?Visibility.Visible:Visibility.Collapsed;
    public Visibility MissingVisibility=>(incoming!=null||transfer)&&(transfer?BankAccounts.Count==0:Accounts.Count==0)?Visibility.Visible:Visibility.Collapsed;
    public Visibility ExpenseVisibility=>incoming==false&&!transfer?Visibility.Visible:Visibility.Collapsed;
    public Visibility IncomeVisibility=>incoming==true&&!transfer?Visibility.Visible:Visibility.Collapsed;
    public Visibility TransferVisibility=>transfer?Visibility.Visible:Visibility.Collapsed;
    public Visibility ErrorVisibility=>error.Length>0?Visibility.Visible:Visibility.Collapsed;
    public ICommand Choose {get;}
    public ICommand Save {get;}
    public ICommand Back {get;}
    public ICommand Close {get;}
    public ICommand GoAccounts {get;}
    public QuickMovementViewModel(IEnumerable<Account> accounts,IEnumerable<Category> categories)
    {
        Accounts=accounts.ToList();
        BankAccounts=Accounts.Where(x=>x.Type!="Efectivo").ToList();
        Categories=categories.Where(x=>x.Type=="Gasto"&&x.Name!="Otro").Concat(new[]{new Category{Id=0,Name="Otro",Type="Gasto",Icon="stars"}}).ToList();
        Choose=new RelayCommand(p=>{var choice=(string?)p;if(choice=="Efectivo"){cashChoice=true;error="";Notify();return;}cashChoice=false;transfer=choice is "Retiro" or "Deposito";withdrawal=choice=="Retiro";incoming=transfer?null:choice=="Ingreso";if(transfer&&Account?.Type=="Efectivo")Account=null;Concept=transfer?(withdrawal?"Retiro de efectivo":"Depósito de efectivo"):"";error="";Notify();});
        Back=new RelayCommand(_=>{if(transfer){transfer=false;cashChoice=true;}else{incoming=null;cashChoice=false;}error="";Notify();});
        Close=new RelayCommand(_=>Closed?.Invoke());
        GoAccounts=new RelayCommand(_=>AccountsRequested?.Invoke());
        Save=new RelayCommand(_=>SaveMovement(),_=>!saving);
    }
    private void SaveMovement()
    {
        error="";
        if(incoming==null&&!transfer)return;
        if(Account==null)error="Selecciona la cuenta del movimiento.";
        else if(string.IsNullOrWhiteSpace(Concept)||Concept.Trim().Length>200)error="Escribe un concepto de hasta 200 caracteres.";
        else if(!decimal.TryParse(Amount,NumberStyles.AllowDecimalPoint|NumberStyles.AllowLeadingWhite|NumberStyles.AllowTrailingWhite,CultureInfo.CurrentCulture,out var value)||value<=0||value>100000000000m||decimal.Round(value,2)!=value)error="Escribe un monto positivo con hasta dos decimales, sin separadores de miles.";
        else if(incoming==false&&!transfer&&Category==null)error="Selecciona una categoría o elige Otro.";
        else
        {
            saving=true;
            try
            {
                if(transfer)
                {
                    var cash=Accounts.Single(x=>x.Type=="Efectivo");
                    store.TransferFunds(withdrawal?Account.Id:cash.Id,withdrawal?cash.Id:Account.Id,checked((long)(value*100)),Concept);
                }
                else store.QuickMovement(incoming!.Value,Concept,checked((long)(value*100)),Account.Id,incoming.Value?null:Category!.Id);
                Saved?.Invoke();
            }
            catch(Exception){error="No se pudo guardar. Comprueba la cuenta e inténtalo de nuevo.";}
            finally {saving=false;}
        }
        Notify();
    }
    private void Notify(){PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(null));CommandManager.InvalidateRequerySuggested();}
}
