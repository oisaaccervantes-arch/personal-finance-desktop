using System.IO;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using PersonalFinance.Models;
using PersonalFinance.ViewModels;

namespace PersonalFinance;
public partial class MainWindow : Window
{
    private readonly MainViewModel vm = new();
    private readonly string[] pages=["Dashboard","Movimientos","Cuentas","Categorías","Presupuestos","Recurrentes","Reportes","Configuración"];
    private readonly string[] icons=["▦","↕","▱","◇","◎","↻","↗","⚙"];
    private string search="", type="Todos";
    private Category? category;
    private DateTime? from, to;
    private bool advancedAppearanceOpen;
    public MainWindow()
    {
        InitializeComponent(); Width=Math.Min(1440,SystemParameters.WorkArea.Width-40); Height=Math.Min(920,SystemParameters.WorkArea.Height-40); DataContext=vm;
        Loaded+=(_,_)=>StartAmbient();
        SizeChanged+=(_,_)=>QuickHost.MaxHeight=Math.Max(280,ActualHeight-105);
        StateChanged+=(_,_)=>{if(WindowState==WindowState.Minimized)StopAmbient();else StartAmbient();if(WindowState==WindowState.Maximized){MaxHeight=SystemParameters.WorkArea.Height;MaxWidth=SystemParameters.WorkArea.Width;}else{MaxHeight=double.PositiveInfinity;MaxWidth=double.PositiveInfinity;}};
        vm.Refreshed+=Render;
        for(int i=0;i<pages.Length;i++)
        {
            var button=new Button {Content=$"{icons[i]}   {pages[i]}",Command=vm.Navigate,CommandParameter=pages[i],HorizontalContentAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,0,6),Padding=new Thickness(18,12,18,12)};
            Navigation.Children.Add(button);
        }
        Render();
    }
    private static Brush Brush(string hex)=>(Brush)new BrushConverter().ConvertFromString(hex)!;
    private static TextBlock Text(string value,double size=14,string color="#F5F6F8")
    {
        var text=new TextBlock{Text=value,FontSize=size,TextWrapping=TextWrapping.Wrap};
        if(color is "#F5F6F8" or "#FFB4AB" or "#9EA5B1") text.SetResourceReference(TextBlock.ForegroundProperty,color=="#F5F6F8"?"TextPrimary":color=="#FFB4AB"?"ErrorText":"TextSecondary");
        else text.Foreground=Brush(color);
        return text;
    }
    private Border Card(UIElement child)=>new Views.GlassPanel(child);
    private Button Action(string label,Action action,bool primary=false)
    {
        var b=new Button {Content=label,HorizontalAlignment=HorizontalAlignment.Left,HorizontalContentAlignment=HorizontalAlignment.Center}; if(primary){b.SetResourceReference(BackgroundProperty,"AccentFill");b.SetResourceReference(ForegroundProperty,"AccentInk");}
        b.Click+=(_,_)=>Safe(action); return b;
    }
    private void Safe(Action action)
    {
        try {action();} catch(Exception ex) {Views.GlassDialog.Show(this,ex is Microsoft.EntityFrameworkCore.DbUpdateException ? "No se pudo guardar. Revisa que no exista un presupuesto para esa categoría y mes, o que el registro no esté en uso." : ex.Message,"Personal Finance");}
    }
    private void Render()
    {
        bool changed=PageTitle.Text!=vm.Page;
        ContentPanel.Children.Clear(); PageTitle.Text=vm.Page; SidebarBalance.Text=MainViewModel.Money(vm.Balance);
        PageSubtitle.Text=vm.Page switch {"Dashboard"=>"Una perspectiva completa de tus finanzas.","Movimientos"=>"Cada ingreso y cada gasto, en su lugar.","Cuentas"=>"Tu patrimonio, organizado por cuenta.","Presupuestos"=>"Dale un propósito a cada peso.","Recurrentes"=>"Cobros, pagos y saldos futuros en un solo lugar.","Reportes"=>"Entiende cómo cambia tu dinero.","Categorías"=>"Organiza tus movimientos a tu manera.",_=>"Tus datos siempre están bajo tu control."};
        foreach(Button b in Navigation.Children)
        {
            bool active=(string)b.CommandParameter==vm.Page;
            b.Tag=active?"NavActive":"NavIdle";
            b.Background=active?(Brush)FindResource("Selected"):Brushes.Transparent;
            b.SetResourceReference(ForegroundProperty,active?"TextPrimary":"TextSecondary");
            b.BorderBrush=active?(Brush)FindResource("ButtonEdge"):Brushes.Transparent;
            b.FontWeight=active?FontWeights.SemiBold:FontWeights.Normal;
        }
        switch(vm.Page){case "Dashboard":Dashboard();break;case "Movimientos":Movements();break;case "Cuentas":Accounts();break;case "Categorías":Categories();break;case "Presupuestos":Budgets();break;case "Recurrentes":Recurring();break;case "Reportes":Reports();break;default:Settings();break;}
        if(changed){PageScroll.ScrollToTop();Views.Motion.Enter(ContentPanel);}
    }
    private void Metrics()=>ContentPanel.Children.Add(MetricGrid());
    private UIElement MetricGrid(int columns=4)
    {
        var grid=new System.Windows.Controls.Primitives.UniformGrid{Columns=columns};
        foreach(var (label,value,note) in new[]{("BALANCE TOTAL",vm.Balance,"Todas tus cuentas"),("INGRESOS DEL MES",vm.Income,"Entradas registradas"),("GASTOS DEL MES",vm.Expense,"Salidas registradas"),("AHORRO DEL MES",vm.Income-vm.Expense,"Ingresos menos gastos")})
        {
            var p=new StackPanel();p.Children.Add(Text(label,10,"#9EA5B1")); var amount=Text(MainViewModel.Money(value),28);amount.FontWeight=FontWeights.SemiBold;amount.Margin=new Thickness(0,20,0,16);p.Children.Add(amount);p.Children.Add(Text(note,11,"#9EA5B1"));var c=Card(p);c.Margin=new Thickness(0,0,12,20);grid.Children.Add(c);
        }
        return grid;
    }
    private void Dashboard()
    {
        var hero=new Grid();
        hero.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        hero.ColumnDefinitions.Add(new(){Width=new GridLength(1.5,GridUnitType.Star)});
        var wallet=new Views.WalletCard();
        var walletPanel=Card(wallet);walletPanel.Margin=new Thickness(0,0,20,20);hero.Children.Add(walletPanel);
        var metrics=MetricGrid(2);Grid.SetColumn(metrics,1);hero.Children.Add(metrics);
        ContentPanel.Children.Add(hero);
        var grid=new Grid();grid.ColumnDefinitions.Add(new(){Width=new GridLength(1.65,GridUnitType.Star)});grid.ColumnDefinitions.Add(new(){Width=new GridLength(1,GridUnitType.Star)});
        var left=new StackPanel();left.Children.Add(Text("Evolución de ingresos y gastos",20));left.Children.Add(Text(Services.ThemeService.IsLight?"Últimos seis meses  ·  violeta: ingresos  /  gris: gastos":"Últimos seis meses  ·  blanco: ingresos  /  gris: gastos",11,"#9EA5B1"));left.Children.Add(Chart());var lc=Card(left);lc.Margin=new Thickness(0,0,20,20);grid.Children.Add(lc);
        var right=new StackPanel();right.Children.Add(Text("Distribución de gastos",20));right.Children.Add(Text(vm.MonthLabel,11,"#9EA5B1"));
        var groups=vm.Current.Where(x=>x.Category.Type=="Gasto").GroupBy(x=>x.Category).OrderByDescending(g=>g.Sum(x=>x.Cents)).ToList();
        if(groups.Count==0) {var empty=Text("Aún no hay gastos este mes.\nRegistra un movimiento para ver su distribución.",14,"#9EA5B1");empty.Margin=new Thickness(0,70,0,60);right.Children.Add(empty);}
        foreach(var g in groups.Take(5)){var p=new StackPanel{Margin=new Thickness(0,20,0,0)};var label=new StackPanel{Orientation=Orientation.Horizontal};label.Children.Add(Views.CategoryIcons.View(g.Key.Icon,19));var title=Text($"  {g.Key.Name}     {MainViewModel.Money(g.Sum(x=>x.Cents))}",13);title.VerticalAlignment=VerticalAlignment.Center;label.Children.Add(title);p.Children.Add(label);p.Children.Add(new ProgressBar{Value=100d*g.Sum(x=>x.Cents)/Math.Max(1,vm.Expense),Height=5,Margin=new Thickness(0,10,0,0),Foreground=Brush(g.Key.Color),Background=Brush("#30343B")});right.Children.Add(p);}
        var rc=Card(right);Grid.SetColumn(rc,1);grid.Children.Add(rc);ContentPanel.Children.Add(grid);
        var bar=new DockPanel{Margin=new Thickness(0,0,0,14)};var all=Action("Ver movimientos →",()=>vm.Navigate.Execute("Movimientos"));DockPanel.SetDock(all,Dock.Right);bar.Children.Add(all);bar.Children.Add(Text("Últimos movimientos",20));ContentPanel.Children.Add(bar);
        if(vm.Movements.Count==0) Empty("Tu historia financiera empieza aquí", "Registra un ingreso o un gasto para comenzar.","Registrar movimiento",()=>OpenQuickMovement(this,new RoutedEventArgs()));
        else ContentPanel.Children.Add(Card(MovementGrid(vm.Movements.Take(6))));
    }
    private UIElement Chart()
    {
        var canvas=new Canvas{Height=245,Margin=new Thickness(0,20,0,0)};
        canvas.SizeChanged+=(_,_)=>{
            canvas.Children.Clear();double w=Math.Max(300,canvas.ActualWidth),h=190;
            var months=Enumerable.Range(-5,6).Select(i=>vm.Month.AddMonths(i)).ToArray();
            var income=months.Select(m=>vm.Movements.Where(x=>x.Date>=m&&x.Date<m.AddMonths(1)&&x.Category.Type=="Ingreso").Sum(x=>x.Cents)).ToArray();
            var expense=months.Select(m=>vm.Movements.Where(x=>x.Date>=m&&x.Date<m.AddMonths(1)&&x.Category.Type=="Gasto").Sum(x=>x.Cents)).ToArray();
            double max=Math.Max(100,Math.Max(income.Max(),expense.Max()));
            for(int i=0;i<4;i++){var l=new Line{X1=0,X2=w,Y1=i*h/3,Y2=i*h/3,Stroke=Brush("#30343B"),StrokeDashArray=new DoubleCollection{3,5}};canvas.Children.Add(l);}
            for(int i=0;i<6;i++){double x=i*w/6;foreach(var (value,offset,color) in new[]{(income[i],12d,Services.ThemeService.IsLight?"#5E437D":"#F1F3F6"),(expense[i],35d,"#737D8C")}){double height=value/max*(h-10);var r=new Rectangle{Width=17,Height=Math.Max(1,height),RadiusX=4,RadiusY=4,Fill=new LinearGradientBrush(((SolidColorBrush)Brush(color)).Color,Color.FromRgb(40,45,52),90)};Canvas.SetLeft(r,x+offset);Canvas.SetTop(r,h-r.Height);canvas.Children.Add(r);}var label=Text(months[i].ToString("MMM",CultureInfo.GetCultureInfo("es-MX")),11,"#9EA5B1");Canvas.SetLeft(label,x+15);Canvas.SetTop(label,210);canvas.Children.Add(label);}
        };return canvas;
    }
    private void Empty(string title,string detail,string action,Action callback)
    {
        var p=new StackPanel{Margin=new Thickness(20),HorizontalAlignment=HorizontalAlignment.Center};p.Children.Add(Text(title,22));var t=Text(detail,14,"#9EA5B1");t.Margin=new Thickness(0,12,0,22);p.Children.Add(t);p.Children.Add(Action(action,callback,true));var panel=Card(p);panel.Margin=new Thickness(0,20,0,0);ContentPanel.Children.Add(panel);
    }
    private DataGrid MovementGrid(IEnumerable<Movement> items)
    {
        var grid=new DataGrid{MinHeight=100,MaxHeight=540,MinWidth=820,ItemsSource=items.Select(x=>new MovementRow(x.Id,x.Date.ToString("dd MMM yyyy",CultureInfo.GetCultureInfo("es-MX")),x.Description,x.Category.Name,x.Account.Name,x.Category.Type,MainViewModel.Money(MainViewModel.Signed(x)))).ToList()};
        var normal=new Style(typeof(TextBlock));normal.Setters.Add(new Setter(TextBlock.ForegroundProperty,FindResource("TextPrimary")));normal.Setters.Add(new Setter(TextBlock.TextTrimmingProperty,TextTrimming.CharacterEllipsis));
        var muted=new Style(typeof(TextBlock));muted.Setters.Add(new Setter(TextBlock.ForegroundProperty,FindResource("TextSecondary")));muted.Setters.Add(new Setter(TextBlock.TextTrimmingProperty,TextTrimming.CharacterEllipsis));
        var strong=new Style(typeof(TextBlock));strong.Setters.Add(new Setter(TextBlock.ForegroundProperty,FindResource("TextPrimary")));strong.Setters.Add(new Setter(TextBlock.FontWeightProperty,FontWeights.SemiBold));strong.Setters.Add(new Setter(TextBlock.TextTrimmingProperty,TextTrimming.CharacterEllipsis));
        foreach(var (header,path,width,style) in new[]{("FECHA","Date",1.1d,muted),("DESCRIPCIÓN","Description",2.2d,strong),("CATEGORÍA","Category",1.3d,normal),("CUENTA","Account",1.3d,normal),("TIPO","Type",.9d,muted)})
            grid.Columns.Add(new DataGridTextColumn{Header=header,Binding=new System.Windows.Data.Binding(path),Width=new DataGridLength(width,DataGridLengthUnitType.Star),ElementStyle=style});
        var amount=new Style(typeof(TextBlock));amount.Setters.Add(new Setter(TextBlock.TextAlignmentProperty,TextAlignment.Right));amount.Setters.Add(new Setter(TextBlock.FontWeightProperty,FontWeights.SemiBold));amount.Setters.Add(new Setter(TextBlock.ForegroundProperty,FindResource("TextPrimary")));
        amount.Triggers.Add(new DataTrigger{Binding=new System.Windows.Data.Binding("Type"),Value="Ingreso",Setters={new Setter(TextBlock.ForegroundProperty,FindResource("IncomeText"))}});
        amount.Triggers.Add(new DataTrigger{Binding=new System.Windows.Data.Binding("Type"),Value="Gasto",Setters={new Setter(TextBlock.ForegroundProperty,FindResource("ExpenseText"))}});
        grid.Columns.Add(new DataGridTextColumn{Header="IMPORTE",Binding=new System.Windows.Data.Binding("Amount"),Width=new DataGridLength(1.15,DataGridLengthUnitType.Star),ElementStyle=amount});
        return grid;
    }
    private IEnumerable<Movement> Filtered()=>vm.Movements.Where(x=>(from==null||x.Date>=from)&&(to==null||x.Date<to.Value.AddDays(1))&&(type=="Todos"||x.Category.Type==type)&&(category==null||x.CategoryId==category.Id)&&(x.Description.Contains(search,StringComparison.OrdinalIgnoreCase)||x.Account.Name.Contains(search,StringComparison.OrdinalIgnoreCase)));
    private static UIElement Labeled(string label, UIElement control) { var p=new StackPanel(); p.Children.Add(Text(label,11,"#9EA5B1"));p.Children.Add(control);return p; }
    private void Movements()
    {
        var filters=new WrapPanel{Margin=new Thickness(0,0,0,18)};
        var query=new TextBox{Text=search,Width=220,ToolTip="Buscar descripción o cuenta",Margin=new Thickness(0,0,10,8)};query.TextChanged+=(_,_)=>search=query.Text;filters.Children.Add(Labeled("Buscar descripción o cuenta",query));
        var types=new ComboBox{ItemsSource=new[]{"Todos","Ingreso","Gasto"},SelectedItem=type,Width=110,Margin=new Thickness(0,0,10,8)};types.SelectionChanged+=(_,_)=>type=(string)types.SelectedItem;filters.Children.Add(Labeled("Tipo",types));
        var cats=new ComboBox{ItemsSource=new[]{new Category{Id=0,Name="Todas las categorías"}}.Concat(vm.Categories),SelectedIndex=0,Width=180,Margin=new Thickness(0,0,10,8)};if(category!=null)cats.SelectedItem=vm.Categories.FirstOrDefault(c=>c.Id==category.Id);cats.SelectionChanged+=(_,_)=>category=cats.SelectedItem is Category c&&c.Id!=0?c:null;filters.Children.Add(Labeled("Categoría",cats));
        var start=new DatePicker{SelectedDate=from,Width=135,ToolTip="Desde",Margin=new Thickness(0,0,10,8)};start.SelectedDateChanged+=(_,_)=>from=start.SelectedDate;filters.Children.Add(Labeled("Desde",start));
        var end=new DatePicker{SelectedDate=to,Width=135,ToolTip="Hasta",Margin=new Thickness(0,0,10,8)};end.SelectedDateChanged+=(_,_)=>to=end.SelectedDate;filters.Children.Add(Labeled("Hasta",end));
        filters.Children.Add(Action("Aplicar",()=>{if(from>to)throw new InvalidOperationException("La fecha inicial debe ser anterior a la final.");Render();}));filters.Children.Add(Action("Limpiar",()=>{search="";type="Todos";category=null;from=to=null;Render();}));ContentPanel.Children.Add(filters);
        var rows=Filtered().ToList();var grid=MovementGrid(rows);
        if(rows.Count==0)
        {
            var empty=new StackPanel{Margin=new Thickness(30,20,30,20),HorizontalAlignment=HorizontalAlignment.Center};
            var mark=Text("◇",32,"#9EA5B1");mark.HorizontalAlignment=HorizontalAlignment.Center;empty.Children.Add(mark);
            var title=Text("No hay movimientos para mostrar",20);title.HorizontalAlignment=HorizontalAlignment.Center;title.Margin=new Thickness(0,10,0,5);empty.Children.Add(title);
            var hint=Text(vm.Movements.Count==0?"Registra tu primer ingreso o gasto con el botón de abajo.":"Prueba con otros filtros o limpia la búsqueda.",13,"#9EA5B1");hint.HorizontalAlignment=HorizontalAlignment.Center;empty.Children.Add(hint);ContentPanel.Children.Add(Card(empty));
        }
        else ContentPanel.Children.Add(Card(grid));
        var bar=new WrapPanel();var edit=Action("Editar selección",()=>{if(grid.SelectedItem is MovementRow r)EditMovement(vm.Movements.First(x=>x.Id==r.Id));});var remove=Action("Eliminar selección",()=>{if(grid.SelectedItem is MovementRow r)Delete<Movement>(r.Id);});
        edit.IsEnabled=false;remove.IsEnabled=false;grid.SelectionChanged+=(_,_)=>{edit.IsEnabled=grid.SelectedItem is MovementRow;remove.IsEnabled=edit.IsEnabled;};
        bar.Children.Add(edit);bar.Children.Add(remove);bar.Children.Add(Action("Exportar CSV",()=>Export(rows)));ContentPanel.Children.Add(bar);ContentPanel.Children.Add(Text($"{rows.Count} movimientos · Total neto: {MainViewModel.Money(rows.Sum(MainViewModel.Signed))}",12,"#9EA5B1"));
    }
    private void Accounts()
    {
        var heading=new DockPanel{Margin=new Thickness(0,0,0,22)};
        var add=Action("＋ Nueva cuenta o tarjeta",()=>EditAccount(null),true);DockPanel.SetDock(add,Dock.Right);heading.Children.Add(add);
        heading.Children.Add(Text($"{vm.Accounts.Count} cuentas · {MainViewModel.Money(vm.Balance)} en total",18));ContentPanel.Children.Add(heading);
        if(vm.Accounts.Count==0){Empty("Diseña tu primera tarjeta","Elige banco, texto, color y saldo inicial. Podrás editarlo cuando quieras.","Crear cuenta",()=>EditAccount(null));return;}
        var wrap=new WrapPanel();
        foreach(var a in vm.Accounts)
        {
            var p=new StackPanel{Width=310};
            if(a.Type=="Efectivo")p.Children.Add(CashFace(a));
            else if(a.Type=="Cuenta de ahorro")
            {
                var summary=new StackPanel{VerticalAlignment=VerticalAlignment.Center};
                summary.Children.Add(Text("◈  AHORRO",12,"#9EA5B1"));
                var amount=Text(MainViewModel.Money(vm.AccountBalance(a)),32);amount.Margin=new Thickness(0,24,0,16);summary.Children.Add(amount);
                summary.Children.Add(Text("Saldo actual · MXN",12,"#9EA5B1"));
                p.Children.Add(new Border{Height=194,Child=summary});
            }
            else p.Children.Add(new Views.CardFace{DataContext=new CardPresentation(a,vm.AccountBalance(a))});
            var title=Text(a.Name,21);title.Margin=new Thickness(0,18,0,6);p.Children.Add(title);
            p.Children.Add(Text(a.Type,12,"#9EA5B1"));
            var initial=Text($"Saldo inicial: {MainViewModel.Money(a.OpeningCents)}",12,"#9EA5B1");initial.Margin=new Thickness(0,10,0,0);p.Children.Add(initial);
            var actions=new WrapPanel{Margin=new Thickness(0,20,0,0)};
            actions.Children.Add(Action("Personalizar",()=>EditAccount(a)));if(a.Type!="Efectivo")actions.Children.Add(Action("Eliminar",()=>Delete<Account>(a.Id)));p.Children.Add(actions);
            var card=Card(p);card.Margin=new Thickness(0,0,20,20);wrap.Children.Add(card);
        }
        ContentPanel.Children.Add(wrap);
        if(vm.Transfers.Count>0){ContentPanel.Children.Add(Text("Movimientos entre tus cuentas",21));foreach(var t in vm.Transfers.Take(8)){var row=new DockPanel{Margin=new Thickness(0,5,0,5)};var amount=Text(MainViewModel.Money(t.Cents),17);DockPanel.SetDock(amount,Dock.Right);row.Children.Add(amount);row.Children.Add(Text($"{t.Date:dd MMM yyyy}   {t.Description}   ·   {t.FromAccount.Name} → {t.ToAccount.Name}",14));ContentPanel.Children.Add(Card(row));}}
    }
    private UIElement CashFace(Account account)
    {
        var face=new Grid{Width=310,Height=194,ClipToBounds=true};
        face.Children.Add(new Border{CornerRadius=new CornerRadius(18),Background=Views.GlassPanel.Gradient(("#FF227B6A",0),("#FF16464E",.55),("#FF162E45",1)),BorderBrush=Brush("#80B6F8DB"),BorderThickness=new Thickness(1)});
        var bill=new Grid{Width=112,Height=66,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(0,18,20,0)};
        bill.Children.Add(new Border{CornerRadius=new CornerRadius(8),BorderBrush=Brush("#BBE8FFE8"),BorderThickness=new Thickness(1.5),Background=Brush("#246DD7A4")});
        bill.Children.Add(new Border{CornerRadius=new CornerRadius(4),BorderBrush=Brush("#77E8FFE8"),BorderThickness=new Thickness(1),Margin=new Thickness(6)});
        bill.Children.Add(new TextBlock{Text="$",FontSize=37,FontWeight=FontWeights.SemiBold,Foreground=Brush("#E2F8E4"),HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center});face.Children.Add(bill);
        var labels=new StackPanel{VerticalAlignment=VerticalAlignment.Bottom,Margin=new Thickness(22,0,22,19)};
        labels.Children.Add(new TextBlock{Text="EFECTIVO  /  BILLETERA",FontSize=11,Foreground=Brush("#D4F6E4"),FontWeight=FontWeights.SemiBold});
        labels.Children.Add(new TextBlock{Text=MainViewModel.Money(vm.AccountBalance(account)),FontSize=31,FontWeight=FontWeights.SemiBold,Foreground=Brush("#FFFFFFFF"),Margin=new Thickness(0,11,0,0)});face.Children.Add(labels);return face;
    }
    private void Categories()
    {
        ContentPanel.Children.Add(Action("＋ Nueva categoría",()=>EditCategory(null),true));var wrap=new WrapPanel{Margin=new Thickness(0,22,0,0)};
        foreach(var c in vm.Categories){var p=new StackPanel{Width=230};var glyph=Views.CategoryIcons.View(c.Icon,32);glyph.HorizontalAlignment=HorizontalAlignment.Left;p.Children.Add(glyph);p.Children.Add(Text(c.Name,20));p.Children.Add(Text(c.Type,12,"#9EA5B1"));var actions=new WrapPanel{Margin=new Thickness(0,20,0,0)};actions.Children.Add(Action("Editar",()=>EditCategory(c)));actions.Children.Add(Action("Eliminar",()=>Delete<Category>(c.Id)));p.Children.Add(actions);var card=Card(p);card.Margin=new Thickness(0,0,18,18);wrap.Children.Add(card);}ContentPanel.Children.Add(wrap);
    }
    private void Budgets()
    {
        ContentPanel.Children.Add(Action("＋ Presupuesto mensual",()=>EditBudget(null),true));
        var budgets=vm.Budgets.Where(x=>x.Month==vm.Month).ToList();if(budgets.Count==0){Empty("Planea antes de gastar","Define límites por categoría para el mes seleccionado.","Crear presupuesto",()=>EditBudget(null));return;}
        foreach(var b in budgets){long spent=vm.Current.Where(x=>x.CategoryId==b.CategoryId).Sum(x=>x.Cents);var p=new StackPanel();var heading=new StackPanel{Orientation=Orientation.Horizontal};heading.Children.Add(Views.CategoryIcons.View(b.Category.Icon,24));var headingText=Text("  "+b.Category.Name,22);headingText.VerticalAlignment=VerticalAlignment.Center;heading.Children.Add(headingText);p.Children.Add(heading);p.Children.Add(Text($"{MainViewModel.Money(spent)} de {MainViewModel.Money(b.LimitCents)}",18));p.Children.Add(new ProgressBar{Value=Math.Min(100,100d*spent/b.LimitCents),Height=7,Margin=new Thickness(0,18,0,14),Foreground=Brush(spent>b.LimitCents?"#FFB4AB":"#D8E0EA"),Background=Brush("#30343B")});p.Children.Add(Text(spent>b.LimitCents?$"Excedido por {MainViewModel.Money(spent-b.LimitCents)}":$"Disponible: {MainViewModel.Money(b.LimitCents-spent)}",13,spent>b.LimitCents?"#FFB4AB":"#9EA5B1"));var bar=new WrapPanel{Margin=new Thickness(0,18,0,0)};bar.Children.Add(Action("Editar",()=>EditBudget(b)));bar.Children.Add(Action("Eliminar",()=>Delete<Budget>(b.Id)));p.Children.Add(bar);var card=Card(p);card.Margin=new Thickness(0,20,0,0);ContentPanel.Children.Add(card);}
    }
    private void Recurring()
    {
        var toolbar=new WrapPanel{Margin=new Thickness(0,0,0,20)};toolbar.Children.Add(Action("＋ Gasto recurrente",()=>EditRecurring(null,"Gasto"),true));toolbar.Children.Add(Action("＋ Ingreso recurrente",()=>EditRecurring(null,"Ingreso"),true));ContentPanel.Children.Add(toolbar);
        var projection=new StackPanel();projection.Children.Add(Text("Proyección de saldo",22));var note=Text("Saldo total estimado al cierre de cada mes. Incluye únicamente recurrencias activas; no anticipa gastos ocasionales.",12,"#9EA5B1");note.Margin=new Thickness(0,7,0,16);projection.Children.Add(note);
        var months=new System.Windows.Controls.Primitives.UniformGrid{Columns=3};
        for(int i=1;i<=6;i++)
        {
            var date=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddMonths(i+1).AddDays(-1);
            var cell=new StackPanel{Margin=new Thickness(0,4,16,14)};cell.Children.Add(Text(date.ToString("MMMM yyyy",CultureInfo.GetCultureInfo("es-MX")),12,"#9EA5B1"));var amount=Text(MainViewModel.Money(vm.ProjectedBalance(date)),23);amount.FontWeight=FontWeights.SemiBold;amount.Margin=new Thickness(0,5,0,0);cell.Children.Add(amount);months.Children.Add(cell);
        }
        projection.Children.Add(months);var projectionCard=Card(projection);projectionCard.Margin=new Thickness(0,0,0,24);ContentPanel.Children.Add(projectionCard);
        ContentPanel.Children.Add(Text("Calendario mensual",21));
        if(vm.RecurringRules.Count==0){var empty=Text("Todavía no hay cobros ni pagos recurrentes. Agrega uno para proyectar tus saldos.",14,"#9EA5B1");empty.Margin=new Thickness(0,18,0,0);ContentPanel.Children.Add(Card(empty));return;}
        foreach(var rule in vm.RecurringRules.OrderBy(x=>x.Active?0:1).ThenBy(x=>x.StartDate.Day))
        {
            var row=new DockPanel();var amount=Text((rule.Type=="Ingreso"?"+":"−")+MainViewModel.Money(rule.Cents),20);amount.FontWeight=FontWeights.SemiBold;amount.SetResourceReference(TextBlock.ForegroundProperty,rule.Type=="Ingreso"?"IncomeText":"ExpenseText");DockPanel.SetDock(amount,Dock.Right);row.Children.Add(amount);
            var detail=new StackPanel();detail.Children.Add(Text(rule.Description,18));detail.Children.Add(Text($"{rule.Type} · {rule.Account.Name} · {rule.Category.Name} · día {rule.StartDate.Day} de cada mes · {(rule.Active?"Activo":"Pausado")}",12,"#9EA5B1"));var actions=new WrapPanel{Margin=new Thickness(0,14,0,0)};actions.Children.Add(Action("Editar próximos",()=>EditRecurring(rule,rule.Type)));actions.Children.Add(Action(rule.Active?"Pausar":"Reanudar",()=>{var changed=new RecurringRule{Id=rule.Id,Description=rule.Description,Cents=rule.Cents,Type=rule.Type,AccountId=rule.AccountId,CategoryId=rule.CategoryId,StartDate=rule.Active?rule.StartDate:DateTime.Today,Active=!rule.Active};vm.SaveRecurring(changed);vm.Reload();}));detail.Children.Add(actions);row.Children.Add(detail);var card=Card(row);card.Margin=new Thickness(0,12,0,0);ContentPanel.Children.Add(card);
        }
        var hint=Text("Cada vencimiento se registra al abrir la aplicación. Un movimiento ya realizado se puede editar desde Movimientos, incluso si se pagó con otra cuenta.",12,"#9EA5B1");hint.Margin=new Thickness(0,20,0,0);ContentPanel.Children.Add(hint);
    }
    private void EditRecurring(RecurringRule? rule,string kind)=>Safe(()=>
    {
        var categories=vm.Categories.Where(x=>x.Type==kind).ToList();
        var f=new Editor(this,rule==null?"Nuevo "+kind.ToLowerInvariant()+" recurrente":"Editar próximos "+kind.ToLowerInvariant()+"s");
        var concept=f.Input("Concepto",rule?.Description??"");var amount=f.Input("Monto mensual · MXN",((rule?.Cents??0)/100m).ToString("F2",CultureInfo.CurrentCulture));
        var date=f.Date("Primer vencimiento",rule?.StartDate??DateTime.Today);var account=f.Select("Cuenta habitual",vm.Accounts,rule?.AccountId??vm.Accounts[0].Id,x=>x.Id);var category=f.Select("Categoría",categories,rule?.CategoryId??categories.FirstOrDefault()?.Id??0,x=>x.Id);
        f.InlineAction("＋  Crear categoría de "+kind.ToLowerInvariant(),()=>
        {
            var created=CreateCategoryFor(f,kind);if(created==null)return;
            categories=vm.Categories.Where(x=>x.Type==kind).ToList();category.ItemsSource=categories;category.SelectedItem=categories.First(x=>x.Id==created.Id);
        });
        f.Save(()=>{if(category.SelectedItem is not Category chosen)throw new InvalidOperationException("Elige o crea una categoría.");var updated=new RecurringRule{Id=rule?.Id??0,Description=Required(concept.Text),Cents=Cents(amount.Text,true),Type=kind,StartDate=date.SelectedDate??throw new InvalidOperationException("Elige la fecha del primer vencimiento."),AccountId=((Account)account.SelectedItem).Id,CategoryId=chosen.Id,Active=true};vm.SaveRecurring(updated);});if(f.ShowDialog()==true)vm.Reload();
    });
    private Category? CreateCategoryFor(Editor owner,string kind)
    {
        Category? created=null;
        var f=new Editor(owner,"Nueva categoría de "+kind.ToLowerInvariant());
        var name=f.Input("Nombre","");var icon=f.PickIcon("Icono","✨");
        var color=f.Select("Color",new[]{"#BAC8D5","#E2E2E6","#9CA3AF","#56FEB7","#B9A5D5","#FFB4AB"},"#BAC8D5",x=>x);
        f.Save(()=>{created=new Category{Name=Required(name.Text),Type=kind,Icon=icon.SelectedEmoji,Color=(string)color.SelectedItem};vm.Save(created,0);});
        if(f.ShowDialog()!=true)return null;
        vm.Reload();return created;
    }
    private void Reports()
    {
        Metrics();var chart=Card(Chart());chart.Margin=new Thickness(0,0,0,22);ContentPanel.Children.Add(chart);
        ContentPanel.Children.Add(Text("Resumen por mes",21));var help=Text("Ingresos, gastos y ahorro de los últimos doce meses.",12,"#9EA5B1");help.Margin=new Thickness(0,7,0,18);ContentPanel.Children.Add(help);
        var entries=Enumerable.Range(-11,12).Reverse().Select(i=>{var month=vm.Month.AddMonths(i);var list=vm.Movements.Where(x=>x.Date>=month&&x.Date<month.AddMonths(1));long income=list.Where(x=>x.Category.Type=="Ingreso").Sum(x=>x.Cents),expense=list.Where(x=>x.Category.Type=="Gasto").Sum(x=>x.Cents);return new{Month=month.ToString("MMMM yyyy",CultureInfo.GetCultureInfo("es-MX")),Income=MainViewModel.Money(income),Expense=MainViewModel.Money(expense),Saving=MainViewModel.Money(income-expense)};}).ToList();
        var grid=new DataGrid{ItemsSource=entries,Height=520,MinWidth=680};
        var period=new Style(typeof(TextBlock));period.Setters.Add(new Setter(TextBlock.FontWeightProperty,FontWeights.SemiBold));period.Setters.Add(new Setter(TextBlock.ForegroundProperty,FindResource("TextPrimary")));
        var money=new Style(typeof(TextBlock));money.Setters.Add(new Setter(TextBlock.TextAlignmentProperty,TextAlignment.Right));money.Setters.Add(new Setter(TextBlock.FontWeightProperty,FontWeights.Medium));money.Setters.Add(new Setter(TextBlock.ForegroundProperty,FindResource("TextPrimary")));
        var incomeStyle=new Style(typeof(TextBlock),money);incomeStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty,FindResource("IncomeText")));
        var expenseStyle=new Style(typeof(TextBlock),money);expenseStyle.Setters.Add(new Setter(TextBlock.ForegroundProperty,FindResource("ExpenseText")));
        foreach(var (header,path,width,style) in new[]{("PERIODO","Month",1.4d,period),("INGRESOS","Income",1d,incomeStyle),("GASTOS","Expense",1d,expenseStyle),("AHORRO","Saving",1d,money)})grid.Columns.Add(new DataGridTextColumn{Header=header,Binding=new System.Windows.Data.Binding(path),Width=new DataGridLength(width,DataGridLengthUnitType.Star),ElementStyle=style});
        var table=Card(grid);table.Margin=new Thickness(0,0,0,22);ContentPanel.Children.Add(table);ContentPanel.Children.Add(Action("Exportar movimientos del mes",()=>Export(vm.Current)));
    }
    private void Settings()
    {
        var appearance=new StackPanel();appearance.Children.Add(Text("Temas",22));
        var help=Text("Selecciona un estilo para ver el cambio al instante. Puedes ajustar cada detalle abajo.",13,"#9EA5B1");help.Margin=new Thickness(0,9,0,20);appearance.Children.Add(help);
        var presets=new WrapPanel();foreach(var preset in Services.ThemePreset.All)presets.Children.Add(ThemeCard(preset));appearance.Children.Add(presets);
        var custom=Action(advancedAppearanceOpen?"−  Ocultar personalización avanzada":"＋  Personalización avanzada",()=>{advancedAppearanceOpen=!advancedAppearanceOpen;Render();});custom.Margin=new Thickness(0,18,0,0);appearance.Children.Add(custom);
        ContentPanel.Children.Add(Card(appearance));
        if(advancedAppearanceOpen)
        {
            var advanced=new StackPanel();advanced.Children.Add(Text("Personalización avanzada",22));
            var note=Text("Elige el modo, la tipografía y los colores del fondo, texto y acento.",13,"#9EA5B1");note.Margin=new Thickness(0,8,0,20);advanced.Children.Add(note);
            var controls=new WrapPanel();
            var mode=new ComboBox{ItemsSource=new[]{"Oscuro","Claro"},SelectedItem=vm.Appearance.Mode,Width=210};
            var font=new ComboBox{ItemsSource=new[]{"Space Grotesk","Segoe UI","Georgia"},SelectedItem=vm.Appearance.FontChoice,Width=210};
            controls.Children.Add(AppearanceField("Modo de la interfaz",mode,230));controls.Children.Add(AppearanceField("Tipografía",font,230));advanced.Children.Add(controls);
            var colors=new WrapPanel{Margin=new Thickness(0,15,0,0)};
            var background=new Views.ColorPicker{Value=vm.Appearance.BackgroundColor,Width=215};
            var textColor=new Views.ColorPicker{Value=Services.FinanceStore.ValidColor(vm.Appearance.TextColor)?vm.Appearance.TextColor:vm.Appearance.Mode=="Claro"?"#211D2D":"#F5F2FA",Width=215};
            var accent=new Views.ColorPicker{Value=Services.FinanceStore.ValidColor(vm.Appearance.AccentColor)?vm.Appearance.AccentColor:vm.Appearance.Mode=="Claro"?"#4778C7":"#5D86F1",Width=215};
            colors.Children.Add(AppearanceField("Fondo · #RRGGBB",background,235));colors.Children.Add(AppearanceField("Texto · #RRGGBB",textColor,235));colors.Children.Add(AppearanceField("Acento · #RRGGBB",accent,235));advanced.Children.Add(colors);
            var save=Action("Guardar personalización",()=>ApplyAppearance(new AppearanceSettings{ThemeName="Personalizado",Mode=(string)mode.SelectedItem,FontChoice=(string)font.SelectedItem,BackgroundColor=background.Value,TextColor=textColor.Value,AccentColor=accent.Value}),true);save.Margin=new Thickness(0,22,0,0);advanced.Children.Add(save);
            ContentPanel.Children.Add(Card(advanced));
        }
        var p=new StackPanel();p.Children.Add(Text("Almacenamiento local",22));var t=Text("Tu información se guarda únicamente en este equipo. Los respaldos contienen tus cuentas y movimientos; guárdalos en una ubicación de confianza.",14,"#9EA5B1");t.Margin=new Thickness(0,16,0,20);p.Children.Add(t);p.Children.Add(Text(Data.FinanceDb.DatabasePath,12,"#9EA5B1"));var bar=new WrapPanel{Margin=new Thickness(0,24,0,0)};
        bar.Children.Add(Action("Crear respaldo",()=>{var d=new SaveFileDialog{Filter="Respaldo SQLite|*.db",FileName=$"PersonalFinance-{DateTime.Today:yyyy-MM-dd}.db"};if(d.ShowDialog(this)==true){vm.Backup(d.FileName);Status.Text="Respaldo guardado.";}}));
        bar.Children.Add(Action("Restaurar respaldo",()=>{var d=new OpenFileDialog{Filter="Respaldo SQLite|*.db;*.bak"};if(d.ShowDialog(this)==true&&Views.GlassDialog.Show(this,"Se reemplazarán los datos actuales. Se conservará una copia automática anterior. ¿Continuar?","Restaurar",true)){vm.Restore(d.FileName);Services.ThemeService.Apply(vm.Appearance);vm.Reload();Status.Text="Respaldo restaurado.";}}));p.Children.Add(bar);ContentPanel.Children.Add(Card(p));ContentPanel.Children.Add(Text("Moneda de esta versión: MXN · Los saldos de tarjetas pueden ser negativos para representar deuda.",13,"#9EA5B1"));
    }
    private void ApplyAppearance(AppearanceSettings value)
    {
        vm.SaveAppearance(value);Services.ThemeService.Apply(vm.Appearance);Render();Status.Text=$"✓ Tema {value.ThemeName.ToLowerInvariant()} guardado.";
    }
    private static FrameworkElement AppearanceField(string label,FrameworkElement control,double width)
    {
        var field=new StackPanel{Width=width,Margin=new Thickness(0,0,16,12)};
        field.Children.Add(Text(label,12,"#9EA5B1"));control.Margin=new Thickness(0,6,0,0);field.Children.Add(control);return field;
    }
    private Border ThemeCard(Services.ThemePreset preset)
    {
        var selected=vm.Appearance.ThemeName==preset.Name;
        var ink=Services.ThemeService.Parse(preset.TextColor);
        var accent=Services.ThemeService.Parse(preset.AccentColor);
        var dark=preset.Mode=="Oscuro";
        var canvas=new Grid{Height=116,Background=new LinearGradientBrush(Services.ThemeService.Parse(preset.BackgroundColor),Services.ThemeService.Mix(Services.ThemeService.Parse(preset.BackgroundColor),dark?Colors.Black:Colors.White,.65),45)};
        var nav=new Border{Width=39,HorizontalAlignment=HorizontalAlignment.Left,Background=new SolidColorBrush(Color.FromArgb(120,(byte)(dark?10:240),(byte)(dark?17:242),(byte)(dark?31:246))),BorderBrush=new SolidColorBrush(Color.FromArgb(100,ink.R,ink.G,ink.B)),BorderThickness=new Thickness(0,0,1,0)};canvas.Children.Add(nav);
        var navDot=new Ellipse{Width=17,Height=17,Fill=new SolidColorBrush(accent),HorizontalAlignment=HorizontalAlignment.Left,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(11,16,0,0)};canvas.Children.Add(navDot);
        var mock=new StackPanel{Margin=new Thickness(53,15,10,12)};
        mock.Children.Add(new TextBlock{Text="PERSONAL FINANCE",Foreground=new SolidColorBrush(ink),FontSize=9,FontWeight=FontWeights.SemiBold,FontFamily=Services.ThemeService.FontFor(preset.FontChoice)});
        var mini=new Border{Height=50,Margin=new Thickness(0,12,0,0),CornerRadius=new CornerRadius(8),BorderThickness=new Thickness(1),BorderBrush=new SolidColorBrush(Color.FromArgb(95,ink.R,ink.G,ink.B)),Background=new SolidColorBrush(Color.FromArgb(65,(byte)(dark?13:255),(byte)(dark?19:255),(byte)(dark?31:255)))};
        var sample=new StackPanel{Margin=new Thickness(10,7,10,7)};sample.Children.Add(new TextBlock{Text="Balance total",Foreground=new SolidColorBrush(ink),Opacity=.7,FontSize=8});sample.Children.Add(new TextBlock{Text="$24,580.00",Foreground=new SolidColorBrush(ink),FontSize=15,FontWeight=FontWeights.SemiBold,FontFamily=Services.ThemeService.FontFor(preset.FontChoice)});mini.Child=sample;mock.Children.Add(mini);canvas.Children.Add(mock);
        var label=new DockPanel{Margin=new Thickness(12,10,12,10)};var state=Text(selected?"✓  EN USO":preset.Mode.ToUpperInvariant(),10,"#9EA5B1");DockPanel.SetDock(state,Dock.Right);label.Children.Add(state);var name=Text(preset.Name,15);name.FontWeight=FontWeights.SemiBold;label.Children.Add(name);
        var panel=new StackPanel();panel.Children.Add(new Border{Child=canvas,CornerRadius=new CornerRadius(10),ClipToBounds=true});panel.Children.Add(label);
        var outer=new Border{Child=panel,Width=214,Margin=new Thickness(0,0,14,12),Padding=new Thickness(5),CornerRadius=new CornerRadius(16),BorderThickness=new Thickness(selected?2:1),Cursor=Cursors.Hand,Focusable=true};outer.SetResourceReference(Border.BackgroundProperty,"PanelFill");outer.SetResourceReference(Border.BorderBrushProperty,selected?"ButtonEdge":"InputEdge");
        void Activate()=>Safe(()=>ApplyAppearance(new AppearanceSettings{ThemeName=preset.Name,Mode=preset.Mode,BackgroundColor=preset.BackgroundColor,TextColor=preset.TextColor,AccentColor=preset.AccentColor,FontChoice=preset.FontChoice}));
        outer.MouseLeftButtonUp+=(_,_)=>Activate();outer.KeyDown+=(_,e)=>{if(e.Key is Key.Enter or Key.Space){Activate();e.Handled=true;}};
        outer.MouseEnter+=(_,_)=>outer.Opacity=.83;outer.MouseLeave+=(_,_)=>outer.Opacity=1;
        return outer;
    }
    private void Delete<T>(int id) where T:class {if(Views.GlassDialog.Show(this,"¿Eliminar este registro? Esta acción no se puede deshacer. Los registros utilizados por movimientos o presupuestos no se pueden eliminar.","Confirmar eliminación",true)){vm.Delete<T>(id);vm.Reload();Status.Text="Registro eliminado.";}}
    private void Export(IEnumerable<Movement> source)
    {
        var d=new SaveFileDialog{Filter="Archivo CSV|*.csv",FileName=$"movimientos-{vm.Month:yyyy-MM}.csv"};if(d.ShowDialog(this)!=true)return;
        static string Q(string s){if(s.Length>0&&"=+-@\t\r".Contains(s[0]))s="'"+s;return "\""+s.Replace("\"","\"\"")+"\"";}
        var lines=new List<string>{"Fecha,Descripcion,Cuenta,Categoria,Tipo,Importe,Moneda"};lines.AddRange(source.Select(x=>string.Join(",",x.Date.ToString("yyyy-MM-dd"),Q(x.Description),Q(x.Account.Name),Q(x.Category.Name),x.Category.Type,(MainViewModel.Signed(x)/100m).ToString("F2",CultureInfo.InvariantCulture),"MXN")));File.WriteAllLines(d.FileName,lines,new UTF8Encoding(true));Status.Text="CSV exportado correctamente.";
    }
    private void EditMovement(Movement? m)=>Safe(()=>
    {
        if(vm.Accounts.Count==0){Views.GlassDialog.Show(this,"Primero crea una cuenta para asociar tus movimientos.","Nueva cuenta");EditAccount(null);return;}
        var f=new Editor(this,m==null?"Nuevo movimiento":"Editar movimiento");var description=f.Input("Descripción",m?.Description??"");var amount=f.Input("Importe · MXN (positivo)",((m?.Cents??0)/100m).ToString("F2",CultureInfo.CurrentCulture));var date=f.Date("Fecha",m?.Date??DateTime.Today);var account=f.Select("Cuenta",vm.Accounts,m?.AccountId??vm.Accounts[0].Id,x=>x.Id);var cat=f.Select("Categoría · determina ingreso o gasto",vm.Categories,m?.CategoryId??vm.Categories[0].Id,x=>x.Id);
        f.Save(()=>{var entity=new Movement{Id=m?.Id??0,Description=Required(description.Text),Cents=Cents(amount.Text,true),Date=date.SelectedDate??throw new InvalidOperationException("Selecciona una fecha."),AccountId=((Account)account.SelectedItem).Id,CategoryId=((Category)cat.SelectedItem).Id};vm.Save(entity,entity.Id);});if(f.ShowDialog()==true)vm.Reload();
    });
    private void EditAccount(Account? account,bool forCard=false)
    {
        var editorVm=new AccountEditorViewModel(account,account==null?0:vm.AccountBalance(account)-account.OpeningCents);
        var editor=new Views.AccountEditor(this,editorVm);
        if(editor.ShowDialog()==true){vm.Reload();Status.Text="✓ Cuenta guardada. La tarjeta del dashboard ya está actualizada.";}
    }
    private void EditCategory(Category? c)
    {
        var f=new Editor(this,c==null?"Nueva categoría":"Editar categoría");var name=f.Input("Nombre",c?.Name??"");var type=f.Select("Tipo",new[]{"Ingreso","Gasto"},c?.Type??"Gasto",x=>x);if(c!=null&&(vm.Movements.Any(x=>x.CategoryId==c.Id)||vm.Budgets.Any(x=>x.CategoryId==c.Id)))type.IsEnabled=false;var icon=f.PickIcon("Icono",c?.Icon??"✨");var color=f.Select("Color",new[]{"#BAC8D5","#E2E2E6","#9CA3AF","#56FEB7","#B9A5D5","#FFB4AB"},c?.Color??"#BAC8D5",x=>x);f.Save(()=>{var entity=new Category{Id=c?.Id??0,Name=Required(name.Text),Type=(string)type.SelectedItem,Icon=icon.SelectedEmoji,Color=(string)color.SelectedItem};vm.Save(entity,entity.Id);});if(f.ShowDialog()==true)vm.Reload();
    }
    private void EditBudget(Budget? b)
    {
        var cats=vm.Categories.Where(x=>x.Type=="Gasto").ToList();if(cats.Count==0)throw new InvalidOperationException("Crea primero una categoría de gasto.");var f=new Editor(this,"Presupuesto · "+vm.MonthLabel);var cat=f.Select("Categoría",cats,b?.CategoryId??cats[0].Id,x=>x.Id);var amount=f.Input("Límite mensual · MXN",((b?.LimitCents??0)/100m).ToString("F2",CultureInfo.CurrentCulture));f.Save(()=>{var entity=new Budget{Id=b?.Id??0,CategoryId=((Category)cat.SelectedItem).Id,Month=vm.Month,LimitCents=Cents(amount.Text,true)};vm.Save(entity,entity.Id);});if(f.ShowDialog()==true)vm.Reload();
    }
    private static string Required(string value)=>string.IsNullOrWhiteSpace(value)?throw new InvalidOperationException("El nombre o la descripción es obligatorio."):value.Trim().Length>200?throw new InvalidOperationException("Usa un máximo de 200 caracteres."):value.Trim();
    private static long Cents(string value,bool positive)
    {
        if(!decimal.TryParse(value,NumberStyles.Number,CultureInfo.CurrentCulture,out var amount)||Math.Abs(amount)>100000000000m||(positive&&amount<=0)||decimal.Round(amount,2)!=amount)throw new InvalidOperationException("Introduce un importe válido con hasta dos decimales"+(positive?" y mayor que cero.":"."));return checked((long)(amount*100));
    }
    private record MovementRow(int Id,string Date,string Description,string Category,string Account,string Type,string Amount);
}

