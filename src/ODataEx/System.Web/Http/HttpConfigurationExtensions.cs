namespace System.Web.Http
{
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Linq;
    using More.Web.OData;
    using More.Web.OData.Formatter;
    using OData.Formatter;
    using OData.Formatter.Deserialization;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpConfiguration"/> class.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        private static FeaturedODataSerializerProvider GetOrAddFeaturedSerializationProvider( this HttpConfiguration configuration )
        {
            var serializerProvider = configuration.Formatters.OfType<ODataMediaTypeFormatter>().Select( f => f.SerializerProvider ).OfType<FeaturedODataSerializerProvider>().FirstOrDefault();

            if ( serializerProvider != null )
                return serializerProvider;

            serializerProvider = new FeaturedODataSerializerProvider();
            var deserializerProvider = new DefaultODataDeserializerProvider();
            var formatters = ODataMediaTypeFormatters.Create( serializerProvider, deserializerProvider );

            configuration.Formatters.InsertRange( 0, formatters );
            return serializerProvider;
        }

        /// <summary>
        /// Adds the specified serialization features to the configured OData serializer provider.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <param name="serializationFeatures">The set of <see cref="IODataSerializationFeature">serialization features</see> to add.</param>
        /// <returns>The original <see cref="HttpConfiguration"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public static HttpConfiguration AddSerializationFeatures( this HttpConfiguration configuration, params IODataSerializationFeature[] serializationFeatures )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( serializationFeatures, nameof( serializationFeatures ) );
            Contract.Ensures( Contract.Result<HttpConfiguration>() != null );

            var serializerProvider = configuration.GetOrAddFeaturedSerializationProvider();

            foreach ( var serializationFeature in serializationFeatures )
                serializerProvider.SerializationFeatures.Add( serializationFeature );

            return configuration;
        }

        /// <summary>
        /// Configures OData formatting with support for media resources.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <returns>The original <see cref="HttpConfiguration"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static HttpConfiguration EnableMediaResources( this HttpConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Contract.Ensures( Contract.Result<HttpConfiguration>() != null );

            var serializerProvider = configuration.GetOrAddFeaturedSerializationProvider();
            var feature = serializerProvider.SerializationFeatures.OfType<MediaResourceSerializationFeature>().FirstOrDefault();

            // guard against the feature being added twice
            if ( feature == null )
                serializerProvider.SerializationFeatures.Add( new MediaResourceSerializationFeature() );

            return configuration;
        }

        /// <summary>
        /// Configures OData formatting with support for instance annotations.
        /// </summary>
        /// <param name="configuration">The extended <see cref="HttpConfiguration">HTTP configuration</see>.</param>
        /// <returns>The original <see cref="HttpConfiguration"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static HttpConfiguration EnableInstanceAnnotations( this HttpConfiguration configuration )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Contract.Ensures( Contract.Result<HttpConfiguration>() != null );

            var serializerProvider = configuration.GetOrAddFeaturedSerializationProvider();
            var feature = serializerProvider.SerializationFeatures.OfType<InstanceAnnotationSerializationFeature>().FirstOrDefault();

            // guard against the feature being added twice
            if ( feature != null )
                return configuration;

            serializerProvider.SerializationFeatures.Add( new InstanceAnnotationSerializationFeature() );

            // HACK: remove this handler once the "Preference-Applied" HTTP header is properly returned by the OData libraries
            configuration.MessageHandlers.Add( new SimplePreferenceAppliedHandler() );
            return configuration;
        }
    }
}
