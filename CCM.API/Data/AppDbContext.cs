using CCM.API.Models; using Microsoft.EntityFrameworkCore;
namespace CCM.API.Data;
public class AppDbContext : DbContext {
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) {}
    public DbSet<User> Users => Set<User>();
    public DbSet<CustomerRequest> CustomerRequests => Set<CustomerRequest>();
    public DbSet<ApprovalTimeline> ApprovalTimelines => Set<ApprovalTimeline>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<SapConfig> SapConfigs => Set<SapConfig>();
    protected override void OnModelCreating(ModelBuilder mb) {
        mb.Entity<CustomerRequest>().HasOne(r=>r.Submitter).WithMany().HasForeignKey(r=>r.SubmittedBy).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<CustomerRequest>().HasOne(r=>r.L1Approver).WithMany().HasForeignKey(r=>r.L1ApprovedBy).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<CustomerRequest>().HasOne(r=>r.L2Approver).WithMany().HasForeignKey(r=>r.L2ApprovedBy).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<ApprovalTimeline>().HasOne(t=>t.Request).WithMany(r=>r.Timeline).HasForeignKey(t=>t.RequestId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<CustomerRequest>().Property(r=>r.CreditLimit).HasColumnType("decimal(18,2)");
    }
}