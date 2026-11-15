using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
namespace PersonalFinance.Views;
public sealed class ColorPicker : UserControl
{
    public static readonly DependencyProperty ValueProperty=DependencyProperty.Register(nameof(Value),typeof(string),typeof(ColorPicker),new FrameworkPropertyMetadata("#625081",FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public string Value {get=>(string)GetValue(ValueProperty);set=>SetValue(ValueProperty,value);}
    public ColorPicker()
    {
        var panel=new StackPanel();var palette=new WrapPanel{Margin=new Thickness(0,0,0,8)};
        foreach(var hex in new[]{"#625081","#2457A6","#008A94","#26855E","#AD446D","#BD662B","#202837","#7E8490","#E9E0CE","#FFFFFF","#7240A8","#C03D43"})
        {
            var button=new Button{Width=28,Height=28,Padding=new Thickness(0),Margin=new Thickness(0,0,6,6),Background=new SolidColorBrush(Services.ThemeService.Parse(hex)),ToolTip=hex};
            button.Click+=(_,_)=>SetCurrentValue(ValueProperty,hex);palette.Children.Add(button);
        }
        panel.Children.Add(palette);
        var input=new TextBox{MaxLength=7,ToolTip="Color hexadecimal #RRGGBB"};input.SetBinding(TextBox.TextProperty,new Binding(nameof(Value)){Source=this,Mode=BindingMode.TwoWay,UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged});panel.Children.Add(input);Content=panel;
    }
}
