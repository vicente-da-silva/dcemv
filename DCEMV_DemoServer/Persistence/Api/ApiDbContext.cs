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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using DCEMV.DemoServer.Persistence.Api.Entities;

namespace DCEMV.DemoServer.Persistence.Api
{
    public class ApiDbContext : DbContext
    {
        public DbSet<AccountPM> Accounts { get; set; }
        public DbSet<TransactionPM> Transactions { get; set; }
        public DbSet<CardPM> Cards { get; set; }
        public DbSet<CCTopUpTransactionPM> TopUpTransactions { get; set; }
        public DbSet<InventoryItemPM> InventoryItems { get; set; }
        public DbSet<InventoryGroupPM> InventoryGroups { get; set; }
        public DbSet<POSTransactionPM> POSTransactions { get; set; }
        public DbSet<POSTransactionItemPM> POSTransactionItems { get; set; }

        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) 
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<AccountPM>().ToTable("DCEMV_Accounts");
            modelBuilder.Entity<CardPM>().ToTable("DCEMV_Cards");
            modelBuilder.Entity<CCTopUpTransactionPM>().ToTable("DCEMV_CCTopUpTransactions");
            modelBuilder.Entity<InventoryGroupPM>().ToTable("DCEMV_InventoryGroups");
            modelBuilder.Entity<InventoryItemPM>().ToTable("DCEMV_InventoryItems");
            modelBuilder.Entity<POSTransactionPM>().ToTable("DCEMV_POSTransactions");
            modelBuilder.Entity<POSTransactionItemPM>().ToTable("DCEMV_POSTransactionItems");
            modelBuilder.Entity<TransactionPM>().ToTable("DCEMV_Transactions");

            modelBuilder.Entity<InventoryItemPM>()
                .HasOne(p => p.Account)
                .WithMany(p => p.InventoryItems)
                .HasForeignKey(p => p.AccountNumberIdRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryItemPM>()
                .HasOne(p => p.Group)
                .WithMany(p => p.InventoryItems)
                .HasForeignKey(p => p.InventoryGroupIdRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InventoryGroupPM>()
                .HasOne(p => p.Account)
                .WithMany(p => p.InventoryGroups)
                .HasForeignKey(p => p.AccountNumberIdRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CCTopUpTransactionPM>()
                .HasOne(p => p.AccountTo)
                .WithMany(p => p.CCTopUpTransactions)
                .HasForeignKey(p => p.AccountNumberIdToRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CardPM>()
                .HasOne(p => p.Account)
                .WithMany(p => p.Cards)
                .HasForeignKey(p=>p.AccountNumberIdRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionPM>()
                .HasOne(p => p.AccountFrom)
                .WithMany(p => p.TransactionsFrom)
                .HasForeignKey(p => p.AccountNumberIdFromRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionPM>()
                .HasOne(p => p.AccountTo)
                .WithMany(p => p.TransactionsTo)
                .HasForeignKey(p => p.AccountNumberIdToRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<POSTransactionPM>()
                .HasOne(p => p.AccountTo)
                .WithMany(p => p.POSTransactionsTo)
                .HasForeignKey(p => p.AccountNumberIdToRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<POSTransactionPM>()
                .HasOne(p => p.AccountFrom)
                .WithMany(p => p.POSTransactionsFrom)
                .HasForeignKey(p => p.AccountNumberIdFromRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionPM>()
                .HasOne(p => p.POSTransaction)
                .WithOne(p => p.Transaction)
                .HasForeignKey<POSTransactionPM>(p=>p.TransactionIdRef)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<POSTransactionItemPM>()
               .HasOne(p => p.POSTransaction)
               .WithMany(p => p.POSTransactionItems)
               .HasForeignKey(p => p.POSTransactionIdRef)
               //.IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CardPM>()
                .HasIndex(p => p.CardSerialNumberId)
                .IsUnique(true);

        }
    }
}
