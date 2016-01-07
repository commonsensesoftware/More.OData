namespace More.Integration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;
    using Xunit;
    using static System.Net.HttpStatusCode;

    /// <summary>
    /// Provides integration unit tests for OData instance annotations.
    /// </summary>
    public class InstanceAnnotationsTest : WebApiUnitTest
    {
        protected override void Initialize( HttpConfiguration configuration )
        {
            var builder = new ODataConventionModelBuilder();

            builder.Namespace = "Test";

            var people = builder.EntitySet<Person>( "People" );
            var person = people.EntityType;

            person.Namespace = builder.Namespace;
            person.HasKey( p => p.Id );
            person.HasAnnotation( p => p.Timestamp );
            person.HasAnnotation( p => p.Flags );
            person.HasAnnotations( p => p.SeoTerms );
            person.HasComplexAnnotations( p => p.Links );
            person.Ignore( p => p.PhotoImage );
            person.Ignore( p => p.PhotoImageType );

            var model = builder.GetEdmModelWithAnnotations();

            configuration.EnableInstanceAnnotations();
            configuration.MapODataServiceRoute( "odata", "api", model );
        }

        [Fact( DisplayName = "http get should return entity with primitive instance annotation" )]
        public async Task HttpGetShouldReturnEntityWithPrimitiveInstanceAnnotation()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=Test.Timestamp";
            var request = NewGetRequest( "api/People(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var timestamp = (DateTime) json["@Test.Timestamp"];

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.Equal( new DateTime( 2016, 1, 4 ), timestamp );
        }

        [Theory( DisplayName = "http get should return entity with nullable primitive instance annotation" )]
        [InlineData( 1, 42 )]
        [InlineData( 2, null )]
        public async Task HttpGetShouldReturnEntityWithNullablePrimitiveInstanceAnnotation( int id, int? expected )
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=Test.Flags";
            var request = NewGetRequest( $"api/People({id})" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            object value;
            int? actual = null;

            if ( json.TryGetValue( "@Test.Flags", out value ) )
                actual = new int?( Convert.ToInt32( value ) );

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "http get should return entity with primitive instance annotations" )]
        public async Task HttpGetShouldReturnEntityWithPrimitiveInstanceAnnotations()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=Test.SeoTerms";
            var request = NewGetRequest( "api/People(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var seoTerms = ( (IEnumerable<object>) json["@Test.SeoTerms"] ).Select( o => o.ToString() );

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.True( seoTerms.SequenceEqual( new[] { "Doe" } ) );
        }

        [Fact( DisplayName = "http get should return entity with complex instance annotations" )]
        public async Task HttpGetShouldReturnEntityWithComplexInstanceAnnotations()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=Test.Links";
            var request = NewGetRequest( "api/People(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var link = ( (IEnumerable<dynamic>) json["@Test.Links"] ).Single();

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.Equal( "receipt", (string) link.Name );
            Assert.Equal( "http://remote/api/receipts(67b4e997-e004-4521-b87d-b8b4693a8043)", (string) link.Url );
        }

        [Fact( DisplayName = "http get should return entity with all instance annotations" )]
        public async Task HttpGetShouldReturnEntityWithAllInstanceAnnotations()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=*";
            var request = NewGetRequest( "api/People(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var timestamp = (DateTime) json["@Test.Timestamp"];
            var flags = Convert.ToInt32( json["@Test.Flags"] );
            var seoTerms = ( (IEnumerable<object>) json["@Test.SeoTerms"] ).Select( o => o.ToString() );
            var link = ( (IEnumerable<dynamic>) json["@Test.Links"] ).Single();

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.Equal( new DateTime( 2016, 1, 4 ), timestamp );
            Assert.Equal( 42, flags );
            Assert.True( seoTerms.SequenceEqual( new[] { "Doe" } ) );
            Assert.Equal( "receipt", (string) link.Name );
            Assert.Equal( "http://remote/api/receipts(67b4e997-e004-4521-b87d-b8b4693a8043)", (string) link.Url );
        }
    }
}
