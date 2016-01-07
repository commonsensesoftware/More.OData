namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using More.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.OData.Builder;
    using static More.StringExtensions;

    /// <summary>
    /// Represents the configuration for a primitive instance annotation.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type">type</see> of primitive annotation.</typeparam>
    public class PrimitiveInstanceAnnotationConfiguration<T> : InstanceAnnotationConfiguration
    {
        private readonly InstanceAnnotation annotation;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveInstanceAnnotationConfiguration{T}"/> class.
        /// </summary>
        /// <param name="typeConfiguration">The associated <see cref="IEdmTypeConfiguration">type configuration</see>.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <param name="annotation">The <see cref="InstanceAnnotation">annotation</see> to configure.</param>
        public PrimitiveInstanceAnnotationConfiguration( IEdmTypeConfiguration typeConfiguration, string name, InstanceAnnotation annotation )
            : base( typeConfiguration, name )
        {
            Arg.NotNull( annotation, nameof( annotation ) );
            this.annotation = annotation;
        }

        /// <summary>
        /// Applies annotation configurations to the specified EDM model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to apply the configuration to.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        protected override void Apply( IEdmModel model )
        {
            Arg.NotNull( model, nameof( model ) );

            var entityType = model.FindDeclaredType( StructuralTypeName );

            if ( entityType == null )
                return;

            // aggregate all property annotations for a given entity type
            var annotations = model.GetAnnotationValue<HashSet<InstanceAnnotation>>( entityType ) ?? new HashSet<InstanceAnnotation>( InstanceAnnotationComparer.Instance );
            var qualifiedName = Invariant( $"{Namespace}.{Name}" );

            // update the qualified name as needed and add the annotation/term
            annotation.QualifiedName = model.IsLowerCamelCaseEnabled() ? qualifiedName.ToCamelCase() : qualifiedName;
            annotations.Add( annotation );
            model.SetAnnotationValue( entityType, annotations );
            model.AddTerm( this, annotation, "EntityType ComplexType" );
        }
    }
}
