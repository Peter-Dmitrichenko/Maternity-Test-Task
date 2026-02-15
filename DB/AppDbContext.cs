using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Name> Names { get; set; }
		public DbSet<GenderLookup> Genders { get; set; }
		public DbSet<ActiveLookup> Actives { get; set; }
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patients");
                entity.HasKey(e => e.Id);
                modelBuilder.Entity<Patient>();
				entity.Property(e => e.GenderId).HasMaxLength(50).IsUnicode(false).HasConversion<string>();
                entity.Property(e => e.BirthDate).HasColumnType("datetime2");
                entity.Property(e => e.ActiveId).IsRequired();

                // One-to-one relationship: Patient <-> Name
                entity.HasOne(e => e.Name)
                      .WithOne(n => n.Patient)
                      .HasForeignKey<Name>(n => n.PatientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Name>(entity =>
            {
                entity.ToTable("Names");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Use).HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.Family).HasMaxLength(200).IsUnicode(true);

                var givenConverter = new ValueConverter<List<string>, string>(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());

                entity.Property(e => e.Given)
                      .HasColumnName("GivenJson")
                      .HasColumnType("nvarchar(max)")
                      .HasConversion(givenConverter);

                entity.HasIndex(e => e.PatientId).HasDatabaseName("IX_Names_PatientId");
            });

			modelBuilder.Entity<GenderLookup>().HasIndex(g => g.Code).IsUnique(); 
            modelBuilder.Entity<ActiveLookup>().HasIndex(a => a.Code).IsUnique();
		}

        /// <summary>
        /// Ensure the database schema exists. If migrations are not present, falls back to EnsureCreatedAsync.
        /// Call this at application startup before other services use the DB.
        /// </summary>
        public static async Task InitializeAsync(IServiceProvider services,
                                                 bool preferMigrate = false,
                                                 int maxRetries = 5,
                                                 CancellationToken cancellationToken = default)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var logger = services.GetService<ILogger<AppDbContext>>() ??
                         services.GetService<ILoggerFactory>()?.CreateLogger("AppDbContext.Init");

            var attempt = 0;
            var delay = TimeSpan.FromSeconds(2);

            while (true)
            {
                attempt++;
                try
                {
                    using var scope = services.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    logger?.LogInformation("DB init: checking connectivity (attempt {Attempt})...", attempt);

                    // If DB server is not reachable, CanConnectAsync may return false.
                    // We'll still attempt EnsureCreated/Migrate below, but this check gives early diagnostics.
                    var canConnect = await context.Database.CanConnectAsync(cancellationToken);
                    if (!canConnect)
                    {
                        logger?.LogWarning("DB init: CanConnectAsync returned false. Will attempt to create/migrate schema.");
                    }

                    // If preferMigrate is true and there are migrations in the assembly, apply them.
                    // Otherwise fall back to EnsureCreated.
                    var migrations = context.Database.GetMigrations();
                    var hasMigrations = migrations != null && migrations.Any();

                    if (preferMigrate && hasMigrations)
                    {
                        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
                        logger?.LogInformation("DB init: migrations found ({Total} total, {Pending} pending). Applying migrations...", migrations?.Count(), pending.Count);
                        await context.Database.MigrateAsync(cancellationToken);
                        logger?.LogInformation("DB init: migrations applied.");
                    }
                    else
                    {
                        // No migrations or migrations not preferred: create schema from model
                        logger?.LogInformation("DB init: creating database schema from model (EnsureCreated)...");
                        var created = await context.Database.EnsureCreatedAsync(cancellationToken);
                        logger?.LogInformation(created ? "DB init: database created." : "DB init: database already exists.");
                    }

                    logger?.LogInformation("DB init: initialization completed successfully.");
                    break;
                }
                catch (Exception ex) when (attempt <= maxRetries && !cancellationToken.IsCancellationRequested)
                {
                    logger?.LogWarning(ex, "DB init: attempt {Attempt} failed. Retrying in {Delay}...", attempt, delay);
                    await Task.Delay(delay, cancellationToken);
                    delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 30000)); // cap backoff
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "DB init: initialization failed and will not be retried.");
                    throw;
                }
            }
        }
    }
}
