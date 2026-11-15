using System.Windows;
using System.Windows.Media;
using PersonalFinance.Models;
using PersonalFinance.Services;
namespace PersonalFinance.ViewModels;
public sealed class CardPresentation(Account account,long balance)
{
    public string CardIssuer=>account.Bank;
    public string CardCustomText=>account.CardText;
    public string CardBalance=>MainViewModel.Money(balance);
    public string CardBalanceLabel=>account.Type=="Tarjeta de crédito"?"SALDO REGISTRADO · MXN":"BALANCE DISPONIBLE · MXN";
    public Brush CardInk=>ThemeService.InkFor(account.CardColor);
    public Brush CardPaint=>Paint(account.CardColor);
    public static Brush Paint(string hex)
    {
        var color=ThemeService.Parse(hex);
        var brush=new LinearGradientBrush{StartPoint=new Point(0,0),EndPoint=new Point(1,1)};
        brush.GradientStops.Add(new GradientStop(ThemeService.Mix(color,Colors.White,.18),0));
        brush.GradientStops.Add(new GradientStop(color,.5));
        brush.GradientStops.Add(new GradientStop(ThemeService.Mix(color,Colors.Black,.12),1));brush.Freeze();return brush;
    }
}
