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
    /// Provides integration unit tests for OData instance annotations using camel casing.
    /// </summary>
    public class CamelCaseInstanceAnnotationsTest : WebApiUnitTest
    {
        protected override void Initialize( HttpConfiguration configuration )
        {
            var builder = new ODataConventionModelBuilder();

            builder.Namespace = "Unit.Test";
            builder.EnableLowerCamelCase();

            var address = builder.ComplexType<Address>();
            var people = builder.EntitySet<Person>( "People" );
            var person = people.EntityType;

            address.Namespace = builder.Namespace;
            address.HasAnnotation( a => a.IsPrimary );

            person.Namespace = builder.Namespace;
            person.HasKey( p => p.Id );
            person.HasAnnotation( p => p.Timestamp );
            person.HasAnnotation( p => p.Flags );
            person.HasAnnotations( p => p.SeoTerms );
            person.HasComplexAnnotations( p => p.Links );
            person.Ignore( p => p.PhotoImage );
            person.Ignore( p => p.PhotoImageType );
            person.HasAnnotation( p => p.Birthday ).ForProperty( p => p.Age );
            person.HasComplexAnnotation( p => p.DisplayStyle ).ForProperty( p => p.FirstName );

            var model = builder.GetEdmModelWithAnnotations();

            configuration.EnableCaseInsensitive( true );
            configuration.EnableInstanceAnnotations();
            configuration.MapODataServiceRoute( "odata", "api", model );
        }

        [Fact( DisplayName = "http get should return entity with primitive instance annotation" )]
        public async Task HttpGetShouldReturnEntityWithPrimitiveInstanceAnnotation()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=unit.test.timestamp";
            var request = NewGetRequest( "api/people(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var timestamp = (DateTime) json["@unit.test.timestamp"];

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
            var includedAnnotations = "odata.include-annotations=unit.test.flags";
            var request = NewGetRequest( $"api/people({id})" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            object value;
            int? actual = null;

            if ( json.TryGetValue( "@unit.test.flags", out value ) )
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
            var includedAnnotations = "odata.include-annotations=unit.test.seoTerms";
            var request = NewGetRequest( "api/people(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var seoTerms = ( (IEnumerable<object>) json["@unit.test.seoTerms"] ).Select( o => o.ToString() );

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.True( seoTerms.SequenceEqual( new[] { "Doe" } ) );
        }

        [Fact( DisplayName = "http get should return entity with complex instance annotations" )]
        public async Task HttpGetShouldReturnEntityWithComplexInstanceAnnotations()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=unit.test.links";
            var request = NewGetRequest( "api/people(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var link = ( (IEnumerable<dynamic>) json["@unit.test.links"] ).Single();

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.Equal( "receipt", (string) link.name );
            Assert.Equal( "http://remote/api/receipts(67b4e997-e004-4521-b87d-b8b4693a8043)", (string) link.url );
        }

        [Fact( DisplayName = "http get should return entity with primitive property instance annotation" )]
        public async Task HttpGetShouldReturnEntityWithPrimitivePropertyInstanceAnnotation()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=unit.test.birthday";
            var request = NewGetRequest( "api/people(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var birthday = (DateTime) json["age@unit.test.birthday"];

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.Equal( new DateTime( 2006, 1, 7 ), birthday );
        }

        [Fact( DisplayName = "http get should return entity with complex property instance annotation" )]
        public async Task HttpGetShouldReturnEntityWithComplexPropertyInstanceAnnotation()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=unit.test.displayStyle";
            var request = NewGetRequest( "api/people(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var displayStyle = (dynamic) json["firstName@unit.test.displayStyle"];

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.Equal( true, (bool) displayStyle.title );
            Assert.Equal( 1, (int) displayStyle.order );
        }

        [Fact( DisplayName = "http get should return entity with complex value with primitive instance annotation" )]
        public async Task HttpGetShouldReturnEntityWithComplexValueWithPrimitiveInstanceAnnotation()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=unit.test.isPrimary";
            var request = NewGetRequest( "api/people(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var address = ( (IEnumerable<dynamic>) json["addresses"] ).First();
            var value = address["@unit.test.isPrimary"];

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.True( (bool) value );
        }

        [Fact( DisplayName = "http get should return entity with all instance annotations" )]
        public async Task HttpGetShouldReturnEntityWithAllInstanceAnnotations()
        {
            // arrange
            var includedAnnotations = "odata.include-annotations=*";
            var request = NewGetRequest( "api/people(1)" );

            request.Headers.Add( "prefer", includedAnnotations );

            // act
            var response = await SendAsync( request );
            var preferenceApplied = response.Headers.GetValues( "preference-applied" ).Single();
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var timestamp = (DateTime) json["@unit.test.timestamp"];
            var flags = Convert.ToInt32( json["@unit.test.flags"] );
            var seoTerms = ( (IEnumerable<object>) json["@unit.test.seoTerms"] ).Select( o => o.ToString() );
            var link = ( (IEnumerable<dynamic>) json["@unit.test.links"] ).Single();
            var birthday = (DateTime) json["age@unit.test.birthday"];
            var displayStyle = (dynamic) json["firstName@unit.test.displayStyle"];
            var hasPrimaryAddress = (bool) ( (IEnumerable<dynamic>) json["addresses"] ).First()["@unit.test.isPrimary"];

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( includedAnnotations, preferenceApplied );
            Assert.Equal( new DateTime( 2016, 1, 4 ), timestamp );
            Assert.Equal( 42, flags );
            Assert.True( seoTerms.SequenceEqual( new[] { "Doe" } ) );
            Assert.Equal( "receipt", (string) link.name );
            Assert.Equal( "http://remote/api/receipts(67b4e997-e004-4521-b87d-b8b4693a8043)", (string) link.url );
            Assert.Equal( new DateTime( 2006, 1, 7 ), birthday );
            Assert.Equal( true, (bool) displayStyle.title );
            Assert.Equal( 1, (int) displayStyle.order );
            Assert.True( hasPrimaryAddress );
        }
    }
}
