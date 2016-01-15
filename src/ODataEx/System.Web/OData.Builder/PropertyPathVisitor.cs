namespace System.Web.OData.Builder
{
    using Collections.Generic;
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Linq;
    using Linq.Expressions;
    using Text;

    /// <summary>
    /// Represents an expression visitor that captures the name of the first referenced member
    /// in a property path and can generate a unique key for the given expression.
    /// </summary>
    internal sealed class PropertyPathVisitor : ExpressionVisitor
    {
        private readonly List<string> members = new List<string>();
        private readonly List<object> literals = new List<object>();
        private bool hasIndexer;

        public string Name
        {
            get
            {
                return members.Count == 0 ? ( hasIndexer ? "Item" : null ) : members[0];
            }
        }

        public string GenerateKey( string @namespace, bool includeLiterals = true )
        {
            Contract.Requires( !string.IsNullOrEmpty( @namespace ) );

            // format: <namespace>.<name>[_<constant>]*
            var key = new StringBuilder( @namespace );

            key.Append( '.' );
            key.Append( Name );

            if ( !includeLiterals )
                return key.ToString();

            foreach ( var literal in literals )
            {
                key.Append( '_' );
                key.Append( literal );
            }

            return key.ToString();
        }

        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        protected override Expression VisitMember( MemberExpression node )
        {
            Contract.Assume( node != null );

            // capture the member name in the expression (ex: o => o.Property)
            members.Add( node.Member.Name );
            return base.VisitMember( node );
        }

        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        protected override Expression VisitMethodCall( MethodCallExpression node )
        {
            Contract.Assume( node != null );

            var method = node.Method;

            if ( method.IsSpecialName && method.Name == "get_Item" )
            {
                // remember if we ever visit an indexer, but don't capture the name (ex: o => o[0]);
                hasIndexer = true;
            }
            else
            {
                // capture the member name in the expression (ex: o => o.GetAnnotation())
                members.Add( method.Name );
            }

            return base.VisitMethodCall( node );
        }

        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        protected override Expression VisitConstant( ConstantExpression node )
        {
            Contract.Assume( node != null );

            literals.Add( node.Value );
            return base.VisitConstant( node );
        }
    }
}
