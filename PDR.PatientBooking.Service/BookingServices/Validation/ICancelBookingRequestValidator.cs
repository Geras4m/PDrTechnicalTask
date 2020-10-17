using PDR.PatientBooking.Service.Validation;
using System;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public interface ICancelBookingRequestValidator
    {
        PdrValidationResult ValidateRequest(Guid bookingId);
    }
}