internal sealed class Editor : Window
{
    private readonly StackPanel panel=new(){Margin=new Thickness(30)};
    public Editor(Window owner,string title)
    {
        Owner=owner;Title=title;Width=480;SizeToContent=SizeToContent.Height;MaxHeight=Math.Min(800,SystemParameters.WorkArea.Height-60);WindowStyle=WindowStyle.None;WindowStartupLocation=WindowStartupLocation.CenterOwner;ResizeMode=ResizeMode.NoResize;Background=Views.GlassPanel.Gradient(("#FF44404F",0),("#FF191820",.4),("#FF272130",1));Foreground=Brushes.White;Resources=owner.Resources;FontFamily=owner.FontFamily;SetResourceReference(BackgroundProperty,"Surface");SetResourceReference(ForegroundProperty,"TextPrimary");Content=new Border{BorderBrush=Views.GlassPanel.Gradient(("#CFDCCBEF",0),("#45413F49",1)),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(20),ClipToBounds=true,Child=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto}};Loaded+=(_,_)=>Views.Motion.Enter(panel);var caption=new TextBlock{Text=title,FontSize=24,Margin=new Thickness(0,0,0,22)};caption.MouseLeftButtonDown+=(_,e)=>{if(e.ButtonState==MouseButtonState.Pressed)DragMove();};panel.Children.Add(caption);
    }
    private void Field(string label,UIElement control){panel.Children.Add(new TextBlock{Text=label,Foreground=(Brush)FindResource("TextSecondary"),Margin=new Thickness(0,15,0,7)});panel.Children.Add(control);}
    public TextBox Input(string label,string value){var input=new TextBox{Text=value,MaxLength=200};Field(label,input);return input;}
    public DatePicker Date(string label,DateTime value){var input=new DatePicker{SelectedDate=value};Field(label,input);return input;}
    public ComboBox Select<T,K>(string label,IEnumerable<T> items,K key,Func<T,K> selector){var list=items.ToList();var input=new ComboBox{ItemsSource=list,SelectedItem=list.FirstOrDefault(x=>Equals(selector(x),key)),Tag=System.Windows.Controls.Primitives.PlacementMode.Top,MaxDropDownHeight=205};if(input.SelectedIndex<0&&list.Count>0)input.SelectedIndex=0;Field(label,input);return input;}
    public Views.CategoryIconPicker PickIcon(string label,string selected){var picker=new Views.CategoryIconPicker(selected);Field(label,picker);return picker;}
    public void InlineAction(string label,Action action){var button=new Button{Content=label,HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,10,0,0),Padding=new Thickness(14,8,14,8)};button.Click+=(_,_)=>{try{action();}catch(Exception ex){Views.GlassDialog.Show(this,ex.Message,"Personal Finance");}};panel.Children.Add(button);}
    public void Save(Action save)
    {
        var error=new TextBlock{Foreground=Brushes.LightSalmon,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(0,16,0,0)};panel.Children.Add(error);var bar=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,Margin=new Thickness(0,24,0,0)};var cancel=new Button{Content="Cancelar",IsCancel=true};var ok=new Button{Content="Guardar",IsDefault=true,Background=Views.GlassPanel.Gradient(("#FF887B99",0),("#FF41354F",1)),Foreground=Brushes.White};ok.Click+=(_,_)=>{try{save();DialogResult=true;}catch(Exception ex){error.Text=ex is Microsoft.EntityFrameworkCore.DbUpdateException?"No se pudo guardar. Comprueba que el presupuesto no esté duplicado.":ex.Message;}};bar.Children.Add(cancel);bar.Children.Add(ok);panel.Children.Add(bar);
    }
}

