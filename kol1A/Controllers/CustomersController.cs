using kol1A.Exceptions;
using kol1A.Models.DTOs;
using kol1A.Services;
using Microsoft.AspNetCore.Mvc;

namespace kol1A.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        public AppointmentsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("{id}", Name = "GetAppointmentInfo")]
        public async Task<IActionResult> GetAppointmentInfo(int id)
        {
            try
            {
                var res = await _dbService.GetAppointmentInfoAsync(id);
                return Ok(res);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewAppointment(CreateAppointmentRequest appointmentRequest)
        {
            if (!appointmentRequest.Services.Any())
            {
                return BadRequest("At least one item is required.");
            }

            try
            {
                await _dbService.AddNewAppointmentAsync(appointmentRequest);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            
            return CreatedAtRoute(
                routeName: "GetAppointmentInfo",
                routeValues: new { id = appointmentRequest.AppointmentId },
                value: null
            );
        }    
    }
}