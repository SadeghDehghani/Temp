using HrWorkflow.Models;
using Microsoft.EntityFrameworkCore;

namespace HrWorkflow.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<ApproverGroup> ApproverGroups => Set<ApproverGroup>();
        public DbSet<ApproverGroupMember> ApproverGroupMembers => Set<ApproverGroupMember>();
        public DbSet<RequestType> RequestTypes => Set<RequestType>();
        public DbSet<Request> Requests => Set<Request>();
        public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
        public DbSet<WorkflowStepDefinition> WorkflowStepDefinitions => Set<WorkflowStepDefinition>();
        public DbSet<WorkflowTransitionDefinition> WorkflowTransitionDefinitions => Set<WorkflowTransitionDefinition>();
        public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
        public DbSet<WorkflowStepInstance> WorkflowStepInstances => Set<WorkflowStepInstance>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApproverGroupMember>()
                .HasKey(x => new { x.ApproverGroupId, x.EmployeeId });

            modelBuilder.Entity<ApproverGroupMember>()
                .HasOne(x => x.ApproverGroup)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.ApproverGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApproverGroupMember>()
                .HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkflowTransitionDefinition>()
                .HasOne(x => x.FromStep)
                .WithMany(x => x.OutgoingTransitions)
                .HasForeignKey(x => x.FromStepId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkflowTransitionDefinition>()
                .HasOne(x => x.ToStep)
                .WithMany(x => x.IncomingTransitions)
                .HasForeignKey(x => x.ToStepId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowInstance>()
                .HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowTransitionDefinition>()
                .HasOne(t => t.WorkflowDefinition)
                .WithMany(d => d.Transitions)
                .HasForeignKey(t => t.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowInstance>()
                .HasOne(x => x.CurrentStep)
                .WithMany()
                .HasForeignKey(x => x.CurrentStepId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkflowStepInstance>()
                .HasOne(x => x.AssignedApproverGroup)
                .WithMany()
                .HasForeignKey(x => x.AssignedApproverGroupId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Request>()
                .HasOne(x => x.WorkflowInstance)
                .WithOne(x => x.Request)
                .HasForeignKey<WorkflowInstance>(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

