namespace More.Web.OData.Builder
{
    using More.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Builder;

    /// <summary>
    /// Represents the configuration for a complex type instance annotation for an entity set.
    /// </summary>
    /// <typeparam name="TStructuralType">The annotation structural <see cref="Type">type</see>.</typeparam>
    public class EntitySetComplexTypeInstanceAnnotationConfiguration<TStructuralType> : EntitySetInstanceAnnotationConfiguration where TStructuralType : class
    {
        private readonly ComplexTypeConfiguration<TStructuralType> annotationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetComplexTypeInstanceAnnotationConfiguration{TStructuralType}"/> class.
        /// </summary>
        /// <param name="entitySetName">The name of the associated entity set.</param>
        /// <param name="typeConfiguration">The associated <see cref="IEdmTypeConfiguration">type configuration</see>.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <param name="annotationType">The associated annotation <see cref="ComplexTypeConfiguration{TComplexType}">complex type configuration</see>.</param>
        /// <param name="annotation">The <see cref="InstanceAnnotation">annotation</see> to configure.</param>
        public EntitySetComplexTypeInstanceAnnotationConfiguration(
            string entitySetName,
            IEdmTypeConfiguration typeConfiguration,
            string name,
            ComplexTypeConfiguration<TStructuralType> annotationType,
            InstanceAnnotation annotation )
            : base( entitySetName, typeConfiguration, name, annotation )
        {
            Arg.NotNull( annotationType, nameof( annotationType ) );
            this.annotationType = annotationType;
        }

        /// <summary>
        /// Gets the type configuration for the associated annotation.
        /// </summary>
        /// <value>The annotation <see cref="ComplexTypeConfiguration{TComplexType}">complex type configuration</see>.</value>
        public ComplexTypeConfiguration<TStructuralType> AnnotationType
        {
            get
            {
                Contract.Ensures( annotationType != null );
                return annotationType;
            }
        }
    }
}
