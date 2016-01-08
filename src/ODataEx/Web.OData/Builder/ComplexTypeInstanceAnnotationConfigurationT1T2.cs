namespace More.Web.OData.Builder
{
    using More.OData.Edm;
    using System;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Builder;

    /// <summary>
    /// Represents the configuration for a complex type instance annotation.
    /// </summary>
    /// <typeparam name="TStructuralType">The declaring structural <see cref="Type">type</see>.</typeparam>
    /// <typeparam name="TProperty">The annotation structural <see cref="Type">type</see>.</typeparam>
    public class ComplexTypeInstanceAnnotationConfiguration<TStructuralType, TProperty> : InstanceAnnotationConfiguration<TStructuralType>
        where TStructuralType : class
        where TProperty : class
    {
        private readonly ComplexTypeConfiguration<TProperty> annotationType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexTypeInstanceAnnotationConfiguration{TStructuralType, TProperty}"/> class.
        /// </summary>
        /// <param name="configuration">The associated <see cref="StructuralTypeConfiguration{TStructuralType}">configuration</see>.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <param name="annotationType">The associated annotation <see cref="ComplexTypeConfiguration{TComplexType}">complex type configuration</see>.</param>
        /// <param name="annotation">The <see cref="InstanceAnnotation">annotation</see> to configure.</param>
        public ComplexTypeInstanceAnnotationConfiguration(
            StructuralTypeConfiguration<TStructuralType> configuration,
            string name,
            ComplexTypeConfiguration<TProperty> annotationType,
            InstanceAnnotation annotation )
            : base( configuration, name, annotation )
        {
            Arg.NotNull( annotationType, nameof( annotationType ) );
            this.annotationType = annotationType;
        }

        /// <summary>
        /// Gets the type configuration for the associated annotation.
        /// </summary>
        /// <value>The annotation <see cref="ComplexTypeConfiguration{TComplexType}">complex type configuration</see>.</value>
        public ComplexTypeConfiguration<TProperty> AnnotationType
        {
            get
            {
                Contract.Ensures( annotationType != null );
                return annotationType;
            }
        }
    }
}
