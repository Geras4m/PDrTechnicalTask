using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;

namespace PDR.PatientBooking.Service.Tests.BookingServices.Validation
{
    [TestFixture]
    public class CancelBookingRequestValidatorTests
    {
        private IFixture _fixture;
        private PatientBookingContext _context;
        private CancelBookingRequestValidator _validator;

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
            _validator = new CancelBookingRequestValidator(_context);
        }

        [Test]
        public void ValidateRequest_AllChecksPass_ReturnsPassedValidationResult()
        {
            //arrange
            var bookingId = GetValidId();

            //act
            var result = _validator.ValidateRequest(bookingId);

            //assert
            using (new AssertionScope())
            {
                result.PassedValidation.Should().BeTrue();
            }
        }

        [Test]
        public void ValidateRequest_BookingNotFound_ReturnsFailedValidation()
        {
            //arrange
            var bookingId = _fixture.Create<Guid>();

            //act
            var result = _validator.ValidateRequest(bookingId);

            //assert
            using (new AssertionScope())
            {
                result.PassedValidation.Should().BeFalse();
                result.Errors.Should().Contain("Booking with provided Id not foud");
            }
        }

        [Test]
        public void ValidateRequest_BookingAlreadyCancelled_ReturnsFailedValidation()
        {
            //arrange
            var bookingId = GetCancelledBookingId();

            //act
            var result = _validator.ValidateRequest(bookingId);

            //assert
            using (new AssertionScope())
            {
                result.PassedValidation.Should().BeFalse();
                result.Errors.Should().Contain("Booking is already cancelled");
            }
        }

        private Guid GetValidId()
        {
            var order = _fixture.Create<Order>();

            _context.Add(order);
            _context.SaveChanges();

            return order.Id;
        }

        private Guid GetCancelledBookingId()
        {
            var order = _fixture.Build<Order>()
                .With(o => o.Status, 1)
                .Create();

            _context.Add(order);
            _context.SaveChanges();

            return order.Id;
        }
    }
}
