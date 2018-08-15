using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DCEMV.DemoServer.Migrations.DCEMVDemoServer.ApiDb
{
    public partial class InitialApi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DCEMV_Accounts",
                columns: table => new
                {
                    AccountNumberId = table.Column<string>(nullable: false),
                    CredentialsId = table.Column<string>(nullable: false),
                    Balance = table.Column<long>(nullable: false),
                    BalanceUpdateTime = table.Column<DateTime>(nullable: false),
                    CustomerType = table.Column<int>(nullable: false),
                    AccountState = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 200, nullable: true),
                    LastName = table.Column<string>(maxLength: 200, nullable: true),
                    BusinessName = table.Column<string>(maxLength: 200, nullable: true),
                    CompanyRegNumber = table.Column<string>(maxLength: 200, nullable: true),
                    TaxNumber = table.Column<string>(maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCEMV_Accounts", x => x.AccountNumberId);
                });

            migrationBuilder.CreateTable(
                name: "DCEMV_Cards",
                columns: table => new
                {
                    CardId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CardSerialNumberId = table.Column<string>(nullable: true),
                    FreindlyName = table.Column<string>(maxLength: 200, nullable: true),
                    DailySpendLimit = table.Column<long>(nullable: false),
                    AvailablegDailySpendLimit = table.Column<long>(nullable: false),
                    MonthlySpendLimit = table.Column<long>(nullable: false),
                    AvailableMonthlySpendLimit = table.Column<long>(nullable: false),
                    CardState = table.Column<int>(nullable: false),
                    AccountNumberIdRef = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCEMV_Cards", x => x.CardId);
                    table.ForeignKey(
                        name: "FK_DCEMV_Cards_DCEMV_Accounts_AccountNumberIdRef",
                        column: x => x.AccountNumberIdRef,
                        principalTable: "DCEMV_Accounts",
                        principalColumn: "AccountNumberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DCEMV_CCTopUpTransactions",
                columns: table => new
                {
                    TopUpTransactionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<long>(nullable: false),
                    TransactionDateTime = table.Column<DateTime>(nullable: false),
                    EMV_Data = table.Column<string>(nullable: false),
                    AccountNumberIdToRef = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCEMV_CCTopUpTransactions", x => x.TopUpTransactionId);
                    table.ForeignKey(
                        name: "FK_DCEMV_CCTopUpTransactions_DCEMV_Accounts_AccountNumberIdToRef",
                        column: x => x.AccountNumberIdToRef,
                        principalTable: "DCEMV_Accounts",
                        principalColumn: "AccountNumberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DCEMV_InventoryGroups",
                columns: table => new
                {
                    InventoryGroupId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    AccountNumberIdRef = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCEMV_InventoryGroups", x => x.InventoryGroupId);
                    table.ForeignKey(
                        name: "FK_DCEMV_InventoryGroups_DCEMV_Accounts_AccountNumberIdRef",
                        column: x => x.AccountNumberIdRef,
                        principalTable: "DCEMV_Accounts",
                        principalColumn: "AccountNumberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DCEMV_Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CardSerialNumberIdFrom = table.Column<string>(nullable: true),
                    CardSerialNumberIdTo = table.Column<string>(nullable: true),
                    Amount = table.Column<long>(nullable: false),
                    TransactionDateTime = table.Column<DateTime>(nullable: false),
                    TransactionType = table.Column<int>(nullable: false),
                    CardFromEMVData = table.Column<string>(nullable: false),
                    TrackingId = table.Column<string>(nullable: true),
                    AccountNumberIdFromRef = table.Column<string>(nullable: false),
                    AccountNumberIdToRef = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCEMV_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_DCEMV_Transactions_DCEMV_Accounts_AccountNumberIdFromRef",
                        column: x => x.AccountNumberIdFromRef,
                        principalTable: "DCEMV_Accounts",
                        principalColumn: "AccountNumberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DCEMV_Transactions_DCEMV_Accounts_AccountNumberIdToRef",
                        column: x => x.AccountNumberIdToRef,
                        principalTable: "DCEMV_Accounts",
                        principalColumn: "AccountNumberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DCEMV_InventoryItems",
                columns: table => new
                {
                    InventoryItemId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Barcode = table.Column<string>(nullable: false),
                    Price = table.Column<long>(nullable: false),
                    InventoryGroupIdRef = table.Column<int>(nullable: false),
                    AccountNumberIdRef = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCEMV_InventoryItems", x => x.InventoryItemId);
                    table.ForeignKey(
                        name: "FK_DCEMV_InventoryItems_DCEMV_Accounts_AccountNumberIdRef",
                        column: x => x.AccountNumberIdRef,
                        principalTable: "DCEMV_Accounts",
                        principalColumn: "AccountNumberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DCEMV_InventoryItems_DCEMV_InventoryGroups_InventoryGroupIdRef",
                        column: x => x.InventoryGroupIdRef,
                        principalTable: "DCEMV_InventoryGroups",
                        principalColumn: "InventoryGroupId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DCEMV_POSTransactions",
                columns: table => new
                {
                    POSTransactionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TransactionDateTime = table.Column<DateTime>(nullable: false),
                    AccountNumberIdToRef = table.Column<string>(nullable: false),
                    AccountNumberIdFromRef = table.Column<string>(nullable: false),
                    TransactionIdRef = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCEMV_POSTransactions", x => x.POSTransactionId);
                    table.ForeignKey(
                        name: "FK_DCEMV_POSTransactions_DCEMV_Accounts_AccountNumberIdFromRef",
                        column: x => x.AccountNumberIdFromRef,
                        principalTable: "DCEMV_Accounts",
                        principalColumn: "AccountNumberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DCEMV_POSTransactions_DCEMV_Accounts_AccountNumberIdToRef",
                        column: x => x.AccountNumberIdToRef,
                        principalTable: "DCEMV_Accounts",
                        principalColumn: "AccountNumberId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DCEMV_POSTransactions_DCEMV_Transactions_TransactionIdRef",
                        column: x => x.TransactionIdRef,
                        principalTable: "DCEMV_Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DCEMV_POSTransactionItems",
                columns: table => new
                {
                    POSTransactionItemId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Quantity = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    InventoryItemId = table.Column<int>(nullable: false),
                    Amount = table.Column<long>(nullable: false),
                    POSTransactionIdRef = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DCEMV_POSTransactionItems", x => x.POSTransactionItemId);
                    table.ForeignKey(
                        name: "FK_DCEMV_POSTransactionItems_DCEMV_POSTransactions_POSTransactionIdRef",
                        column: x => x.POSTransactionIdRef,
                        principalTable: "DCEMV_POSTransactions",
                        principalColumn: "POSTransactionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_Cards_AccountNumberIdRef",
                table: "DCEMV_Cards",
                column: "AccountNumberIdRef");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_Cards_CardSerialNumberId",
                table: "DCEMV_Cards",
                column: "CardSerialNumberId",
                unique: true,
                filter: "[CardSerialNumberId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_CCTopUpTransactions_AccountNumberIdToRef",
                table: "DCEMV_CCTopUpTransactions",
                column: "AccountNumberIdToRef");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_InventoryGroups_AccountNumberIdRef",
                table: "DCEMV_InventoryGroups",
                column: "AccountNumberIdRef");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_InventoryItems_AccountNumberIdRef",
                table: "DCEMV_InventoryItems",
                column: "AccountNumberIdRef");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_InventoryItems_InventoryGroupIdRef",
                table: "DCEMV_InventoryItems",
                column: "InventoryGroupIdRef");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_POSTransactionItems_POSTransactionIdRef",
                table: "DCEMV_POSTransactionItems",
                column: "POSTransactionIdRef");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_POSTransactions_AccountNumberIdFromRef",
                table: "DCEMV_POSTransactions",
                column: "AccountNumberIdFromRef");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_POSTransactions_AccountNumberIdToRef",
                table: "DCEMV_POSTransactions",
                column: "AccountNumberIdToRef");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_POSTransactions_TransactionIdRef",
                table: "DCEMV_POSTransactions",
                column: "TransactionIdRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_Transactions_AccountNumberIdFromRef",
                table: "DCEMV_Transactions",
                column: "AccountNumberIdFromRef");

            migrationBuilder.CreateIndex(
                name: "IX_DCEMV_Transactions_AccountNumberIdToRef",
                table: "DCEMV_Transactions",
                column: "AccountNumberIdToRef");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DCEMV_Cards");

            migrationBuilder.DropTable(
                name: "DCEMV_CCTopUpTransactions");

            migrationBuilder.DropTable(
                name: "DCEMV_InventoryItems");

            migrationBuilder.DropTable(
                name: "DCEMV_POSTransactionItems");

            migrationBuilder.DropTable(
                name: "DCEMV_InventoryGroups");

            migrationBuilder.DropTable(
                name: "DCEMV_POSTransactions");

            migrationBuilder.DropTable(
                name: "DCEMV_Transactions");

            migrationBuilder.DropTable(
                name: "DCEMV_Accounts");
        }
    }
}
