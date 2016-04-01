namespace System.Web.OData.Builder
{
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Extensions;
    using Linq;
    using Linq.Expressions;
    using Microsoft.OData.Core;
    using Microsoft.OData.Core.UriParser;
    using Microsoft.OData.Edm;
    using Routing;
    using System;
    using static Reflection.BindingFlags;

    /// <summary>
    /// Provides extension methods for OData configuration objects.
    /// </summary>
    public static class ConfigurationExtensions
    {
        internal static string GetEntitySetName<TEntity>( this EntitySetConfiguration<TEntity> configuration ) where TEntity : class
        {
            Contract.Requires( configuration != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var type = typeof( NavigationSourceConfiguration<TEntity> );
            var field = type.GetField( "_configuration", Instance | NonPublic );
            var innerConfig = (NavigationSourceConfiguration) field.GetValue( configuration );

            return innerConfig.Name;
        }

        /// <summary>
        /// Configures the navigation property of an entity in an entity using the specified property expression
        /// and target entity set name.
        /// </summary>
        /// <typeparam name="TEntity">The <see cref="Type">type</see> of entity.</typeparam>
        /// <typeparam name="TValue">The <see cref="Type">type</see> of property value.</typeparam>
        /// <param name="entitySet">The <see cref="EntitySetConfiguration{TEntityType}">entity set configuration</see> to add the navigation property to.</param>
        /// <param name="navigationProperty">The <see cref="NavigationPropertyConfiguration">navigation property configuration</see> to configure.</param>
        /// <param name="foreignKeyProperty">The property <see cref="Expression{TDelegate}">expression</see> representing the key in the target entity set.</param>
        /// <param name="targetEntitySetName">The target entity set name.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generics." )]
        public static void HasNavigationPropertyLink<TEntity, TValue>(
            this EntitySetConfiguration<TEntity> entitySet,
            NavigationPropertyConfiguration navigationProperty,
            Expression<Func<TEntity, TValue>> foreignKeyProperty,
            string targetEntitySetName ) where TEntity : class
        {
            Arg.NotNull( entitySet, nameof( entitySet ) );
            Arg.NotNull( navigationProperty, nameof( navigationProperty ) );
            Arg.NotNull( foreignKeyProperty, nameof( foreignKeyProperty ) );
            Arg.NotNullOrEmpty( targetEntitySetName, nameof( targetEntitySetName ) );

            var foreignKeyPropertyName = foreignKeyProperty.GetPropertyName();

            entitySet.HasNavigationPropertyLink(
                navigationProperty,
                ( context, property ) =>
                {
                    var entity = context.EdmObject;
                    object value;

                    if ( !entity.TryGetPropertyValue( foreignKeyPropertyName, out value ) )
                        return null;

                    if ( value == null )
                        return null;

                    var entitySetPath = new EntitySetPathSegment( targetEntitySetName );
                    var keyValuePath = new KeyValuePathSegment( ODataUriUtils.ConvertToUriLiteral( value, ODataVersion.V4 ) );
                    var url = new Uri( context.Url.CreateODataLink( entitySetPath, keyValuePath ) );

                    return url;
                },
                false );
        }
    }
}
