namespace System.Web.OData.Builder
{
    using Collections.Concurrent;
    using Collections.Generic;
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using IO;
    using Linq;
    using Linq.Expressions;
    using Microsoft.OData.Edm;
    using Microsoft.Spatial;
    using More;
    using More.OData.Edm;
    using More.Web.OData;
    using More.Web.OData.Builder;
    using System;
    using static Globalization.CultureInfo;
    using static Reflection.BindingFlags;

    /// <summary>
    /// Provides extension methods related to entity model construction.
    /// </summary>
    [SuppressMessage( "Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This due to the high number of overloads. Coupling is allowed given they are extension methods. Consider partial classes if necessary." )]
    public static class BuilderExtensions
    {
        private static readonly IList<IAnnotationConfiguration> NoConfigurations = new IAnnotationConfiguration[0];
        private static readonly ConcurrentDictionary<ODataModelBuilder, List<IAnnotationConfiguration>> annotations = new ConcurrentDictionary<ODataModelBuilder, List<IAnnotationConfiguration>>();
        private static readonly ConcurrentDictionary<object, ODataModelBuilder> configurationToModelBuilderMap = new ConcurrentDictionary<object, ODataModelBuilder>();

        /// <summary>
        /// Returns the model builder associated with the configuration.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">configuration</see> to get the model builder for.</param>
        /// <returns>The associated <see cref="ODataModelBuilder">model builder</see>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static ODataModelBuilder GetModelBuilder<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration ) where TEntityType : class
        {
            Contract.Requires( configuration != null );
            Contract.Ensures( Contract.Result<ODataModelBuilder>() != null );

            // optimize to checking the model builder on the collection first since that tends to be not null
            var builder = configuration.Collection?.ModelBuilder ?? configuration.BaseType?.ModelBuilder;

            if ( builder != null )
                return builder;

            // fallback: there appears to be no other way to get the underlying model builder if we don't have a base type or collection
            // note: we can't build a dynamic using a lambda expression, so use a dictionary to minimize reflection use
            return configurationToModelBuilderMap.GetOrAdd(
                configuration,
                key =>
                {
                    var type = typeof( StructuralTypeConfiguration<TEntityType> );
                    var field = type.GetField( "_configuration", Instance | NonPublic );
                    var innerConfiguration = (StructuralTypeConfiguration) field.GetValue( key );
                    return innerConfiguration.ModelBuilder;
                } );
        }

        /// <summary>
        /// Attempts to register a callback for the specified annotation configuration and entity type.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder">model builder</see> to register the callback with.</param>
        /// <param name="configuration">The <see cref="EntityInstanceAnnotationConfiguration">configuration</see> to register the callback for.</param>
        /// <param name="entityType">The entity <see cref="Type">type</see> associated with the configuration.</param>
        /// <remarks>
        /// <para>
        /// A callback will only be registered if the <paramref name="modelBuilder">model builder</paramref> is of type <see cref="ODataConventionModelBuilder"/>
        /// because <see cref="ODataModelBuilder"/> does not define the <see cref="ODataConventionModelBuilder.OnModelCreating"/> callback. The <see cref="ODataModelBuilder"/>
        /// is used in case this ever refactored in the future.
        /// </para>
        /// <para>
        /// This method is only meant to be used by the authors of custom entity annotation configurations. Annotation configurations typically need to be called back
        /// before the model is finalized because the associated entity name may have changed due to other configuration options.
        /// </para>
        /// </remarks>
        public static bool TryRegisterCallback( this ODataModelBuilder modelBuilder, EntityInstanceAnnotationConfiguration configuration, Type entityType )
        {
            Arg.NotNull( modelBuilder, nameof( modelBuilder ) );
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( entityType, nameof( entityType ) );

            // note: if the OnModelCreated property is redefined on the ODataModelBuilder or
            // an alternate mechanism is provided, this should be refactored
            var builder = modelBuilder as ODataConventionModelBuilder;

            // we can only register a callback if the convention builder was used
            if ( builder == null )
                return false;

            // we register a callback because the initially configured type names may have changed
            // since the time when the annotation was originally defined
            builder.OnModelCreating += b => configuration.EntityTypeName = b.GetTypeConfigurationOrNull( entityType )?.FullName ?? configuration.EntityTypeName;
            return true;
        }

