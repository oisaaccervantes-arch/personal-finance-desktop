using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Data;
using PersonalFinance.Models;
using System.IO;

namespace PersonalFinance.Services;
public sealed class FinanceStore
{
    public FinanceStore()
    {
        using var db = new FinanceDb();
        db.Database.EnsureCreated();
        SchemaUpgrade.Apply();
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(new Category { Name="Sueldo", Type="Ingreso", Icon="↗" }, new Category {Name="Otros ingresos", Type="Ingreso", Icon="+"},
                new Category {Name="Alimentación", Icon="◈"}, new Category {Name="Hogar", Icon="⌂"}, new Category {Name="Transporte", Icon="↔"}, new Category {Name="Compras", Icon="◇"}, new Category {Name="Salud", Icon="+"});
            db.SaveChanges();
        }
        if(!db.Accounts.Any(x=>x.Type=="Efectivo")){db.Accounts.Add(new Account{Name="Efectivo",Type="Efectivo",OpeningCents=0});db.SaveChanges();}
    }
    public List<Account> Accounts() { using var db=new FinanceDb(); return db.Accounts.AsNoTracking().OrderBy(x=>x.Type=="Efectivo"?0:1).ThenBy(x=>x.Name).ToList(); }
    public List<Category> Categories() { using var db=new FinanceDb(); return db.Categories.AsNoTracking().OrderBy(x=>x.Name).ToList(); }
    public List<Movement> Movements() { using var db=new FinanceDb(); return db.Movements.AsNoTracking().Include(x=>x.Account).Include(x=>x.Category).OrderByDescending(x=>x.Date).ThenByDescending(x=>x.Id).ToList(); }
    public List<Budget> Budgets() { using var db=new FinanceDb(); return db.Budgets.AsNoTracking().Include(x=>x.Category).ToList(); }
    public List<Transfer> Transfers(){using var db=new FinanceDb();return db.Transfers.AsNoTracking().Include(x=>x.FromAccount).Include(x=>x.ToAccount).OrderByDescending(x=>x.Date).ThenByDescending(x=>x.Id).ToList();}
    public List<RecurringRule> RecurringRules(){using var db=new FinanceDb();return db.RecurringRules.AsNoTracking().Include(x=>x.Account).Include(x=>x.Category).OrderBy(x=>x.StartDate).ToList();}
    public static IEnumerable<DateTime> Occurrences(RecurringRule rule,DateTime through)
    {
        var start=rule.StartDate.Date;
        for(int month=0;month<=2400;month++)
        {
            var first=new DateTime(start.Year,start.Month,1).AddMonths(month);
            var date=new DateTime(first.Year,first.Month,Math.Min(start.Day,DateTime.DaysInMonth(first.Year,first.Month)));
            if(date>through.Date)yield break;
            if(date>=start)yield return date;
        }
    }
    public void ProcessDue(DateTime? today=null)
    {
        var day=(today??DateTime.Today).Date;using var db=new FinanceDb();using var transaction=db.Database.BeginTransaction();
        foreach(var rule in db.RecurringRules.Where(x=>x.Active&&x.StartDate<=day).ToList())
        {
            var existing=db.Movements.Where(x=>x.RecurringRuleId==rule.Id).Select(x=>x.OccurrenceDate).ToHashSet();
            foreach(var date in Occurrences(rule,day))if(!existing.Contains(date))db.Movements.Add(new Movement{Description=rule.Description,Cents=rule.Cents,Date=date,AccountId=rule.AccountId,CategoryId=rule.CategoryId,RecurringRuleId=rule.Id,OccurrenceDate=date});
        }
        db.SaveChanges();transaction.Commit();
    }
    public void SaveRecurring(RecurringRule rule)
    {
        if(rule.Description.Trim().Length is <1 or >200||rule.Cents is <=0 or >10000000000000||rule.Type is not ("Ingreso" or "Gasto")||rule.StartDate.Date<DateTime.Today.AddYears(-10)||rule.StartDate.Date>DateTime.Today.AddYears(10))throw new InvalidOperationException("Revisa concepto, monto y fecha de inicio de la recurrencia.");
        using var db=new FinanceDb();if(!db.Accounts.Any(x=>x.Id==rule.AccountId)||!db.Categories.Any(x=>x.Id==rule.CategoryId&&x.Type==rule.Type))throw new InvalidOperationException("Selecciona una cuenta y categoría del mismo tipo.");
        rule.Description=rule.Description.Trim();rule.StartDate=rule.StartDate.Date;
        if(rule.Id==0)db.RecurringRules.Add(rule);else db.RecurringRules.Update(rule);db.SaveChanges();ProcessDue();
    }
    public void DeleteRecurring(int id){using var db=new FinanceDb();var rule=db.RecurringRules.Find(id);if(rule==null)return;rule.Active=false;db.SaveChanges();}
    public void TransferFunds(int fromId,int toId,long cents,string description,DateTime? date=null)
    {
        if(fromId==toId||cents is <=0 or >10000000000000||string.IsNullOrWhiteSpace(description)||description.Length>200)throw new InvalidOperationException("Selecciona dos cuentas distintas y un monto positivo.");
        using var db=new FinanceDb();var source=db.Accounts.FirstOrDefault(x=>x.Id==fromId);if(source==null||!db.Accounts.Any(x=>x.Id==toId))throw new InvalidOperationException("Selecciona cuentas existentes.");
        if(source.Type=="Efectivo")
        {
            long available=source.OpeningCents+db.Movements.Include(x=>x.Category).Where(x=>x.AccountId==fromId).AsEnumerable().Sum(x=>x.Category.Type=="Ingreso"?x.Cents:-x.Cents)+db.Transfers.Where(x=>x.ToAccountId==fromId).Sum(x=>x.Cents)-db.Transfers.Where(x=>x.FromAccountId==fromId).Sum(x=>x.Cents);
            if(available<cents)throw new InvalidOperationException("No hay suficiente efectivo para este depósito.");
        }
        db.Transfers.Add(new Transfer{FromAccountId=fromId,ToAccountId=toId,Cents=cents,Description=description.Trim(),Date=(date??DateTime.Today).Date});db.SaveChanges();
    }
    public AppearanceSettings Appearance(){using var db=new FinanceDb();return db.Appearance.AsNoTracking().FirstOrDefault(x=>x.Id==1)??new AppearanceSettings();}
    public void SaveAppearance(string mode,string color)=>SaveAppearance(new AppearanceSettings{ThemeName="Personalizado",Mode=mode,BackgroundColor=color});
    public void SaveAppearance(AppearanceSettings value)
    {
        if(value.Mode is not ("Claro" or "Oscuro")||!ValidColor(value.BackgroundColor)||value.TextColor.Length>0&&!ValidColor(value.TextColor)||value.AccentColor.Length>0&&!ValidColor(value.AccentColor)||value.FontChoice is not ("Space Grotesk" or "Segoe UI" or "Georgia"))throw new InvalidOperationException("Revisa modo, colores hexadecimales (#RRGGBB) y tipografía.");
        using var db=new FinanceDb();var settings=db.Appearance.Find(1);
        if(settings==null){settings=new AppearanceSettings();db.Appearance.Add(settings);}
        settings.ThemeName=value.ThemeName;settings.Mode=value.Mode;settings.BackgroundColor=value.BackgroundColor.ToUpperInvariant();settings.TextColor=value.TextColor.ToUpperInvariant();settings.AccentColor=value.AccentColor.ToUpperInvariant();settings.FontChoice=value.FontChoice;db.SaveChanges();
    }
    public static bool ValidColor(string value)=>System.Text.RegularExpressions.Regex.IsMatch(value??"",@"^#[0-9a-fA-F]{6}$");
    public void QuickMovement(bool incoming,string concept,long cents,int accountId,int? categoryId)
    {
        if(string.IsNullOrWhiteSpace(concept)||concept.Trim().Length>200||cents<=0||cents>10000000000000)
            throw new InvalidOperationException("Escribe un concepto y un importe positivo válido.");
        using var db=new FinanceDb();
        using var transaction=db.Database.BeginTransaction();
        if(!db.Accounts.Any(x=>x.Id==accountId))throw new InvalidOperationException("Selecciona una cuenta existente.");
        Category category;
        if(!incoming&&categoryId is >0)
            category=db.Categories.SingleOrDefault(x=>x.Id==categoryId&&x.Type=="Gasto")??throw new InvalidOperationException("Selecciona una categoría de gasto.");
        else
        {
            string name=incoming?"Otros ingresos":"Otro",type=incoming?"Ingreso":"Gasto";
            category=db.Categories.FirstOrDefault(x=>x.Name==name&&x.Type==type)??new Category{Name=name,Type=type,Icon=incoming?"↗":"◇"};
            if(category.Id==0){db.Categories.Add(category);db.SaveChanges();}
        }
        db.Movements.Add(new Movement{Description=concept.Trim(),Cents=cents,Date=DateTime.Today,AccountId=accountId,CategoryId=category.Id});
        db.SaveChanges();transaction.Commit();
    }
    public void Save<T>(T entity, int id) where T:class
    {
        using var db=new FinanceDb();
        switch(entity)
        {
            case Account a when string.IsNullOrWhiteSpace(a.Name) || a.Name.Length>200:
                throw new InvalidOperationException("Indica un nombre de cuenta de hasta 200 caracteres.");
            case Account a:
                if(a.Bank.Length>40||a.CardText.Length>48||!ValidColor(a.CardColor)||a.OpeningCents is < -10000000000000 or > 10000000000000)
                    throw new InvalidOperationException("Revisa el banco (40 caracteres), texto (48), color (#RRGGBB) y saldo inicial.");
                if(a.Type=="Efectivo"&&db.Accounts.Any(x=>x.Type=="Efectivo"&&x.Id!=id))throw new InvalidOperationException("Ya existe la cuenta de efectivo. Solo puedes editar su saldo inicial.");
                if(id!=0&&db.Accounts.Any(x=>x.Id==id&&x.Type=="Efectivo")&&a.Type!="Efectivo")throw new InvalidOperationException("La cuenta de efectivo debe conservar su tipo.");
                a.CardColor=a.CardColor.ToUpperInvariant();break;
            case Category c:
                if(string.IsNullOrWhiteSpace(c.Name)||c.Name.Length>200||!new[]{"Ingreso","Gasto"}.Contains(c.Type)) throw new InvalidOperationException("Categoría no válida.");
                var old=db.Categories.AsNoTracking().FirstOrDefault(x=>x.Id==id);
                if(old!=null&&old.Type!=c.Type&&(db.Movements.Any(x=>x.CategoryId==id)||db.Budgets.Any(x=>x.CategoryId==id))) throw new InvalidOperationException("No se puede cambiar el tipo de una categoría utilizada.");
                break;
            case Movement m:
                if(string.IsNullOrWhiteSpace(m.Description)||m.Description.Length>200||m.Cents<=0||m.Cents>10000000000000) throw new InvalidOperationException("Movimiento no válido.");
                if(!db.Accounts.Any(x=>x.Id==m.AccountId)||!db.Categories.Any(x=>x.Id==m.CategoryId)) throw new InvalidOperationException("Selecciona una cuenta y categoría existentes.");
                m.Date=m.Date.Date;
                if(id!=0){var previous=db.Movements.AsNoTracking().FirstOrDefault(x=>x.Id==id);if(previous!=null){m.RecurringRuleId=previous.RecurringRuleId;m.OccurrenceDate=previous.OccurrenceDate;}}
                m.Account=null!;m.Category=null!;
                break;
            case Budget b:
                if(b.LimitCents<=0||!db.Categories.Any(x=>x.Id==b.CategoryId&&x.Type=="Gasto")) throw new InvalidOperationException("El presupuesto requiere un límite positivo y una categoría de gasto.");
                b.Month=new DateTime(b.Month.Year,b.Month.Month,1);
                break;
        }
        if(id==0) db.Add(entity); else db.Update(entity);
        db.SaveChanges();
    }
    public void Delete<T>(int id) where T:class
    {
        using var db=new FinanceDb(); var entity=db.Set<T>().Find(id);
        if(entity is Account a&&a.Type=="Efectivo")throw new InvalidOperationException("La cuenta de efectivo es permanente.");
        if(entity is Movement m&&m.RecurringRuleId!=null)throw new InvalidOperationException("Los movimientos recurrentes se pueden editar. Para detener los próximos, pausa la recurrencia.");
        if(entity!=null) { db.Remove(entity); db.SaveChanges(); }
    }
    public void Backup(string path)
    {
        if(Path.GetFullPath(path)==Path.GetFullPath(FinanceDb.DatabasePath)) throw new InvalidOperationException("Elige una ubicación distinta de la base activa.");
        using var source=new SqliteConnection($"Data Source={FinanceDb.DatabasePath}");
        using var target=new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=path}.ToString());
        source.Open(); target.Open(); source.BackupDatabase(target);
    }
    public void Restore(string path)
    {
        if(string.Equals(Path.GetFullPath(path),Path.GetFullPath(FinanceDb.DatabasePath),StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Selecciona un respaldo distinto de la base activa.");
        using var source=new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=path, Mode=SqliteOpenMode.ReadOnly}.ToString()); source.Open();
        using(var check=source.CreateCommand()) { check.CommandText="PRAGMA integrity_check"; if((string?)check.ExecuteScalar()!="ok") throw new InvalidOperationException("El respaldo está dañado."); }
        using(var check=source.CreateCommand()) { check.CommandText="SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('Accounts','Categories','Movements','Budgets')"; if(Convert.ToInt32(check.ExecuteScalar())!=4) throw new InvalidOperationException("No es un respaldo de Personal Finance."); }
        foreach(var query in new[]{"SELECT Id,Name,Type,OpeningCents FROM Accounts LIMIT 1", "SELECT Id,Name,Type,Icon,Color FROM Categories LIMIT 1", "SELECT Id,Description,Cents,Date,AccountId,CategoryId FROM Movements LIMIT 1", "SELECT Id,CategoryId,Month,LimitCents FROM Budgets LIMIT 1"})
        { using var check=source.CreateCommand(); check.CommandText=query; using var reader=check.ExecuteReader(); }
        using(var check=source.CreateCommand()) { check.CommandText="PRAGMA foreign_key_check"; using var reader=check.ExecuteReader(); if(reader.Read()) throw new InvalidOperationException("El respaldo contiene relaciones inválidas."); }
        Backup(FinanceDb.DatabasePath+$".{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.before-restore.bak");
        using var target=new SqliteConnection($"Data Source={FinanceDb.DatabasePath}"); target.Open(); source.BackupDatabase(target);
        target.Close();SchemaUpgrade.Apply();
        using var restored=new FinanceDb();if(!restored.Accounts.Any(x=>x.Type=="Efectivo")){restored.Accounts.Add(new Account{Name="Efectivo",Type="Efectivo"});restored.SaveChanges();}
    }
}
