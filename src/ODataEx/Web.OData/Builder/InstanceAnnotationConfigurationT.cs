namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using More.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;
    using System.Web.OData.Builder;
    using static More.StringExtensions;

    /// <summary>
    /// Represents the configuration for an instance annotation.
    /// </summary>
    /// <typeparam name="TStructuralType">The declaring structural <see cref="Type">type</see>.</typeparam>
    public class InstanceAnnotationConfiguration<TStructuralType> : InstanceAnnotationConfiguration where TStructuralType : class
    {
        private readonly StructuralTypeConfiguration<TStructuralType> configuration;
        private readonly InstanceAnnotation annotation;

        /// <summary>
        /// Instantiates a new instance of the <see cref="InstanceAnnotationConfiguration{TStructuralType}"/> class.
        /// </summary>
        /// <param name="configuration">The associated <see cref="StructuralTypeConfiguration{TStructuralType}">configuration</see>.</param>
        /// <param name="name">The name of the annotation.</param>
        /// <param name="annotation">The <see cref="InstanceAnnotation">annotation</see> to configure.</param>
        public InstanceAnnotationConfiguration( StructuralTypeConfiguration<TStructuralType> configuration, string name, InstanceAnnotation annotation )
            : base( configuration?.ToEdmTypeConfiguration(), name )
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( annotation, nameof( annotation ) );

            this.configuration = configuration;
            this.annotation = annotation;
        }

        /// <summary>
        /// Gets the annotation associated with the configuration.
        /// </summary>
        /// <value>The <see cref="InstanceAnnotation">instance annotation</see> to configure.</value>
        protected InstanceAnnotation Annotation
        {
            get
            {
                Contract.Ensures( annotation != null );
                return annotation;
            }
        }

        /// <summary>
        /// Gets the configured target property.
        /// </summary>
        /// <value>The target <see cref="StructuralPropertyConfiguration">property configuration</see>. The default value is <c>null</c>.</value>
        /// <remarks>This property is always <c>null</c> until the configuration is configured for another property.</remarks>
        protected StructuralPropertyConfiguration TargetProperty
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the structural type configuration that this configuration is declared by.
        /// </summary>
        /// <value>The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> that this configuration is declared by.</value>
        public StructuralTypeConfiguration<TStructuralType> DeclaringType
        {
            get
            {
                Contract.Ensures( configuration != null );
                return configuration;
            }
        }

        /// <summary>
        /// Specifies the target property the annotation is for.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type">type</see>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> representing the target property.</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public void ForProperty<T>( Expression<Func<TStructuralType, T>> propertyExpression ) where T : struct
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            TargetProperty = configuration.Property( propertyExpression );
        }

        /// <summary>
        /// Specifies the target property the annotation is for.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type">type</see>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> representing the target property.</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public void ForProperty<T>( Expression<Func<TStructuralType, T?>> propertyExpression ) where T : struct
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            TargetProperty = configuration.Property( propertyExpression );
        }

        /// <summary>
        /// Specifies the target property the annotation is for.
        /// </summary>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> representing the target property.</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public void ForProperty( Expression<Func<TStructuralType, string>> propertyExpression )
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            TargetProperty = configuration.Property( propertyExpression );
        }

        /// <summary>
        /// Specifies the target property the annotation is for.
        /// </summary>
        /// <typeparam name="T">The enumeration <see cref="Type">type</see>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> representing the target property.</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public void ForEnumProperty<T>( Expression<Func<TStructuralType, T>> propertyExpression ) where T : struct
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            TargetProperty = configuration.EnumProperty( propertyExpression );
        }

        /// <summary>
        /// Specifies the target property the annotation is for.
        /// </summary>
        /// <typeparam name="T">The enumeration <see cref="Type">type</see>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> representing the target property.</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public void ForEnumProperty<T>( Expression<Func<TStructuralType, T?>> propertyExpression ) where T : struct
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            TargetProperty = configuration.EnumProperty( propertyExpression );
        }

        /// <summary>
        /// Specifies the target property the annotation is for.
        /// </summary>
        /// <typeparam name="TComplexType">The complex property <see cref="Type">type</see>.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> representing the target property.</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public void ForComplexProperty<TComplexType>( Expression<Func<TStructuralType, TComplexType>> propertyExpression ) where TComplexType : class
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            TargetProperty = configuration.ComplexProperty( propertyExpression );
        }

        /// <summary>
        /// Specifies the target property the annotation is for.
        /// </summary>
        /// <typeparam name="TElementType">The <see cref="Type">type</see> of element.</typeparam>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> representing the target property.</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public void ForCollectionProperty<TElementType>( Expression<Func<TStructuralType, IEnumerable<TElementType>>> propertyExpression ) where TElementType : class
        {
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            TargetProperty = configuration.ComplexProperty( propertyExpression );
        }

        private void ApplyToStructuralType( IEdmModel model )
        {
            Contract.Requires( model != null );

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

        private void ApplyToStructuralProperty( IEdmModel model, StructuralPropertyConfiguration targetProperty )
        {
            Contract.Requires( model != null );
            Contract.Requires( targetProperty != null );

            var structuralType = model.FindDeclaredType( StructuralTypeName ) as IEdmStructuredType;

            // the specified type is not a structural type (e.g. entity or complex)
            if ( structuralType == null )
                return;

            var property = structuralType.FindProperty( targetProperty.Name );

            // the target property couldn't be found
            if ( property == null )
                return;

            // aggregate all property annotations for a given entity type
            var annotations = model.GetAnnotationValue<HashSet<InstanceAnnotation>>( property ) ?? new HashSet<InstanceAnnotation>( InstanceAnnotationComparer.Instance );
            var qualifiedName = Invariant( $"{Namespace}.{Name}" );

            // update the qualified name as needed and add the annotation/term
            Annotation.QualifiedName = model.IsLowerCamelCaseEnabled() ? qualifiedName.ToCamelCase() : qualifiedName;
            annotations.Add( Annotation );
            model.SetAnnotationValue( property, annotations );
            model.AddTerm( this, Annotation, "Property" );
        }

        /// <summary>
        /// Applies the configuration to the specified EDM model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to apply the configuration to.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        protected override void Apply( IEdmModel model )
        {
            Arg.NotNull( model, nameof( model ) );

            var targetProperty = TargetProperty;

            if ( targetProperty == null )
                ApplyToStructuralType( model );
            else
                ApplyToStructuralProperty( model, targetProperty );
        }
    }
}
