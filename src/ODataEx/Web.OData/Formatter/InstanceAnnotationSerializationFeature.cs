using System.Diagnostics.Contracts;
namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Library;
    using More.OData.Edm;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Formatter.Serialization;
    using static More.StringExtensions;

    /// <summary>
    /// Represents a serialization feature to generate instance annotations for OData entity entries.
    /// </summary>
    public class InstanceAnnotationSerializationFeature : IODataSerializationFeature
    {
        private static IEnumerable<ODataComplexValue> GetComplexValues( object value, ODataComplexTypeSerializer complexSerializer, IEdmComplexTypeReference complexType, ODataSerializerContext serializerContext )
        {
            Contract.Requires( complexSerializer != null );
            Contract.Requires( complexType != null );
            Contract.Requires( serializerContext != null );

            var items = value as System.Collections.IEnumerable;

            if ( items == null )
                yield break;

            foreach ( var graph in items )
                yield return complexSerializer.CreateODataComplexValue( graph, complexType, serializerContext );
        }

        private static ODataValue GetComplexValue( InstanceAnnotation entryAnnotation, object annotation, IEdmModel model, ODataComplexTypeSerializer complexSerializer, ODataSerializerContext serializerContext )
        {
            Contract.Requires( entryAnnotation != null );
            Contract.Requires( annotation != null );
            Contract.Requires( model != null );
            Contract.Requires( complexSerializer != null );
            Contract.Requires( serializerContext != null );

            // look up the complex type for the annotation
            var complexType = (IEdmComplexType) model.FindDeclaredType( entryAnnotation.AnnotationTypeName );

            // skip incorrectly configured annotations
            if ( complexType == null )
                return null;

            var typeRef = new EdmComplexTypeReference( complexType, entryAnnotation.IsNullable );

            // serialize the annotation as a complex type
            if ( !entryAnnotation.IsCollection )
                return complexSerializer.CreateODataComplexValue( annotation, typeRef, serializerContext );

            var typeName = Invariant( $"Collection({typeRef.FullName()})" );
            var items = GetComplexValues( annotation, complexSerializer, typeRef, serializerContext );

            // create and serialize the annotations as a collection of complex types
            return new ODataCollectionValue() { Items = items, TypeName = typeName };
        }

        private static ODataValue GetPrimitiveValue( InstanceAnnotation entryAnnotation, object annotation, IEdmModel model )
        {
            // just wrap the primitive if this is not a collecton
            if ( !entryAnnotation.IsCollection )
                return new ODataPrimitiveValue( annotation );

            // primitives are not wrapped as a collection of ODataPrimitiveValue; simply build
            //  he qualified collection name and get the value as a sequence of items
            var type = model.FindType( entryAnnotation.AnnotationTypeName );
            var typeName = Invariant( $"Collection({type.FullName()})" );
            var items = (System.Collections.IEnumerable) annotation;

            return new ODataCollectionValue() { Items = items, TypeName = typeName };
        }

        private static void AddAnnotations( ODataSerializationFeatureContext context, IEnumerable<InstanceAnnotation> modelAnnotations, ICollection<ODataInstanceAnnotation> instanceAnnotations )
        {
            Contract.Requires( context != null );
            Contract.Requires( modelAnnotations != null );
            Contract.Requires( instanceAnnotations != null );

            var instance = context.Instance;
            var model = context.SerializerContext.Model;
            var complexTypeSerializer = context.ComplexTypeSerializer;
            var serializerContext = context.SerializerContext;

            foreach ( var modelAnnotation in modelAnnotations )
            {
                // get the annotation value for the entity instance
                var annotation = modelAnnotation.GetValue( instance );

                // skip annotation of there is nothing to serialize
                if ( annotation == null )
                    continue;

                // serialize as primitive or complex type
                var value = modelAnnotation.IsComplex ?
                            GetComplexValue( modelAnnotation, annotation, model, complexTypeSerializer, serializerContext ) :
                            GetPrimitiveValue( modelAnnotation, annotation, model );

                // skip annotation which cannot be rendered; this should only occur from incorrectly configured annotations
                if ( value == null )
                    continue;

                // add the instance annotation to the current entry
                var instanceAnnotation = new ODataInstanceAnnotation( modelAnnotation.QualifiedName, value );
                instanceAnnotations.Add( instanceAnnotation );
            }
        }

        private static IEdmStructuredType GetStructuredType( IEdmElement element )
        {
            Contract.Requires( element != null );
            Contract.Ensures( Contract.Result<IEdmStructuredType>() != null );

            var type = element as IEdmStructuredType;

            if ( type == null )
                return (IEdmStructuredType) ( (IEdmStructuredTypeReference) element ).Definition;

            return type;
        }

        /// <summary>
        /// Applies the serialization feature to the specified OData feed using the provided context.
        /// </summary>
        /// <param name="feed">The <see cref="ODataFeed"/> to apply the serialization feature to.</param>
        /// <param name="context">The <see cref="ODataSerializationFeatureContext"/> used to apply feature-specific serialization.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public virtual void Apply( ODataFeed feed, ODataSerializationFeatureContext context )
        {
            Arg.NotNull( feed, nameof( feed ) );
            Arg.NotNull( context, nameof( context ) );

            var model = context.SerializerContext.Model;
            var annotations = model.GetAnnotationValue<HashSet<InstanceAnnotation>>( context.EdmElement );

            if ( annotations != null )
                AddAnnotations( context, annotations, feed.InstanceAnnotations );
        }

        /// <summary>
        /// Applies the serialization feature to the specified OData entry using the provided context.
        /// </summary>
        /// <param name="entry">The <see cref="ODataEntry"/> to apply the serialization feature to.</param>
        /// <param name="context">The <see cref="ODataSerializationFeatureContext"/> used to apply feature-specific serialization.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public virtual void Apply( ODataEntry entry, ODataSerializationFeatureContext context )
        {
            Arg.NotNull( entry, nameof( entry ) );
            Arg.NotNull( context, nameof( context ) );

            // note: currently no way to add annotations inside a projection (e.g. $select)
            if ( context.Instance == null )
                return;

            var model = context.SerializerContext.Model;
            var entityType = GetStructuredType( context.EdmElement );
            var entryAnnotations = model.GetAnnotationValue<HashSet<InstanceAnnotation>>( entityType );

            // add entity instance annotations
            if ( entryAnnotations != null )
                AddAnnotations( context, entryAnnotations, entry.InstanceAnnotations );

            // add property instance annotations
            foreach ( var property in entry.Properties )
            {
                var propertyType = entityType.FindProperty( property.Name );
                var propertyAnnotations = model.GetAnnotationValue<HashSet<InstanceAnnotation>>( propertyType );

                if ( propertyAnnotations != null )
                    AddAnnotations( context, propertyAnnotations, property.InstanceAnnotations );
            }
        }

        /// <summary>
        /// Applies the serialization feature to the specified OData complex value using the provided context.
        /// </summary>
        /// <param name="complexValue">The <see cref="ODataComplexValue"/> to apply the serialization feature to.</param>
        /// <param name="context">The <see cref="ODataSerializationFeatureContext"/> used to apply feature-specific serialization.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public virtual void Apply( ODataComplexValue complexValue, ODataSerializationFeatureContext context )
        {
            Arg.NotNull( complexValue, nameof( complexValue ) );
            Arg.NotNull( context, nameof( context ) );

            var model = context.SerializerContext.Model;
            var complexType = GetStructuredType( context.EdmElement );
            var complexTypeAnnotations = model.GetAnnotationValue<HashSet<InstanceAnnotation>>( complexType );

            // add complex type instance annotations
            if ( complexTypeAnnotations != null )
                AddAnnotations( context, complexTypeAnnotations, complexValue.InstanceAnnotations );

            // add property instance annotations
            foreach ( var property in complexValue.Properties )
            {
                var propertyType = complexType.FindProperty( property.Name );
                var propertyAnnotations = model.GetAnnotationValue<HashSet<InstanceAnnotation>>( propertyType );

                if ( propertyAnnotations != null )
                    AddAnnotations( context, propertyAnnotations, property.InstanceAnnotations );
            }
        }
    }
}
