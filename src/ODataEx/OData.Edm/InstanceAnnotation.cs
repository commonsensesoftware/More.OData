namespace More.OData.Edm
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Builder;

    /// <summary>
    /// Represents an instance annotation for a property associated with a structural type.
    /// </summary>
    public class InstanceAnnotation
    {
        private readonly Lazy<Func<object, object>> accessor;
        private readonly IEdmTypeConfiguration annotationType;
        private string qualifiedName;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceAnnotation"/> class.
        /// </summary>
        /// <param name="accessor">The <see cref="Func{T, TResult}">method</see> used to retrieve the
        /// annotation value from an entity instance.</param>
        /// <param name="qualifiedName">The namespace qualified name of the annotation.</param>
        /// <param name="annotationType">The annotation <see cref="IEdmTypeConfiguration">type configuration</see>.</param>
        public InstanceAnnotation( Func<object, object> accessor, string qualifiedName, IEdmTypeConfiguration annotationType )
        {
            Arg.NotNull( accessor, nameof( accessor ) );
            Arg.NotNull( annotationType, nameof( annotationType ) );

            this.accessor = new Lazy<Func<object, object>>( () => accessor );
            this.qualifiedName = qualifiedName;
            this.annotationType = annotationType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceAnnotation"/> class.
        /// </summary>
        /// <param name="accessor">The <see cref="Lazy{T}">lazy-initialized</see> <see cref="Func{T, TResult}">method</see> used
        /// to retrieve the annotation value from an entity instance.</param>
        /// <param name="qualifiedName">The namespace qualified name of the annotation.</param>
        /// <param name="annotationType">The annotation <see cref="IEdmTypeConfiguration">type configuration</see>.</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generic, lazy func." )]
        public InstanceAnnotation( Lazy<Func<object, object>> accessor, string qualifiedName, IEdmTypeConfiguration annotationType )
        {
            Arg.NotNull( accessor, nameof( accessor ) );
            Arg.NotNull( annotationType, nameof( annotationType ) );

            this.accessor = accessor;
            this.qualifiedName = qualifiedName;
            this.annotationType = annotationType;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the annotation is a complex type.
        /// </summary>
        /// <value>True if the annotation is a complex type; otherwise, false. The default value is <c>false</c>.</value>
        /// <remarks>An annotation is complex if it is not one of the intrinsic primitive types. Complex annotation types
        /// are typically a custom class or structure.</remarks>
        public bool IsComplex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the annotation is a collection of annotation.
        /// </summary>
        /// <value>True if the annotation is a collection of annotations; otherwise, false. The default value is <c>false</c>.</value>
        public bool IsCollection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the annotation can be null.
        /// </summary>
        /// <value>True if the annotation can be <c>null</c>; otherwise, false. The default value is <c>false</c>.</value>
        /// <remarks>This property returns true for reference types or <see cref="Nullable{T}">nullable</see> value types
        /// and returns false for value types.</remarks>
        public bool IsNullable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the qualified name of the annotation.
        /// </summary>
        /// <value>The qualified name of the annotation.</value>
        public string QualifiedName
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( qualifiedName ) );
                return qualifiedName;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                qualifiedName = value;
            }
        }

        /// <summary>
        /// Gets the name of the annotation type.
        /// </summary>
        /// <value>The qualified name of the annotation type.</value>
        public string AnnotationTypeName
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );
                return annotationType.FullName;
            }
        }

        /// <summary>
        /// Gets the annotated value from an instance of the associated entity type.
        /// </summary>
        /// <param name="instance">The instance of the entity to get the annotation value from.</param>
        /// <returns>The configured annotation value.</returns>
        /// <exception cref="InvalidCastException">The specified <paramref name="instance"/> is not the configured entity type.</exception>
        public object GetValue( object instance )
        {
            Arg.NotNull( instance, nameof( instance ) );
            return accessor.Value( instance );
        }
    }
}
