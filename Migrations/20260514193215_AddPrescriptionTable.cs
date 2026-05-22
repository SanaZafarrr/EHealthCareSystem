using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthCareSystem.Migrations
{
    public partial class AddPrescriptionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    PrescriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    AppointmentId = table.Column<int>(type: "int", nullable: false),

                    UserId = table.Column<int>(type: "int", nullable: false),

                    DoctorId = table.Column<int>(type: "int", nullable: false),

                    PatientName = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    DoctorName = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    Diagnosis = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    Medicines = table.Column<string>(type: "nvarchar(max)", nullable: false),

                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.PrescriptionId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prescriptions");
        }
    }
}