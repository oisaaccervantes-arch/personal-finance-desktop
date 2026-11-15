using System.Windows;
using System.Windows.Media;
using PersonalFinance.Models;
namespace PersonalFinance.Services;
public static class ThemeService
{
    public static string Mode {get;private set;}="Oscuro";
    public static string BackgroundColor {get;private set;}="#625081";
    public static string FontChoice {get;private set;}="Space Grotesk";
    public static FontFamily FontFor(string choice)=>new(choice=="Space Grotesk"?"pack://application:,,,/PersonalFinance;component/Assets/Fonts/#Space Grotesk":choice);
    public static bool IsLight=>Mode=="Claro";
    public static Color Parse(string value)=>FinanceStore.ValidColor(value)?(Color)ColorConverter.ConvertFromString(value):Color.FromRgb(98,80,129);
    public static Color Mix(Color a,Color b,double weight)=>Color.FromRgb((byte)(a.R+(b.R-a.R)*weight),(byte)(a.G+(b.G-a.G)*weight),(byte)(a.B+(b.B-a.B)*weight));
    private static double Luminance(Color c){static double L(byte v){double n=v/255d;return n<=.04045?n/12.92:Math.Pow((n+.055)/1.055,2.4);}return .2126*L(c.R)+.7152*L(c.G)+.0722*L(c.B);}
    public static Brush InkFor(string value)
    {
        var c=Parse(value);double L(byte v){double n=v/255d;return n<=.04045?n/12.92:Math.Pow((n+.055)/1.055,2.4);}
        return .2126*L(c.R)+.7152*L(c.G)+.0722*L(c.B)>.2?new SolidColorBrush(Color.FromRgb(18,22,31)):Brushes.White;
    }
    public static void Apply(AppearanceSettings settings)
    {
        Mode=settings.Mode=="Claro"?"Claro":"Oscuro";BackgroundColor=FinanceStore.ValidColor(settings.BackgroundColor)?settings.BackgroundColor:"#625081";FontChoice=settings.FontChoice is "Segoe UI" or "Georgia"?settings.FontChoice:"Space Grotesk";
        if(Application.Current==null)return;
        var resources=Application.Current.Resources;
        resources["AppFont"]=FontFor(FontChoice);
        void Set(string key,string dark,string light){var brush=new SolidColorBrush((Color)ColorConverter.ConvertFromString(IsLight?light:dark));brush.Freeze();resources[key]=brush;}
        Set("TextPrimary","#F5F2FA","#211D2D");Set("TextSecondary","#BFB7CE","#62596F");
        if(FinanceStore.ValidColor(settings.TextColor))
        {
            var ink=Parse(settings.TextColor);resources["TextPrimary"]=new SolidColorBrush(ink);
            resources["TextSecondary"]=new SolidColorBrush(Mix(ink,IsLight?Colors.Gray:Colors.LightGray,.35));
        }
        Set("Surface","#F222202E","#F2F7F4FC");Set("Input","#90181522","#D9FFFFFF");
        Set("InputEdge","#62566F","#B9ABC9");Set("InputFocus","#AA30263F","#FFFFFFFF");
        Set("Chrome","#E018151F","#E0F3EFF8");Set("Overlay","#75090711","#5C91859E");
        Set("Selected","#902B4C93","#8CB7CFFF");Set("Hover","#405C4779","#407F65A0");
        Set("ButtonFill","#B017203A","#D9FAFBFF");Set("ButtonEdge","#B080A6EF","#B07B9FCD");Set("ButtonHover","#D9263E75","#FFF1F6FF");
        Set("PanelFill","#52151120","#B8FFFFFF");Set("PanelStroke","#A0E9DDF5","#B089769F");
        Set("TableRow","#3015151D","#60FFFFFF");Set("TableAlternate","#301E1D29","#409F8AB5");Set("TableHeader","#6013111C","#809E87B7");
        Set("Divider","#403C344A","#50A497B5");Set("ErrorText","#FFD5DD","#862A3C");
        Set("IncomeText","#BBEFD8","#206548");Set("ExpenseText","#E8C8EF","#733E80");
        resources["GlassFill"]=resources["PanelFill"];resources["GlassEdge"]=resources["PanelStroke"];
        var color=Parse(BackgroundColor);
        var bright=Luminance(color)>.45;
        var baseColor=Mix(color,IsLight?Colors.White:Colors.Black,IsLight?.86:bright?.9:.8);
        var highlight=Mix(color,IsLight?Colors.White:Colors.Black,IsLight?.55:bright?.7:.25);
        var backdrop=new RadialGradientBrush(highlight,baseColor){Center=new Point(.25,.05),RadiusX=1,RadiusY=1.2};backdrop.Freeze();resources["Backdrop"]=backdrop;
        var accent=new SolidColorBrush(Mix(color,IsLight?Colors.White:Colors.Black,.15));accent.Freeze();resources["AccentFill"]=accent;
        if(FinanceStore.ValidColor(settings.AccentColor))
        {
            var chosen=Parse(settings.AccentColor);resources["AccentFill"]=new SolidColorBrush(chosen);
            resources["ButtonEdge"]=new SolidColorBrush(Color.FromArgb(190,chosen.R,chosen.G,chosen.B));
            resources["Selected"]=new SolidColorBrush(Color.FromArgb(IsLight?(byte)135:(byte)150,chosen.R,chosen.G,chosen.B));
        }
        var activeAccent=(SolidColorBrush)resources["AccentFill"];
        resources["AccentInk"]=InkFor(activeAccent.Color.ToString().Replace("#FF","#"));
    }
}
