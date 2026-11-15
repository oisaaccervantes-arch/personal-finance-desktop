using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PersonalFinance.ViewModels;
using PersonalFinance.Views;

namespace PersonalFinance;
public partial class MainWindow
{
    private void StartAmbient()
    {
        if(!SystemParameters.ClientAreaAnimation||AmbientDrift.HasAnimatedProperties)return;
        AmbientDrift.BeginAnimation(TranslateTransform.XProperty,new DoubleAnimation(-24,24,TimeSpan.FromSeconds(19)){AutoReverse=true,RepeatBehavior=RepeatBehavior.Forever,EasingFunction=new SineEase{EasingMode=EasingMode.EaseInOut}});
        AmbientDrift.BeginAnimation(TranslateTransform.YProperty,new DoubleAnimation(15,-18,TimeSpan.FromSeconds(23)){AutoReverse=true,RepeatBehavior=RepeatBehavior.Forever,EasingFunction=new SineEase{EasingMode=EasingMode.EaseInOut}});
    }
    private void StopAmbient()
    {
        AmbientDrift.BeginAnimation(TranslateTransform.XProperty,null);
        AmbientDrift.BeginAnimation(TranslateTransform.YProperty,null);
    }
    private void DragTitle(object sender,MouseButtonEventArgs e)
    {
        if(e.ClickCount==2)MaximizeWindow(sender,e);
        else if(e.ButtonState==MouseButtonState.Pressed)DragMove();
    }
    private void MinimizeWindow(object sender,RoutedEventArgs e)=>WindowState=WindowState.Minimized;
    private void MaximizeWindow(object sender,RoutedEventArgs e)=>WindowState=WindowState==WindowState.Maximized?WindowState.Normal:WindowState.Maximized;
    private void CloseWindow(object sender,RoutedEventArgs e)=>Close();
    private void OpenQuickMovement(object sender,RoutedEventArgs e)
    {
        var quick=new QuickMovementViewModel(vm.Accounts,vm.Categories);
        quick.Closed+=CloseQuick;
        quick.AccountsRequested+=()=>{CloseQuick();vm.Navigate.Execute("Cuentas");};
        quick.Saved+=()=>{CloseQuick();vm.Reload();Status.Text="✓ Movimiento guardado. Los saldos ya están actualizados.";};
        var view=new QuickMovementView{DataContext=quick};
        QuickHost.Content=view;QuickOverlay.Visibility=Visibility.Visible;
        Workspace.IsEnabled=false;SidebarShell.IsEnabled=false;QuickAddButton.IsEnabled=false;
        QuickHost.MaxHeight=Math.Max(280,ActualHeight-105);
        Motion.Enter(QuickHost,22);
        Dispatcher.BeginInvoke(()=>view.MoveFocus(new TraversalRequest(FocusNavigationDirection.First)));
    }
    private void CloseQuick()
    {
        QuickOverlay.Visibility=Visibility.Collapsed;QuickHost.Content=null;
        Workspace.IsEnabled=true;SidebarShell.IsEnabled=true;QuickAddButton.IsEnabled=true;QuickAddButton.Focus();
    }
    private void QuickOverlayKeyDown(object sender,KeyEventArgs e)
    {
        if(e.Key==Key.Escape){CloseQuick();e.Handled=true;}
    }
}
