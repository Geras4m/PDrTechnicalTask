using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;

namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    public class AddBookingRequestValidatorTests
    {
        private IFixture _fixture;
        private PatientBookingContext _context;
        private AddBookingRequestValidator _validator;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _fixture = new Fixture();

            //Prevent fixture from generating from entity circular references 
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

            // Sut instantiation
            _validator = new AddBookingRequestValidator(
                _context
            );
        }

        [Test]
        public void ValidateRequest_AllChecksPass_ReturnsPassedValidationResult()
        {
            //arrange
            var request = GetValidRequest();

            //act
            var result = _validator.ValidateRequest(request);

            //assert
            using (new AssertionScope())
            {
                result.PassedValidation.Should().BeTrue();
            }
        }

        [Test]
        public void ValidateRequest_InvalidTimeFrame_ReturnsFailedValidation()
        {
            //arrange
            var request = GetValidRequest();
            request.StartTime = request.EndTime.AddHours(1);

            //act
            var result = _validator.ValidateRequest(request);

            //assert
            using (new AssertionScope())
            {
                result.PassedValidation.Should().BeFalse();
                result.Errors.Should().Contain("Start time should be earlier than the end time");
            }
        }

        [Test]
        public void ValidateRequest_DateInThePast_ReturnsFailedValidation()
        {
            //arrange
            var request = GetValidRequest();
            request.StartTime = DateTime.UtcNow.AddDays(-1);

            //act
            var result = _validator.ValidateRequest(request);

            //assert
            using (new AssertionScope())
            {
                result.PassedValidation.Should().BeFalse();
                result.Errors.Should().Contain("Appointment date should not be in the past");
            }
        }

        [TestCase(0, 0)]
        [TestCase(0, 5)]
        [TestCase(5, 0)]
        [TestCase(-5, -5)]
        [TestCase(-5, 0)]
        public void ValidateRequest_SlotIsBooked_ReturnsFailedValidation(
            int startTimeOffset,
            int endTimeOffset)
        {
            //arrange
            var request = GetInvalidRequest();
            request.StartTime = request.StartTime.AddMinutes(startTimeOffset);
            request.EndTime = request.EndTime.AddMinutes(endTimeOffset);


            //act
            var result = _validator.ValidateRequest(request);

            //assert
            using (new AssertionScope())
            {
                result.PassedValidation.Should().BeFalse();
                result.Errors.Should().Contain("This time slot is already booked");
            }
        }

        private AddBookingRequest GetValidRequest()
        {
            var request = _fixture.Create<AddBookingRequest>();
            request.StartTime = request.StartTime.AddYears(10);
            request.EndTime = request.StartTime.AddMinutes(30);
            return request;
        }

        private AddBookingRequest GetInvalidRequest()
        {
            var order = _fixture.Create<Order>();
            order.StartTime = order.StartTime.AddYears(10);
            order.EndTime = order.StartTime.AddMinutes(30);

            _context.Add(order);
            _context.SaveChanges();

            var request = _fixture.Build<AddBookingRequest>()
                .With(r => r.Id, order.Id)
                .With(r => r.StartTime, order.StartTime)
                .With(r => r.EndTime, order.EndTime)
                .With(r => r.DoctorId, order.DoctorId)
                .Create();

            return request;
        }
    }
}
