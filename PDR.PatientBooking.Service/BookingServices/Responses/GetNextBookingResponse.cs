using PDR.PatientBooking.Service.Enums;
using System;

namespace PDR.PatientBooking.Service.BookingServices.Responses
{
    public class GetNextBookingResponse
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public long DoctorId { get; set; }
        public OrderStatus Status { get; set; }
    }
}
