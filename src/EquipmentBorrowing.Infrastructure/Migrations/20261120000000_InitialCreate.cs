using System;
using EquipmentBorrowing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EquipmentBorrowing.Infrastructure.Migrations
{
    /// <summary>
    /// Initial PostgreSQL schema for the Lab 5 API: students, equipment, borrowings.
    /// If your Lab 4 database already has these tables, see README "Migrating an existing Lab 4 database".
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261120000000_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EquipmentCodeNormalized = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StudentNumberNormalized = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Contact = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Borrowings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    BorrowedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReturnedDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Borrowings", x => x.Id);
                    table.CheckConstraint("CK_Borrowings_ExpectedAfterBorrowed", "\"ExpectedReturnDate\" > \"BorrowedDate\"");
                    table.CheckConstraint("CK_Borrowings_ReturnedNotBeforeBorrowed", "\"ReturnedDate\" IS NULL OR \"ReturnedDate\" >= \"BorrowedDate\"");
                    table.ForeignKey(
                        name: "FK_Borrowings_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Borrowings_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_EquipmentId_Active",
                table: "Borrowings",
                column: "EquipmentId",
                unique: true,
                filter: "\"ReturnedDate\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Borrowings_StudentId",
                table: "Borrowings",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_EquipmentCodeNormalized",
                table: "Equipment",
                column: "EquipmentCodeNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumberNormalized",
                table: "Students",
                column: "StudentNumberNormalized",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Borrowings");
            migrationBuilder.DropTable(name: "Equipment");
            migrationBuilder.DropTable(name: "Students");
        }
    }
}
