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
        private IEnumerable<ODataComplexValue> GetComplexValues( object value, ODataComplexTypeSerializer complexSerializer, IEdmComplexTypeReference complexType, ODataSerializerContext serializerContext )
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

        private ODataValue GetComplexValue( InstanceAnnotation entryAnnotation, object annotation, IEdmModel model, ODataComplexTypeSerializer complexSerializer, ODataSerializerContext serializerContext )
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

        private void AddAnnotations( ODataEntrySerializationContext context, IEdmModel model, IEdmEntityType entityType, object instance, ODataSerializerContext serializerContext )
        {
            Contract.Requires( context != null );
            Contract.Requires( model != null );
            Contract.Requires( entityType != null );
            Contract.Requires( instance != null );
            Contract.Requires( serializerContext != null );

            // get all entry annotations for the entity
            var entryAnnotations = model.GetAnnotationValue<HashSet<InstanceAnnotation>>( entityType );

            // short-circuit if there are no annotations
            if ( entryAnnotations == null )
                return;

            var entry = context.Entry;
            var complexSerializer = context.ComplexTypeSerializer;

            foreach ( var entryAnnotation in entryAnnotations )
            {
                // get the annotation value for the entity instance
                var annotation = entryAnnotation.GetValue( instance );

                // skip annotation of there is nothing to serialize
                if ( annotation == null )
                    continue;

                // serialize as primitive or complex type
                var value = entryAnnotation.IsComplex ?
                            GetComplexValue( entryAnnotation, annotation, model, complexSerializer, serializerContext ) :
                            GetPrimitiveValue( entryAnnotation, annotation, model );

                // skip annotation which cannot be rendered; this should only occur from incorrectly configured annotations
                if ( value == null )
                    continue;

                // add the instance annotation to the current entry
                var instanceAnnotation = new ODataInstanceAnnotation( entryAnnotation.QualifiedName, value );
                entry.InstanceAnnotations.Add( instanceAnnotation );
            }
        }

        /// <summary>
        /// Applies the serialization feature using the provided context.
        /// </summary>
        /// <param name="context">The <see cref="ODataEntrySerializationContext"/> used to apply feature-specific serialization.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public virtual void Apply( ODataEntrySerializationContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            var entityContext = context.EntityInstanceContext;
            var serializerContext = entityContext.SerializerContext;

            // note: currently no way to build the related links inside a projection (e.g. $select)
            if ( serializerContext.SelectExpandClause != null )
                return;

            var entityType = entityContext.EntityType;
            var instance = entityContext.EntityInstance;

            if ( entityType != null && instance != null )
                AddAnnotations( context, entityContext.EdmModel, entityType, instance, serializerContext );
        }
    }
}
