namespace More.Integration
{
    using ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;
    using Web.OData;
    using static System.Net.Mime.MediaTypeNames.Application;

    [ODataRoutePrefix( "api/People" )]
    public class PeopleController : ODataController
    {
        private readonly IReadOnlyRepository<Person> repository = new PeopleRepository();

        // GET ~/api/people
        [ODataRoute]
        public async Task<IHttpActionResult> Get( ODataQueryOptions<Person> options ) =>
            this.Success( await repository.GetAsync( q => options.ApplyTo( q ) ) );

        // GET ~/api/people({id})
        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Get( int id, ODataQueryOptions<Person> options ) =>
            this.SuccessOrNotFound( ( await repository.GetAsync( q => options.ApplyTo( q.Where( p => p.Id == id ) ) ) ).Cast<object>().SingleOrDefault() );

        // GET ~/api/people({id})/$value
        [HttpGet]
        [ODataRoute( "({key})/$value" )]
        public async Task<IHttpActionResult> GetValue( [FromODataUri] int id )
        {
            var person = await repository.GetSingleAsync( p => p.Id == id );

            if ( person == null || person.PhotoImage == null )
                return NotFound();

            var mediaType = person.PhotoImageType;

            if ( string.IsNullOrEmpty( mediaType ) )
                mediaType = Octet;

            var stream = new MemoryStream( person.PhotoImage );

            // note: this will return part of the stream if the client specified the "range" header
            return this.SuccessOrPartialContent( stream, mediaType );
        }

        // HEAD ~/api/people({id})/$value
        [HttpHead]
        [ODataRoute( "({key})/$value" )]
        public async Task<IHttpActionResult> HeadValue( [FromODataUri] int id )
        {
            var person = await repository.GetSingleAsync( p => p.Id == id );

            if ( person == null || person.PhotoImage == null )
                return NotFound();

            // note: use an empty stream so that web api reports the correct "content-length" header
            // if you try to directly set content-length without content, it will always update to zero
            var contentLength = person.PhotoImage.Length;
            var response = new HttpResponseMessage( HttpStatusCode.OK );
            var content = new StreamContent( new EmptyStream( contentLength ) );
            var mediaType = person.PhotoImageType;

            if ( string.IsNullOrEmpty( mediaType ) )
                mediaType = Octet;

            content.Headers.ContentType = new MediaTypeHeaderValue( mediaType );
            response.Headers.AcceptRanges.Add( "bytes" );
            response.Content = content;

            return ResponseMessage( response );
        }
    }
}
