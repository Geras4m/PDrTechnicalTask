using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Enums;
using System;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly PatientBookingContext _context;
        private readonly IAddBookingRequestValidator _addBookingValidator;
        private readonly ICancelBookingRequestValidator _cancelBookingValidator;

        public BookingService(PatientBookingContext context,
            IAddBookingRequestValidator addBookingValidator,
            ICancelBookingRequestValidator cancelBookingValidator)
        {
            _context = context;
            _addBookingValidator = addBookingValidator;
            _cancelBookingValidator = cancelBookingValidator;
        }

        public void AddBooking(AddBookingRequest request)
        {
            var validationResult = _addBookingValidator.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            _context.Order.Add(new Order
            {
                Id = request.Id,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId
            });

            _context.SaveChanges();
        }

        public void CancelBooking(Guid bookingId)
        {
            var validationResult = _cancelBookingValidator.ValidateRequest(bookingId);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }
            var booking = _context.Order.Single(o => o.Id == bookingId);

            _context.Order.Update(booking).Entity.Status = 1;
            _context.SaveChanges();
        }

        public GetNextBookingResponse GetPatientNextBooking(long patientId)
        {
            if(!_context.Patient.Any(o => o.Id == patientId))
            {
                throw new ArgumentException("Wrong patient id");
            }

            if(!_context.Order.Any(o => o.PatientId == patientId)
                || !_context.Order.Any(o => o.PatientId == patientId && o.Status == 0))
            {
                return null;
            }

            var order = _context.Order
                .Where(o => o.PatientId == patientId && o.Status == 0)
                .OrderByDescending(x => x.StartTime)
                .FirstOrDefault();

            var result = new GetNextBookingResponse()
            {
                Id = order.Id,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                DoctorId = order.DoctorId,
                Status = (OrderStatus)order.Status,
            };

            return result;
        }
    }
}
