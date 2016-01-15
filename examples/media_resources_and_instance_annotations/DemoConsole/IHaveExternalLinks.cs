namespace More.Examples
{
    using System.Collections.Generic;

    // note: there's nothing special about this interface, but it also us to define a single
    // convention-based extension method to deserialize and populate link annotations
    public interface IHaveExternalLinks
    {
        IList<Link> Links
        {
            get;
        }
    }
}
