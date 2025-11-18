using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Admin",
                columns: table => new
                {
                    adminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.adminId);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Doctor",
                columns: table => new
                {
                    doctorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    specialization = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phoneNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.doctorId);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Nurse",
                columns: table => new
                {
                    nurseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phoneNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.nurseId);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    patientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gender = table.Column<string>(type: "enum('Male','Female','Other')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    phoneNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    medicalHistory = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.patientId);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Truck",
                columns: table => new
                {
                    truckId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    licensePlate = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    currentLocation = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    capacity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.truckId);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    roleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    roleName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.roleId);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "SMSNotification",
                columns: table => new
                {
                    smsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    patientId = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sentAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    sentBy = table.Column<string>(type: "enum('System','Nurse','Doctor')", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    senderId = table.Column<int>(type: "int", nullable: true),
                    senderRole = table.Column<string>(type: "enum('Doctor','Nurse')", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.smsId);
                    table.ForeignKey(
                        name: "fk_sms_patient",
                        column: x => x.patientId,
                        principalTable: "Patient",
                        principalColumn: "patientId");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "Appointment",
                columns: table => new
                {
                    appointmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    patientId = table.Column<int>(type: "int", nullable: false),
                    doctorId = table.Column<int>(type: "int", nullable: true),
                    nurseId = table.Column<int>(type: "int", nullable: true),
                    truckId = table.Column<int>(type: "int", nullable: true),
                    appointmentDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    status = table.Column<string>(type: "enum('Scheduled','Completed','Cancelled')", nullable: true, defaultValueSql: "'Scheduled'", collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.appointmentId);
                    table.ForeignKey(
                        name: "appointment_ibfk_1",
                        column: x => x.patientId,
                        principalTable: "Patient",
                        principalColumn: "patientId");
                    table.ForeignKey(
                        name: "appointment_ibfk_2",
                        column: x => x.doctorId,
                        principalTable: "Doctor",
                        principalColumn: "doctorId");
                    table.ForeignKey(
                        name: "appointment_ibfk_3",
                        column: x => x.nurseId,
                        principalTable: "Nurse",
                        principalColumn: "nurseId");
                    table.ForeignKey(
                        name: "appointment_ibfk_4",
                        column: x => x.truckId,
                        principalTable: "Truck",
                        principalColumn: "truckId");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "TruckAssignment",
                columns: table => new
                {
                    assignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    truckId = table.Column<int>(type: "int", nullable: true),
                    patientId = table.Column<int>(type: "int", nullable: true),
                    dateAssigned = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.assignmentId);
                    table.ForeignKey(
                        name: "fk_truckassignment_patient",
                        column: x => x.patientId,
                        principalTable: "Patient",
                        principalColumn: "patientId");
                    table.ForeignKey(
                        name: "fk_truckassignment_truck",
                        column: x => x.truckId,
                        principalTable: "Truck",
                        principalColumn: "truckId");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "TruckMaintenance",
                columns: table => new
                {
                    maintenanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    truckId = table.Column<int>(type: "int", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    maintenanceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    nextServiceDue = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.maintenanceId);
                    table.ForeignKey(
                        name: "fk_maintenance_truck",
                        column: x => x.truckId,
                        principalTable: "Truck",
                        principalColumn: "truckId");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "TruckStaffAssignment",
                columns: table => new
                {
                    assignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    truckId = table.Column<int>(type: "int", nullable: false),
                    staffId = table.Column<int>(type: "int", nullable: false),
                    role = table.Column<string>(type: "enum('Doctor','Nurse')", nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dateAssigned = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.assignmentId);
                    table.ForeignKey(
                        name: "truckstaffassignment_ibfk_1",
                        column: x => x.truckId,
                        principalTable: "Truck",
                        principalColumn: "truckId");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    roleId = table.Column<int>(type: "int", nullable: false),
                    relatedId = table.Column<int>(type: "int", nullable: false),
                    isActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValueSql: "'1'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.userId);
                    table.ForeignKey(
                        name: "fk_user_role",
                        column: x => x.roleId,
                        principalTable: "UserRole",
                        principalColumn: "roleId");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "TreatmentRecord",
                columns: table => new
                {
                    treatmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    appointmentId = table.Column<int>(type: "int", nullable: false),
                    patientId = table.Column<int>(type: "int", nullable: false),
                    diagnosis = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    treatmentDetails = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    treatmentDate = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DoctorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.treatmentId);
                    table.ForeignKey(
                        name: "fk_treatment_appointment",
                        column: x => x.appointmentId,
                        principalTable: "Appointment",
                        principalColumn: "appointmentId");
                    table.ForeignKey(
                        name: "fk_treatment_patient",
                        column: x => x.patientId,
                        principalTable: "Patient",
                        principalColumn: "patientId");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateTable(
                name: "LoginSession",
                columns: table => new
                {
                    sessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    userId = table.Column<int>(type: "int", nullable: false),
                    loginTime = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    logoutTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    ipAddress = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    userAgent = table.Column<string>(type: "text", nullable: true, collation: "utf8mb4_0900_ai_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.sessionId);
                    table.ForeignKey(
                        name: "fk_loginsession_user",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "userId");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_0900_ai_ci");

            migrationBuilder.CreateIndex(
                name: "email",
                table: "Admin",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_appointment_doctor",
                table: "Appointment",
                column: "doctorId");

            migrationBuilder.CreateIndex(
                name: "idx_appointment_nurse",
                table: "Appointment",
                column: "nurseId");

            migrationBuilder.CreateIndex(
                name: "idx_appointment_patient",
                table: "Appointment",
                column: "patientId");

            migrationBuilder.CreateIndex(
                name: "idx_appointment_truck",
                table: "Appointment",
                column: "truckId");

            migrationBuilder.CreateIndex(
                name: "email1",
                table: "Doctor",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "fk_loginsession_user",
                table: "LoginSession",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "email2",
                table: "Nurse",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "email3",
                table: "Patient",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_sms_patient",
                table: "SMSNotification",
                column: "patientId");

            migrationBuilder.CreateIndex(
                name: "fk_treatment_appointment",
                table: "TreatmentRecord",
                column: "appointmentId");

            migrationBuilder.CreateIndex(
                name: "idx_treatment_patient",
                table: "TreatmentRecord",
                column: "patientId");

            migrationBuilder.CreateIndex(
                name: "licensePlate",
                table: "Truck",
                column: "licensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "fk_truckassignment_truck",
                table: "TruckAssignment",
                column: "truckId");

            migrationBuilder.CreateIndex(
                name: "idx_truckassignment_patient",
                table: "TruckAssignment",
                column: "patientId");

            migrationBuilder.CreateIndex(
                name: "fk_maintenance_truck",
                table: "TruckMaintenance",
                column: "truckId");

            migrationBuilder.CreateIndex(
                name: "idx_truckstaff_truck",
                table: "TruckStaffAssignment",
                column: "truckId");

            migrationBuilder.CreateIndex(
                name: "fk_user_role",
                table: "User",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "username",
                table: "User",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "roleName",
                table: "UserRole",
                column: "roleName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin");

            migrationBuilder.DropTable(
                name: "LoginSession");

            migrationBuilder.DropTable(
                name: "SMSNotification");

            migrationBuilder.DropTable(
                name: "TreatmentRecord");

            migrationBuilder.DropTable(
                name: "TruckAssignment");

            migrationBuilder.DropTable(
                name: "TruckMaintenance");

            migrationBuilder.DropTable(
                name: "TruckStaffAssignment");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Appointment");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "Patient");

            migrationBuilder.DropTable(
                name: "Doctor");

            migrationBuilder.DropTable(
                name: "Nurse");

            migrationBuilder.DropTable(
                name: "Truck");
        }
    }
}
