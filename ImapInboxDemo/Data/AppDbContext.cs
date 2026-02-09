using ImapInboxDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;

namespace ImapInboxDemo.Data
{
    public class AppDbContext : DbContext
    {
        //This is how EF Core gets the configuration for connecting to your database (connection string, provider, etc.).
        //You don’t do anything special here — you just pass the options to the base class.
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        //“Create a table called EmailMessages in the database, and each row will be an EmailMessage  object.”
        //A DbSet represents a table.
        //An EmailMessage represents a row in that table.
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<SyncState> SyncState { get; set; }




        //This method lets you customize how EF Core builds the database.
        /**This tells EF Core:

In everyday terms:
• 	Index = makes lookups faster
• 	Unique = prevents duplicates**/
        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SyncState>(entity =>
            {
                entity.ToTable("SyncState");
                entity.HasKey(x => x.Id);
            });

            modelBuilder.Entity<EmailMessage>(entity =>
            {
                entity.HasIndex(e => e.MessageId).HasDatabaseName("IX_EmailMessages_MessageId");
            });

            modelBuilder.Entity<EmailMessage>()
        .HasIndex(e => e.MessageId)
        .IsUnique();
}
}
}
