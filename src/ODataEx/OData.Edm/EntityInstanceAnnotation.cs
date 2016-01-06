namespace More.OData.Edm
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using static More.StringExtensions;

    /// <summary>
    /// Represents an instance annotation for a property associated with an entity entry.
    /// </summary>
    public class EntityInstanceAnnotation
    {
        private readonly Lazy<Func<object, object>> accessor;
        private string @namespace;
        private string name;
        private string qualifiedName;
        private string annotationTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInstanceAnnotation"/> class.
        /// </summary>
        /// <param name="accessor">The <see cref="Func{T, TResult}">method</see> used to retrieve the
        /// annotation value from an entity instance.</param>
        /// <param name="namespace">The namespace of the annotation.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <param name="annotationTypeName">The name of the annotation type.</param>
        public EntityInstanceAnnotation( Func<object, object> accessor, string @namespace, string name, string annotationTypeName )
        {
            Arg.NotNull( accessor, nameof( accessor ) );
            Arg.NotNullOrEmpty( @namespace, nameof( @namespace ) );
            Arg.NotNullOrEmpty( name, nameof( name ) );
            Arg.NotNullOrEmpty( annotationTypeName, nameof( annotationTypeName ) );

            this.accessor = new Lazy<Func<object, object>>( () => accessor );
            this.@namespace = @namespace;
            this.name = name;
            this.annotationTypeName = annotationTypeName;
            qualifiedName = Invariant( $"{@namespace}.{name}" );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInstanceAnnotation"/> class.
        /// </summary>
        /// <param name="accessor">The <see cref="Lazy{T}">lazy-initialized</see> <see cref="Func{T, TResult}">method</see> used
        /// to retrieve the annotation value from an entity instance.</param>
        /// <param name="namespace">The namespace of the annotation.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <param name="annotationTypeName">The name of the annotation type.</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generic, lazy func." )]
        public EntityInstanceAnnotation( Lazy<Func<object, object>> accessor, string @namespace, string name, string annotationTypeName )
        {
            Arg.NotNull( accessor, nameof( accessor ) );
            Arg.NotNullOrEmpty( @namespace, nameof( @namespace ) );
            Arg.NotNullOrEmpty( name, nameof( name ) );
            Arg.NotNullOrEmpty( annotationTypeName, nameof( annotationTypeName ) );

            this.accessor = accessor;
            this.@namespace = @namespace;
            this.name = name;
            this.annotationTypeName = annotationTypeName;
            qualifiedName = Invariant( $"{@namespace}.{name}" );
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
        /// Gets or sets the namespace of the annotation.
        /// </summary>
        /// <value>The annotation namespace.</value>
        public string Namespace
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( @namespace ) );
                return @namespace;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );

                if ( @namespace == value )
                    return;

                @namespace = value;
                qualifiedName = Invariant( $"{@namespace}.{name}" );
            }
        }

        /// <summary>
        /// Gets or sets the name of the annotation.
        /// </summary>
        /// <value>The annotation name.</value>
        public string Name
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( name ) );
                return name;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );

                if ( name == value )
                    return;

                name = value;
                qualifiedName = Invariant( $"{@namespace}.{name}" );
            }
        }

        /// <summary>
        /// Gets the qualified name of the annotation.
        /// </summary>
        /// <value>The qualified name of the annotation.</value>
        public string QualifiedName
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( qualifiedName ) );
                return qualifiedName;
            }
        }

        /// <summary>
        /// Gets or sets the name of the annotation type.
        /// </summary>
        /// <value>The qualified name of the annotation type.</value>
        public string AnnotationTypeName
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( annotationTypeName ) );
                return annotationTypeName;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                annotationTypeName = value;
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
