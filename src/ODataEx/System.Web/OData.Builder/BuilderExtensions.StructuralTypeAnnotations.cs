namespace System.Web.OData.Builder
{
    using Collections.Generic;
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Linq.Expressions;
    using More;
    using More.OData.Edm;
    using More.Web.OData.Builder;
    using System;
    using static Globalization.CultureInfo;
    using static More.ExceptionMessage;
    using static More.StringExtensions;

    /// <content>
    /// Provides structural type instance annotation extension methods.
    /// </content>
    public static partial class BuilderExtensions
    {
        private static PrimitiveInstanceAnnotationConfiguration<TProperty> HasPrimitiveAnnotation<TStructuralType, TProperty>(
            this StructuralTypeConfiguration<TStructuralType> configuration,
            Expression<Func<TStructuralType, TProperty>> propertyExpression )
            where TStructuralType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<PrimitiveInstanceAnnotationConfiguration<TProperty>>() != null );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // build an annotation for the property
            var builder = configuration.GetModelBuilder();
            var name = ( (MemberExpression) propertyExpression.Body ).Member.Name;
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var qualifiedName = Invariant( $"{configuration.Namespace}.{name}" );
            var annotationType = typeof( TProperty );
            var annotationTypeConfig = builder.GetTypeConfigurationOrNull( annotationType );
            var annotation = new InstanceAnnotation( accessor, qualifiedName, annotationTypeConfig )
            {
                IsNullable = annotationType.IsNullable()
            };
            var annotationConfig = new PrimitiveInstanceAnnotationConfiguration<TProperty>( configuration.ToEdmTypeConfiguration(), name, annotation );

            builder.GetAnnotationConfigurations().Add( annotationConfig );

            return annotationConfig;
        }

        private static PrimitiveInstanceAnnotationConfiguration<TProperty> HasPrimitiveAnnotations<TStructuralType, TProperty>(
            this StructuralTypeConfiguration<TStructuralType> configuration,
            Expression<Func<TStructuralType, IEnumerable<TProperty>>> propertyExpression )
            where TStructuralType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<PrimitiveInstanceAnnotationConfiguration<TProperty>>() != null );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // build an annotation for the entity property
            var builder = configuration.GetModelBuilder();
            var name = ( (MemberExpression) propertyExpression.Body ).Member.Name;
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var qualifiedName = Invariant( $"{configuration.Namespace}.{name}" );
            var annotationType = typeof( TProperty );
            var annotationTypeConfig = builder.GetTypeConfigurationOrNull( annotationType );
            var annotation = new InstanceAnnotation( accessor, qualifiedName, annotationTypeConfig )
            {
                IsCollection = true,
                IsNullable = annotationType.IsNullable()
            };
            var annotationConfig = new PrimitiveInstanceAnnotationConfiguration<TProperty>( configuration.ToEdmTypeConfiguration(), name, annotation );

            builder.GetAnnotationConfigurations().Add( annotationConfig );

            return annotationConfig;
        }

        /// <summary>
        /// Configures the property of a structural type as an annotation.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="T">The annotation <see cref="Type">type</see>.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>The original <see cref="StructuralTypeConfiguration{TStructuralType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static PrimitiveInstanceAnnotationConfiguration<T> HasAnnotation<TStructuralType, T>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, T>> propertyExpression )
            where TStructuralType : class
            where T : struct => configuration.HasPrimitiveAnnotation( propertyExpression );

        /// <summary>
        /// Configures the property of a structural type as a collection of annotations.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="T">The annotation <see cref="Type">type</see>.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>The original <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static PrimitiveInstanceAnnotationConfiguration<T> HasAnnotations<TStructuralType, T>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, IEnumerable<T>>> propertyExpression )
            where TStructuralType : class
            where T : struct => configuration.HasPrimitiveAnnotations( propertyExpression );


        /// <summary>
        /// Configures the property of a structural type as an annotation.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="T">The annotation <see cref="Type">type</see>.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>The original <see cref="StructuralTypeConfiguration{TStructuralType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static PrimitiveInstanceAnnotationConfiguration<T?> HasAnnotation<TStructuralType, T>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, T?>> propertyExpression )
            where TStructuralType : class
            where T : struct => configuration.HasPrimitiveAnnotation( propertyExpression );

        /// <summary>
        /// Configures the property of a structural type as an annotation.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>The original <see cref="StructuralTypeConfiguration{TStructuralType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static PrimitiveInstanceAnnotationConfiguration<string> HasAnnotation<TStructuralType>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, string>> propertyExpression )
            where TStructuralType : class => configuration.HasPrimitiveAnnotation( propertyExpression );

        /// <summary>
        /// Configures the property of a structural type as a collection of annotations.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>The original <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static PrimitiveInstanceAnnotationConfiguration<string> HasAnnotations<TStructuralType>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, IEnumerable<string>>> propertyExpression )
            where TStructuralType : class => configuration.HasPrimitiveAnnotations( propertyExpression );

        /// <summary>
        /// Configures the property of a structural type as an annotation.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="TProperty">The <see cref="Type">type</see> of value being annotated.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="ComplexTypeConfiguration{TComplexType}">complex type configuration</see> for the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static ComplexTypeInstanceAnnotationConfiguration<TProperty> HasComplexAnnotation<TStructuralType, TProperty>(
            this EntityTypeConfiguration<TStructuralType> configuration,
            Expression<Func<TStructuralType, TProperty>> propertyExpression )
            where TStructuralType : class
            where TProperty : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<ComplexTypeInstanceAnnotationConfiguration<TProperty>>() != null );

            var annotationType = typeof( TProperty );

            // ensure this isn't a collection; must use HasAnnotations instead
            if ( annotationType.IsEnumerable() )
                throw new InvalidOperationException( string.Format( CurrentCulture, InvalidComplexType, annotationType ) );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // build an annotation for the entity property
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
            var annotationConfig = new ComplexTypeInstanceAnnotationConfiguration<TProperty>( configuration.ToEdmTypeConfiguration(), name, annotationTypeConfig, annotation );

            builder.GetAnnotationConfigurations().Add( annotationConfig );

            return annotationConfig;
        }

        /// <summary>
        /// Configures the property of a structural type as a collection of annotations.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="TProperty">The <see cref="Type">type</see> of value being annotated.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="ComplexTypeConfiguration{TComplexType}">complex type configuration</see> for the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static ComplexTypeInstanceAnnotationConfiguration<TProperty> HasComplexAnnotations<TStructuralType, TProperty>(
            this EntityTypeConfiguration<TStructuralType> configuration,
            Expression<Func<TStructuralType, IEnumerable<TProperty>>> propertyExpression )
            where TStructuralType : class
            where TProperty : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<ComplexTypeInstanceAnnotationConfiguration<TProperty>>() != null );

            var annotationType = typeof( TProperty );

            // ensure a nested collection hasn't been specified
            if ( annotationType.IsEnumerable() )
                throw new InvalidOperationException( string.Format( CurrentCulture, InvalidComplexTypeCollection, annotationType ) );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // build an annotation for the entity property
            var builder = configuration.GetModelBuilder();
            var name = ( (MemberExpression) propertyExpression.Body ).Member.Name;
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var qualifiedName = Invariant( $"{configuration.Namespace}.{name}" );
            var annotationTypeConfig = builder.ComplexType<TProperty>();
            var annotation = new InstanceAnnotation( accessor, qualifiedName, annotationTypeConfig.ToEdmTypeConfiguration() )
            {
                IsComplex = true,
                IsCollection = true,
                IsNullable = true
            };
            var annotationConfig = new ComplexTypeInstanceAnnotationConfiguration<TProperty>( configuration.ToEdmTypeConfiguration(), name, annotationTypeConfig, annotation );

            builder.GetAnnotationConfigurations().Add( annotationConfig );

            return annotationConfig;
        }
    }
}
