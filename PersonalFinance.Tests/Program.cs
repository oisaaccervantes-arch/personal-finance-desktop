using PersonalFinance.Models;
using PersonalFinance.Services;
using PersonalFinance.ViewModels;
using System.IO;

if(args.Contains("--scroll")){ScrollChecks.Run();return;}

var folder=Path.Combine(Path.GetTempPath(),"PersonalFinance-tests-"+Guid.NewGuid());
Environment.SetEnvironmentVariable("PERSONAL_FINANCE_DATA_DIR",folder);
Directory.CreateDirectory(folder);
using(var legacy=new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={Path.Combine(folder,"finance.db")}"))
{
    legacy.Open();using var sql=legacy.CreateCommand();sql.CommandText="""
    CREATE TABLE Accounts (Id INTEGER PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,Type TEXT NOT NULL,OpeningCents INTEGER NOT NULL);
    CREATE TABLE Categories (Id INTEGER PRIMARY KEY AUTOINCREMENT,Name TEXT NOT NULL,Type TEXT NOT NULL,Icon TEXT NOT NULL,Color TEXT NOT NULL);
    CREATE TABLE Movements (Id INTEGER PRIMARY KEY AUTOINCREMENT,Description TEXT NOT NULL,Cents INTEGER NOT NULL,Date TEXT NOT NULL,AccountId INTEGER NOT NULL REFERENCES Accounts(Id),CategoryId INTEGER NOT NULL REFERENCES Categories(Id));
    CREATE TABLE Budgets (Id INTEGER PRIMARY KEY AUTOINCREMENT,CategoryId INTEGER NOT NULL REFERENCES Categories(Id),Month TEXT NOT NULL,LimitCents INTEGER NOT NULL);
    CREATE UNIQUE INDEX IX_Budgets_CategoryId_Month ON Budgets(CategoryId,Month);
    INSERT INTO Accounts(Name,Type,OpeningCents) VALUES ('Cuenta anterior','Efectivo',0);
    """;sql.ExecuteNonQuery();
}
var legacyBackup=Path.Combine(folder,"legacy.db");File.Copy(Path.Combine(folder,"finance.db"),legacyBackup);
var store=new FinanceStore();
int checks=0;
void Check(bool pass,string label){if(!pass)throw new Exception(label);Console.WriteLine("PASS "+label);checks++;}
Check(store.Accounts().Any(x=>x.Name=="Cuenta anterior"&&x.CardColor=="#625081"&&x.Bank==""),"La actualización conserva cuentas del esquema anterior");
var account=new Account{Name="Test",Type="Cuenta de ahorro",OpeningCents=10000};store.Save(account,0);
var categories=store.Categories();var inc=categories.First(x=>x.Type=="Ingreso");var exp=categories.First(x=>x.Type=="Gasto");
var movement=new Movement{Description="Ingreso",Cents=125055,Date=DateTime.Today,AccountId=account.Id,CategoryId=inc.Id};store.Save(movement,0);
var debit=new Movement{Description="Gasto",Cents=5030,Date=DateTime.Today,AccountId=account.Id,CategoryId=exp.Id};store.Save(debit,0);
var vm=new MainViewModel();Check(vm.Balance==130025&&vm.Income==125055&&vm.Expense==5030,"Saldos exactos en centavos");
debit.Cents=6010;store.Save(debit,debit.Id);vm.Reload();Check(vm.Expense==6010,"Edición actualiza resumen");
var previous=new Movement{Description="Mes anterior",Cents=100,Date=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddDays(-1),AccountId=account.Id,CategoryId=exp.Id};store.Save(previous,0);vm.Reload();Check(vm.Expense==6010,"Límite de periodo mensual");
var budget=new Budget{CategoryId=exp.Id,Month=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1),LimitCents=10000};store.Save(budget,0);Check(store.Budgets().Count==1,"Persistencia de presupuesto");
bool protectedAccount=false;try{store.Delete<Account>(account.Id);}catch(Microsoft.EntityFrameworkCore.DbUpdateException){protectedAccount=true;}Check(protectedAccount,"Protección de cuenta utilizada");
bool rejected=false;try{store.Save(new Movement{Description="Inválido",Cents=-1,AccountId=account.Id,CategoryId=exp.Id},0);}catch(InvalidOperationException){rejected=true;}Check(rejected,"Rechazo de importes negativos");
var backup=Path.Combine(folder,"backup.db");store.Backup(backup);store.Delete<Movement>(debit.Id);Check(store.Movements().Count==2,"Eliminación persistida");store.Restore(backup);Check(store.Movements().Count==3,"Restauración recupera movimientos");
var reopened=new FinanceStore();Check(reopened.Movements().Count==3,"Persistencia al reabrir");
Check(!vm.HasCard&&!vm.NextCard.CanExecute(null)&&vm.CardBalance=="—","Sin tarjetas no se inventa un saldo ni se activa el selector");
var debitCard=new Account{Name="BBVA Débito",Type="Tarjeta de débito",OpeningCents=20000};store.Save(debitCard,0);
var creditCard=new Account{Name="Otra tarjeta",Type="Tarjeta de crédito",OpeningCents=-5000};store.Save(creditCard,0);
store.Save(new Movement{Description="Compra de prueba",Cents=1500,Date=DateTime.Today,AccountId=debitCard.Id,CategoryId=exp.Id},0);
vm.Reload();Check(vm.CardAccounts.Count==2&&vm.SelectedCard?.Id==debitCard.Id&&vm.CardBalance==MainViewModel.Money(18500),"La tarjeta muestra su saldo propio y excluye efectivo");
vm.NextCard.Execute(null);Check(vm.SelectedCard?.Id==creditCard.Id&&vm.CardBalance==MainViewModel.Money(-5000)&&vm.CardBalanceLabel.StartsWith("SALDO REGISTRADO"),"Alternar actualiza cuenta, importe y etiqueta de crédito");
vm.Reload();Check(vm.SelectedCard?.Id==creditCard.Id,"La selección se conserva al recargar movimientos");
vm.NextCard.Execute(null);Check(vm.SelectedCard?.Id==debitCard.Id,"El selector vuelve a la primera tarjeta");
vm.NextCard.Execute(null);store.Delete<Account>(creditCard.Id);vm.Reload();Check(vm.SelectedCard?.Id==debitCard.Id&&!vm.NextCard.CanExecute(null),"Eliminar la selección vuelve a la tarjeta restante");
var quick=new QuickMovementViewModel(store.Accounts(),store.Categories());int saved=0;quick.Saved+=()=>saved++;
quick.Choose.Execute("Ingreso");quick.Save.Execute(null);Check(saved==0&&quick.Error.Length>0,"Registro rápido exige una cuenta y campos válidos");
quick.Account=store.Accounts().First(x=>x.Id==debitCard.Id);quick.Concept="Depósito rápido";quick.Amount=123.45m.ToString(System.Globalization.CultureInfo.CurrentCulture);quick.Save.Execute(null);
var incoming=store.Movements().First(x=>x.Description=="Depósito rápido");Check(saved==1&&incoming.Cents==12345&&incoming.Category.Type=="Ingreso"&&incoming.AccountId==debitCard.Id,"Entrante guarda concepto, monto y cuenta con categoría de ingreso");
quick.Choose.Execute("Gasto");quick.Concept="Compra rápida";quick.Category=quick.Categories.First(x=>x.Id==exp.Id);quick.Amount="25";quick.Save.Execute(null);
Check(saved==2&&store.Movements().First(x=>x.Description=="Compra rápida").CategoryId==exp.Id,"Saliente respeta la categoría configurada");
quick.Category=quick.Categories.Single(x=>x.Id==0);quick.Concept="Gasto sin clasificar";quick.Save.Execute(null);quick.Concept="Segundo gasto otro";quick.Save.Execute(null);
Check(store.Categories().Count(x=>x.Name=="Otro"&&x.Type=="Gasto")==1&&store.Movements().First(x=>x.Description=="Gasto sin clasificar").Category.Name=="Otro","Otro se crea una vez y se reutiliza");
int before=store.Movements().Count;quick.Amount="-5";quick.Save.Execute(null);Check(store.Movements().Count==before,"El registro rápido rechaza montos negativos sin escribir");
bool wrongType=false;try{store.QuickMovement(false,"Categoría inválida",100,debitCard.Id,inc.Id);}catch(InvalidOperationException){wrongType=true;}Check(wrongType&&store.Movements().Count==before,"No se permite asociar un gasto a una categoría de ingreso");
var styled=store.Accounts().Single(x=>x.Id==debitCard.Id);styled.Bank="Mi banco";styled.CardText="Mi tarjeta diaria";styled.CardColor="#F0C850";styled.OpeningCents=80000;store.Save(styled,styled.Id);vm.Reload();
Check(vm.CardIssuer=="Mi banco"&&vm.CardCustomText=="Mi tarjeta diaria"&&vm.SelectedCard!.CardColor=="#F0C850","Banco, texto y color editados se reflejan en el dashboard");
Check(vm.CardBalance==MainViewModel.Money(vm.AccountBalance(styled)),"El cambio de saldo inicial conserva y suma los movimientos");
store.SaveAppearance("Claro","#2468AB");Check(new FinanceStore().Appearance().Mode=="Claro"&&new FinanceStore().Appearance().BackgroundColor=="#2468AB","El modo y fondo persisten al reabrir");
store.SaveAppearance(new AppearanceSettings{ThemeName="Obsidiana",Mode="Oscuro",BackgroundColor="#183050",TextColor="#F5F7FF",AccentColor="#5D86F1",FontChoice="Segoe UI"});
var savedTheme=new FinanceStore().Appearance();Check(savedTheme.ThemeName=="Obsidiana"&&savedTheme.TextColor=="#F5F7FF"&&savedTheme.AccentColor=="#5D86F1"&&savedTheme.FontChoice=="Segoe UI","El tema, colores y tipografía persisten al reabrir");
store.SaveAppearance("Claro","#2468AB");
var personalizedBackup=Path.Combine(folder,"personalized.db");store.Backup(personalizedBackup);styled.Bank="Cambio temporal";store.Save(styled,styled.Id);store.SaveAppearance("Oscuro","#625081");store.Restore(personalizedBackup);
Check(store.Accounts().Single(x=>x.Id==debitCard.Id).Bank=="Mi banco"&&store.Appearance().Mode=="Claro","El respaldo recupera personalización de tarjetas y apariencia");
var cash=store.Accounts().Single(x=>x.Type=="Efectivo");Check(store.Accounts()[0].Id==cash.Id,"Efectivo es la primera cuenta y existe por defecto");
bool duplicateCash=false;try{store.Save(new Account{Name="Otro efectivo",Type="Efectivo"},0);}catch(InvalidOperationException){duplicateCash=true;}Check(duplicateCash,"Solo se permite una cuenta de efectivo");
vm.Reload();var totalBefore=vm.Balance;var cardBefore=vm.AccountBalance(styled);store.TransferFunds(debitCard.Id,cash.Id,2500,"Retiro de efectivo");vm.Reload();
Check(vm.Balance==totalBefore&&vm.AccountBalance(styled)==cardBefore-2500&&vm.AccountBalance(cash)==2500,"Retiro mueve saldo sin cambiar el patrimonio");
store.TransferFunds(cash.Id,debitCard.Id,1000,"Depósito de efectivo");vm.Reload();Check(vm.AccountBalance(cash)==1500&&vm.AccountBalance(styled)==cardBefore-1500,"Depósito devuelve saldo a la cuenta elegida");
bool cashOverdraw=false;try{store.TransferFunds(cash.Id,debitCard.Id,1600,"Depósito imposible");}catch(InvalidOperationException){cashOverdraw=true;}Check(cashOverdraw&&store.Transfers().Count==2,"No se puede depositar más efectivo del disponible");
var recurring=new RecurringRule{Description="Pago mensual",Type="Gasto",Cents=1200,StartDate=DateTime.Today.AddMonths(-1),AccountId=debitCard.Id,CategoryId=exp.Id};store.SaveRecurring(recurring);vm.Reload();
var posted=store.Movements().Where(x=>x.RecurringRuleId==recurring.Id).ToList();Check(posted.Count==2,"Se registran las fechas recurrentes vencidas");
store.ProcessDue();Check(store.Movements().Count(x=>x.RecurringRuleId==recurring.Id)==2,"Reabrir no duplica vencimientos");
var realized=posted[0];realized.AccountId=account.Id;store.Save(realized,realized.Id);store.ProcessDue();Check(store.Movements().Single(x=>x.Id==realized.Id).AccountId==account.Id&&store.Movements().Count(x=>x.RecurringRuleId==recurring.Id)==2,"Un pago recurrente realizado se puede cambiar de cuenta");
vm.Reload();var future=new DateTime(DateTime.Today.Year,DateTime.Today.Month,1).AddMonths(2).AddDays(-1);Check(vm.ProjectedBalance(future)==vm.Balance-1200,"La proyección incluye el próximo gasto mensual");
var recurringIncome=new RecurringRule{Description="Cobro mensual",Type="Ingreso",Cents=2500,StartDate=DateTime.Today.AddDays(1),AccountId=debitCard.Id,CategoryId=inc.Id};store.SaveRecurring(recurringIncome);vm.Reload();
var expectedProjection=vm.Balance+FinanceStore.Occurrences(recurringIncome,future).Count(x=>x>DateTime.Today)*2500-FinanceStore.Occurrences(recurring,future).Count(x=>x>DateTime.Today)*1200;
Check(vm.ProjectedBalance(future)==expectedProjection&&store.Movements().All(x=>x.RecurringRuleId!=recurringIncome.Id),"Los ingresos futuros aparecen en la proyección sin cobrarse antes");
var quickCash=new QuickMovementViewModel(store.Accounts(),store.Categories());quickCash.Choose.Execute("Efectivo");quickCash.Choose.Execute("Retiro");quickCash.Account=store.Accounts().Single(x=>x.Id==debitCard.Id);quickCash.Amount="10";var cashBefore=vm.AccountBalance(cash);quickCash.Save.Execute(null);vm.Reload();Check(vm.AccountBalance(cash)==cashBefore+1000&&quickCash.Error.Length==0,"El registro rápido retira efectivo hacia la billetera");
var recurringBackup=Path.Combine(folder,"recurring-demo.db");store.Backup(recurringBackup);
store.SaveAppearance("Oscuro","#FFFFFF");PersonalFinance.Services.ThemeService.Apply(store.Appearance());Check(PersonalFinance.Services.ThemeService.BackgroundColor=="#FFFFFF","El fondo blanco se conserva en modo oscuro");
bool colorRejected=false;try{store.SaveAppearance("Claro","rojo");}catch(InvalidOperationException){colorRejected=true;}Check(colorRejected,"No se guardan colores inválidos");
store.Restore(legacyBackup);Check(store.Accounts().Single().Name=="Cuenta anterior"&&store.Accounts().Single().CardColor=="#625081"&&store.Appearance().Mode=="Oscuro","Se pueden restaurar respaldos anteriores y actualizar su esquema");
Console.WriteLine($"{checks} comprobaciones correctas. Base aislada: {folder}");
