using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidator : IAddBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public AddBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (InvalidTimeFrame(request, ref result))
            {
                return result;
            }

            if (DateInThePast(request, ref result))
            {
                return result;
            }

            if (SlotIsBooked(request, ref result))
            {
                return result;
            }

            return result;
        }

        private bool InvalidTimeFrame(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (request.StartTime >= request.EndTime)
            {
                result.PassedValidation = false;
                result.Errors.Add("Start time should be earlier than the end time");
                return true;
            }

            return false;
        }

        private bool DateInThePast(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (request.StartTime < DateTime.UtcNow)
            {
                result.PassedValidation = false;
                result.Errors.Add("Appointment date should not be in the past");
                return true;
            }

            return false;
        }

        private bool SlotIsBooked(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (_context.Order.Any(o => o.DoctorId == request.DoctorId
                && (request.StartTime >= o.StartTime && request.StartTime < o.EndTime
                || request.EndTime > o.StartTime && request.EndTime <= o.EndTime)))
            {
                result.PassedValidation = false;
                result.Errors.Add("This time slot is already booked");
                return true;
            }

            return false;
        }
    }
}
