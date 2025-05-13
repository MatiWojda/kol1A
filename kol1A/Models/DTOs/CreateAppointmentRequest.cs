using System.ComponentModel.DataAnnotations;

namespace kol1A.Models.DTOs;

public class CreateAppointmentRequest
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    [MaxLength(7)]
    public string PWZ { get; set; } = string.Empty;
    public List<CreateAppointmentServiceItem> Services { get; set; } =  new List<CreateAppointmentServiceItem>();
}

public class CreateAppointmentServiceItem
{
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServiceFee { get; set; }
}