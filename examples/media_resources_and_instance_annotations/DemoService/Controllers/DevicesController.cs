namespace More.Examples.Controllers
{
    using Components;
    using Models;
    using More.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    /// <summary>
    /// Represents an OData service for the devices.
    /// </summary>
    [ODataRoutePrefix( "Devices" )]
    public class DevicesController : ODataController
    {
        private readonly IReadOnlyRepository<Device> repository = new DeviceRepository();

        // OPTIONS ~/devices
        [HttpOptions]
        [ODataRoute]
        public IHttpActionResult Options()
        {
            var response = new HttpResponseMessage( HttpStatusCode.OK );
            response.Content = new StringContent( string.Empty );
            response.Content.Headers.Add( "Allow", new[] { "GET", "OPTIONS" } );
            response.Content.Headers.ContentType = null;
            return ResponseMessage( response );
        }

        // GET ~/devices
        [ODataRoute]
        public async Task<IHttpActionResult> Get( ODataQueryOptions<Device> options ) =>
            this.Success( await repository.GetAsync( q => options.ApplyTo( q ) ) );

        // GET ~/devices(serialNumber)
        [ODataRoute( "({serialNumber})" )]
        public async Task<IHttpActionResult> Get( [FromODataUri] string serialNumber, ODataQueryOptions<Device> options ) =>
            this.SuccessOrNotFound( ( await repository.GetAsync( q => options.ApplyTo( q.Where( d => d.SerialNumber == serialNumber ) ) ) ).Cast<object>().SingleOrDefault() );
    }
}
