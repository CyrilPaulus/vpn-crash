﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using vpn_crash.DB;

namespace vpn_crash.Migrations
{
    [DbContext(typeof(VPNCrashDB))]
    partial class VPNCrashDBModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3");

            modelBuilder.Entity("vpn_crash.DB.CrashEntry", b =>
                {
                    b.Property<ulong>("MessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("User")
                        .HasColumnType("TEXT");

                    b.HasKey("MessageId");

                    b.ToTable("CrashEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
