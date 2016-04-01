namespace System.Web.OData.Formatter
{
    using System;
    using System.Dynamic;

    internal sealed class SimpleMemberBinder : GetMemberBinder
    {
        internal SimpleMemberBinder( string memberName )
            : base( memberName, false )
        {
        }

        public override DynamicMetaObject FallbackGetMember( DynamicMetaObject target, DynamicMetaObject errorSuggestion )
        {
            throw new NotSupportedException();
        }
    }
}
