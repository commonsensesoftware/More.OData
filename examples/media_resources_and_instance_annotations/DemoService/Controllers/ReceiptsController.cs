namespace More.Examples.Controllers
{
    using Components;
    using Models;
    using More.ComponentModel;
    using More.IO;
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    /// <summary>
    /// Represents an OData service for the receipts.
    /// </summary>
    [ODataRoutePrefix( "Receipts" )]
    public class ReceiptsController : ODataController
    {
        private readonly IReadOnlyRepository<Receipt> repository = new ReceiptRepository();
        private readonly IFileSystem fileSystem = new FileSystem();

        // OPTIONS ~/receipts
        [HttpOptions]
        [ODataRoute]
        public IHttpActionResult Options()
        {
            var response = new HttpResponseMessage( HttpStatusCode.OK );
            response.Content = new StringContent( string.Empty );
            response.Content.Headers.Add( "Allow", new[] { "GET", "HEAD", "OPTIONS" } );
            response.Content.Headers.ContentType = null;
            return ResponseMessage( response );
        }

        // GET ~/receipts
        [ODataRoute]
        public async Task<IHttpActionResult> Get( ODataQueryOptions<Receipt> options ) =>
            this.Success( await repository.GetAsync( q => options.ApplyTo( q ) ) );

        // GET ~/receipts(<guid>)
        [ODataRoute( "({key})" )]
        public async Task<IHttpActionResult> Get( [FromODataUri] Guid key, ODataQueryOptions<Receipt> options ) =>
            this.SuccessOrNotFound( ( await repository.GetAsync( q => options.ApplyTo( q.Where( r => r.Id == key ) ) ) ).Cast<object>().SingleOrDefault() );

        // GET ~/receipts(<guid>)/$value
        [HttpGet]
        [ODataRoute( "({key})/$value" )]
        public async Task<IHttpActionResult> GetValue( [FromODataUri] Guid key )
        {
            var receipt = await repository.GetSingleAsync( r => r.Id == key );

            if ( receipt == null )
                return NotFound();

            var file = await fileSystem.TryGetFileAsync( receipt.ImagePath );

            if ( file == null )
                return NotFound();

            var stream = await file.OpenReadAsync();

            return this.SuccessOrPartialContent( stream, receipt.ImageType );
        }

        // HEAD ~/receipts(<guid>)/$value
        [HttpHead]
        [ODataRoute( "({key})/$value" )]
        public async Task<IHttpActionResult> HeadValue( [FromODataUri] Guid key )
        {
            var receipt = await repository.GetSingleAsync( r => r.Id == key );

            if ( receipt == null || !string.IsNullOrEmpty( receipt.ImagePath ) )
                return NotFound();

            return this.OkWithContentHeaders( receipt.ImageSize, receipt.ImageType );
        }
    }
}
