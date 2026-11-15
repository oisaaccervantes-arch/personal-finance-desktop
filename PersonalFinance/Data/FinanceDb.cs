using Microsoft.EntityFrameworkCore;
using PersonalFinance.Models;
using System.IO;

namespace PersonalFinance.Data;
public class FinanceDb : DbContext
{
    public static string DatabasePath { get; } = Path.Combine(Environment.GetEnvironmentVariable("PERSONAL_FINANCE_DATA_DIR") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PersonalFinance"), "finance.db");
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Movement> Movements => Set<Movement>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<AppearanceSettings> Appearance => Set<AppearanceSettings>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<RecurringRule> RecurringRules => Set<RecurringRule>();
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        options.UseSqlite($"Data Source={DatabasePath}");
    }
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Movement>().HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Movement>().HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Budget>().HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Budget>().HasIndex(x => new { x.CategoryId, x.Month }).IsUnique();
        b.Entity<Movement>().HasIndex(x => x.Date);
        b.Entity<Movement>().HasIndex(x => new { x.RecurringRuleId, x.OccurrenceDate }).IsUnique();
        b.Entity<Transfer>().HasOne(x=>x.FromAccount).WithMany().HasForeignKey(x=>x.FromAccountId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Transfer>().HasOne(x=>x.ToAccount).WithMany().HasForeignKey(x=>x.ToAccountId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<RecurringRule>().HasOne(x=>x.Account).WithMany().HasForeignKey(x=>x.AccountId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<RecurringRule>().HasOne(x=>x.Category).WithMany().HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
