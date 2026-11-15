using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PersonalFinance.Views;

public sealed record CategoryIconChoice(string Emoji,string Name,string File);

public static class CategoryIcons
{
    public static IReadOnlyList<CategoryIconChoice> Choices { get; } =
    [
        new("🛒","Compras","1f6d2"), new("🍽","Comida","1f37d"),
        new("🏠","Hogar","1f3e0"), new("💊","Salud","1f48a"),
        new("🚗","Transporte","1f697"), new("🔁","Suscripciones","1f501"),
        new("🎮","Entretenimiento","1f3ae"), new("🛍","Tienda","1f6cd"),
        new("📚","Educación","1f4da"), new("✈","Viajes","2708"),
        new("🧾","Servicios","1f9fe"), new("💼","Trabajo","1f4bc"),
        new("🎁","Regalos","1f381"), new("🐷","Ahorro","1f437"),
        new("✨","Otro","2728"), new("💸","Dinero","1f4b8")
    ];

    private static readonly Dictionary<string,BitmapImage> Images=Choices.ToDictionary(x=>x.Emoji,x=>Load(x.File));

    private static BitmapImage Load(string file)
    {
        var image=new BitmapImage();image.BeginInit();image.UriSource=new Uri($"pack://application:,,,/PersonalFinance;component/Assets/Twemoji/{file}.png",UriKind.Absolute);image.CacheOption=BitmapCacheOption.OnLoad;image.EndInit();image.Freeze();return image;
    }

    public static FrameworkElement View(string icon,double size=24)
    {
        if(Images.TryGetValue(icon,out var source))return new Image{Source=source,Width=size,Height=size,Stretch=Stretch.Uniform,SnapsToDevicePixels=true};
        return new TextBlock{Text=icon,FontSize=size,VerticalAlignment=VerticalAlignment.Center,Foreground=new SolidColorBrush(Color.FromRgb(180,195,215))};
    }
}
