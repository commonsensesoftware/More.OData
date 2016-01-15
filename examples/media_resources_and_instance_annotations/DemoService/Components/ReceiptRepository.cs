namespace More.Examples.Components
{
    using Models;
    using More.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using static System.DateTime;
    using static System.Globalization.CultureInfo;
    using static System.IO.Directory;
    using static System.IO.Path;
    using static System.Web.Hosting.HostingEnvironment;
    using static System.Xml.Linq.XDocument;

    public class ReceiptRepository : IReadOnlyRepository<Receipt>
    {
        private readonly Lazy<IQueryable<Receipt>> database = new Lazy<IQueryable<Receipt>>( CreateDatabase );

        private static IQueryable<Receipt> CreateDatabase()
        {
            var path = MapPath( "~/Receipts" );
            var files = GetFiles( path, "*.metadata" );
            var receipts = from file in files
                           let xml = Load( file ).Root
                           select new Receipt()
                           {
                               Id = new Guid( GetFileNameWithoutExtension( file ) ),
                               Date = Parse( (string) xml.Element( nameof( Receipt.Date ) ), CurrentCulture ),
                               Name = (string) xml.Element( nameof( Receipt.Name ) ),
                               ReceiptNumber = (string) xml.Element( nameof( Receipt.ReceiptNumber ) ),
                               Retailer = (string) xml.Element( nameof( Receipt.Retailer ) ),
                               Image = new Image()
                               {
                                   Type = (string) xml.Element( "ImageType" ),
                                   Path = ChangeExtension( file, ".bin" ),
                                   Size = long.Parse( (string) xml.Element( "ImageSize" ) )
                               }
                           };

            return receipts.AsQueryable();
        }

        private IQueryable<Receipt> Database
        {
            get
            {
                return database.Value;
            }
        }

        public Task<IEnumerable<Receipt>> GetAsync( Func<IQueryable<Receipt>, IQueryable<Receipt>> queryShaper, CancellationToken cancellationToken ) => Task.FromResult( queryShaper( Database ).AsEnumerable() );

        public Task<TResult> GetAsync<TResult>( Func<IQueryable<Receipt>, TResult> queryShaper, CancellationToken cancellationToken ) => Task.FromResult( queryShaper( Database ) );
    }
}
