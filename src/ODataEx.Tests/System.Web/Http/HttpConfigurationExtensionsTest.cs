namespace System.Web.Http
{
    using Moq;
    using More.Web.OData.Formatter;
    using OData.Formatter;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="HttpConfigurationExtensions"/>.
    /// </summary>
    public class HttpConfigurationExtensionsTest
    {
        [Fact( DisplayName = "add serialization features should add expected features" )]
        public void AddSerializationFeaturesShouldAddExpectedFeatures()
        {
            // arrange
            var feature = new Mock<IODataSerializationFeature>().Object;
            var configuration = new HttpConfiguration();

            configuration.Formatters.Clear();

            // act
            var other = configuration.AddSerializationFeatures( feature );
            var formatter = (FeaturedODataSerializerProvider) ( (ODataMediaTypeFormatter) configuration.Formatters.First() ).SerializerProvider;

            // assert
            Assert.Same( other, configuration );
            Assert.Equal( feature, formatter.SerializationFeatures.Single() );
        }

        [Fact( DisplayName = "enable media resources should add expected serialization feature" )]
        public void EnableMediaResourcesShouldAddExpectedSerializationFeature()
        {
            // arrange
            var configuration = new HttpConfiguration();

            configuration.Formatters.Clear();

            // act
            var other = configuration.EnableMediaResources();
            var formatter = (FeaturedODataSerializerProvider) ( (ODataMediaTypeFormatter) configuration.Formatters.First() ).SerializerProvider;
            var feature = formatter.SerializationFeatures.Single();

            // assert
            Assert.Same( other, configuration );
            Assert.IsType( typeof( MediaResourceSerializationFeature ), feature );
        }

        [Fact( DisplayName = "enable media resources should be idempotent" )]
        public void EnableMediaResourcesShouldBeIdempotent()
        {
            // arrange
            var configuration = new HttpConfiguration();

            configuration.Formatters.Clear();

            // act
            configuration.EnableMediaResources();
            configuration.EnableMediaResources();

            var formatter = (FeaturedODataSerializerProvider) ( (ODataMediaTypeFormatter) configuration.Formatters.First() ).SerializerProvider;

            // assert
            Assert.Equal( 1, formatter.SerializationFeatures.Count );
        }

        [Fact( DisplayName = "enable instance annotations should add expected serialization feature" )]
        public void EnableInstanceAnnotationsShouldAddExpectedSerializationFeature()
        {
            // arrange
            var configuration = new HttpConfiguration();

            configuration.Formatters.Clear();

            // act
            var other = configuration.EnableInstanceAnnotations();
            var formatter = (FeaturedODataSerializerProvider) ( (ODataMediaTypeFormatter) configuration.Formatters.First() ).SerializerProvider;
            var feature = formatter.SerializationFeatures.Single();

            // assert
            Assert.Same( other, configuration );
            Assert.IsType( typeof( InstanceAnnotationSerializationFeature ), feature );
        }

        [Fact( DisplayName = "enable instance annotations should be idempotent" )]
        public void EnableInstanceAnnotationsShouldBeIdempotent()
        {
            // arrange
            var configuration = new HttpConfiguration();

            configuration.Formatters.Clear();

            // act
            configuration.EnableInstanceAnnotations();
            configuration.EnableInstanceAnnotations();

            var formatter = (FeaturedODataSerializerProvider) ( (ODataMediaTypeFormatter) configuration.Formatters.First() ).SerializerProvider;

            // assert
            Assert.Equal( 1, formatter.SerializationFeatures.Count );
        }
    }
}
