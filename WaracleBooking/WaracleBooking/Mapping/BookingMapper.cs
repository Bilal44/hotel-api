using WaracleBooking.Models.Domain.Booking.ResponseModels;
using WaracleBooking.Persistence.Entities;
using WaracleBooking.Persistence.Entities.Enums;
using BookingStatusModel = WaracleBooking.Models.Domain.Enums.BookingStatus;
using HotelModel = WaracleBooking.Models.Domain.Hotel;
using RoomModel = WaracleBooking.Models.Domain.Room;
using RoomTypeModel = WaracleBooking.Models.Domain.Enums.RoomType;

namespace WaracleBooking.Mapping;

public static class BookingMapper
{
    public static BookingResponse MapToBooking(Booking booking) =>
        new BookingResponse
        {
            Id = booking.Id,
            BookingTime = booking.CreatedAt,
            GuestNames = booking.GuestNames,
            NumberOfGuests = booking.NumberOfGuests,
            CheckIn = booking.CheckIn,
            CheckOut = booking.CheckOut,
            Status = MapBookingStatus(booking.Status)
        };

    public static IEnumerable<HotelModel> MapToHotels(List<Hotel> hotels) =>
        hotels.Select(hotel => new HotelModel
        {
            Id = hotel.Id,
            Name = hotel.Name,
        });

    public static IEnumerable<RoomModel> MapToRooms(List<Room> rooms) =>
        rooms.Select(room => new RoomModel
        {
            Id = room.Id,
            Capacity = room.Capacity,
            Type = MapRoomType(room.Type)
        });

    private static BookingStatusModel MapBookingStatus(BookingStatus status) =>
        status switch
        {
            BookingStatus.Success => BookingStatusModel.Confirmed,
            _ => BookingStatusModel.Failed
        };

    private static RoomTypeModel MapRoomType(RoomType type) =>
        type switch
        {
            RoomType.Single => RoomTypeModel.Single,
            RoomType.Double => RoomTypeModel.Double,
            RoomType.Deluxe => RoomTypeModel.Deluxe,
            _ => throw new NotSupportedException($"Unsupported room type: [{type}]")
        };
}