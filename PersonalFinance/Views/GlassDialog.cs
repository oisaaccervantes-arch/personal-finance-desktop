using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace PersonalFinance.Views;
internal static class GlassDialog
{
    public static bool Show(Window owner,string message,string title,bool confirm=false)
    {
        var window=new Window{Owner=owner,Title=title,Width=460,SizeToContent=SizeToContent.Height,MaxHeight=SystemParameters.WorkArea.Height-60,WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,ShowInTaskbar=false,WindowStartupLocation=WindowStartupLocation.CenterOwner,Background=new SolidColorBrush(Color.FromRgb(34,29,43)),Foreground=Brushes.White,Resources=owner.Resources,FontFamily=owner.FontFamily,FontSize=owner.FontSize};
        window.SetResourceReference(Window.BackgroundProperty,"Surface");window.SetResourceReference(Window.ForegroundProperty,"TextPrimary");
        var panel=new StackPanel{Margin=new Thickness(28)};
        panel.Children.Add(new TextBlock{Text=title,FontSize=23,FontWeight=FontWeights.SemiBold,TextWrapping=TextWrapping.Wrap});
        panel.Children.Add(new TextBlock{Text=message,FontSize=14,Foreground=(Brush)owner.FindResource("TextSecondary"),TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,18,0,26)});
        var actions=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
        if(confirm)actions.Children.Add(new Button{Content="Cancelar",IsCancel=true});
        var accept=new Button{Content=confirm?"Confirmar":"Entendido",IsDefault=true,Background=new SolidColorBrush(Color.FromRgb(99,75,124))};
        accept.Click+=(_,_)=>window.DialogResult=true;actions.Children.Add(accept);panel.Children.Add(actions);
        window.Content=new Border{CornerRadius=new CornerRadius(20),BorderBrush=GlassPanel.Gradient(("#C9D9C4E9",0),("#48413F49",1)),BorderThickness=new Thickness(1),Child=panel};
        window.Loaded+=(_,_)=>Motion.Enter(panel);
        window.PreviewKeyDown+=(_,e)=>{if(e.Key==System.Windows.Input.Key.Escape)window.DialogResult=false;};
        return window.ShowDialog()==true;
    }
}
