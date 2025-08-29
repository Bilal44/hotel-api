using System.Linq.Expressions;
using System.Net;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using WaracleBooking.Common;
using WaracleBooking.Exceptions;
using WaracleBooking.Models.Domain.Booking.RequestModels;
using WaracleBooking.Persistence.Entities.Enums;
using WaracleBooking.Persistence.Repositories.Interfaces;
using WaracleBooking.Services;
using WaracleBooking.Services.Interfaces;
using Booking = WaracleBooking.Persistence.Entities.Booking;
using Hotel = WaracleBooking.Persistence.Entities.Hotel;
using Room = WaracleBooking.Persistence.Entities.Room;

namespace WaracleBooking.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _hotelRepository = A.Fake<IHotelRepository>();
            _roomRepository = A.Fake<IRoomRepository>();
            _bookingRepository = A.Fake<IBookingRepository>();

            _bookingService = new BookingService(
                _hotelRepository,
                _roomRepository,
                _bookingRepository,
                A.Fake<IDateTimeSource>(),
                A.Fake<ILogger<BookingService>>());
        }

        [Theory]
        [MemberData(nameof(HotelSearchTestCases))]
        public async Task GetHotelsByNameAsync_WithName_ReturnsMatchingHotels(
            string? name,
            List<Hotel> hotels,
            List<Hotel> expectedResult
        )
        {
            // Arrange
            A.CallTo(() => _hotelRepository.FilterByAsync(
                    A<Expression<Func<Hotel, bool>>>._,
                    A<CancellationToken>._))
                .ReturnsLazily((Expression<Func<Hotel, bool>> predicate, CancellationToken _) =>
                    hotels.Where(predicate.Compile()).ToList());

            // Act
            var result = await _bookingService.GetHotelsByNameAsync(name, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(
                expectedResult.Where(h =>
                    h.Name.Contains(name?.Trim() ?? string.Empty, StringComparison.CurrentCultureIgnoreCase))
            );
        }

        [Fact]
        public async Task GetHotelsByNameAsync_WhenRepositoryThrows_ThrowsApiException()
        {
            // Arrange
            A.CallTo(() =>
                    _hotelRepository.FilterByAsync(A<Expression<Func<Hotel, bool>>>._,
                        A<CancellationToken>._))
                .Throws(new Exception());

            // Act
            var act = async () => await _bookingService.GetHotelsByNameAsync("Test", CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Theory]
        [MemberData(nameof(AvailableRoomTestCases))]
        public async Task GetAvailableRoomsAsync_ReturnsExpectedRooms(
            int hotelId,
            DateOnly from,
            DateOnly to,
            int numberOfGuests,
            List<Room> rooms,
            List<Room> expectedResult)
        {
            A.CallTo(() => _roomRepository.FilterByAsync(
                    A<Expression<Func<Room, bool>>>._,
                    A<CancellationToken>._))
                .ReturnsLazily((Expression<Func<Room, bool>> predicate, CancellationToken _) =>
                    rooms.Where(predicate.Compile()).ToList());

            var result = await _bookingService.GetAvailableRoomsAsync(
                hotelId,
                from,
                to,
                numberOfGuests,
                CancellationToken.None);

            result.Should().BeEquivalentTo(expectedResult);
        }
        
        [Fact]
        public async Task GetAvailableRoomsAsync_WhenRepositoryThrows_ThrowsApiException()
        {
            // Arrange
            A.CallTo(() =>
                    _roomRepository.FilterByAsync(A<Expression<Func<Room, bool>>>._,
                        A<CancellationToken>._))
                .Throws(new Exception());

            // Act
            var act = () => _bookingService.GetAvailableRoomsAsync(1,
                DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(1)), 2,
                CancellationToken.None);
            
            // Assert
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
        }
        
        
        [Fact]
        public async Task GetBookingByIdAsync_WithValidId_ReturnsBooking()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var booking = new Booking { Id = bookingId };
            A.CallTo(() => _bookingRepository.GetByIdAsync(bookingId))
                .Returns(booking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(bookingId);

            // Assert
            result.Should().BeEquivalentTo(booking);
        }
        
        [Fact]
        public async Task GetBookingByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            A.CallTo(() => _bookingRepository.GetByIdAsync(bookingId))
                .Returns((Booking)null);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(bookingId);

            // Assert
            result.Should().BeNull();
        }
        
        [Fact]
        public async Task GetBookingByIdAsync_WhenRepositoryThrows_ThrowsApiException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            A.CallTo(() => _bookingRepository.GetByIdAsync(bookingId))
                .Throws(new Exception());

            // Act
            var act  = async () => await _bookingService.GetBookingByIdAsync(bookingId);
            
            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task CreateBookingAsync_WithValidRequest_CreatesBookingSuccessfully()
        {
            // Arrange
            var room = new Room { Id = 1, Capacity = 2, Bookings = new List<Booking>() };
            var request = new BookingRequest
            {
                RoomId = room.Id,
                From = DateOnly.FromDateTime(DateTime.Today),
                To = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                NumberOfGuests = 2,
                GuestNames = "John Doe"
            };

            A.CallTo(() => _roomRepository.GetByIdAsync(room.Id))
                .Returns(room);

            // Act
            var result = await _bookingService.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeTrue();
            result.Booking.RoomId.Should().Be(room.Id);

            A.CallTo(() => _bookingRepository.AddAsync(A<Booking>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _bookingRepository.UpdateAsync(A<Booking>._))
                .MustHaveHappenedOnceExactly();
        }
        
        [Fact]
        public async Task CreateBookingAsync_WithConcurrentBookingAttempts_HandlesRaceCondition()
        {
            // Arrange
            var room = new Room { Id = 1, Capacity = 2, Bookings = new List<Booking>() };
            var request = new BookingRequest
            {
                RoomId = room.Id,
                From = DateOnly.FromDateTime(DateTime.Today),
                To = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                NumberOfGuests = 2,
                GuestNames = "John Doe"
            };

            A.CallTo(() => _roomRepository.GetByIdAsync(room.Id))
                .Returns(room);

            // Act
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => _bookingService.CreateBookingAsync(request))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            // Assert
            var successfulBookings = results.Count(r => r.Success);
            successfulBookings.Should().Be(1);

            A.CallTo(() => _bookingRepository.AddAsync(A<Booking>._))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _bookingRepository.UpdateAsync(A<Booking>._))
                .MustHaveHappenedOnceExactly();
        }
        
        [Theory]
        [MemberData(nameof(InvalidBookingTestCases))]
        public async Task CreateBookingAsync_WithInvalidRequest_ReturnsValidationFailure(
            BookingRequest request,
            Room? room,
            string expectedError)
        {
            // Arrange
            A.CallTo(() => _roomRepository.GetByIdAsync(request.RoomId)).Returns(room);

            // Act
            var result = await _bookingService.CreateBookingAsync(request);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be(expectedError);

            A.CallTo(() => _bookingRepository.AddAsync(A<Booking>._))
                .MustNotHaveHappened();
            A.CallTo(() => _bookingRepository.UpdateAsync(A<Booking>._))
                .MustNotHaveHappened();
        }
        
        [Fact]
        public async Task CreateBookingAsync_WhenRepositoryThrows_ThrowsApiException()
        {
            // Arrange
            var room = new Room { Id = 1, Capacity = 2, Bookings = new List<Booking>() };
            var request = new BookingRequest
            {
                RoomId = room.Id,
                From = DateOnly.FromDateTime(DateTime.Today),
                To = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                NumberOfGuests = 2,
                GuestNames = "John Doe"
            };
            
            A.CallTo(() => _bookingRepository.AddAsync(A<Booking>._))
                .Throws(new Exception());

            // Act
            var act  = async () => await _bookingService.CreateBookingAsync(request);
            
            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.InternalServerError);
        }
        
        public static IEnumerable<object[]> HotelSearchTestCases =>
            new List<object[]>
            {
                new object[]
                {
                    " test ",
                    new List<Hotel>
                    {
                        new Hotel { Name = "Test Hotel" },
                        new Hotel { Name = "City Inn" }
                    },
                    new List<Hotel>
                    {
                        new Hotel { Name = "Test Hotel" }
                    }
                },
                new object[]
                {
                    null,
                    new List<Hotel>
                    {
                        new Hotel { Name = "Hotel 1" },
                        new Hotel { Name = "Hotel 2" }
                    },
                    new List<Hotel>
                    {
                        new Hotel { Name = "Hotel 1" },
                        new Hotel { Name = "Hotel 2" }
                    }
                },
                new object[]
                {
                    string.Empty,
                    new List<Hotel>
                    {
                        new Hotel { Name = "Hotel 1" },
                        new Hotel { Name = "Hotel 2" },
                        new Hotel { Name = "Hotel 3" }
                    },
                    new List<Hotel>
                    {
                        new Hotel { Name = "Hotel 1" },
                        new Hotel { Name = "Hotel 2" },
                        new Hotel { Name = "Hotel 3" }
                    }
                }
            };

        public static IEnumerable<object[]> AvailableRoomTestCases =>
            new List<object[]>
            {
                // Valid parameters, returns all rooms
                new object[]
                {
                    1,
                    DateOnly.FromDateTime(DateTime.Today),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    1,
                    new List<Room>
                    {
                        new Room { HotelId = 1, Capacity = 2, Bookings = new List<Booking>() },
                        new Room { HotelId = 1, Capacity = 1, Bookings = new List<Booking>(), }
                    },
                    new List<Room>
                    {
                        new Room { HotelId = 1, Capacity = 2, Bookings = new List<Booking>() },
                        new Room { HotelId = 1, Capacity = 1, Bookings = new List<Booking>(), }
                    }
                },

                // Exceeding guest count, filters out small rooms
                new object[]
                {
                    1,
                    DateOnly.FromDateTime(DateTime.Today),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    2,
                    new List<Room>
                    {
                        new Room { Id = 1, HotelId = 1, Capacity = 1, Bookings = new List<Booking>() },
                        new Room { Id = 2, HotelId = 1, Capacity = 2, Bookings = new List<Booking>() }
                    },
                    new List<Room>
                    {
                        new Room { Id = 2, HotelId = 1, Capacity = 2, Bookings = new List<Booking>() }
                    }
                },

                // Overlapping bookings, excludes booked and pending rooms
                new object[]
                {
                    1,
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                    2,
                    new List<Room>
                    {
                        new Room
                        {
                            Id = 1,
                            HotelId = 1,
                            Capacity = 2,
                            Bookings = new List<Booking>
                            {
                                new Booking
                                {
                                    CheckIn = DateOnly.FromDateTime(DateTime.Today),
                                    CheckOut = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                                    Status = BookingStatus.Success
                                }
                            }
                        },
                        new Room
                        {
                            Id = 2,
                            HotelId = 1,
                            Capacity = 2,
                            Bookings = new List<Booking>
                            {
                                new Booking
                                {
                                    CheckIn = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                                    CheckOut = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                                    Status = BookingStatus.Pending
                                }
                            }
                        }
                    },
                    new List<Room>() // Expected: empty
                },

                // Specific hotel ID, filters out other hotels
                new object[]
                {
                    1,
                    DateOnly.FromDateTime(DateTime.Today),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    2,
                    new List<Room>
                    {
                        new Room { HotelId = 1, Capacity = 2, Bookings = new List<Booking>() },
                        new Room { HotelId = 99, Capacity = 2, Bookings = new List<Booking>() }
                    },
                    new List<Room>
                    {
                        new Room { HotelId = 1, Capacity = 2, Bookings = new List<Booking>() }
                    }
                },

                // Non-existent hotel ID, returns empty
                new object[]
                {
                    999,
                    DateOnly.FromDateTime(DateTime.Today),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    2,
                    new List<Room>
                    {
                        new Room { HotelId = 1, Capacity = 2, Bookings = new List<Booking>() },
                        new Room { HotelId = 1, Capacity = 2, Bookings = new List<Booking>() }
                    },
                    new List<Room>() // Expected: empty
                },

                // Cancelled bookings, room should be available
                new object[]
                {
                    1,
                    DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                    DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                    2,
                    new List<Room>
                    {
                        new Room
                        {
                            HotelId = 1,
                            Capacity = 2,
                            Bookings = new List<Booking>
                            {
                                new Booking
                                {
                                    CheckIn = DateOnly.FromDateTime(DateTime.Today),
                                    CheckOut = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                                    Status = BookingStatus.Cancelled
                                }
                            }
                        }
                    },
                    new List<Room>
                    {
                        new Room
                        {
                            HotelId = 1,
                            Capacity = 2,
                            Bookings = new List<Booking>
                            {
                                new Booking
                                {
                                    CheckIn = DateOnly.FromDateTime(DateTime.Today),
                                    CheckOut = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                                    Status = BookingStatus.Cancelled
                                }
                            }
                        }
                    }
                }
            };

        public static IEnumerable<object[]> InvalidBookingTestCases =>
            new List<object[]>
            {
                // Non-existent room
                new object[]
                {
                    new BookingRequest { RoomId = 1 },
                    null,
                    "Room not found."
                },
                
                // Same day check out
                new object[]
                {
                    new BookingRequest
                    {
                        RoomId = 1,
                        From = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                        To = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                        NumberOfGuests = 1
                    },
                    new Room { Id = 1, Capacity = 2, Bookings = new List<Booking>() },
                    "`To` date must be after `From` date."
                },
                
                // Invalid date range
                new object[]
                {
                    new BookingRequest
                    {
                        RoomId = 1,
                        From = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                        To = DateOnly.FromDateTime(DateTime.Today),
                        NumberOfGuests = 1
                    },
                    new Room { Id = 1, Capacity = 2, Bookings = new List<Booking>() },
                    "`To` date must be after `From` date."
                },
                
                // Invalid guest count
                new object[]
                {
                    new BookingRequest
                    {
                        RoomId = 1,
                        From = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                        To = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                        NumberOfGuests = 0
                    },
                    new Room { Id = 1, Capacity = 2, Bookings = new List<Booking>() },
                    "Invalid number of guests specified, must be at least 1."
                },

                // Overlapping booking
                new object[]
                {
                    new BookingRequest
                    {
                        RoomId = 1,
                        From = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                        To = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                        NumberOfGuests = 2
                    },
                    new Room
                    {
                        Id = 1,
                        Capacity = 2,
                        Bookings = new List<Booking>
                        {
                            new Booking
                            {
                                RoomId = 1,
                                CheckIn = DateOnly.FromDateTime(DateTime.Today),
                                CheckOut = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                                Status = BookingStatus.Success
                            }
                        }
                    },
                    "Room is unavailable for the selected dates."
                },

                // Guest count exceeds room capacity
                new object[]
                {
                    new BookingRequest
                    {
                        RoomId = 1,
                        From = DateOnly.FromDateTime(DateTime.Today),
                        To = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                        NumberOfGuests = 3
                    },
                    new Room { Id = 1, Capacity = 2, Bookings = new List<Booking>() },
                    "Room can only accommodate 2 guests."
                }
            };
    }
}