﻿// <auto-generated />
using AirTransit_Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace AirTransit_Core.Migrations
{
    [DbContext(typeof(MessagingContext))]
    [Migration("20180215210135_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125");

            modelBuilder.Entity("AirTransit_Core.Models.Contact", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("PhoneNumber");

                    b.HasKey("Id");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("AirTransit_Core.Models.KeySet", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("PhoneNumber");

                    b.Property<string>("PrivateKey");

                    b.Property<string>("PublicKey");

                    b.HasKey("Id");

                    b.ToTable("KeySet");
                });

            modelBuilder.Entity("AirTransit_Core.Models.Message", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content");

                    b.Property<string>("DestinationPhoneNumber");

                    b.Property<int?>("SenderId");

                    b.Property<DateTime>("Timestamp");

                    b.HasKey("Id");

                    b.HasIndex("SenderId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("AirTransit_Core.Models.Message", b =>
                {
                    b.HasOne("AirTransit_Core.Models.Contact", "Sender")
                        .WithMany("Messages")
                        .HasForeignKey("SenderId");
                });
#pragma warning restore 612, 618
        }
    }
}
