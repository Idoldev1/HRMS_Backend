using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRMS.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicJobPostingIdAndNormalizedApplicationEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobApplications_JobPostingId_CandidateEmail",
                table: "JobApplications");

            migrationBuilder.AddColumn<string>(
                name: "JobPostingId",
                table: "JobPostings",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedCandidateEmail",
                table: "JobApplications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                @"UPDATE [JobPostings]
                  SET [JobPostingId] = CONVERT(nvarchar(36), NEWID())
                  WHERE [JobPostingId] = '';");

            migrationBuilder.Sql(
                @"UPDATE [JobApplications]
                  SET [NormalizedCandidateEmail] = LOWER(LTRIM(RTRIM([CandidateEmail])))
                  WHERE [NormalizedCandidateEmail] = '';");

            migrationBuilder.CreateIndex(
                name: "IX_JobPostings_JobPostingId",
                table: "JobPostings",
                column: "JobPostingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobPostingId_NormalizedCandidateEmail",
                table: "JobApplications",
                columns: new[] { "JobPostingId", "NormalizedCandidateEmail" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobPostings_JobPostingId",
                table: "JobPostings");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_JobPostingId_NormalizedCandidateEmail",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "JobPostingId",
                table: "JobPostings");

            migrationBuilder.DropColumn(
                name: "NormalizedCandidateEmail",
                table: "JobApplications");

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_JobPostingId_CandidateEmail",
                table: "JobApplications",
                columns: new[] { "JobPostingId", "CandidateEmail" },
                unique: true);
        }
    }
}
