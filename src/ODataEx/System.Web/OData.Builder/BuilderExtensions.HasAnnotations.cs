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
        private static InstanceAnnotationConfiguration<TStructuralType> HasPrimitiveAnnotation<TStructuralType, TProperty>(
            this StructuralTypeConfiguration<TStructuralType> configuration,
            Expression<Func<TStructuralType, TProperty>> propertyExpression )
            where TStructuralType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<InstanceAnnotationConfiguration<TStructuralType>>() != null );

            var builder = configuration.GetModelBuilder();
            var configurations = builder.GetAnnotationConfigurations();
            InstanceAnnotationConfiguration<TStructuralType> annotationConfig;

            // if the property has already been configured, return the existing configuration
            if ( configurations.TryGet( propertyExpression, out annotationConfig ) )
                return annotationConfig;

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // build an annotation for the property
            var name = ( (MemberExpression) propertyExpression.Body ).Member.Name;
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var qualifiedName = Invariant( $"{configuration.Namespace}.{name}" );
            var annotationType = typeof( TProperty );
            var annotationTypeConfig = builder.GetTypeConfigurationOrNull( annotationType );
            var annotation = new InstanceAnnotation( accessor, qualifiedName, annotationTypeConfig )
            {
                IsNullable = annotationType.IsNullable()
            };

            annotationConfig = new InstanceAnnotationConfiguration<TStructuralType>( configuration, name, annotation );
            configurations.Add( propertyExpression, annotationConfig );

            return annotationConfig;
        }

        private static InstanceAnnotationConfiguration<TStructuralType> HasPrimitiveAnnotations<TStructuralType, TProperty>(
            this StructuralTypeConfiguration<TStructuralType> configuration,
            Expression<Func<TStructuralType, IEnumerable<TProperty>>> propertyExpression )
            where TStructuralType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<InstanceAnnotationConfiguration<TStructuralType>>() != null );

            var builder = configuration.GetModelBuilder();
            var configurations = builder.GetAnnotationConfigurations();
            InstanceAnnotationConfiguration<TStructuralType> annotationConfig;

            // if the property has already been configured, return the existing configuration
            if ( configurations.TryGet( propertyExpression, out annotationConfig ) )
                return annotationConfig;

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // build an annotation for the entity property
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

            annotationConfig = new InstanceAnnotationConfiguration<TStructuralType>( configuration, name, annotation );
            configurations.Add( propertyExpression, annotationConfig );

            return annotationConfig;
        }

        /// <summary>
        /// Configures the property of a structural type as an annotation.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="T">The annotation <see cref="Type">type</see>.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="InstanceAnnotationConfiguration{TStructuralType}">configuration</see> object that can be used to further configure the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type. This method can be called multiple times for the same property.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static InstanceAnnotationConfiguration<TStructuralType> HasAnnotation<TStructuralType, T>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, T>> propertyExpression )
            where TStructuralType : class
            where T : struct => configuration.HasPrimitiveAnnotation( propertyExpression );

        /// <summary>
        /// Configures the property of a structural type as a collection of annotations.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="T">The annotation <see cref="Type">type</see>.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="InstanceAnnotationConfiguration{TStructuralType}">configuration</see> object that can be used to further configure the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type. This method can be called multiple times for the same property.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static InstanceAnnotationConfiguration<TStructuralType> HasAnnotations<TStructuralType, T>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, IEnumerable<T>>> propertyExpression )
            where TStructuralType : class
            where T : struct => configuration.HasPrimitiveAnnotations( propertyExpression );

        /// <summary>
        /// Configures the property of a structural type as an annotation.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="T">The annotation <see cref="Type">type</see>.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="InstanceAnnotationConfiguration{TStructuralType}">configuration</see> object that can be used to further configure the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type. This method can be called multiple times for the same property.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static InstanceAnnotationConfiguration<TStructuralType> HasAnnotation<TStructuralType, T>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, T?>> propertyExpression )
            where TStructuralType : class
            where T : struct => configuration.HasPrimitiveAnnotation( propertyExpression );

        /// <summary>
        /// Configures the property of a structural type as an annotation.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="InstanceAnnotationConfiguration{TStructuralType}">configuration</see> object that can be used to further configure the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type. This method can be called multiple times for the same property.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static InstanceAnnotationConfiguration<TStructuralType> HasAnnotation<TStructuralType>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, string>> propertyExpression )
            where TStructuralType : class => configuration.HasPrimitiveAnnotation( propertyExpression );

        /// <summary>
        /// Configures the property of a structural type as a collection of annotations.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="InstanceAnnotationConfiguration{TStructuralType}">configuration</see> object that can be used to further configure the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type. This method can be called multiple times for the same property.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static InstanceAnnotationConfiguration<TStructuralType> HasAnnotations<TStructuralType>( this StructuralTypeConfiguration<TStructuralType> configuration, Expression<Func<TStructuralType, IEnumerable<string>>> propertyExpression )
            where TStructuralType : class => configuration.HasPrimitiveAnnotations( propertyExpression );

        /// <summary>
        /// Configures the property of a structural type as an annotation.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="TProperty">The <see cref="Type">type</see> of value being annotated.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="ComplexTypeInstanceAnnotationConfiguration{TStructuralType, TProperty}">configuration</see> object that can be used to further configure the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type. This method can be called multiple times for the same property.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static ComplexTypeInstanceAnnotationConfiguration<TStructuralType, TProperty> HasComplexAnnotation<TStructuralType, TProperty>(
            this StructuralTypeConfiguration<TStructuralType> configuration,
            Expression<Func<TStructuralType, TProperty>> propertyExpression )
            where TStructuralType : class
            where TProperty : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<ComplexTypeInstanceAnnotationConfiguration<TStructuralType, TProperty>>() != null );

            var builder = configuration.GetModelBuilder();
            var configurations = builder.GetAnnotationConfigurations();
            ComplexTypeInstanceAnnotationConfiguration<TStructuralType, TProperty> annotationConfig;

            // if the property has already been configured, return the existing configuration
            if ( configurations.TryGet( propertyExpression, out annotationConfig ) )
                return annotationConfig;

            var annotationType = typeof( TProperty );

            // ensure this isn't a collection; must use HasAnnotations instead
            if ( annotationType.IsEnumerable() )
                throw new InvalidOperationException( string.Format( CurrentCulture, InvalidComplexType, annotationType ) );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // build an annotation for the entity property
            var name = ( (MemberExpression) propertyExpression.Body ).Member.Name;
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var qualifiedName = Invariant( $"{configuration.Namespace}.{name}" );
            var annotationTypeConfig = builder.ComplexType<TProperty>();
            var annotation = new InstanceAnnotation( accessor, qualifiedName, annotationTypeConfig.ToEdmTypeConfiguration() )
            {
                IsComplex = true,
                IsNullable = true
            };

            annotationConfig = new ComplexTypeInstanceAnnotationConfiguration<TStructuralType, TProperty>( configuration, name, annotationTypeConfig, annotation );
            configurations.Add( propertyExpression, annotationConfig );

            return annotationConfig;
        }

        /// <summary>
        /// Configures the property of a structural type as a collection of annotations.
        /// </summary>
        /// <typeparam name="TStructuralType">The structural <see cref="Type">type</see> to annotate.</typeparam>
        /// <typeparam name="TProperty">The <see cref="Type">type</see> of value being annotated.</typeparam>
        /// <param name="configuration">The <see cref="StructuralTypeConfiguration{TStructuralType}">structural type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the property that represents the annotation.</param>
        /// <returns>A <see cref="ComplexTypeInstanceAnnotationConfiguration{TStructuralType, TProperty}">configuration</see> object that can be used to further configure the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the structural type. This method can be called multiple times for the same property.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static ComplexTypeInstanceAnnotationConfiguration<TStructuralType, TProperty> HasComplexAnnotations<TStructuralType, TProperty>(
            this StructuralTypeConfiguration<TStructuralType> configuration,
            Expression<Func<TStructuralType, IEnumerable<TProperty>>> propertyExpression )
            where TStructuralType : class
            where TProperty : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<ComplexTypeInstanceAnnotationConfiguration<TStructuralType, TProperty>>() != null );

            var builder = configuration.GetModelBuilder();
            var configurations = builder.GetAnnotationConfigurations();
            ComplexTypeInstanceAnnotationConfiguration<TStructuralType, TProperty> annotationConfig;

            // if the property has already been configured, return the existing configuration
            if ( configurations.TryGet( propertyExpression, out annotationConfig ) )
                return annotationConfig;

            var annotationType = typeof( TProperty );

            // ensure a nested collection hasn't been specified
            if ( annotationType.IsEnumerable() )
                throw new InvalidOperationException( string.Format( CurrentCulture, InvalidComplexTypeCollection, annotationType ) );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // build an annotation for the entity property
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

            annotationConfig = new ComplexTypeInstanceAnnotationConfiguration<TStructuralType, TProperty>( configuration, name, annotationTypeConfig, annotation );
            configurations.Add( propertyExpression, annotationConfig );

            return annotationConfig;
        }
    }
}
