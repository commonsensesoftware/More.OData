namespace System.Web.OData.Builder
{
    using Collections.Concurrent;
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Linq;
    using Linq.Expressions;
    using Microsoft.OData.Edm;
    using More.OData.Edm;
    using More.Web.OData.Builder;
    using Reflection;
    using System;
    using static Reflection.BindingFlags;

    /// <summary>
    /// Provides extension methods related to entity model construction.
    /// </summary>
    [SuppressMessage( "Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This due to the high number of overloads. Coupling is allowed given they are extension methods. Consider partial classes if necessary." )]
    public static partial class BuilderExtensions
    {
        private static readonly Lazy<MethodInfo> applyLowerCamelCase = new Lazy<MethodInfo>( () => typeof( LowerCamelCaser ).GetMethod( nameof( LowerCamelCaser.ApplyLowerCamelCase ) ) );
        private static readonly ConcurrentDictionary<ODataModelBuilder, AnnotationConfigurationCollection> annotationConfigurations = new ConcurrentDictionary<ODataModelBuilder, AnnotationConfigurationCollection>();
        private static readonly ConcurrentDictionary<object, StructuralTypeConfiguration> structuralTypeConfigToModelBuilderMap = new ConcurrentDictionary<object, StructuralTypeConfiguration>();

        private static Lazy<Func<object, object>> ToLazyContravariantFunc<TStructuralType, TProperty>( this Expression<Func<TStructuralType, TProperty>> propertyExpression )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( Contract.Result<Lazy<Func<object, object>>>() != null );

            var expression = propertyExpression;

            // compile the property expression into a lazy-initialized contravariant function
            // we do this so that we don't have to use reflection later to get the annotation value
            return new Lazy<Func<object, object>>( () =>
            {
                var func = expression.Compile();
                return o => func( (TStructuralType) o );
            } );
        }

        private static bool IsLowerCamelCaseEnabled( this ODataModelBuilder modelBuilder )
        {
            Contract.Requires( modelBuilder != null );

            var builder = modelBuilder as ODataConventionModelBuilder;

            if ( builder == null )
                return false;

            // note: lower camel casing is enabled if the EnableLowerCamelCase has been called, which adds the
            // LowerCamelCaser.ApplyLowerCamelCase method to the OnModelCreating callback
            var modelCreating = builder.OnModelCreating;
            return modelCreating?.GetInvocationList().Any( d => d.Method == applyLowerCamelCase.Value ) ?? false;
        }

        private static void ClearModelBuilderMap( ODataModelBuilder modelBuilder )
        {
            Contract.Requires( modelBuilder != null );

            var keys = structuralTypeConfigToModelBuilderMap.Select( kvp => kvp.Value.ModelBuilder == modelBuilder ).ToArray();
            StructuralTypeConfiguration value;

            foreach ( var key in keys )
                structuralTypeConfigToModelBuilderMap.TryRemove( key, out value );
        }

        internal static StructuralTypeConfiguration GetInnerConfiguration<TStructuralType>( this StructuralTypeConfiguration<TStructuralType> configuration ) where TStructuralType : class
        {
            Contract.Requires( configuration != null );
            Contract.Ensures( Contract.Result<StructuralTypeConfiguration>() != null );

            // there appears to be no other way to get the underlying model builder
            // note: we can't build a dynamic using a lambda expression, so use a dictionary to minimize reflection use
            return structuralTypeConfigToModelBuilderMap.GetOrAdd(
                configuration,
                key =>
                {
                    var type = typeof( StructuralTypeConfiguration<TStructuralType> );
                    var field = type.GetField( "_configuration", Instance | NonPublic );
                    return (StructuralTypeConfiguration) field.GetValue( key );
                } );
        }

        /// <summary>
        /// Returns the model builder associated with the configuration.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see>.</typeparam>
        /// <param name="configuration">The <see cref="EntitySetConfiguration{TStructuralType}">configuration</see> to get the model builder for.</param>
        /// <returns>The associated <see cref="ODataModelBuilder">model builder</see>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static ODataModelBuilder GetModelBuilder<TStructuralType>( this EntitySetConfiguration<TStructuralType> configuration ) where TStructuralType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Contract.Ensures( Contract.Result<ODataModelBuilder>() != null );
            return configuration.EntityType.GetModelBuilder();
        }

        /// <summary>
        /// Returns the model builder associated with the configuration.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see>.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">configuration</see> to get the model builder for.</param>
        /// <returns>The associated <see cref="ODataModelBuilder">model builder</see>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static ODataModelBuilder GetModelBuilder<TStructuralType>( this StructuralTypeConfiguration<TStructuralType> configuration ) where TStructuralType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Contract.Ensures( Contract.Result<ODataModelBuilder>() != null );
            return configuration.GetInnerConfiguration().ModelBuilder;
        }

        /// <summary>
        /// Gets the annotation configurations associated with the specified model builder.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder">model builder</see> get the annotation configurations for.</param>
        /// <returns>A <see cref="AnnotationConfigurationCollection">collection</see> of <see cref="IAnnotationConfiguration">annotation configurations</see>.</returns>
        public static AnnotationConfigurationCollection GetAnnotationConfigurations( this ODataModelBuilder modelBuilder )
        {
            Arg.NotNull( modelBuilder, nameof( modelBuilder ) );
            Contract.Ensures( Contract.Result<AnnotationConfigurationCollection>() != null );
            return annotationConfigurations.GetOrAdd( modelBuilder, b => new AnnotationConfigurationCollection() );
        }

        /// <summary>
        /// Applies the annotation configurations for the model builder to the specified model.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder">model builder</see> whose annotation configurations to apply.</param>
        /// <param name="model">The <see cref="IEdmModel">model</see> to apply the annotation configurations to.</param>
        /// <remarks>This method clears all annotation configurations associated with the <paramref name="modelBuilder">model builder</paramref>
        /// after they have been applied to the model.</remarks>
        public static void ApplyAnnotations( this ODataModelBuilder modelBuilder, IEdmModel model )
        {
            Arg.NotNull( modelBuilder, nameof( modelBuilder ) );
            Arg.NotNull( model, nameof( model ) );

            // apply casing annotation so that configurations can interogate this from the model later
            model.SetAnnotationValue( model, new LowerCamelCaseAnnotation( modelBuilder.IsLowerCamelCaseEnabled() ) );

            AnnotationConfigurationCollection configurations;

            // short-circuit if there's nothing to do
            if ( !annotationConfigurations.TryRemove( modelBuilder, out configurations ) )
                goto ClearMap;

            foreach ( var configuration in configurations )
                configuration.Apply( model );

            configurations.Clear();

            ClearMap:
            ClearModelBuilderMap( modelBuilder );
        }

        /// <summary>
        /// Creates and returns a Entity Data Model (EDM) and applies any associated annotations.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder">model builder</see> to create, apply, and return the model for.</param>
        /// <returns>The created <see cref="IEdmModel">model</see> with all associated annotations applied.</returns>
        /// <remarks>This method is the equivalent of calling <see cref="ODataModelBuilder.GetEdmModel"/> and <see cref="ApplyAnnotations(ODataModelBuilder, IEdmModel)"/>;
        /// therefore, this method also clears all annotation configurations associated with the <paramref name="modelBuilder">model builder</paramref>
        /// after they have been applied to the model.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0" )]
        public static IEdmModel GetEdmModelWithAnnotations( this ODataModelBuilder modelBuilder )
        {
            Arg.NotNull( modelBuilder, nameof( modelBuilder ) );
            Contract.Ensures( Contract.Result<IEdmModel>() != null );

            var model = modelBuilder.GetEdmModel();
            modelBuilder.ApplyAnnotations( model );
            return model;
        }

        /// <summary>
        /// Configures the specified entity as a media link entry (MLE) and uses the provided property to retrieve the media content type.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to configure.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to configure as a media link entry.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the media content type.</param>
        /// <returns>The <see cref="MediaTypeConfiguration{TEntityType}">configuration</see> that can be used to further configure the media type.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity. This method can be called multiple times for the same property.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public static MediaTypeConfiguration<TEntityType> MediaType<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, string>> propertyExpression ) where TEntityType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<MediaTypeConfiguration<TEntityType>>() != null );

            // an entity that represents a media type can only have a single link entry. check for presense of an existing configuration.
            var builder = configuration.GetModelBuilder();
            var configurations = builder.GetAnnotationConfigurations();
            MediaTypeConfiguration<TEntityType> mediaTypeConfig;

            // if the media type has already been configured, return the existing configuration
            if ( configurations.TryGet( propertyExpression, out mediaTypeConfig ) )
                return mediaTypeConfig;

            // always ignore the content type property from the entity model and call the build-in MediaType
            // extension method, which will configure the entity model with HasStream = true for the entity
            configuration.Ignore( propertyExpression );
            configuration.MediaType();

            // compile the property expression into a lazy-initialized, contravariant function
            // we do this so that we don't have to use reflection later to get the content type
            // nor require the developer to write explicit code to get the value
            var contentType = new Lazy<Func<object, string>>( () =>
            {
                var func = propertyExpression.Compile();
                return o => func( (TEntityType) o );
            } );
            var annotation = new MediaLinkEntryAnnotation( contentType );

            mediaTypeConfig = new MediaTypeConfiguration<TEntityType>( configuration, annotation );
            configurations.Add( propertyExpression, mediaTypeConfig );

            return mediaTypeConfig;
        }
    }
}
