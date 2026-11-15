using Microsoft.Data.Sqlite;
using System.IO;
namespace PersonalFinance.Data;
internal static class SchemaUpgrade
{
    public static void Apply()
    {
        using var connection=new SqliteConnection($"Data Source={FinanceDb.DatabasePath}");connection.Open();
        var names=new HashSet<string>();
        using(var command=connection.CreateCommand())
        {command.CommandText="PRAGMA table_info(Accounts)";using var reader=command.ExecuteReader();while(reader.Read())names.Add(reader.GetString(1));}
        using var versionCommand=connection.CreateCommand();versionCommand.CommandText="PRAGMA user_version";int version=Convert.ToInt32(versionCommand.ExecuteScalar());
        if(version<4)
        {
            using var backup=new SqliteConnection($"Data Source={FinanceDb.DatabasePath}.before-v4-{DateTime.Now:yyyyMMddHHmmss}.bak");
            backup.Open();connection.BackupDatabase(backup);
        }
        using var transaction=connection.BeginTransaction();
        foreach(var (name,definition) in new[]{("Bank","TEXT NOT NULL DEFAULT ''"),("CardText","TEXT NOT NULL DEFAULT ''"),("CardColor","TEXT NOT NULL DEFAULT '#625081'")})
        {
            if(names.Contains(name))continue;
            using var command=connection.CreateCommand();command.Transaction=transaction;
            command.CommandText=$"ALTER TABLE Accounts ADD COLUMN {name} {definition}";command.ExecuteNonQuery();
        }
        var movementNames=new HashSet<string>();
        using(var command=connection.CreateCommand()){command.Transaction=transaction;command.CommandText="PRAGMA table_info(Movements)";using var reader=command.ExecuteReader();while(reader.Read())movementNames.Add(reader.GetString(1));}
        foreach(var (name,definition) in new[]{("RecurringRuleId","INTEGER NULL"),("OccurrenceDate","TEXT NULL")})
        {
            if(movementNames.Contains(name))continue;
            using var command=connection.CreateCommand();command.Transaction=transaction;command.CommandText=$"ALTER TABLE Movements ADD COLUMN {name} {definition}";command.ExecuteNonQuery();
        }
        using(var command=connection.CreateCommand())
        {
            command.Transaction=transaction;command.CommandText="CREATE TABLE IF NOT EXISTS Appearance (Id INTEGER NOT NULL PRIMARY KEY, Mode TEXT NOT NULL, BackgroundColor TEXT NOT NULL)";command.ExecuteNonQuery();
        }
        var appearanceNames=new HashSet<string>();
        using(var command=connection.CreateCommand()){command.Transaction=transaction;command.CommandText="PRAGMA table_info(Appearance)";using var reader=command.ExecuteReader();while(reader.Read())appearanceNames.Add(reader.GetString(1));}
        foreach(var (name,definition) in new[]{("ThemeName","TEXT NOT NULL DEFAULT 'Personalizado'"),("TextColor","TEXT NOT NULL DEFAULT ''"),("AccentColor","TEXT NOT NULL DEFAULT ''"),("FontChoice","TEXT NOT NULL DEFAULT 'Space Grotesk'")})
        {
            if(appearanceNames.Contains(name))continue;
            using var command=connection.CreateCommand();command.Transaction=transaction;command.CommandText=$"ALTER TABLE Appearance ADD COLUMN {name} {definition}";command.ExecuteNonQuery();
        }
        using(var command=connection.CreateCommand())
        {
            command.Transaction=transaction;
            command.CommandText="CREATE TABLE IF NOT EXISTS Transfers (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, FromAccountId INTEGER NOT NULL, ToAccountId INTEGER NOT NULL, Cents INTEGER NOT NULL, Date TEXT NOT NULL, Description TEXT NOT NULL, FOREIGN KEY(FromAccountId) REFERENCES Accounts(Id) ON DELETE RESTRICT, FOREIGN KEY(ToAccountId) REFERENCES Accounts(Id) ON DELETE RESTRICT); CREATE TABLE IF NOT EXISTS RecurringRules (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, Description TEXT NOT NULL, Cents INTEGER NOT NULL, Type TEXT NOT NULL, AccountId INTEGER NOT NULL, CategoryId INTEGER NOT NULL, StartDate TEXT NOT NULL, Active INTEGER NOT NULL, FOREIGN KEY(AccountId) REFERENCES Accounts(Id) ON DELETE RESTRICT, FOREIGN KEY(CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT); CREATE UNIQUE INDEX IF NOT EXISTS IX_Movements_RecurringRuleId_OccurrenceDate ON Movements(RecurringRuleId,OccurrenceDate); PRAGMA user_version=4;";
            command.ExecuteNonQuery();
        }
        transaction.Commit();
    }
}
