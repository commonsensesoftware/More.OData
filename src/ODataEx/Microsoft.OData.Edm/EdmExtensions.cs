namespace Microsoft.OData.Edm
{
    using Library;
    using More.OData.Edm;
    using More.Web.OData.Builder;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.OData;
    using System.Web.OData.Builder;
    using static More.StringExtensions;

    /// <summary>
    /// Provides Entity Data Model (EDM) extension methods.
    /// </summary>
    public static class EdmExtensions
    {
        /// <summary>
        /// Gets a value indicating whether lower camel casing is enabled for the model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">model</see> to evaluate.</param>
        /// <returns>True if lower camel casing is enabled; otherwise, false.</returns>
        public static bool IsLowerCamelCaseEnabled( this IEdmModel model )
        {
            Arg.NotNull( model, nameof( model ) );
            return model.GetAnnotationValue<LowerCamelCaseAnnotation>( model )?.IsEnabled ?? false;
        }

        /// <summary>
        /// Clones the specified operation using the qualified name of the action as the name.
        /// </summary>
        /// <param name="action">The <see cref="IEdmAction">action</see> to clone.</param>
        /// <returns>A new <see cref="IEdmAction"/> that is a clone of the original <paramref name="action"/> where the
        /// <see cref="IEdmNamedElement.Name"/> is the qualified name of the action.</returns>
        /// <remarks>This extension method is a workaround solution for a bug in the
        /// <see cref="LinkGenerationHelpers.GenerateActionLink(EntityInstanceContext, IEdmOperation)"/>
        /// which does not use the qualified name of the action.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static IEdmAction CloneWithQualifiedName( this IEdmAction action )
        {
            Arg.NotNull( action, nameof( action ) );
            Contract.Ensures( Contract.Result<IEdmAction>() != null );

            var qualifiedName = Invariant( $"{action.Namespace}.{action.Name}" );
            var clone = new EdmAction( action.Namespace, qualifiedName, action.ReturnType, action.IsBound, action.EntitySetPath );

            foreach ( var parameter in action.Parameters )
                clone.AddParameter( parameter );

            return clone;
        }

        internal static void AddTerm( this IEdmModel model, IAnnotationConfiguration configuration, InstanceAnnotation annotation, string appliesTo )
        {
            Contract.Requires( model != null );
            Contract.Requires( configuration != null );
            Contract.Requires( annotation != null );
            Contract.Requires( !string.IsNullOrEmpty( appliesTo ) );

            // short-circuit if the term has already been added
            if ( model.FindValueTerm( annotation.QualifiedName ) != null )
                return;

            var typeRef = annotation.IsComplex ? AddComplexTerm( model, annotation ) : AddPrimitiveTerm( model, annotation );
            var termType = annotation.IsCollection ? new EdmCollectionTypeReference( new EdmCollectionType( typeRef ) ) : typeRef;

            AddTerm( model, configuration, termType, appliesTo );
        }

        private static void AddTerm( IEdmModel model, IAnnotationConfiguration configuration, IEdmTypeReference termType, string appliesTo )
        {
            Contract.Requires( model != null );
            Contract.Requires( configuration != null );
            Contract.Requires( termType != null );
            Contract.Requires( !string.IsNullOrEmpty( appliesTo ) );

            // TODO: refactor this, when possible, to not use/rely on the following cast
            //
            // casting to EdmModel is not safe, but there is seemingly no other way to add
            // the term for the annotation to the model. this would be true even if we
            // subclass ODataModelBuilder or ODataConventionModelBuilder since GetEmdModel
            // returns IEdmModel. Completely reimplementing the internals of ODataModelBuilder
            // is not worth it at this point.
            var edmModel = (EdmModel) model;
            var term = new EdmTerm( configuration.Namespace, configuration.Name, termType, appliesTo );

            edmModel.AddElement( term );
        }

        private static IEdmTypeReference AddComplexTerm( IEdmModel model, InstanceAnnotation annotation )
        {
            Contract.Requires( model != null );
            Contract.Requires( annotation != null );
            Contract.Ensures( Contract.Result<IEdmTypeReference>() != null );

            var complexType = (IEdmComplexType) model.FindDeclaredType( annotation.AnnotationTypeName );
            var complexTypeRef = new EdmComplexTypeReference( complexType, annotation.IsNullable );
            return complexTypeRef;
        }

        private static IEdmTypeReference AddPrimitiveTerm( IEdmModel model, InstanceAnnotation annotation )
        {
            Contract.Requires( model != null );
            Contract.Requires( annotation != null );
            Contract.Ensures( Contract.Result<IEdmTypeReference>() != null );

            var primitiveType = (IEdmPrimitiveType) model.FindType( annotation.AnnotationTypeName );
            var primitiveTypeRef = new EdmPrimitiveTypeReference( primitiveType, annotation.IsNullable );
            return primitiveTypeRef;
        }
    }
}
