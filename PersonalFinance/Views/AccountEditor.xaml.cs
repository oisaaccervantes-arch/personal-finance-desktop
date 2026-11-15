using System.Windows;
using System.Windows.Input;
using PersonalFinance.ViewModels;
namespace PersonalFinance.Views;
public partial class AccountEditor : Window
{
    public AccountEditor(Window owner,AccountEditorViewModel vm)
    {
        InitializeComponent();Owner=owner;Resources=owner.Resources;DataContext=vm;Height=Math.Min(710,SystemParameters.WorkArea.Height-50);
        vm.Saved+=()=>DialogResult=true;vm.Cancelled+=()=>DialogResult=false;
        Loaded+=(_,_)=>Motion.Enter((UIElement)Content);
    }
    private void DragHeader(object sender,MouseButtonEventArgs e){if(e.ButtonState==MouseButtonState.Pressed)DragMove();}
}
