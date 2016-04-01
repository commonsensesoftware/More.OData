namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using More.OData.Edm;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Builder;
    using static More.StringExtensions;

    /// <summary>
    /// Represents the instance annotation configuration for an entity set.
    /// </summary>
    public class EntitySetInstanceAnnotationConfiguration : InstanceAnnotationConfiguration
    {
        private readonly string entitySetName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetInstanceAnnotationConfiguration"/> class.
        /// </summary>
        /// <param name="entitySetName">The name of the associated entity set.</param>
        /// <param name="typeConfiguration">The associated <see cref="IEdmTypeConfiguration">type configuration</see>.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <param name="annotation">The <see cref="InstanceAnnotation">annotation</see> to configure.</param>
        public EntitySetInstanceAnnotationConfiguration( string entitySetName, IEdmTypeConfiguration typeConfiguration, string name, InstanceAnnotation annotation )
            : base( typeConfiguration, name, annotation )
        {
            Arg.NotNullOrEmpty( entitySetName, nameof( entitySetName ) );
            this.entitySetName = entitySetName;
        }

        /// <summary>
        /// Gets the name of the associated entity set.
        /// </summary>
        /// <value>The name of the associated entity set.</value>
        public string EntitySetName
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( entitySetName ) );
                return entitySetName;
            }
        }

        /// <summary>
        /// Applies the configuration to the specified EDM model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to apply the configuration to.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        protected override void Apply( IEdmModel model )
        {
            Arg.NotNull( model, nameof( model ) );

            var entitySet = model.EntityContainer.FindEntitySet( EntitySetName );

            if ( entitySet == null )
                return;

            // update casing just before the annotation is applied
            if ( model.IsLowerCamelCaseEnabled() )
            {
                Name = Name.ToCamelCase();
                Namespace = Namespace.ToCamelCase();
            }

            // aggregate all annotations for a given entity set
            var annotations = model.GetAnnotationValue<HashSet<InstanceAnnotation>>( entitySet ) ?? new HashSet<InstanceAnnotation>( InstanceAnnotationComparer.Instance );

            // update the qualified name as needed and add the annotation/term
            Annotation.QualifiedName = Invariant( $"{Namespace}.{Name}" );
            annotations.Add( Annotation );
            model.SetAnnotationValue( entitySet, annotations );
            model.AddTerm( this, Annotation, "EntitySet" );
        }
    }
}
