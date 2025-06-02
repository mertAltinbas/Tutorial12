using Microsoft.EntityFrameworkCore;
using Tutorial12.DTOs;
using Tutorial12.Exceptions;
using Tutorial12.Models;

namespace Tutorial12.Services;

public class DbService : IDbService
{
    private readonly Tut12Context _context;

    public DbService(Tut12Context context)
    {
        _context = context;
    }

    public async Task<TripListResponseDTO> GetTripAsync(int page, int pageSize)
    {
        var totalTrips = await _context.Trips.CountAsync();
        if (totalTrips == 0)
        {
            throw new NotFoundException("No trips found.");
        }

        var totalPages = (int)Math.Ceiling(totalTrips / (double)pageSize);

        var trips = await _context.Trips
            .Include(t => t.IdCountries)
            .Include(t => t.ClientTrips).ThenInclude(ct => ct.IdClientNavigation)
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new TripListResponseDTO
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = totalPages,
            Trips = trips.Select(t => new TripDTO
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new CountryDTO
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(c => new ClientDTO
                {
                    FirstName = c.IdClientNavigation.FirstName,
                    LastName = c.IdClientNavigation.LastName
                }).ToList()
            }).ToList()
        };

        return result;
    }

    public async Task DeleteClientAsync(int clientId)
    {
        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == clientId);

        if (client == null)
            throw new NotFoundException("Client not found.");

        if (client.ClientTrips.Any())
            throw new InvalidOperationException("Cannot delete client with associated trips.");

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }

    public async Task AssignClientToTripAsync(int idTrip, AssignClientToTripDTO assignClientDTO)
    {
        // check client existence
        var existingClient = await _context.Clients
            .FirstOrDefaultAsync(c => c.Pesel == assignClientDTO.Pesel);

        if (existingClient is not null)
        {
            // check if client is already assigned to the trip
            var alreadyRegistered = await _context.ClientTrips
                .FirstOrDefaultAsync(a => a.IdClient == existingClient.IdClient && a.IdTrip == idTrip);
            if (alreadyRegistered is not null)
                throw new ConflictException("Client is already registered for this trip.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(assignClientDTO.Email))
                throw new InvalidOperationException("Email alanı boş olamaz.");

            // create new client
            existingClient = new Client
            {
                FirstName = assignClientDTO.FirstName,
                LastName = assignClientDTO.LastName,
                Email = assignClientDTO.Email,
                Telephone = assignClientDTO.Telephone,
                Pesel = assignClientDTO.Pesel
            };
            _context.Clients.Add(existingClient);
            await _context.SaveChangesAsync();
        }

        // check trip existence and validate date
        var trip = await _context.Trips.FirstOrDefaultAsync(t => t.IdTrip == idTrip);
        if (trip is null)
            throw new NotFoundException("Trip not found.");
        if (trip.DateFrom <= DateTime.Now)
            throw new ConflictException("Cannot assign client to a trip that has already started.");

        // create new client-trip
        var clientTrip = new ClientTrip
        {
            IdClient = existingClient.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = assignClientDTO.PaymentDate
        };
        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();
    }
}