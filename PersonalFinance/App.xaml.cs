using System.Configuration;
using System.Data;
using System.Windows;

namespace PersonalFinance;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try { Services.ThemeService.Apply(new Services.FinanceStore().Appearance()); MainWindow=new MainWindow(); MainWindow.Show(); }
        catch(Exception ex)
        {
            MessageBox.Show("No se pudo abrir Personal Finance. Comprueba el acceso a la carpeta de datos. No se han eliminado tus archivos.\n\n"+ex.Message,"Personal Finance",MessageBoxButton.OK,MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}

