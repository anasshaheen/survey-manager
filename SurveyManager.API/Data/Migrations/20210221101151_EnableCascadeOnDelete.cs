using Microsoft.EntityFrameworkCore.Migrations;

namespace SurveyManager.API.Migrations
{
    public partial class EnableCascadeOnDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyShares_Surveys_SurveyId",
                table: "SurveyShares");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyShares_Surveys_SurveyId",
                table: "SurveyShares",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SurveyShares_Surveys_SurveyId",
                table: "SurveyShares");

            migrationBuilder.AddForeignKey(
                name: "FK_SurveyShares_Surveys_SurveyId",
                table: "SurveyShares",
                column: "SurveyId",
                principalTable: "Surveys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
