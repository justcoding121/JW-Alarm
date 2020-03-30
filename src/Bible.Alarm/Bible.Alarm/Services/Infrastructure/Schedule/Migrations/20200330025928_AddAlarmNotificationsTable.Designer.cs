﻿// <auto-generated />
using System;
using Bible.Alarm.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bible.Alarm.Services.Infrastructure.Schedule.Migrations
{
    [DbContext(typeof(ScheduleDbContext))]
    [Migration("20200330025928_AddAlarmNotificationsTable")]
    partial class AddAlarmNotificationsTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3");

            modelBuilder.Entity("Bible.Alarm.Models.AlarmMusic", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AlarmScheduleId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LanguageCode")
                        .HasColumnType("TEXT");

                    b.Property<int>("MusicType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PublicationCode")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Repeat")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TrackNumber")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AlarmScheduleId")
                        .IsUnique();

                    b.ToTable("AlarmMusic");
                });

            modelBuilder.Entity("Bible.Alarm.Models.AlarmSchedule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("AlwaysPlayFromStart")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CurrentPlayItem")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DaysOfWeek")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Hour")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<long>("LatestAlarmNotificationId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Minute")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("MusicEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("NumberOfChaptersToRead")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Second")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SnoozeMinutes")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("AlarmSchedules");
                });

            modelBuilder.Entity("Bible.Alarm.Models.BibleReadingSchedule", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AlarmScheduleId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BookNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterNumber")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("FinishedDuration")
                        .HasColumnType("TEXT");

                    b.Property<string>("LanguageCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("PublicationCode")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("AlarmScheduleId")
                        .IsUnique();

                    b.ToTable("BibleReadingSchedules");
                });

            modelBuilder.Entity("Bible.Alarm.Models.GeneralSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("GeneralSettings");
                });

            modelBuilder.Entity("Bible.Alarm.Models.Schedule.AlarmNotification", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AlarmScheduleId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("CancellationRequested")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Cancelled")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Fired")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("ScheduledTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Sent")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AlarmScheduleId");

                    b.ToTable("AlarmNotifications");
                });

            modelBuilder.Entity("Bible.Alarm.Models.AlarmMusic", b =>
                {
                    b.HasOne("Bible.Alarm.Models.AlarmSchedule", "AlarmSchedule")
                        .WithOne("Music")
                        .HasForeignKey("Bible.Alarm.Models.AlarmMusic", "AlarmScheduleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Bible.Alarm.Models.BibleReadingSchedule", b =>
                {
                    b.HasOne("Bible.Alarm.Models.AlarmSchedule", "AlarmSchedule")
                        .WithOne("BibleReadingSchedule")
                        .HasForeignKey("Bible.Alarm.Models.BibleReadingSchedule", "AlarmScheduleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Bible.Alarm.Models.Schedule.AlarmNotification", b =>
                {
                    b.HasOne("Bible.Alarm.Models.AlarmSchedule", "AlarmSchedule")
                        .WithMany("AlarmNotifications")
                        .HasForeignKey("AlarmScheduleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
