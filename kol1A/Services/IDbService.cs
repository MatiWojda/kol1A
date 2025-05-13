using kol1A.Models.DTOs;

namespace kol1A.Services;

public interface IDbService
{
    Task<AppointmentDto> GetAppointmentInfoAsync(int appointmentId);
    Task AddNewAppointmentAsync(CreateAppointmentRequest appointmentRequest);
}