        /// <summary>
        /// Attempts to register a callback for the specified annotation configuration, entity type, and annotation type.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder">model builder</see> to register the callback with.</param>
        /// <param name="configuration">The <see cref="EntityInstanceAnnotationConfiguration">configuration</see> to register the callback for.</param>
        /// <param name="entityType">The entity <see cref="Type">type</see> associated with the configuration.</param>
        /// <param name="annotationType">The annotation <see cref="Type">type</see> associated with the configuration.</param>
        /// <returns>True if a callback was registered; otherwise, false.</returns>
        /// <remarks>
        /// <para>
        /// A callback will only be registered if the <paramref name="modelBuilder">model builder</paramref> is of type <see cref="ODataConventionModelBuilder"/>
        /// because <see cref="ODataModelBuilder"/> does not define the <see cref="ODataConventionModelBuilder.OnModelCreating"/> callback. The <see cref="ODataModelBuilder"/>
        /// is used in case this ever refactored in the future.
        /// </para>
        /// <para>
        /// This method is only meant to be used by the authors of custom entity annotation configurations. Annotation configurations typically need to be called back
        /// before the model is finalized because the associated entity or annotation name may have changed due to other configuration options.
        /// </para>
        /// </remarks>
        public static bool TryRegisterCallback( this ODataModelBuilder modelBuilder, EntityInstanceAnnotationConfiguration configuration, Type entityType, Type annotationType )
        {
            Arg.NotNull( modelBuilder, nameof( modelBuilder ) );
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( entityType, nameof( entityType ) );
            Arg.NotNull( annotationType, nameof( annotationType ) );

            // note: if the OnModelCreated property is redefined on the ODataModelBuilder or
            // an alternate mechanism is provided, this should be refactored
            var builder = modelBuilder as ODataConventionModelBuilder;

            // we can only register a callback if the convention builder was used
            if ( builder == null )
                return false;

            // we register a callback because the initially configured type names may have changed
            // since the time when the annotation was originally defined
            builder.OnModelCreating += b =>
            {
                var annotation = configuration.Annotation;
                configuration.EntityTypeName = b.GetTypeConfigurationOrNull( entityType )?.FullName ?? configuration.EntityTypeName;
                annotation.AnnotationTypeName = b.GetTypeConfigurationOrNull( annotationType )?.FullName ?? annotation.AnnotationTypeName;
            };

            return true;
        }

        /// <summary>
        /// Gets the annotation configurations associated with the specified model builder.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder">model builder</see> get the annotation configurations for.</param>
        /// <returns>A <see cref="IList{T}">list</see> of <see cref="IAnnotationConfiguration">annotation configurations</see>.</returns>
        public static IList<IAnnotationConfiguration> GetAnnotationConfigurations( this ODataModelBuilder modelBuilder )
        {
            Arg.NotNull( modelBuilder, nameof( modelBuilder ) );
            Contract.Ensures( Contract.Result<IList<IAnnotationConfiguration>>() != null );
            return annotations.GetOrAdd( modelBuilder, b => new List<IAnnotationConfiguration>() );
        }

        /// <summary>
        /// Applies the annotation configurations for the model builder to the specified model.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder">model builder</see> whose annotation configurations to apply.</param>
        /// <param name="model">The <see cref="IEdmModel">model</see> to apply the annotation configurations to.</param>
        /// <remarks>This method clears all annotation configurations associated with the <paramref name="modelBuilder">model builder</paramref>
        /// after they have been applied to the model.</remarks>
        public static void ApplyAnnotations( this ODataModelBuilder modelBuilder, IEdmModel model )
        {
            Arg.NotNull( modelBuilder, nameof( modelBuilder ) );
            Arg.NotNull( model, nameof( model ) );

            List<IAnnotationConfiguration> value;
            var configurations = annotations.TryRemove( modelBuilder, out value ) ? value : NoConfigurations;

            foreach ( var configuration in configurations )
                configuration.Apply( model );
        }

        /// <summary>
        /// Creates and returns a Entity Data Model (EDM) and applies any associated annotations.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ODataModelBuilder">model builder</see> to create, apply, and return the model for.</param>
        /// <returns>The created <see cref="IEdmModel">model</see> with all associated annotations applied.</returns>
        /// <remarks>This method is the equivalent of calling <see cref="ODataModelBuilder.GetEdmModel"/> and <see cref="ApplyAnnotations(ODataModelBuilder, IEdmModel)"/>;
        /// therefore, this method also clears all annotation configurations associated with the <paramref name="modelBuilder">model builder</paramref>
        /// after they have been applied to the model.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0" )]
        public static IEdmModel GetEdmModelWithAnnotations( this ODataModelBuilder modelBuilder )
        {
            Arg.NotNull( modelBuilder, nameof( modelBuilder ) );
            Contract.Ensures( Contract.Result<IEdmModel>() != null );

            var model = modelBuilder.GetEdmModel();
            modelBuilder.ApplyAnnotations( model );
            return model;
        }

