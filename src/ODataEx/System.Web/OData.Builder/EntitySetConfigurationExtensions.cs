namespace System.Web.OData.Builder
{
    using Diagnostics.CodeAnalysis;
    using Extensions;
    using Linq.Expressions;
    using Microsoft.OData.Core;
    using Microsoft.OData.Core.UriParser;
    using Routing;
    using System;

    /// <summary>
    /// Provides extension methods for the entity set configurations.
    /// </summary>
    public static class EntitySetConfigurationExtensions
    {
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

            var foreignKeyPropertyName = ( (MemberExpression) foreignKeyProperty.Body ).Member.Name;

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
