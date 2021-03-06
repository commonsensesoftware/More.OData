using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.OData.Edm", Justification = "Mirrors built-in namespace." )]
[assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "More.OData.Edm", Justification = "Will grow over time." )]
[assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "More.Web.OData", Justification = "Will grow over time." )]
[assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "More.Web.OData.Builder", Justification = "Will grow over time." )]
[assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "More.Web.OData.Routing", Justification = "Will grow over time." )]
[assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http", Justification = "Mirrors built-in namespace." )]
[assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.OData", Justification = "Mirrors built-in namespace." )]
[assembly: SuppressMessage( "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.OData.Builder", Justification = "Mirrors built-in namespace." )]
[assembly: SuppressMessage( "Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "More.Web.OData.Formatter.EdmHelper.#.cctor()", Justification = "This is due to the large dictionary mapping, which is acceptable in this case." )]

