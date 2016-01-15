namespace More.Examples
{
	using System;
    using System.Collections.Generic;

    // note: links are annotations and not directly part of the entity model, use this partial class
    // to attach link annotations to the deserialized client models
    public partial class Device : IHaveExternalLinks
    {
        public DateTimeOffset? LastModified
        {
            get;
            set;
        }

        public string SkuType
        {
            get;
            set;
        }

        public IList<Link> Links
        {
            get;
        } = new List<Link>();
    }
}
