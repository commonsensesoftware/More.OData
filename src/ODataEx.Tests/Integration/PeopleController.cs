namespace More.Integration
{
    using ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;

    [ODataRoutePrefix( "People" )]
    public class PeopleController : ODataController
    {
        private readonly IReadOnlyRepository<Person> repository = new PeopleRepository();

        // GET ~/people
        [ODataRoute]
        public async Task<IHttpActionResult> Get( ODataQueryOptions<Person> options ) =>
            this.Success( await repository.GetAsync( q => options.ApplyTo( q ) ) );

        // GET ~/people({id})
        [ODataRoute( "({id})" )]
        public async Task<IHttpActionResult> Get( int id, ODataQueryOptions<Person> options ) =>
            this.SuccessOrNotFound( ( await repository.GetAsync( q => options.ApplyTo( q.Where( p => p.Id == id ) ) ) ).Cast<object>().SingleOrDefault() );

        // GET ~/people({id})/$value
        [HttpGet]
        [ODataRoute( "({id})/$value" )]
        public async Task<IHttpActionResult> GetValue( [FromODataUri] int id )
        {
            var person = await repository.GetSingleAsync( p => p.Id == id );

            if ( person == null || person.PhotoImage == null )
                return NotFound();

            // note: this will return part of the stream if the client specified the "range" header
            return this.SuccessOrPartialContent( person.PhotoImage, person.PhotoImageType );
        }

        // HEAD ~/people({id})/$value
        [HttpHead]
        [ODataRoute( "({id})/$value" )]
        public async Task<IHttpActionResult> HeadValue( [FromODataUri] int id )
        {
            var person = await repository.GetSingleAsync( p => p.Id == id );

            if ( person == null || person.PhotoImage == null )
                return NotFound();

            return this.OkWithContentHeaders( person.PhotoImage.Length, person.PhotoImageType );
        }
    }
}
