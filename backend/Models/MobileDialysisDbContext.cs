using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace backend.Models;

public partial class MobileDialysisDbContext : DbContext
{
    public MobileDialysisDbContext()
    {
    }

    public MobileDialysisDbContext(DbContextOptions<MobileDialysisDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<Appointmentdetail> Appointmentdetails { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<LoginSession> LoginSessions { get; set; }

    public virtual DbSet<Nurse> Nurses { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Smsnotification> Smsnotifications { get; set; }

    public virtual DbSet<TreatmentRecord> TreatmentRecords { get; set; }

    public virtual DbSet<Truck> Trucks { get; set; }

    public virtual DbSet<TruckAssignment> TruckAssignments { get; set; }

    public virtual DbSet<TruckMaintenance> TruckMaintenances { get; set; }

    public virtual DbSet<TruckStaffAssignment> TruckStaffAssignments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Userdetailsview> Userdetailsviews { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("PRIMARY");

            entity.ToTable("Admin");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.AdminId).HasColumnName("adminId");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentId).HasName("PRIMARY");

            entity.ToTable("Appointment");

            //entity.HasIndex(e => e.DoctorId, "idx_appointment_doctor");

            entity.HasIndex(e => e.NurseId, "idx_appointment_nurse");

            entity.HasIndex(e => e.PatientId, "idx_appointment_patient");

            entity.HasIndex(e => e.TruckId, "idx_appointment_truck");

            entity.Property(e => e.AppointmentId).HasColumnName("appointmentId");
            entity.Property(e => e.AppointmentDate)
                .HasColumnType("datetime")
                .HasColumnName("appointmentDate");
           
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.NurseId).HasColumnName("nurseId");
            entity.Property(e => e.PatientId).HasColumnName("patientId");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Scheduled'")
                .HasColumnType("enum('Scheduled','Completed','Cancelled')")
                .HasColumnName("status");
            entity.Property(e => e.TruckId).HasColumnName("truckId");

          

            entity.HasOne(d => d.Nurse).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.NurseId)
                .HasConstraintName("appointment_ibfk_3");

            entity.HasOne(d => d.Patient).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("appointment_ibfk_1");

