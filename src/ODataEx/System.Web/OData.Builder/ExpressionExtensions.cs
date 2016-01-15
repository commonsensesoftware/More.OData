namespace System.Web.OData.Builder
{
    using Diagnostics.Contracts;
    using Linq.Expressions;
    using Reflection;
    using static Globalization.CultureInfo;
    using static More.ExceptionMessage;

    internal static class ExpressionExtensions
    {
        internal static void IgnoredBy<TStructuralType, TProperty>( this Expression<Func<TStructuralType, TProperty>> propertyExpression, StructuralTypeConfiguration<TStructuralType> configuration ) where TStructuralType : class
        {
            Contract.Requires( propertyExpression != null );
            Contract.Requires( configuration != null );

            var visitor = new InitialPropertyVisitor();

            visitor.Visit( propertyExpression );

            var property = visitor.InitialProperty;

            // remove the property from the configuration (equivalent to Ignore, but without using Reflection to create the required generic method)
            // note: we only need to consider ignoring properties. fields, methods, and indexers can be used, but are never exposed in the entity model
            if ( property != null )
                configuration.GetInnerConfiguration().RemoveProperty( property );
        }

        internal static string GetPropertyName<TObject, TProperty>( this Expression<Func<TObject, TProperty>> propertyExpression )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var node = propertyExpression.Body as MemberExpression;

            // the expression wasn't valid; throw meaningful exception
            if ( node == null || node.Member.MemberType != MemberTypes.Property )
                throw new ArgumentException( string.Format( CurrentCulture, InvalidPropertyExpression, propertyExpression ), nameof( propertyExpression ) );

            return node.Member.Name;
        }

        internal static string GetMediaResourceKey<TObject, TProperty>( this Expression<Func<TObject, TProperty>> propertyExpression )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var visitor = new PropertyPathVisitor();

            // visit the expression and get the name of the first member (e.g. property or field)
            visitor.Visit( propertyExpression );

            // the expression wasn't valid; throw meaningful exception
            if ( string.IsNullOrEmpty( visitor.Name ) )
                throw new ArgumentException( string.Format( CurrentCulture, InvalidMediaResourcePropertyExpression, propertyExpression ), nameof( propertyExpression ) );

            // build unique key
            return visitor.GenerateKey( typeof( TObject ).FullName, includeLiterals: false );
        }

        internal static string GetInstanceAnnotationKey<TObject, TProperty>( this Expression<Func<TObject, TProperty>> propertyExpression, out string name )
        {
            Contract.Requires( propertyExpression != null );
            Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );

            var visitor = new PropertyPathVisitor();

            // visit the expression and get the name of the first member (e.g. property or field)
            visitor.Visit( propertyExpression );

            // the expression wasn't valid; throw meaningful exception
            if ( string.IsNullOrEmpty( name = visitor.Name ) )
                throw new ArgumentException( string.Format( CurrentCulture, InvalidAnnotationPropertyExpression, propertyExpression ), nameof( propertyExpression ) );

            // build unique key
            return visitor.GenerateKey( typeof( TObject ).FullName );
        }
    }
}
