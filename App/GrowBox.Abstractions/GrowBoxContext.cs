using GrowBox.Abstractions.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GrowBox.Abstractions;

public class DateTimeToDateTimeUtc()
    : ValueConverter<DateTime, DateTime>(
        c => DateTime.SpecifyKind(c, DateTimeKind.Utc),
        c => c);
public class DateTimeToDateTimeUtcN()
    : ValueConverter<DateTime?, DateTime?>(
        c => c == null ? null : DateTime.SpecifyKind(c.Value, DateTimeKind.Utc),
        c => c);

public class GrowBoxContext(DbContextOptions<GrowBoxContext> options) : DbContext(options: options)
{
    public DbSet<Model.GrowBox> GrowBoxes { get; set; } = default!;
    public DbSet<SensorReading> SensorReadings { get; set; } = default!;

    public DbSet<Grow> Grows { get; set; } = default!;
    public DbSet<GrowDiaryNote> GrowDiaryNotes { get; set; } = default!;
    public DbSet<GrowDiaryNoteMedia> GrowDiaryNoteMedias { get; set; } = default!;

    public bool IsCreationMarkingEnabled { get; set; } = true;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
    {   
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>()
            .HaveConversion(typeof(DateTimeToDateTimeUtc));
        configurationBuilder.Properties<DateTime?>()
            .HaveConversion(typeof(DateTimeToDateTimeUtcN));
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<SensorReading>()
            .HasOne<Model.GrowBox>()
            .WithMany()
            .HasForeignKey(x => x.GrowBoxId);

        mb.Entity<Grow>()
            .HasOne<Model.GrowBox>()
            .WithMany()
            .HasForeignKey(x => x.GrowBoxId);
        
        mb.Entity<GrowDiaryNote>()
            .HasOne<Grow>()
            .WithMany(x => x.GrowDiaryNotes)
            .HasForeignKey(x => x.GrowId);
        
        mb.Entity<GrowDiaryNoteMedia>()
            .HasOne<GrowDiaryNote>()
            .WithMany(x => x.GrowDiaryNoteMedias)
            .HasForeignKey(x => x.GrowDiaryNoteId);

        base.OnModelCreating(mb);
    }

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
        if (IsCreationMarkingEnabled)
        {
            var markCreatedEntities = ChangeTracker.Entries()
                .Where(x => x is { State: EntityState.Added, Entity: IEntityCreated })
                .Select(x => (IEntityCreated)x.Entity);

            foreach (var entity in markCreatedEntities)
            {
                if (entity.Created == DateTime.MinValue)
                    entity.Created = DateTime.UtcNow;
            }
        }

        var markUpdatedEntities = ChangeTracker.Entries()
            .Where(x => x is  { State: EntityState.Modified or EntityState.Added, Entity: IEntityUpdated })
            .Select(x => (IEntityUpdated)x.Entity);

		foreach (var entity in markUpdatedEntities)
		{
            entity.Updated = DateTime.UtcNow;
		}

		return base.SaveChangesAsync(cancellationToken);
	}
}
