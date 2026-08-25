using DotNet_App.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNet_App.Data;

public sealed class RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var user = modelBuilder.Entity<User>();
        user.HasKey(item => item.Id);
        user.HasIndex(item => item.Username).IsUnique();
        user.HasIndex(item => item.Email).IsUnique();
        user.Property(item => item.Username).HasMaxLength(50).IsRequired();
        user.Property(item => item.PasswordHash).HasMaxLength(500).IsRequired();
        user.Property(item => item.Email).HasMaxLength(254).IsRequired();
        user.Property(item => item.Gender).HasMaxLength(30).IsRequired();
    }
}
