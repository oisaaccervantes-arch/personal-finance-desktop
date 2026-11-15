using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace PersonalFinance.Views;

/// <summary>Lightweight optical layers over the shared, cached soft-focus backdrop.</summary>
public sealed class GlassPanel : Border
{
    private readonly Grid layers = new();
    public GlassPanel(UIElement content)
    {
        CornerRadius=new CornerRadius(24);
        Margin=new Thickness(0,0,0,20);
        Background=Brushes.Transparent;
        // The background is blurred and cached once by the window. Each panel only
        // alpha-composites frozen brushes; scrolling never resamples a live VisualBrush.
        var surface=new Border{IsHitTestVisible=false};surface.SetResourceReference(BackgroundProperty,"PanelFill");layers.Children.Add(surface);
        layers.Children.Add(new Border { Background=Gradient(("#62FFFFFF",0),("#1CFFFFFF",.18),("#00FFFFFF",.46),("#08FFFFFF",.8),("#25E5D9FF",1)), IsHitTestVisible=false });
        layers.Children.Add(new Border { Background=new RadialGradientBrush { Center=new Point(.45,1.08), GradientOrigin=new Point(.45,1.08), RadiusX=.65, RadiusY=.19, GradientStops=new GradientStopCollection { new(Color.FromArgb(60,240,230,255),0),new(Color.FromArgb(12,240,230,255),.55),new(Colors.Transparent,1) } }, IsHitTestVisible=false });
        layers.Children.Add(new Border { CornerRadius=new CornerRadius(23), BorderThickness=new Thickness(1.2), BorderBrush=Gradient(("#EEFFFFFF",0),("#55FFFFFF",.22),("#14FFFFFF",.5),("#38D5C8EB",.73),("#BDFFFFFF",1)), IsHitTestVisible=false });
        layers.Children.Add(new Border { Margin=new Thickness(3), CornerRadius=new CornerRadius(20), BorderThickness=new Thickness(1), BorderBrush=Gradient(("#68FFFFFF",0),("#00FFFFFF",.25),("#00FFFFFF",.65),("#70FFFFFF",1)), IsHitTestVisible=false });
        layers.Children.Add(new Border { Padding=new Thickness(24), Child=content });
        Child=layers;
        SizeChanged+=(_,_)=>layers.Clip=new RectangleGeometry(new Rect(RenderSize),24,24);
    }
    public static LinearGradientBrush Gradient(params (string Color,double Offset)[] stops)
    {
        var brush=new LinearGradientBrush { StartPoint=new Point(0,0),EndPoint=new Point(1,1) };
        foreach(var stop in stops) brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(stop.Color),stop.Offset));
        brush.Freeze();return brush;
    }
}
