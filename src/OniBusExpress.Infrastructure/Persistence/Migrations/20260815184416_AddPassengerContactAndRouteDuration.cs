using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OniBusExpress.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPassengerContactAndRouteDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "estimated_duration",
                table: "route",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateOnly>(
                name: "passenger_date_of_birth",
                table: "reservation",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "passenger_email",
                table: "reservation",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estimated_duration",
                table: "route");

            migrationBuilder.DropColumn(
                name: "passenger_date_of_birth",
                table: "reservation");

            migrationBuilder.DropColumn(
                name: "passenger_email",
                table: "reservation");
        }
    }
}
