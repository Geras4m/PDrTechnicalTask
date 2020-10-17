using PDR.PatientBooking.Service.BookingServices.Requests;
using System;

namespace PDR.PatientBooking.Service.BookingServices
{
    public interface IBookingService
    {
        void AddBooking(AddBookingRequest request);
    }
}