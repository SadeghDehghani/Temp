using HrWorkflow.Models;
using Microsoft.EntityFrameworkCore;

namespace HrWorkflow.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            await db.Database.MigrateAsync();

            if (!await db.Employees.AnyAsync())
            {
                db.Employees.AddRange(
                    new Employee { FullName = "ادمین سیستم", Email = "admin@example.com", Department = "IT", Position = "Admin" },
                    new Employee { FullName = "مدیر منابع انسانی", Email = "hr.manager@example.com", Department = "HR", Position = "Manager" },
                    new Employee { FullName = "کارمند نمونه", Email = "employee@example.com", Department = "Sales", Position = "Staff" }
                );
            }

            if (!await db.ApproverGroups.AnyAsync())
            {
                var hrManagers = new ApproverGroup { Name = "HR Managers" };
                var itAdmins = new ApproverGroup { Name = "IT Admins" };
                db.ApproverGroups.AddRange(hrManagers, itAdmins);
                await db.SaveChangesAsync();

                var hrManager = await db.Employees.FirstAsync(e => e.Email == "hr.manager@example.com");
                var admin = await db.Employees.FirstAsync(e => e.Email == "admin@example.com");
                db.ApproverGroupMembers.AddRange(
                    new ApproverGroupMember { ApproverGroupId = hrManagers.Id, EmployeeId = hrManager.Id },
                    new ApproverGroupMember { ApproverGroupId = itAdmins.Id, EmployeeId = admin.Id }
                );
            }

            if (!await db.RequestTypes.AnyAsync())
            {
                db.RequestTypes.AddRange(
                    new RequestType { Name = "مرخصی", Description = "درخواست مرخصی" },
                    new RequestType { Name = "ماموریت", Description = "درخواست ماموریت" }
                );
            }

            await db.SaveChangesAsync();

            if (!await db.WorkflowDefinitions.AnyAsync())
            {
                var leaveType = await db.RequestTypes.FirstAsync(rt => rt.Name == "مرخصی");

                var def = new WorkflowDefinition
                {
                    Name = "گردش‌کار مرخصی",
                    RequestTypeId = leaveType.Id,
                    IsActive = true
                };

                var stepStart = new WorkflowStepDefinition { WorkflowDefinition = def, Name = "شروع", StepType = WorkflowStepType.Start };
                var stepManager = new WorkflowStepDefinition { WorkflowDefinition = def, Name = "تایید مدیر HR", StepType = WorkflowStepType.Task };
                var stepEnd = new WorkflowStepDefinition { WorkflowDefinition = def, Name = "پایان", StepType = WorkflowStepType.End };

                db.WorkflowStepDefinitions.AddRange(stepStart, stepManager, stepEnd);
                await db.SaveChangesAsync();

                var hrGroup = await db.ApproverGroups.FirstAsync(g => g.Name == "HR Managers");

                var t1 = new WorkflowTransitionDefinition
                {
                    WorkflowDefinition = def,
                    FromStepId = stepStart.Id,
                    ToStepId = stepManager.Id,
                    ActionName = "Submit"
                };
                var tApprove = new WorkflowTransitionDefinition
                {
                    WorkflowDefinition = def,
                    FromStepId = stepManager.Id,
                    ToStepId = stepEnd.Id,
                    ActionName = "Approve",
                    ApproverGroupId = hrGroup.Id
                };
                var tReject = new WorkflowTransitionDefinition
                {
                    WorkflowDefinition = def,
                    FromStepId = stepManager.Id,
                    ToStepId = stepEnd.Id,
                    ActionName = "Reject",
                    ApproverGroupId = hrGroup.Id
                };

                db.WorkflowDefinitions.Add(def);
                db.WorkflowTransitionDefinitions.AddRange(t1, tApprove, tReject);
                await db.SaveChangesAsync();
            }
        }
    }
}

