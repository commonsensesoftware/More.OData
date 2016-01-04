namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Library;
    using More.OData.Edm;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents the annotation configuration for a property associated with an entity entry.
    /// </summary>
    public class EntityInstanceAnnotationConfiguration : EntityAnnotationConfiguration
    {
        private const string AppliesTo = "EntitySet";
        private readonly EntityInstanceAnnotation annotation;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInstanceAnnotationConfiguration"/> class.
        /// </summary>
        /// <param name="entityTypeName">The name of the entity type to configure.</param>
        /// <param name="annotation">The <see cref="EntityInstanceAnnotation">annotation</see> to configure.</param>
        public EntityInstanceAnnotationConfiguration( string entityTypeName, EntityInstanceAnnotation annotation )
            : base( entityTypeName )
        {
            Arg.NotNull( annotation, nameof( annotation ) );
            this.annotation = annotation;
        }

        /// <summary>
        /// Gets the associated annotation to configure.
        /// </summary>
        /// <value>An <see cref="EntityInstanceAnnotation">entity property annotation</see>.</value>
        public EntityInstanceAnnotation Annotation
        {
            get
            {
                Contract.Ensures( annotation != null );
                return annotation;
            }
        }

        /// <summary>
        /// Applies annotation configurations to the specified EDM model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to apply the configuration to.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "String interpolation is invariant in this context." )]
        public override void Apply( IEdmModel model )
        {
            Arg.NotNull( model, nameof( model ) );

            var entityType = model.FindDeclaredType( EntityTypeName );

            if ( entityType == null )
                return;

            // aggregate all property annotations for a given entity type
            var annotations = model.GetAnnotationValue<List<EntityInstanceAnnotation>>( entityType ) ?? new List<EntityInstanceAnnotation>();

            annotations.Add( annotation );
            model.SetAnnotationValue( entityType, annotations );

            var qualifiedName = $"{entityType.Namespace}.{Annotation.Name}";

            // short-circuit if the term has already been added
            if ( model.FindValueTerm( qualifiedName ) != null )
                return;

            // TODO: refactor this, when possible, to not use/rely on the following cast
            //
            // casting to EdmModel is not safe, but there is seemingly no other way to add
            // the term for the annotation to the model. this would be true even if we
            // subclass ODataModelBuilder or ODataConventionModelBuilder since GetEmdModel
            // returns IEdmModel. Completely reimplementing the internals of ODataModelBuilder
            // is not worth it at this point.
            var edmModel = (EdmModel) model;

            if ( annotation.IsComplex )
                AddComplexTerm( edmModel, entityType, annotation );
            else
                AddPrimitiveTerm( edmModel, entityType, annotation );
        }

        private static void AddTerm( EdmModel model, IEdmSchemaType entityType, EntityInstanceAnnotation annotation, IEdmTypeReference annotationType )
        {
            Contract.Requires( model != null );
            Contract.Requires( entityType != null );
            Contract.Requires( annotation != null );
            Contract.Requires( annotationType != null );

            EdmTerm term;

            if ( annotation.IsCollection )
            {
                var collectionType = new EdmCollectionType( annotationType );
                var collectionRef = new EdmCollectionTypeReference( collectionType );
                term = new EdmTerm( entityType.Namespace, annotation.Name, collectionRef, AppliesTo );
            }
            else
            {
                term = new EdmTerm( entityType.Namespace, annotation.Name, annotationType, AppliesTo );
            }

            model.AddElement( term );
        }

        private static void AddComplexTerm( EdmModel model, IEdmSchemaType entityType, EntityInstanceAnnotation annotation )
        {
            Contract.Requires( model != null );
            Contract.Requires( entityType != null );
            Contract.Requires( annotation != null );

            var complexType = (IEdmComplexType) model.FindDeclaredType( annotation.AnnotationTypeName );
            var complexTypeRef = new EdmComplexTypeReference( complexType, annotation.IsNullable );
            AddTerm( model, entityType, annotation, complexTypeRef );
        }

        private static void AddPrimitiveTerm( EdmModel model, IEdmSchemaType entityType, EntityInstanceAnnotation annotation )
        {
            Contract.Requires( model != null );
            Contract.Requires( entityType != null );
            Contract.Requires( annotation != null );

            var primitiveType = (IEdmPrimitiveType) model.FindType( annotation.AnnotationTypeName );
            var primitiveTypeRef = new EdmPrimitiveTypeReference( primitiveType, annotation.IsNullable );
            AddTerm( model, entityType, annotation, primitiveTypeRef );
        }
    }
}
