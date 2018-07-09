/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

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
                    AccountState = table.Column<int>(nullable: false),
                    Balance = table.Column<long>(nullable: false),
                    BalanceUpdateTime = table.Column<DateTime>(nullable: false),
                    BusinessName = table.Column<string>(maxLength: 200, nullable: true),
                    CompanyRegNumber = table.Column<string>(maxLength: 200, nullable: true),
                    CredentialsId = table.Column<string>(nullable: false),
                    CustomerType = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 200, nullable: true),
                    LastName = table.Column<string>(maxLength: 200, nullable: true),
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
                    AccountNumberIdRef = table.Column<string>(nullable: false),
                    AvailableMonthlySpendLimit = table.Column<long>(nullable: false),
                    AvailablegDailySpendLimit = table.Column<long>(nullable: false),
                    CardSerialNumberId = table.Column<string>(nullable: true),
                    CardState = table.Column<int>(nullable: false),
                    DailySpendLimit = table.Column<long>(nullable: false),
                    FreindlyName = table.Column<string>(maxLength: 200, nullable: true),
                    MonthlySpendLimit = table.Column<long>(nullable: false)
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
                    AccountNumberIdToRef = table.Column<string>(nullable: false),
                    Amount = table.Column<long>(nullable: false),
                    EMV_Data = table.Column<string>(nullable: false),
                    TransactionDateTime = table.Column<DateTime>(nullable: false)
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
                    AccountNumberIdRef = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false)
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
                    AccountNumberIdFromRef = table.Column<string>(nullable: false),
                    AccountNumberIdToRef = table.Column<string>(nullable: false),
                    Amount = table.Column<long>(nullable: false),
                    CardFromEMVData = table.Column<string>(nullable: false),
                    CardSerialNumberIdFrom = table.Column<string>(nullable: true),
                    CardSerialNumberIdTo = table.Column<string>(nullable: true),
                    TransactionDateTime = table.Column<DateTime>(nullable: false),
                    TransactionType = table.Column<int>(nullable: false)
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
                    AccountNumberIdRef = table.Column<string>(nullable: false),
                    Barcode = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    InventoryGroupIdRef = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Price = table.Column<long>(nullable: false)
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
                    AccountNumberIdFromRef = table.Column<string>(nullable: false),
                    AccountNumberIdToRef = table.Column<string>(nullable: false),
                    TransactionDateTime = table.Column<DateTime>(nullable: false),
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
                    Amount = table.Column<long>(nullable: false),
                    InventoryItemId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    POSTransactionIdRef = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false)
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
                unique: true);

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