            entity.HasOne(d => d.Truck).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.TruckId)
                .HasConstraintName("appointment_ibfk_4");
        });

        modelBuilder.Entity<Appointmentdetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("appointmentdetails");

            entity.Property(e => e.AppointmentDate)
                .HasColumnType("datetime")
                .HasColumnName("appointmentDate");
            entity.Property(e => e.AppointmentId).HasColumnName("appointmentId");
            entity.Property(e => e.DoctorName)
                .HasMaxLength(100)
                .HasColumnName("doctorName");
            entity.Property(e => e.Notes)
                .HasColumnType("text")
                .HasColumnName("notes");
            entity.Property(e => e.NurseName)
                .HasMaxLength(100)
                .HasColumnName("nurseName");
            entity.Property(e => e.PatientName)
                .HasMaxLength(100)
                .HasColumnName("patientName");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'Scheduled'")
                .HasColumnType("enum('Scheduled','Completed','Cancelled')")
                .HasColumnName("status");
            entity.Property(e => e.TruckPlate)
                .HasMaxLength(20)
                .HasColumnName("truckPlate");
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorId).HasName("PRIMARY");

            entity.ToTable("Doctor");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.DoctorId).HasColumnName("doctorId");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phoneNumber");
            entity.Property(e => e.Specialization)
                .HasMaxLength(100)
                .HasColumnName("specialization");
        });

        modelBuilder.Entity<LoginSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PRIMARY");

            entity.ToTable("LoginSession");

            entity.HasIndex(e => e.UserId, "fk_loginsession_user");

            entity.Property(e => e.SessionId).HasColumnName("sessionId");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ipAddress");
            entity.Property(e => e.LoginTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("loginTime");
            entity.Property(e => e.LogoutTime)
                .HasColumnType("datetime")
                .HasColumnName("logoutTime");
            entity.Property(e => e.UserAgent)
                .HasColumnType("text")
                .HasColumnName("userAgent");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.LoginSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_loginsession_user");
        });

        modelBuilder.Entity<Nurse>(entity =>
        {
            entity.HasKey(e => e.NurseId).HasName("PRIMARY");

            entity.ToTable("Nurse");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.NurseId).HasColumnName("nurseId");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phoneNumber");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PRIMARY");

            entity.ToTable("Patient");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.Property(e => e.PatientId).HasColumnName("patientId");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.DateOfBirth).HasColumnName("dateOfBirth");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasColumnType("enum('Male','Female','Other')")
                .HasColumnName("gender");
            entity.Property(e => e.MedicalHistory)
                .HasColumnType("text")
                .HasColumnName("medicalHistory");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phoneNumber");
        });

        modelBuilder.Entity<Smsnotification>(entity =>
        {
            entity.HasKey(e => e.SmsId).HasName("PRIMARY");

            entity.ToTable("SMSNotification");

            entity.HasIndex(e => e.PatientId, "idx_sms_patient");

            entity.Property(e => e.SmsId).HasColumnName("smsId");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.PatientId).HasColumnName("patientId");
            entity.Property(e => e.SenderId).HasColumnName("senderId");
            entity.Property(e => e.SenderRole)
                .HasColumnType("enum('Doctor','Nurse')")
                .HasColumnName("senderRole");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("sentAt");
            entity.Property(e => e.SentBy)
                .HasColumnType("enum('System','Nurse','Doctor')")
                .HasColumnName("sentBy");

            entity.HasOne(d => d.Patient).WithMany(p => p.Smsnotifications)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sms_patient");
        });

        modelBuilder.Entity<TreatmentRecord>(entity =>
        {
            entity.HasKey(e => e.TreatmentId).HasName("PRIMARY");

            entity.ToTable("TreatmentRecord");

            entity.HasIndex(e => e.AppointmentId, "fk_treatment_appointment");

            entity.HasIndex(e => e.PatientId, "idx_treatment_patient");

            entity.Property(e => e.TreatmentId).HasColumnName("treatmentId");
            entity.Property(e => e.AppointmentId).HasColumnName("appointmentId");
            entity.Property(e => e.Diagnosis)
                .HasColumnType("text")
                .HasColumnName("diagnosis");
            entity.Property(e => e.PatientId).HasColumnName("patientId");
            entity.Property(e => e.TreatmentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("treatmentDate");
            entity.Property(e => e.TreatmentDetails)
                .HasColumnType("text")
                .HasColumnName("treatmentDetails");

            entity.HasOne(d => d.Appointment).WithMany(p => p.TreatmentRecords)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_treatment_appointment");

            entity.HasOne(d => d.Patient).WithMany(p => p.TreatmentRecords)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_treatment_patient");
        });

        modelBuilder.Entity<Truck>(entity =>
        {
            entity.HasKey(e => e.TruckId).HasName("PRIMARY");

            entity.ToTable("Truck");

            entity.HasIndex(e => e.LicensePlate, "licensePlate").IsUnique();

            entity.Property(e => e.TruckId).HasColumnName("truckId");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.CurrentLocation)
                .HasMaxLength(100)
                .HasColumnName("currentLocation");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(20)
                .HasColumnName("licensePlate");
        });

        modelBuilder.Entity<TruckAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PRIMARY");

            entity.ToTable("TruckAssignment");

            entity.HasIndex(e => e.TruckId, "fk_truckassignment_truck");

            entity.HasIndex(e => e.PatientId, "idx_truckassignment_patient");

            entity.Property(e => e.AssignmentId).HasColumnName("assignmentId");
            entity.Property(e => e.DateAssigned).HasColumnName("dateAssigned");
            entity.Property(e => e.PatientId).HasColumnName("patientId");
            entity.Property(e => e.TruckId).HasColumnName("truckId");

            entity.HasOne(d => d.Patient).WithMany(p => p.TruckAssignments)
                .HasForeignKey(d => d.PatientId)
                .HasConstraintName("fk_truckassignment_patient");

            entity.HasOne(d => d.Truck).WithMany(p => p.TruckAssignments)
                .HasForeignKey(d => d.TruckId)
                .HasConstraintName("fk_truckassignment_truck");
        });

        modelBuilder.Entity<TruckMaintenance>(entity =>
        {
            entity.HasKey(e => e.MaintenanceId).HasName("PRIMARY");

            entity.ToTable("TruckMaintenance");

            entity.HasIndex(e => e.TruckId, "fk_maintenance_truck");

            entity.Property(e => e.MaintenanceId).HasColumnName("maintenanceId");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.MaintenanceDate).HasColumnName("maintenanceDate");
            entity.Property(e => e.NextServiceDue).HasColumnName("nextServiceDue");
            entity.Property(e => e.TruckId).HasColumnName("truckId");

            entity.HasOne(d => d.Truck).WithMany(p => p.TruckMaintenances)
                .HasForeignKey(d => d.TruckId)
                .HasConstraintName("fk_maintenance_truck");
        });

        modelBuilder.Entity<TruckStaffAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PRIMARY");

            entity.ToTable("TruckStaffAssignment");

            entity.HasIndex(e => e.TruckId, "idx_truckstaff_truck");

            entity.Property(e => e.AssignmentId).HasColumnName("assignmentId");
            entity.Property(e => e.DateAssigned).HasColumnName("dateAssigned");
            entity.Property(e => e.Role)
                .HasColumnType("enum('Doctor','Nurse')")
                .HasColumnName("role");
            entity.Property(e => e.StaffId).HasColumnName("staffId");
            entity.Property(e => e.TruckId).HasColumnName("truckId");

            entity.HasOne(d => d.Truck).WithMany(p => p.TruckStaffAssignments)
                .HasForeignKey(d => d.TruckId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("truckstaffassignment_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.ToTable("User");

            entity.HasIndex(e => e.RoleId, "fk_user_role");

            entity.HasIndex(e => e.Username, "username").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("isActive");
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.RelatedId).HasColumnName("relatedId");
            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_role");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.ToTable("UserRole");

            entity.HasIndex(e => e.RoleName, "roleName").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("roleName");
        });

        modelBuilder.Entity<Userdetailsview>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("userdetailsview");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("fullName");
            entity.Property(e => e.RelatedId).HasColumnName("relatedId");
            entity.Property(e => e.RoleId).HasColumnName("roleId");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("roleName");
            entity.Property(e => e.Specialization)
                .HasMaxLength(100)
                .HasColumnName("specialization");
            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
