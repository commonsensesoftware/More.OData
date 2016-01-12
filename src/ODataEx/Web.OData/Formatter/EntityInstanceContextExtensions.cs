namespace More.Web.OData.Formatter
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.OData;

    internal static class EntityInstanceContextExtensions
    {
        [SuppressMessage( "Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method should never throw an exception." )]
        internal static object TryGetEntityInstance( this EntityInstanceContext context )
        {
            Contract.Requires( context != null );

            if ( context.SerializerContext.SelectExpandClause == null )
                return context.EntityInstance;

            try
            {
                // this might through an exception inside a projection (ex: $select)
                return context.EntityInstance;
            }
            catch
            {
                return null;
            }
        }
    }
}
