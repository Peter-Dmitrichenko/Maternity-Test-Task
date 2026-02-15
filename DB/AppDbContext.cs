using DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

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
    }
}
