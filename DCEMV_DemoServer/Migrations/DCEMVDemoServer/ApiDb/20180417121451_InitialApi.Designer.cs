using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using DCEMV.DemoServer.Persistence.Api;
using DCEMV.ServerShared;

namespace DCEMV.DemoServer.Migrations.DCEMVDemoServer.ApiDb
{
    [DbContext(typeof(ApiDbContext))]
    [Migration("20180417121451_InitialApi")]
    partial class InitialApi
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.AccountPM", b =>
                {
                    b.Property<string>("AccountNumberId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountState");

                    b.Property<long>("Balance");

                    b.Property<DateTime>("BalanceUpdateTime");

                    b.Property<string>("BusinessName")
                        .HasMaxLength(200);

                    b.Property<string>("CompanyRegNumber")
                        .HasMaxLength(200);

                    b.Property<string>("CredentialsId")
                        .IsRequired();

                    b.Property<int>("CustomerType");

                    b.Property<string>("FirstName")
                        .HasMaxLength(200);

                    b.Property<string>("LastName")
                        .HasMaxLength(200);

                    b.Property<string>("TaxNumber")
                        .HasMaxLength(200);

                    b.HasKey("AccountNumberId");

                    b.ToTable("DCEMV_Accounts");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.CardPM", b =>
                {
                    b.Property<int>("CardId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountNumberIdRef")
                        .IsRequired();

                    b.Property<long>("AvailableMonthlySpendLimit");

                    b.Property<long>("AvailablegDailySpendLimit");

                    b.Property<string>("CardSerialNumberId");

                    b.Property<int>("CardState");

                    b.Property<long>("DailySpendLimit");

                    b.Property<string>("FreindlyName")
                        .HasMaxLength(200);

                    b.Property<long>("MonthlySpendLimit");

                    b.HasKey("CardId");

                    b.HasIndex("AccountNumberIdRef");

                    b.HasIndex("CardSerialNumberId")
                        .IsUnique();

                    b.ToTable("DCEMV_Cards");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.CCTopUpTransactionPM", b =>
                {
                    b.Property<int>("TopUpTransactionId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountNumberIdToRef")
                        .IsRequired();

                    b.Property<long>("Amount");

                    b.Property<string>("EMV_Data")
                        .IsRequired();

                    b.Property<DateTime>("TransactionDateTime");

                    b.HasKey("TopUpTransactionId");

                    b.HasIndex("AccountNumberIdToRef");

                    b.ToTable("DCEMV_CCTopUpTransactions");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.InventoryGroupPM", b =>
                {
                    b.Property<int>("InventoryGroupId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountNumberIdRef")
                        .IsRequired();

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("InventoryGroupId");

                    b.HasIndex("AccountNumberIdRef");

                    b.ToTable("DCEMV_InventoryGroups");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.InventoryItemPM", b =>
                {
                    b.Property<int>("InventoryItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountNumberIdRef")
                        .IsRequired();

                    b.Property<string>("Barcode")
                        .IsRequired();

                    b.Property<string>("Description")
                        .IsRequired();

                    b.Property<int>("InventoryGroupIdRef");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<long>("Price");

                    b.HasKey("InventoryItemId");

                    b.HasIndex("AccountNumberIdRef");

                    b.HasIndex("InventoryGroupIdRef");

                    b.ToTable("DCEMV_InventoryItems");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.POSTransactionItemPM", b =>
                {
                    b.Property<int>("POSTransactionItemId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Amount");

                    b.Property<int>("InventoryItemId");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("POSTransactionIdRef");

                    b.Property<int>("Quantity");

                    b.HasKey("POSTransactionItemId");

                    b.HasIndex("POSTransactionIdRef");

                    b.ToTable("DCEMV_POSTransactionItems");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.POSTransactionPM", b =>
                {
                    b.Property<int>("POSTransactionId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountNumberIdFromRef")
                        .IsRequired();

                    b.Property<string>("AccountNumberIdToRef")
                        .IsRequired();

                    b.Property<DateTime>("TransactionDateTime");

                    b.Property<int>("TransactionIdRef");

                    b.HasKey("POSTransactionId");

                    b.HasIndex("AccountNumberIdFromRef");

                    b.HasIndex("AccountNumberIdToRef");

                    b.HasIndex("TransactionIdRef")
                        .IsUnique();

                    b.ToTable("DCEMV_POSTransactions");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.TransactionPM", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountNumberIdFromRef")
                        .IsRequired();

                    b.Property<string>("AccountNumberIdToRef")
                        .IsRequired();

                    b.Property<long>("Amount");

                    b.Property<string>("CardFromEMVData")
                        .IsRequired();

                    b.Property<string>("CardSerialNumberIdFrom");

                    b.Property<string>("CardSerialNumberIdTo");

                    b.Property<DateTime>("TransactionDateTime");

                    b.Property<int>("TransactionType");

                    b.HasKey("TransactionId");

                    b.HasIndex("AccountNumberIdFromRef");

                    b.HasIndex("AccountNumberIdToRef");

                    b.ToTable("DCEMV_Transactions");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.CardPM", b =>
                {
                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.AccountPM", "Account")
                        .WithMany("Cards")
                        .HasForeignKey("AccountNumberIdRef");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.CCTopUpTransactionPM", b =>
                {
                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.AccountPM", "AccountTo")
                        .WithMany("CCTopUpTransactions")
                        .HasForeignKey("AccountNumberIdToRef");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.InventoryGroupPM", b =>
                {
                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.AccountPM", "Account")
                        .WithMany("InventoryGroups")
                        .HasForeignKey("AccountNumberIdRef");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.InventoryItemPM", b =>
                {
                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.AccountPM", "Account")
                        .WithMany("InventoryItems")
                        .HasForeignKey("AccountNumberIdRef");

                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.InventoryGroupPM", "Group")
                        .WithMany("InventoryItems")
                        .HasForeignKey("InventoryGroupIdRef");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.POSTransactionItemPM", b =>
                {
                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.POSTransactionPM", "POSTransaction")
                        .WithMany("POSTransactionItems")
                        .HasForeignKey("POSTransactionIdRef");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.POSTransactionPM", b =>
                {
                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.AccountPM", "AccountFrom")
                        .WithMany("POSTransactionsFrom")
                        .HasForeignKey("AccountNumberIdFromRef");

                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.AccountPM", "AccountTo")
                        .WithMany("POSTransactionsTo")
                        .HasForeignKey("AccountNumberIdToRef");

                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.TransactionPM", "Transaction")
                        .WithOne("POSTransaction")
                        .HasForeignKey("DCEMV.DemoServer.Persistence.Api.Entities.POSTransactionPM", "TransactionIdRef");
                });

            modelBuilder.Entity("DCEMV.DemoServer.Persistence.Api.Entities.TransactionPM", b =>
                {
                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.AccountPM", "AccountFrom")
                        .WithMany("TransactionsFrom")
                        .HasForeignKey("AccountNumberIdFromRef");

                    b.HasOne("DCEMV.DemoServer.Persistence.Api.Entities.AccountPM", "AccountTo")
                        .WithMany("TransactionsTo")
                        .HasForeignKey("AccountNumberIdToRef");
                });
        }
    }
}
