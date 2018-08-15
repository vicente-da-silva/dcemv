using Microsoft.EntityFrameworkCore.Migrations;

namespace DCEMV.DemoServer.Migrations.DCEMVDemoServer.ApiDb
{
    public partial class ApiDbContext_V2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TrackingId",
                table: "DCEMV_Transactions",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardFromEMVData",
                table: "DCEMV_Transactions",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_Transactions_TrackingId",
                table: "DCEMV_Transactions",
                column: "TrackingId",
                unique: true,
                filter: "[TrackingId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DCEMV_Transactions_TrackingId",
                table: "DCEMV_Transactions");

            migrationBuilder.AlterColumn<string>(
                name: "TrackingId",
                table: "DCEMV_Transactions",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardFromEMVData",
                table: "DCEMV_Transactions",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
