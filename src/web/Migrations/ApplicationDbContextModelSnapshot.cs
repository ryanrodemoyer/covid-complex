﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using web.Data;

namespace web.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7");

            modelBuilder.Entity("web.Data.County", b =>
                {
                    b.Property<int>("CountyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CountyName")
                        .HasColumnType("TEXT");

                    b.Property<int>("Fips")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Population")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StateId")
                        .HasColumnType("INTEGER");

                    b.HasKey("CountyId");

                    b.HasIndex("StateId");

                    b.ToTable("Counties");
                });

            modelBuilder.Entity("web.Data.CovidDay", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Cases")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CountyId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int>("Deaths")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CountyId", "Date");

                    b.ToTable("CovidDays");
                });

            modelBuilder.Entity("web.Data.CovidDayStaging", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Cases")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CountyId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<int>("Deaths")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CountyId");

                    b.ToTable("CovidDaysStaging");
                });

            modelBuilder.Entity("web.Data.SiteSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUpdatedWhen")
                        .HasColumnType("TEXT");

                    b.Property<string>("USCountiesSha")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("SiteSettings");
                });

            modelBuilder.Entity("web.Data.State", b =>
                {
                    b.Property<int>("StateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("StateName")
                        .HasColumnType("TEXT");

                    b.HasKey("StateId");

                    b.ToTable("States");
                });

            modelBuilder.Entity("web.Data.County", b =>
                {
                    b.HasOne("web.Data.State", "State")
                        .WithMany("Counties")
                        .HasForeignKey("StateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("web.Data.CovidDay", b =>
                {
                    b.HasOne("web.Data.County", "County")
                        .WithMany("Records")
                        .HasForeignKey("CountyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("web.Data.CovidDayStaging", b =>
                {
                    b.HasOne("web.Data.County", "County")
                        .WithMany("RecordsStaging")
                        .HasForeignKey("CountyId");
                });
#pragma warning restore 612, 618
        }
    }
}
