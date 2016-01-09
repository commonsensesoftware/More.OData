namespace More.Examples.Components
{
    using Models;
    using More.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class DeviceRepository : IReadOnlyRepository<Device>
    {
        private readonly Lazy<IQueryable<Device>> database = new Lazy<IQueryable<Device>>( CreateDatabase );

        private static IQueryable<Device> CreateDatabase()
        {
            var devices = new[]
            {
                new Device() { SerialNumber = "12345", Sku = "ABCDE", SkuType = "Cog", IsActive = true, PartNumber = "A2B3C5", LastModified = DateTime.Today, ReceiptId = new Guid( "67b4e997-e004-4521-b87d-b8b4693a8043" ) },
                new Device() { SerialNumber = "67890", Sku = "FGHIJ", SkuType = "Gear", IsActive = true, PartNumber = "D4E5F6", LastModified = DateTime.Today }
            };

            return devices.AsQueryable();
        }

        private IQueryable<Device> Database
        {
            get
            {
                return database.Value;
            }
        }

        public Task<IEnumerable<Device>> GetAsync( Func<IQueryable<Device>, IQueryable<Device>> queryShaper, CancellationToken cancellationToken ) => Task.FromResult( queryShaper( Database ).AsEnumerable() );

        public Task<TResult> GetAsync<TResult>( Func<IQueryable<Device>, TResult> queryShaper, CancellationToken cancellationToken ) => Task.FromResult( queryShaper( Database ) );
    }
}
