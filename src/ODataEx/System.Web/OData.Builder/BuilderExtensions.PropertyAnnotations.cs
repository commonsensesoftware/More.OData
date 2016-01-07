namespace System.Web.OData.Builder
{
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Linq.Expressions;
    using More;
    using More.OData.Edm;
    using More.Web.OData.Builder;
    using System;
    using static More.StringExtensions;

    /// <content>
    /// Provides property instance annotation extension methods.
    /// </content>
    public static partial class BuilderExtensions
    {
        /// <summary>
        /// Configures the property of a structural as an annotation for another property.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="TProperty">The <see cref="Type">type</see> of value being annotated.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TEntityType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="ComplexTypeConfiguration{TComplexType}">complex type configuration</see> for the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static PropertyInstanceAnnotationConfiguration<TStructuralType> HasPropertyAnnotation<TStructuralType, TProperty>(
            this StructuralTypeConfiguration<TStructuralType> configuration,
            Expression<Func<TStructuralType, TProperty>> propertyExpression )
            where TStructuralType : class
            where TProperty : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<PropertyInstanceAnnotationConfiguration<TStructuralType>>() != null );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // build an annotation for the property
            var name = ( (MemberExpression) propertyExpression.Body ).Member.Name;
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var builder = configuration.GetModelBuilder();
            var qualifiedName = Invariant( $"{configuration.Namespace}.{name}" );
            var annotationTypeConfig = builder.ComplexType<TProperty>();
            var annotation = new InstanceAnnotation( accessor, qualifiedName, annotationTypeConfig.ToEdmTypeConfiguration() )
            {
                IsComplex = true,
                IsNullable = true
            };
            var annotationConfig = new PropertyInstanceAnnotationConfiguration<TStructuralType>( configuration, name, annotation );

            builder.GetAnnotationConfigurations().Add( annotationConfig );

            return annotationConfig;
        }
    }
}
