namespace System.Web.OData.Builder
{
    using Diagnostics.Contracts;
    using Linq.Expressions;
    using Reflection;
    using Text;
    using static Globalization.CultureInfo;
    using static More.ExceptionMessage;
    using static More.StringExtensions;

    internal static class ExpressionExtensions
    {
        internal static void IgnoredBy<TStructuralType, TProperty>( this Expression<Func<TStructuralType, TProperty>> propertyExpression, StructuralTypeConfiguration<TStructuralType> configuration ) where TStructuralType : class
        {
            Contract.Requires( propertyExpression != null );
            Contract.Requires( configuration != null );

            var node = (MemberExpression) propertyExpression.Body;

            // simple scenario; just short-circuit based on the current expression
            // ex: o => o.Property
            if ( node.Expression.NodeType == ExpressionType.Parameter )
            {
                configuration.Ignore( propertyExpression );
                return;
            }

            // reduce the expression so we exclude the property and its entire object graph
            // ex: o => o.Property.SubProperty :: o => o.Property
            do
            {
                node = (MemberExpression) node.Expression;
            }
            while ( node.Expression.NodeType != ExpressionType.Parameter );

            var property = node.Member as PropertyInfo;

            if ( property == null )
                throw new ArgumentException( string.Format( CurrentCulture, InvalidPropertyPathExpression, propertyExpression ), nameof( propertyExpression ) );

            // remove the property from the configuration (equivalent to Ignore, but without using Reflection to create the required generic method)
            configuration.GetInnerConfiguration().RemoveProperty( property );
        }

        internal static string GetPropertyName<TObject, TProperty>( this Expression<Func<TObject, TProperty>> propertyExpression )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var node = propertyExpression.Body as MemberExpression;

            // the expression wasn't valid; throw meaningful exception
            if ( node == null )
                throw new ArgumentException( string.Format( CurrentCulture, InvalidPropertyExpression, propertyExpression ), nameof( propertyExpression ) );

            return node.Member.Name;
        }

        internal static string GetMediaResourceKey<TObject, TProperty>( this Expression<Func<TObject, TProperty>> propertyExpression )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var visitor = new FirstAccessedMemberVisitor();

            // visit the expression and get the name of the first member (e.g. property or field)
            visitor.Visit( propertyExpression );

            // the expression wasn't valid; throw meaningful exception
            if ( string.IsNullOrEmpty( visitor.Name ) )
                throw new ArgumentException( string.Format( CurrentCulture, InvalidMediaResourcePropertyExpression, propertyExpression ), nameof( propertyExpression ) );

            // build unique key
            return Invariant( $"{typeof( TObject ).FullName}.{visitor.Name}" );
        }

        internal static string GetInstanceAnnotationKey<TObject, TProperty>( this Expression<Func<TObject, TProperty>> propertyExpression, out string name )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            name = null;
            var node = propertyExpression.Body as MemberExpression;

            // the expression wasn't valid; throw meaningful exception
            if ( node == null )
                throw new ArgumentException( string.Format( CurrentCulture, InvalidAnnotationPropertyExpression, propertyExpression ), nameof( propertyExpression ) );

            // use the name of the tail member expression as the initial annotation name            
            var key = new StringBuilder( name = node.Member.Name );

            // build up expression path
            while ( node.Expression.NodeType != ExpressionType.Parameter )
            {
                // the expression node wasn't valid; throw meaningful exception
                if ( ( node = node.Expression as MemberExpression ) == null )
                    throw new ArgumentException( string.Format( CurrentCulture, InvalidAnnotationPropertyExpression, propertyExpression ), nameof( propertyExpression ) );

                key.Insert( 0, '.' );
                key.Insert( 0, node.Member.Name );
            }

            // build up unique key
            key.Insert( 0, '.' );
            key.Insert( 0, typeof( TObject ).FullName );

            return key.ToString();
        }
    }
}
