using System.Data.Common;
using kol1A.Exceptions;
using kol1A.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace kol1A.Services;

public class DbService : IDbService
{
    private readonly string _connectionString;
    public DbService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    public async Task<AppointmentDto> GetAppointmentInfoAsync(int appointmentId)
    {
        var query =
            @"SELECT a.date, p.first_name, p.last_name, p.date_of_birth, d.doctor_id, d.PWZ, s.name, aps.service_fee
            FROM Appointment a
            JOIN Patient p ON a.patient_id = p.patient_id
            JOIN Doctor d ON a.doctor_id = d.doctor_id
            JOIN Appointment_Service aps ON a.appointment_id = aps.appointment_id
            JOIN Service s ON aps.service_id = s.service_id
            WHERE a.appointment_id = @appointmentId;";
        
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        command.CommandText = query;
        await connection.OpenAsync();
        
        command.Parameters.AddWithValue("@appointmentId", appointmentId);
        var reader = await command.ExecuteReaderAsync();
        
        AppointmentDto? visit = null;
        
        while (await reader.ReadAsync())
        {
            if (visit is null)
            {
                visit = new AppointmentDto()
                {
                    Date = reader.GetDateTime(0),
                    Patient = new PatientDto()
                    {
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        DateOfBirth = reader.GetDateTime(3)
                    },
                    Doctor = new DoctorDto()
                    {
                        DoctorId = reader.GetInt32(4),
                        pwz = reader.GetString(5)
                    },
                    Services = new List<AppointmentServiceDto>(),
                };
            }
            
            visit.Services.Add(new AppointmentServiceDto()
            {
                Name = reader.GetString(6),
                ServiceFee = reader.GetDecimal(7)
            });
            
        }       
        
        if (visit is null)
        {
            throw new NotFoundException("No visit found for specified ID");
        }
        
        return visit;
    }

    public async Task AddNewAppointmentAsync(CreateAppointmentRequest appointmentRequest)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.Parameters.Clear();
            command.CommandText = "SELECT 1 FROM Patient WHERE patient_id = @IdPatient;";
            command.Parameters.AddWithValue("@IdPatient", appointmentRequest.PatientId);
                
            var patientIdRes = await command.ExecuteScalarAsync();
            if(patientIdRes is null)
                throw new NotFoundException($"Patient with ID - {appointmentRequest.PatientId} - not found.");
            
            command.Parameters.Clear();
            command.CommandText = "SELECT doctor_id FROM Doctor WHERE PWZ = @PWZ;";
            command.Parameters.AddWithValue("@PWZ", appointmentRequest.PWZ);
            var doctorIdRes = await command.ExecuteScalarAsync();
            if (doctorIdRes is null)
                throw new NotFoundException($"Doctor with PWZ - {appointmentRequest.PWZ} - not found.");
            var doctorId = (int)doctorIdRes;
            
            command.Parameters.Clear();
            command.CommandText = 
                @"INSERT INTO Appointment
            VALUES(@IdAppointment, @PatientId, @DoctorId, GETDATE());";

            command.Parameters.AddWithValue("@IdAppointment", appointmentRequest.AppointmentId);
            command.Parameters.AddWithValue("@PatientId", appointmentRequest.PatientId);
            command.Parameters.AddWithValue("@DoctorId", doctorId);
            
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw new ConflictException("Appointment with the same ID already exists.");
            }
            

            foreach (var service in appointmentRequest.Services)
            {
                command.Parameters.Clear();
                command.CommandText = "SELECT service_id FROM Service WHERE name = @ServiceName;";
                command.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                
                var serviceId = await command.ExecuteScalarAsync();
                if(serviceId is null)
                    throw new NotFoundException($"Service with name: - {service.ServiceName} - not found.");
                
                command.Parameters.Clear();
                command.CommandText = 
                    @"INSERT INTO Appointment_Service
                        VALUES(@IdAppointment, @IdService, @ServiceFee);";
        
                command.Parameters.AddWithValue("@IdAppointment", appointmentRequest.AppointmentId);
                command.Parameters.AddWithValue("@IdService", serviceId);
                command.Parameters.AddWithValue("@ServiceFee", service.ServiceFee);
                
                await command.ExecuteNonQueryAsync();
            }
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
        

    }
}