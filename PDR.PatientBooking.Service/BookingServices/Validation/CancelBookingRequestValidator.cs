using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class CancelBookingRequestValidator : ICancelBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public CancelBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }
        public PdrValidationResult ValidateRequest(Guid bookingId)
        {
            var result = new PdrValidationResult(true);

            if (BookingNotFound(bookingId, ref result))
            {
                return result;
            }

            if (BookingAlreadyCancelled(bookingId, ref result))
            {
                return result;
            }

            return result;
        }

        private bool BookingAlreadyCancelled(Guid bookingId, ref PdrValidationResult result)
        {
            if (_context.Order.Any(o => o.Id == bookingId && o.Status == 1))
            {
                result.PassedValidation = false;
                result.Errors.Add("Booking is already cancelled");
                return true;
            }

            return false;
        }

        private bool BookingNotFound(Guid bookingId, ref PdrValidationResult result)
        {
            if (!_context.Order.Any(o => o.Id == bookingId))
            {
                result.PassedValidation = false;
                result.Errors.Add("Booking with provided Id not foud");
                return true;
            }

            return false;
        }
    }
}
