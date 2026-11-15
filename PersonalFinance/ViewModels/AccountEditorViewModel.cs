using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using PersonalFinance.Models;
using PersonalFinance.Services;
namespace PersonalFinance.ViewModels;
public sealed class AccountEditorViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? Saved;
    public event Action? Cancelled;
    private readonly FinanceStore store=new();
    private readonly int id;
    private readonly long movementDelta;
    private string name,bank,text,color,type,opening,error="";
    public string Title=>id==0?"Nueva cuenta o tarjeta":"Personalizar cuenta";
    public string Name{get=>name;set{name=value;Notify();}}
    public string Bank{get=>bank;set{bank=value;Notify();}}
    public string CardText{get=>text;set{text=value;Notify();}}
    public string CardColor{get=>color;set{color=value;Notify();}}
    public string Type{get=>type;set{type=value;Notify();}}
    public string Opening{get=>opening;set{opening=value;Notify();}}
    public string Error=>error;
    public string[] Types {get;}
    public CardPresentation Preview=>new(new Account{Bank=Bank,CardText=CardText,CardColor=CardColor,Type=Type},ParseOpening(out var cents)?cents+movementDelta:movementDelta);
    public ICommand Save {get;}
    public ICommand Cancel {get;}
    public AccountEditorViewModel(Account? account,long delta=0)
    {
        id=account?.Id??0;movementDelta=delta;name=account?.Name??"";bank=account?.Bank??"";text=account?.CardText??"";color=account?.CardColor??"#625081";type=account?.Type??"Tarjeta de débito";opening=((account?.OpeningCents??0)/100m).ToString("F2",CultureInfo.CurrentCulture);
        Types=account?.Type=="Efectivo"?["Efectivo"]:["Cuenta bancaria","Tarjeta de débito","Tarjeta de crédito","Cuenta de ahorro"];
        Save=new RelayCommand(_=>SaveAccount());Cancel=new RelayCommand(_=>Cancelled?.Invoke());
    }
    private bool ParseOpening(out long cents)
    {
        cents=0;if(!decimal.TryParse(Opening,NumberStyles.Number,CultureInfo.CurrentCulture,out var value)||value is < -100000000000m or > 100000000000m||decimal.Round(value,2)!=value)return false;cents=(long)(value*100);return true;
    }
    private void SaveAccount()
    {
        error="";
        if(string.IsNullOrWhiteSpace(Name))error="Escribe un nombre para identificar esta cuenta.";
        else if(!FinanceStore.ValidColor(CardColor))error="Escribe un color hexadecimal válido, por ejemplo #2457A6.";
        else if(!ParseOpening(out var cents))error="Escribe un saldo inicial válido con hasta dos decimales.";
        else
        {
            try{store.Save(new Account{Id=id,Name=Name.Trim(),Bank=Bank.Trim(),CardText=CardText.Trim(),CardColor=CardColor,Type=Type,OpeningCents=cents},id);Saved?.Invoke();}
            catch(Exception ex){error=ex is InvalidOperationException?ex.Message:"No se pudo guardar la cuenta. Inténtalo de nuevo.";}
        }
        Notify();
    }
    private void Notify()=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(null));
}
