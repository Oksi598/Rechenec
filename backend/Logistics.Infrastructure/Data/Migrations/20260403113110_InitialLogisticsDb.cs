using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialLogisticsDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[DeliveryProofs]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [DeliveryProofs] (
                        [Id] uniqueidentifier NOT NULL,
                        [OrderId] uniqueidentifier NOT NULL,
                        [ClientProofId] nvarchar(450) NOT NULL,
                        [PhotoUrl] nvarchar(2048) NOT NULL,
                        [Signature] nvarchar(2048) NOT NULL,
                        [DeliveredAt] datetimeoffset NOT NULL,
                        CONSTRAINT [PK_DeliveryProofs] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[Depots]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Depots] (
                        [Id] uniqueidentifier NOT NULL,
                        [Name] nvarchar(max) NOT NULL,
                        [Latitude] float NOT NULL,
                        [Longitude] float NOT NULL,
                        [CapacityWeight] float NOT NULL,
                        [CapacityVolume] float NOT NULL,
                        CONSTRAINT [PK_Depots] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[OrderAssignments]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [OrderAssignments] (
                        [Id] uniqueidentifier NOT NULL,
                        [OrderId] uniqueidentifier NOT NULL,
                        [RouteId] uniqueidentifier NOT NULL,
                        [AssignedAt] datetimeoffset NOT NULL,
                        CONSTRAINT [PK_OrderAssignments] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[Orders]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Orders] (
                        [Id] uniqueidentifier NOT NULL,
                        [CustomerId] uniqueidentifier NOT NULL,
                        [PickupDepotId] uniqueidentifier NOT NULL,
                        [DeliveryLatitude] float NOT NULL,
                        [DeliveryLongitude] float NOT NULL,
                        [Weight] float NOT NULL,
                        [Volume] float NOT NULL,
                        [Priority] int NOT NULL,
                        [Status] nvarchar(128) NOT NULL,
                        [PriceEstimate] decimal(18,2) NOT NULL,
                        [CreatedAt] datetimeoffset NOT NULL,
                        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[RoutePoints]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [RoutePoints] (
                        [Id] uniqueidentifier NOT NULL,
                        [RouteId] uniqueidentifier NOT NULL,
                        [OrderId] uniqueidentifier NOT NULL,
                        [Sequence] int NOT NULL,
                        [Latitude] float NOT NULL,
                        [Longitude] float NOT NULL,
                        [ArrivalTime] datetimeoffset NULL,
                        [DepartureTime] datetimeoffset NULL,
                        [Type] nvarchar(128) NOT NULL,
                        CONSTRAINT [PK_RoutePoints] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[Routes]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Routes] (
                        [Id] uniqueidentifier NOT NULL,
                        [VehicleId] uniqueidentifier NOT NULL,
                        [DriverId] uniqueidentifier NOT NULL,
                        [StartTime] datetimeoffset NOT NULL,
                        [EndTime] datetimeoffset NOT NULL,
                        [Status] nvarchar(128) NOT NULL,
                        [RowVersion] rowversion NOT NULL,
                        CONSTRAINT [PK_Routes] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[Users]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Users] (
                        [Id] uniqueidentifier NOT NULL,
                        [Email] nvarchar(max) NOT NULL,
                        [PasswordHash] nvarchar(max) NOT NULL,
                        [FullName] nvarchar(max) NOT NULL,
                        [Phone] nvarchar(max) NOT NULL,
                        [Role] nvarchar(max) NOT NULL,
                        [CreatedAt] datetimeoffset NOT NULL,
                        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[VehicleLocations]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [VehicleLocations] (
                        [Id] uniqueidentifier NOT NULL,
                        [VehicleId] uniqueidentifier NOT NULL,
                        [Latitude] float NOT NULL,
                        [Longitude] float NOT NULL,
                        [Speed] float NOT NULL,
                        [RecordedAt] datetimeoffset NOT NULL,
                        CONSTRAINT [PK_VehicleLocations] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[Vehicles]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Vehicles] (
                        [Id] uniqueidentifier NOT NULL,
                        [PlateNumber] nvarchar(max) NOT NULL,
                        [CapacityWeight] float NOT NULL,
                        [CapacityVolume] float NOT NULL,
                        [FuelConsumption] float NOT NULL,
                        [VehicleType] nvarchar(max) NOT NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id])
                    );
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[DeliveryProofs]', N'U') IS NOT NULL
                    AND NOT EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = N'IX_DeliveryProofs_ClientProofId'
                          AND object_id = OBJECT_ID(N'[DeliveryProofs]')
                    )
                BEGIN
                    CREATE UNIQUE INDEX [IX_DeliveryProofs_ClientProofId]
                    ON [DeliveryProofs] ([ClientProofId]);
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[DeliveryProofs]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [DeliveryProofs];
                END
                """);

            migrationBuilder.DropTable(
                name: "Depots");

            migrationBuilder.DropTable(
                name: "OrderAssignments");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "RoutePoints");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VehicleLocations");

            migrationBuilder.DropTable(
                name: "Vehicles");
        }
    }
}
