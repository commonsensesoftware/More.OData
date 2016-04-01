namespace System.Web.OData.Builder
{
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Linq.Expressions;

    /// <summary>
    /// Represents an expression visitor that captures the name of the first referenced member.
    /// </summary>
    /// <example>
    /// o => o.Property;              // matches 'Property'
    /// o => o.Property.SubProperty;  // matches 'SubProperty'
    /// o => o.Property.ToString();   // matches 'Property'
    /// </example>
    internal sealed class FirstAccessedMemberVisitor : ExpressionVisitor
    {
        public string Name
        {
            get;
            private set;
        }

        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        protected override Expression VisitMember( MemberExpression node )
        {
            Contract.Assume( node != null );

            if ( node.Expression.NodeType == ExpressionType.Parameter )
                Name = node.Member.Name;

            return base.VisitMember( node );
        }
    }
}
