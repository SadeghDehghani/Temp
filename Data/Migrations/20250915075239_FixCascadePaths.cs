using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrWorkflow.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Requests_RequestId",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_WorkflowDefinitions_WorkflowDefinitionId",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowTransitionDefinitions_WorkflowDefinitions_WorkflowDefinitionId",
                table: "WorkflowTransitionDefinitions");

            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                table: "Requests");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Requests_RequestId",
                table: "WorkflowInstances",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_WorkflowDefinitions_WorkflowDefinitionId",
                table: "WorkflowInstances",
                column: "WorkflowDefinitionId",
                principalTable: "WorkflowDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowTransitionDefinitions_WorkflowDefinitions_WorkflowDefinitionId",
                table: "WorkflowTransitionDefinitions",
                column: "WorkflowDefinitionId",
                principalTable: "WorkflowDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_Requests_RequestId",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowInstances_WorkflowDefinitions_WorkflowDefinitionId",
                table: "WorkflowInstances");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkflowTransitionDefinitions_WorkflowDefinitions_WorkflowDefinitionId",
                table: "WorkflowTransitionDefinitions");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowInstanceId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_Requests_RequestId",
                table: "WorkflowInstances",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowInstances_WorkflowDefinitions_WorkflowDefinitionId",
                table: "WorkflowInstances",
                column: "WorkflowDefinitionId",
                principalTable: "WorkflowDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkflowTransitionDefinitions_WorkflowDefinitions_WorkflowDefinitionId",
                table: "WorkflowTransitionDefinitions",
                column: "WorkflowDefinitionId",
                principalTable: "WorkflowDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
