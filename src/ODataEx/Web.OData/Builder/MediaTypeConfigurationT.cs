namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using More.OData.Edm;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.OData;
    using System.Web.OData.Builder;

    /// <summary>
    /// Represents the configuration for a media link entry (MLE).
    /// </summary>
    /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to configure the media type for.</typeparam>
    public class MediaTypeConfiguration<TEntityType> : IAnnotationConfiguration where TEntityType : class
    {
        private const int Read = 1;
        private const int Write = 2;
        private readonly EntityTypeConfiguration<TEntityType> entityType;
        private readonly MediaLinkEntryAnnotation annotation;
        private int access = Read;
        private Func<EntityInstanceContext, Uri> currentReadLinkFactory = DefaultLinkFactory;
        private Func<EntityInstanceContext, Uri> currentEditLinkFactory = DefaultLinkFactory;
        private Func<EntityInstanceContext, string> currentEtagGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeConfiguration{TEntityType}"/> class.
        /// </summary>
        /// <param name="entityType">The associated <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration.</see>.</param>
        /// <param name="annotation">The <see cref="MediaLinkEntryAnnotation">annotation</see> to configure.</param>
        public MediaTypeConfiguration( EntityTypeConfiguration<TEntityType> entityType, MediaLinkEntryAnnotation annotation )
        {
            Arg.NotNull( entityType, nameof( entityType ) );
            Arg.NotNull( annotation, nameof( annotation ) );

            this.entityType = entityType;
            this.annotation = annotation;
        }

        private static Uri DefaultLinkFactory( EntityInstanceContext entityContext )
        {
            Contract.Requires( entityContext != null );
            Contract.Ensures( Contract.Result<Uri>() != null );

            // build the read link using the standard conventions
            var selfLink = entityContext.GenerateSelfLink( false );
            var builder = new UriBuilder( selfLink );
            builder.Path = builder.Path.TrimEnd( '/' ) + "/$value";
            return builder.Uri;
        }

        /// <summary>
        /// Gets the entity type configuration the media type configuration is associated with.
        /// </summary>
        /// <value>The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>
        /// associated with the current media type configuration.</value>
        public EntityTypeConfiguration<TEntityType> EntityType
        {
            get
            {
                Contract.Ensures( entityType != null );
                return entityType;
            }
        }

        [SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Not applicable in this type and assessible via the EntityType property." )]
        string IAnnotationConfiguration.Namespace
        {
            get
            {
                return entityType.Namespace;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                entityType.Namespace = value;
            }
        }

        [SuppressMessage( "Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes", Justification = "Not applicable in this type and assessible via the EntityType property." )]
        string IAnnotationConfiguration.Name
        {
            get
            {
                return entityType.Name;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                entityType.Name = value;
            }
        }

        /// <summary>
        /// Registers a factory that generates read links for the media resource.
        /// </summary>
        /// <param name="readLinkFactory">The factory <see cref="Func{T, TResult}">method</see> that generates read links for the media resource.</param>
        /// <returns>The original <see cref="MediaTypeConfiguration{TEntityType}">media type configuration</see>.</returns>
        /// <remarks>This method can be used to generate read links that do not follow the standard OData conventions. The default
        /// read link factory creates links in the form of "~/entityset/key/$value". Registering a read link factory will also implicitly
        /// indicate read access to the media resource.</remarks>
        public MediaTypeConfiguration<TEntityType> HasReadLink( Func<EntityInstanceContext, Uri> readLinkFactory )
        {
            Arg.NotNull( readLinkFactory, nameof( readLinkFactory ) );
            Contract.Ensures( Contract.Result<MediaTypeConfiguration<TEntityType>>() != null );

            currentReadLinkFactory = readLinkFactory;
            access |= Read;
            return this;
        }

        /// <summary>
        /// Registers a factory that generates edit links for the media resource.
        /// </summary>
        /// <param name="editLinkFactory">The factory <see cref="Func{T, TResult}">method</see> that generates edit links for the media resource.</param>
        /// <returns>The original <see cref="MediaTypeConfiguration{TEntityType}">media type configuration</see>.</returns>
        /// <remarks>This method can be used to generate read links that do not follow the standard OData conventions. The default
        /// read link factory creates links in the form of "~/entityset/key/$value". Registering an edit link factory will also implicitly
        /// indicate write access to the media resource.</remarks>
        public MediaTypeConfiguration<TEntityType> HasEditLink( Func<EntityInstanceContext, Uri> editLinkFactory )
        {
            Arg.NotNull( editLinkFactory, nameof( editLinkFactory ) );
            Contract.Ensures( Contract.Result<MediaTypeConfiguration<TEntityType>>() != null );

            currentEditLinkFactory = editLinkFactory;
            access |= Write;
            return this;
        }

        /// <summary>
        /// Registers a factory that generates entity tags for the media resource.
        /// </summary>
        /// <param name="etagGenerator">The factory <see cref="Func{T, TResult}">method</see> that generates entity tags for the media resource.</param>
        /// <returns>The original <see cref="MediaTypeConfiguration{TEntityType}">media type configuration</see>.</returns>
        public MediaTypeConfiguration<TEntityType> HasETag( Func<EntityInstanceContext, string> etagGenerator )
        {
            Arg.NotNull( etagGenerator, nameof( etagGenerator ) );
            Contract.Ensures( Contract.Result<MediaTypeConfiguration<TEntityType>>() != null );

            currentEtagGenerator = etagGenerator;
            return this;
        }

        /// <summary>
        /// Indicates that the media type only supports write operations.
        /// </summary>
        /// <returns>The original <see cref="MediaTypeConfiguration{TEntityType}">media type configuration</see>.</returns>
        /// <remarks>A media type is read-only by default.</remarks>
        public MediaTypeConfiguration<TEntityType> IsWriteOnly()
        {
            Contract.Ensures( Contract.Result<MediaTypeConfiguration<TEntityType>>() != null );
            access = Write;
            return this;
        }

        /// <summary>
        /// Indicates that the media type supports read and write operations.
        /// </summary>
        /// <returns>The original <see cref="MediaTypeConfiguration{TEntityType}">media type configuration</see>.</returns>
        /// <remarks>A media type is read-only by default. This method enables write access without registering an explicit
        /// <see cref="HasEditLink(Func{EntityInstanceContext, Uri})">edit link</see> factory.</remarks>
        public MediaTypeConfiguration<TEntityType> IsReadWrite()
        {
            Contract.Ensures( Contract.Result<MediaTypeConfiguration<TEntityType>>() != null );
            access = Read | Write;
            return this;
        }

        /// <summary>
        /// Applies the configuration to the specified EDM model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to apply the configuration to.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        protected virtual void Apply( IEdmModel model )
        {
            Arg.NotNull( model, nameof( model ) );

            var type = typeof( TEntityType );
            var typeConfig = EntityType.GetModelBuilder().GetTypeConfigurationOrNull( type );
            var entityTypeName = typeConfig?.FullName ?? type.GetQualifiedEdmTypeName();
            var schemaType = model.FindDeclaredType( entityTypeName );

            if ( schemaType == null )
                return;

            if ( ( access & Read ) == Read )
                annotation.GenerateReadLink = currentReadLinkFactory;

            if ( ( access & Write ) == Write )
                annotation.GenerateEditLink = currentEditLinkFactory;

            annotation.GenerateETag = currentEtagGenerator;
            model.SetAnnotationValue( schemaType, annotation );
        }

        [SuppressMessage( "Microsoft.Contracts", "CC1055", Justification = "Enforced in protected implementation of Apply" )]
        void IAnnotationConfiguration.Apply( IEdmModel model ) => Apply( model );
    }
}
