using System.Windows;
using System.Windows.Media;
using SharpVectors.Converters;

namespace PersonalFinance.Views;

public sealed record CategoryIconChoice(string Key,string Name);

public static class CategoryIcons
{
    public static IReadOnlyList<CategoryIconChoice> Choices { get; } =
    [
        new("basket","Compras"), new("cup-hot","Comida"),
        new("house","Hogar"), new("capsule","Salud"),
        new("car-front","Transporte"), new("arrow-repeat","Suscripciones"),
        new("controller","Entretenimiento"), new("bag","Tienda"),
        new("book","Educación"), new("airplane","Viajes"),
        new("receipt","Servicios"), new("briefcase","Trabajo"),
        new("gift","Regalos"), new("piggy-bank","Ahorro"),
        new("stars","Otro"), new("cash","Dinero")
    ];

    private static readonly Dictionary<string,string> Legacy = new()
    {
        ["🛒"]="basket", ["🍽"]="cup-hot", ["🏠"]="house", ["💊"]="capsule",
        ["🚗"]="car-front", ["🔁"]="arrow-repeat", ["🎮"]="controller", ["🛍"]="bag",
        ["📚"]="book", ["✈"]="airplane", ["🧾"]="receipt", ["💼"]="briefcase",
        ["🎁"]="gift", ["🐷"]="piggy-bank", ["✨"]="stars", ["💸"]="cash"
    };

    public static string Normalize(string? icon)
    {
        if(icon!=null && Choices.Any(x=>x.Key==icon))return icon;
        return icon!=null && Legacy.TryGetValue(icon,out var key)?key:"stars";
    }

    public static FrameworkElement View(string icon,double size=24)
    {
        var key=Normalize(icon);
        var image=new SvgIcon
        {
            UriSource=new Uri($"pack://application:,,,/PersonalFinance;component/Assets/BootstrapIcons/{key}.svg",UriKind.Absolute),
            Width=size,Height=size,Stretch=Stretch.Uniform,SnapsToDevicePixels=true
        };
        image.SetResourceReference(SvgIcon.FillProperty,"TextPrimary");
        return image;
    }
}
