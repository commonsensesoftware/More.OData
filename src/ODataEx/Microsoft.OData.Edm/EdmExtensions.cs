namespace Microsoft.OData.Edm
{
    using Library;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.OData;
    using System.Web.OData.Builder;

    /// <summary>
    /// Provides Entity Data Model (EDM) extension methods.
    /// </summary>
    public static class EdmExtensions
    {
        /// <summary>
        /// Clones the specified operation using the qualified name of the action as the name.
        /// </summary>
        /// <param name="action">The <see cref="IEdmAction">action</see> to clone.</param>
        /// <returns>A new <see cref="IEdmAction"/> that is a clone of the original <paramref name="action"/> where the
        /// <see cref="IEdmNamedElement.Name"/> is the qualified name of the action.</returns>
        /// <remarks>This extension method is a workaround solution for a bug in the
        /// <see cref="LinkGenerationHelpers.GenerateActionLink(EntityInstanceContext, IEdmOperation)"/>
        /// which does not use the qualified name of the action.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)", Justification = "String interpolation is invariant in this context." )]
        public static IEdmAction CloneWithQualifiedName( this IEdmAction action )
        {
            Arg.NotNull( action, nameof( action ) );
            Contract.Ensures( Contract.Result<IEdmAction>() != null );

            var qualifiedName = $"{action.Namespace}.{action.Name}";
            var clone = new EdmAction( action.Namespace, qualifiedName, action.ReturnType, action.IsBound, action.EntitySetPath );

            foreach ( var parameter in action.Parameters )
                clone.AddParameter( parameter );

            return clone;
        }
    }
}
