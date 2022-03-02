using System.Reflection;
using BuildingBlocks.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Flight.Data
{
    public sealed class FlightDbContext : AppDbContextBase
    {
        public FlightDbContext(DbContextOptions<FlightDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options, httpContextAccessor)
        {
        }

        public DbSet<Flight.Models.Flight> Flights => Set<Flight.Models.Flight>();
        public DbSet<Airport.Models.Airport> Airports => Set<Airport.Models.Airport>();
        public DbSet<Aircraft.Models.Aircraft> Aircraft => Set<Aircraft.Models.Aircraft>();
        public DbSet<Flight.Models.Seat> Seats => Set<Flight.Models.Seat>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);
        }
    }
}
