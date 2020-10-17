using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Linq;

namespace PDR.PatientBooking.Service.Tests.BookingServices
{
    [TestFixture]
    public class BookingServiceTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;
        private PatientBookingContext _context;
        private Mock<IAddBookingRequestValidator> _addBookingValidator;
        private Mock<ICancelBookingRequestValidator> _cancelBookingValidator;
        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            //Prevent fixture from generating circular references
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _addBookingValidator = _mockRepository.Create<IAddBookingRequestValidator>();
            _cancelBookingValidator = _mockRepository.Create<ICancelBookingRequestValidator>();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _bookingService = new BookingService(
                _context,
                _addBookingValidator.Object,
                _cancelBookingValidator.Object
            );
        }

        private void SetupMockDefaults()
        {
            _addBookingValidator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>()))
                .Returns(new PdrValidationResult(true));
            _cancelBookingValidator.Setup(x => x.ValidateRequest(It.IsAny<Guid>()))
                .Returns(new PdrValidationResult(true));
        }

        [Test]
        public void AddBooking_ValidatesRequest()
        {
            //arrange
            var request = _fixture.Create<AddBookingRequest>();

            //act
            _bookingService.AddBooking(request);

            //assert
            using (new AssertionScope())
            {
                _addBookingValidator.Verify(x => x.ValidateRequest(request), Times.Once);
            }
        }

        [Test]
        public void AddBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            _addBookingValidator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.AddBooking(_fixture.Create<AddBookingRequest>()));

            //assert
            using (new AssertionScope())
            {
                exception.Message.Should().Be(failedValidationResult.Errors.First());
            }
        }

        [Test]
        public void AddBooking_AddsBookingToContextWithGeneratedId()
        {
            //arrange
            var request = _fixture.Create<AddBookingRequest>();

            var expected = new Order
            {
                Id = request.Id,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId
            };

            //act
            _bookingService.AddBooking(request);

            //assert
            _context.Order.Should().ContainEquivalentOf(expected, options => options
                .Excluding(order => order.SurgeryType));
        }

        [Test]
        public void CancelBooking_ValidatesRequest()
        {
            //arrange
            var booking = _fixture.Create<Order>();
            _context.Order.Add(booking);
            _context.SaveChanges();

            //act
            _bookingService.CancelBooking(booking.Id);

            //assert
            using (new AssertionScope())
            {
                _cancelBookingValidator.Verify(x => x.ValidateRequest(booking.Id), Times.Once);
            }
        }

        [Test]
        public void CancelBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            _cancelBookingValidator.Setup(x => x.ValidateRequest(It.IsAny<Guid>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.CancelBooking(_fixture.Create<Guid>()));

            //assert
            using (new AssertionScope())
            {
                exception.Message.Should().Be(failedValidationResult.Errors.First());
            }
        }

        [Test]
        public void GetPatientNextBooking_GetsNextActiveBooking()
        {
            //arrange
            var booking = _fixture.Build<Order>()
                .With(o => o.Status, 0)
                .Create();
            _context.Add(booking);
            _context.SaveChanges();

            var expected = new GetNextBookingResponse
            {
                Id = booking.Id,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                DoctorId = booking.DoctorId
            };

            //act
            var result = _bookingService.GetPatientNextBooking(booking.PatientId);

            //assert
            using (new AssertionScope())
            {
                result.Should().BeEquivalentTo(expected);
            }
        }

        [TestCase(55555)]
        public void GetPatientNextBooking_WrongPatientId_ThrowsArgumentException(long patientId)
        {
            //arrange            

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.GetPatientNextBooking(patientId));

            //assert
            using (new AssertionScope())
            {
                exception.Should().BeOfType(typeof(ArgumentException));
            }
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
    }
}
