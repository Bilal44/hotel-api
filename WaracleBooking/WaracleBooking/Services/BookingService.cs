using System.Net;
using WaracleBooking.Common;
using WaracleBooking.Exceptions;
using WaracleBooking.Models.Domain.Booking;
using WaracleBooking.Models.Domain.Booking.RequestModels;
using WaracleBooking.Models.Domain.Booking.Validators;
using WaracleBooking.Persistence.Entities.Enums;
using WaracleBooking.Persistence.Repositories.Interfaces;
using WaracleBooking.Services.Interfaces;
using WaracleBooking.Persistence.Entities;

namespace WaracleBooking.Services;

public class BookingService(
    IHotelRepository hotelRepository,
    IRoomRepository roomRepository,
    IBookingRepository bookingRepository,
    IDateTimeSource dateTimeSource,
    ILogger<BookingService> logger)
    : IBookingService
{
    public async Task<List<Hotel>> GetHotelsByNameAsync(string? name, CancellationToken cancellationToken)
    {
        try
        {
            return await hotelRepository.FilterByAsync(h =>
                    string.IsNullOrWhiteSpace(name) ||
                    h.Name.Contains(name.Trim(), StringComparison.CurrentCultureIgnoreCase),
                cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(
                "Encountered an error while searching for [{name}]",
                name?.Trim());

            throw new ApiException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(
        int hotelId,
        DateOnly from,
        DateOnly to,
        int numberOfGuests,
        CancellationToken cancellationToken)
    {
        try
        {
            /*
             Only retrieve rooms that:
             1- Belong to the hotel
             2- Can accommodate the specified number of guests
             3- Have no `Pending` or `Successful` overlapping booking
             
             A booking's check-in date can occur on the same day
                as the check-out date of another one
                (e.g.  booking 1 = 25 Jan in -> 28 Jan out is valid if
                existing booking = 22 Jan in -> 25 Jan out)
             */
            return await roomRepository.FilterByAsync(room =>
                room.HotelId == hotelId &&
                room.Capacity >= numberOfGuests &&
                !room.Bookings.Any(booking =>
                    booking.CheckIn < to &&
                    booking.CheckOut > from &&
                    (booking.Status == BookingStatus.Success ||
                     booking.Status == BookingStatus.Pending)),
                cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(
                "Encountered an error while checking room available for hotel id [{HotelId}] from [{FromDate}] to [{ToDate}]",
                hotelId,
                from,
                to);

            throw new ApiException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Booking?> GetBookingByIdAsync(Guid id)
    {
        try
        {
            return await bookingRepository.GetByIdAsync(id);
        }
        catch (Exception e)
        {
            logger.LogError(
                "Encountered an error while retrieving booking for [{BookingId}]",
                id);

            throw new ApiException(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<BookingValidationResult> CreateBookingAsync(BookingRequest request)
    {
        try
        {
            var room = await roomRepository.GetByIdAsync(request.RoomId);
            if (room == null)
                return BookingValidationResult.Fail("Room not found.");

            var response = ValidateBookingRequest(request, room);
            if (!response.Success)
                return response;

            var booking = new Booking
            {
                RoomId = room.Id,
                CreatedAt = dateTimeSource.UtcNow,
                GuestNames = request.GuestNames,
                CheckIn = request.From,
                CheckOut = request.To,
                NumberOfGuests = request.NumberOfGuests,
                Status = BookingStatus.Pending
            };
            room.Bookings.Add(booking);
            await bookingRepository.AddAsync(booking);

            // Payment logic and additional calls to external APIs

            booking.Status = BookingStatus.Success;
            booking.UpdatedAt = dateTimeSource.UtcNow;
            await bookingRepository.UpdateAsync(booking);

            return BookingValidationResult.Pass(booking);
        }
        catch (Exception e)
        {
            logger.LogError(
                "Encountered an error while trying to create a new booking for request [{@BookingRequest}]",
                request);

            throw new ApiException(HttpStatusCode.InternalServerError, e.Message);
        }
    }
    
    private static BookingValidationResult ValidateBookingRequest(BookingRequest request, Room room)
    {
        var error = DateAndGuestInputValidator.Validate(request.From, request.To, request.NumberOfGuests);
        if (error is not null)
            return BookingValidationResult.Fail(error);

        var isBooked = room.Bookings.Any(b =>
            b.RoomId == request.RoomId &&
            b.CheckIn < request.To &&
            b.CheckOut > request.From &&
            b.Status is BookingStatus.Success or BookingStatus.Pending);

        if (isBooked)
            return BookingValidationResult.Fail("Room is unavailable for the selected dates.");

        if (request.NumberOfGuests > room.Capacity)
            return BookingValidationResult.Fail($"Room can only accommodate {room.Capacity} guest{(room.Capacity > 1 ? "s" : "")}.");

        return new BookingValidationResult { Success = true };
    }
}