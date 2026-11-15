using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace PersonalFinance.Views;

public sealed class CategoryIconPicker : UserControl
{
    private readonly Dictionary<string,Border> tiles=[];
    public string SelectedKey {get;private set;}

    public CategoryIconPicker(string selected)
    {
        SelectedKey=CategoryIcons.Normalize(selected);
        var grid=new UniformGrid{Columns=8};
        foreach(var choice in CategoryIcons.Choices)
        {
            var glyph=CategoryIcons.View(choice.Key,26);glyph.HorizontalAlignment=HorizontalAlignment.Center;glyph.VerticalAlignment=VerticalAlignment.Center;
            var tile=new Border{Width=43,Height=43,Margin=new Thickness(0,0,8,8),CornerRadius=new CornerRadius(11),BorderThickness=new Thickness(1),Cursor=Cursors.Hand,Focusable=true,ToolTip=choice.Name,Child=glyph};
            tile.MouseLeftButtonUp+=(_,_)=>Select(choice.Key);
            tile.KeyDown+=(_,e)=>{if(e.Key is Key.Enter or Key.Space){Select(choice.Key);e.Handled=true;}};
            tile.MouseEnter+=(_,_)=>Paint(tile,true);
            tile.MouseLeave+=(_,_)=>Paint(tile,false);
            tiles.Add(choice.Key,tile);grid.Children.Add(tile);
        }
        Content=grid;Select(SelectedKey);
    }

    private void Select(string key)
    {
        SelectedKey=key;
        foreach(var tile in tiles.Values)Paint(tile,false);
    }

    private void Paint(Border tile,bool hover)
    {
        bool selected=tiles.TryGetValue(SelectedKey,out var active)&&ReferenceEquals(tile,active);
        tile.Background=selected?new SolidColorBrush(Color.FromArgb(130,66,100,168)):hover?new SolidColorBrush(Color.FromArgb(70,103,117,147)):new SolidColorBrush(Color.FromArgb(42,38,46,63));
        tile.BorderBrush=selected?new SolidColorBrush(Color.FromRgb(130,171,245)):hover?new SolidColorBrush(Color.FromArgb(150,168,183,218)):new SolidColorBrush(Color.FromArgb(70,164,180,207));
    }
}