        private static Lazy<Func<object, object>> ToLazyContravariantFunc<TEntityType, TProperty>( this Expression<Func<TEntityType, TProperty>> propertyExpression )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( Contract.Result<Lazy<Func<object, object>>>() != null );

            var expression = propertyExpression;

            // compile the property expression into a lazy-initialized contravariant function
            // we do this so that we don't have to use reflection later to get the annotation value
            return new Lazy<Func<object, object>>( () =>
            {
                var func = expression.Compile();
                return o => func( (TEntityType) o );
            } );
        }

        private static EntityTypeConfiguration<TEntityType> HasPrimitiveAnnotation<TEntityType, TProperty>(
            this EntityTypeConfiguration<TEntityType> configuration,
            Expression<Func<TEntityType, TProperty>> propertyExpression,
            string name )
            where TEntityType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<EntityTypeConfiguration<TEntityType>>() != null );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // if a name wasn't specified, derive from the name of the property
            if ( string.IsNullOrEmpty( name ) )
                name = ( (MemberExpression) propertyExpression.Body ).Member.Name;

            // build an annotation for the entity property
            // note: since the annotation must be a primitive in this context,
            // we know the qualified type name upfront and it can't change
            var builder = configuration.GetModelBuilder();
            var entityType = typeof( TEntityType );
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var annotationType = typeof( TProperty );
            var annotationTypeName = builder.GetTypeConfigurationOrNull( annotationType ).FullName;
            var annotation = new EntityInstanceAnnotation( accessor, name, annotationTypeName ) { IsNullable = annotationType.IsNullable() };
            var annotationConfig = new EntityInstanceAnnotationConfiguration( entityType.GetQualifiedEdmTypeName(), annotation );

            // associate the annotation with the builder and register a callback, when possible
            builder.GetAnnotationConfigurations().Add( annotationConfig );
            builder.TryRegisterCallback( annotationConfig, entityType );

            return configuration;
        }

        private static EntityTypeConfiguration<TEntityType> HasPrimitiveAnnotations<TEntityType, TProperty>(
            this EntityTypeConfiguration<TEntityType> configuration,
            Expression<Func<TEntityType, IEnumerable<TProperty>>> propertyExpression,
            string name )
            where TEntityType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<EntityTypeConfiguration<TEntityType>>() != null );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // if a name wasn't specified, derive from the name of the property
            if ( string.IsNullOrEmpty( name ) )
                name = ( (MemberExpression) propertyExpression.Body ).Member.Name;

            // build an annotation for the entity property
            // note: since the annotation must be a primitive in this context,
            // we know the qualified type name upfront and it can't change
            var builder = configuration.GetModelBuilder();
            var entityType = typeof( TEntityType );
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var annotationType = typeof( TProperty );
            var annotationTypeName = builder.GetTypeConfigurationOrNull( annotationType ).FullName;
            var annotation = new EntityInstanceAnnotation( accessor, name, annotationTypeName )
            {
                IsCollection = true,
                IsNullable = annotationType.IsNullable()
            };
            var annotationConfig = new EntityInstanceAnnotationConfiguration( entityType.GetQualifiedEdmTypeName(), annotation );

            // associate the annotation with the builder and register a callback, when possible
            builder.GetAnnotationConfigurations().Add( annotationConfig );
            builder.TryRegisterCallback( annotationConfig, entityType );

            return configuration;
        }

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, string>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<string>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, bool>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, bool?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, byte>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<byte>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, byte?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, DateTime>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<DateTime>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, DateTime?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, DateTimeOffset>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<DateTimeOffset>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, DateTimeOffset?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, TimeSpan>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<TimeSpan>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, TimeSpan?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, decimal>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<decimal>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, decimal?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, double>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<double>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, double?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>

        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, Guid>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<Guid>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, Guid?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, short>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<short>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, short?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, int>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<int>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, int?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, long>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<long>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, long?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, sbyte>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<sbyte>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, sbyte?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, float>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<float>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, float?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, ushort>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<ushort>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, ushort?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, uint>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<uint>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, uint?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, ulong>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotations<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, IEnumerable<ulong>>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotations( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [CLSCompliant( false )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, ulong?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, char>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, char?>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, char[]>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, byte[]>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, Stream>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, Geography>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeographyPoint>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeographyLineString>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeographyPolygon>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeographyCollection>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>

        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeographyMultiLineString>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeographyMultiPoint>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeographyMultiPolygon>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, Geometry>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeometryPoint>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeometryLineString>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeometryPolygon>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeometryCollection>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeometryMultiLineString>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeometryMultiPoint>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>The original <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see>.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static EntityTypeConfiguration<TEntityType> HasAnnotation<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, GeometryMultiPolygon>> propertyExpression, string name = null ) where TEntityType : class =>
            configuration.HasPrimitiveAnnotation( propertyExpression, name );

        /// <summary>
        /// Configures the property of an entity as an annotation.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <typeparam name="TProperty">The <see cref="Type">type</see> of value being annotated.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotation.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>A <see cref="ComplexTypeConfiguration{TComplexType}">complex type configuration</see> for the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static ComplexTypeConfiguration<TProperty> HasAnnotation<TEntityType, TProperty>(
            this EntityTypeConfiguration<TEntityType> configuration,
            Expression<Func<TEntityType, TProperty>> propertyExpression,
            string name = null )
            where TEntityType : class
            where TProperty : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<ComplexTypeConfiguration<TProperty>>() != null );

            var annotationType = typeof( TProperty );

            // ensure this isn't a collection; must use HasAnnotations instead
            if ( annotationType.IsEnumerable() )
                throw new InvalidOperationException( string.Format( CurrentCulture, ExceptionMessage.InvalidComplexType, annotationType ) );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // if a name wasn't specified, derive from the name of the property
            if ( string.IsNullOrEmpty( name ) )
                name = ( (MemberExpression) propertyExpression.Body ).Member.Name;

            // build an annotation for the entity property
            var entityType = typeof( TEntityType );
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var builder = configuration.GetModelBuilder();
            var annotation = new EntityInstanceAnnotation( accessor, name, annotationType.GetUnderlyingType().GetQualifiedEdmTypeName() )
            {
                IsComplex = true,
                IsNullable = true
            };
            var annotationConfig = new EntityInstanceAnnotationConfiguration( entityType.GetQualifiedEdmTypeName(), annotation );

            // associate the annotation with the builder and register a callback, when possible
            builder.GetAnnotationConfigurations().Add( annotationConfig );
            builder.TryRegisterCallback( annotationConfig, entityType, annotationType );

            return builder.ComplexType<TProperty>();
        }

        /// <summary>
        /// Configures the property of an entity as a collection of annotations.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to annotate.</typeparam>
        /// <typeparam name="TProperty">The <see cref="Type">type</see> of value being annotated.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to add annotations to.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the annotations.</param>
        /// <param name="name">The name of the annotation. If this parameter is <c>null</c> or empty, it is derived from the property expression.</param>
        /// <returns>A <see cref="ComplexTypeConfiguration{TComplexType}">complex type configuration</see> for the annotation.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "The name derives from the property expression by default. This would otherwise require 2x methods." )]
        public static ComplexTypeConfiguration<TProperty> HasAnnotations<TEntityType, TProperty>(
            this EntityTypeConfiguration<TEntityType> configuration,
            Expression<Func<TEntityType, IEnumerable<TProperty>>> propertyExpression,
            string name = null )
            where TEntityType : class
            where TProperty : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<ComplexTypeConfiguration<TProperty>>() != null );

            var annotationType = typeof( TProperty );

            // ensure a nested collection hasn't been specified
            if ( annotationType.IsEnumerable() )
                throw new InvalidOperationException( string.Format( CurrentCulture, ExceptionMessage.InvalidComplexTypeCollection, annotationType ) );

            // always ignore the annotation property from the entity model
            configuration.Ignore( propertyExpression );

            // if a name wasn't specified, derive from the name of the property
            if ( string.IsNullOrEmpty( name ) )
                name = ( (MemberExpression) propertyExpression.Body ).Member.Name;

            // build an annotation for the entity property
            var builder = configuration.GetModelBuilder();
            var entityType = typeof( TEntityType );
            var accessor = propertyExpression.ToLazyContravariantFunc();
            var annotation = new EntityInstanceAnnotation( accessor, name, annotationType.GetUnderlyingType().GetQualifiedEdmTypeName() )
            {
                IsComplex = true,
                IsCollection = true,
                IsNullable = true
            };
            var annotationConfig = new EntityInstanceAnnotationConfiguration( entityType.GetQualifiedEdmTypeName(), annotation );

            // associate the annotation with the builder and register a callback, when possible
            builder.GetAnnotationConfigurations().Add( annotationConfig );
            builder.TryRegisterCallback( annotationConfig, entityType, annotationType );

            return builder.ComplexType<TProperty>();
        }

        /// <summary>
        /// Configures the specified link type to use the standard conventions.
        /// </summary>
        /// <typeparam name="TLink">The <see cref="Type">type</see> of <see cref="Link">link</see> to configure.</typeparam>
        /// <param name="configuration">The <see cref="ComplexTypeConfiguration{TComplexType}">configuration</see> to apply the conventions to.</param>
        /// <remarks>This method applies the following standard conventions to the <paramref name="configuration"/>: namespace is "Microsoft",
        /// name is the <see cref="T:Type.Name"/> of <typeparamref name="TLink"/>, <see cref="Link.Relation"/> is aliased as "rel",
        /// and <see cref="Link.Url"/> is aliased as "href".</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static void UseStandardConventions<TLink>( this ComplexTypeConfiguration<TLink> configuration ) where TLink : Link
        {
            Arg.NotNull( configuration, nameof( configuration ) );

            configuration.Namespace = nameof( Microsoft );
            configuration.Name = typeof( TLink ).Name;
            configuration.Property( link => link.Relation ).Name = "rel";
            configuration.Property( link => link.Url ).Name = "href";
        }

        /// <summary>
        /// Configures the specified entity as a media link entry (MLE) and uses the provided property to retrieve the media content type.
        /// </summary>
        /// <typeparam name="TEntityType">The <see cref="Type">type</see> of entity to configure.</typeparam>
        /// <param name="configuration">The <see cref="EntityTypeConfiguration{TEntityType}">entity type configuration</see> to configure as a media link entry.</param>
        /// <param name="propertyExpression">The <see cref="Expression{TDelegate}">expression</see> for the entity property that represents the media content type.</param>
        /// <returns>The <see cref="MediaTypeConfiguration{TEntityType}">configuration</see> that can be used to further configure the media type.</returns>
        /// <remarks>The property represented by the <paramref name="propertyExpression">expression</paramref> will be
        /// <see cref="StructuralTypeConfiguration{TStructuralType}.Ignore{TProperty}(Expression{Func{TStructuralType, TProperty}})">ignored</see>
        /// since the data will appear as an annotation (e.g. metadata) instead of as part of the entity.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public static MediaTypeConfiguration<TEntityType> MediaType<TEntityType>( this EntityTypeConfiguration<TEntityType> configuration, Expression<Func<TEntityType, string>> propertyExpression ) where TEntityType : class
        {
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNull( propertyExpression, nameof( propertyExpression ) );
            Contract.Ensures( Contract.Result<MediaTypeConfiguration<TEntityType>>() != null );

            // an entity that represents a media type can only have a single link entry. check for presense of an existing configuration.
            var builder = configuration.GetModelBuilder();
            var mediaTypeConfig = builder.GetAnnotationConfigurations().OfType<MediaTypeConfiguration<TEntityType>>().FirstOrDefault();

            // the media type has already been initialize configured, so just return the existing configuration
            if ( mediaTypeConfig != null )
                return mediaTypeConfig;

            // always ignore the content type property from the entity model and call the build-in MediaType
            // extension method, which will configure the entity model with HasStream = true for the entity
            configuration.Ignore( propertyExpression );
            configuration.MediaType();

            // compile the property expression into a lazy-initialized contravariant function
            // we do this so that we don't have to use reflection later to get the content type
            // nor require the developer to write explicit code to get the value
            var contentType = new Lazy<Func<object, string>>( () =>
            {
                var func = propertyExpression.Compile();
                return o => func( (TEntityType) o );
            } );
            var annotation = new MediaLinkEntryAnnotation( contentType );

            mediaTypeConfig = new MediaTypeConfiguration<TEntityType>( configuration, annotation );
            builder.GetAnnotationConfigurations().Add( mediaTypeConfig );

            return mediaTypeConfig;
        }
    }
}
