namespace System.Web.OData.Builder
{
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Linq.Expressions;
    using Reflection;

    internal sealed class InitialPropertyVisitor : ExpressionVisitor
    {
        public PropertyInfo InitialProperty
        {
            get;
            private set;
        }

        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        protected override Expression VisitMember( MemberExpression node )
        {
            Contract.Assume( node != null );

            if ( node.Expression.NodeType == ExpressionType.Parameter )
                InitialProperty = node.Member as PropertyInfo;

            return base.VisitMember( node );
        }
    }
}
