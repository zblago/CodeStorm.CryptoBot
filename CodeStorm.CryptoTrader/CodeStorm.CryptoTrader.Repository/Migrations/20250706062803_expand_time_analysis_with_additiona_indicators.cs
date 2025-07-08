using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeStorm.CryptoTrader.Repository.Migrations
{
    /// <inheritdoc />
    public partial class expand_time_analysis_with_additiona_indicators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentEma21",
                table: "TimelineAnalysis",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentEma9",
                table: "TimelineAnalysis",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentRSI",
                table: "TimelineAnalysis",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrevRSI",
                table: "TimelineAnalysis",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentEma21",
                table: "TimelineAnalysis");

            migrationBuilder.DropColumn(
                name: "CurrentEma9",
                table: "TimelineAnalysis");

            migrationBuilder.DropColumn(
                name: "CurrentRSI",
                table: "TimelineAnalysis");

            migrationBuilder.DropColumn(
                name: "PrevRSI",
                table: "TimelineAnalysis");
        }
    }
}
