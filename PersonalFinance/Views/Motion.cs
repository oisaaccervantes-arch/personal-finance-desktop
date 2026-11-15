using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace PersonalFinance.Views;
internal static class Motion
{
    public static void Enter(UIElement element,double distance=12)
    {
        if(!SystemParameters.ClientAreaAnimation)return;
        var transform=new TranslateTransform();element.RenderTransform=transform;
        var easing=new CubicEase {EasingMode=EasingMode.EaseOut};
        transform.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(distance,0,TimeSpan.FromMilliseconds(240)){EasingFunction=easing});
        element.BeginAnimation(UIElement.OpacityProperty,new DoubleAnimation(0,1,TimeSpan.FromMilliseconds(200)));
    }
}
