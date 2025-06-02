using Tutorial12.DTOs;

namespace Tutorial12.Services;

public interface IDbService
{
    Task<TripListResponseDTO> GetTripAsync(int page, int pageSize);
    Task DeleteClientAsync(int clientId);
    Task AssignClientToTripAsync(int idTrip, AssignClientToTripDTO assignClientDTO);
}