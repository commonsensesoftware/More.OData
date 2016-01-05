namespace More.Integration
{
    using System;
    using System.Collections.Generic;
    using Web.OData;

    public class Person
    {
        public int Id
        {
            get;
            set;
        }

        public string FirstName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public byte[] PhotoImage
        {
            get;
            set;
        }

        public string PhotoImageType
        {
            get;
            set;
        }

        public IList<Link> Links
        {
            get;
        } = new List<Link>();

        public IList<string> SeoTerms
        {
            get;
        } = new List<string>();

        public DateTimeOffset Timestamp
        {
            get;
            set;
        }

        public int? Flags
        {
            get;
            set;
        }
    }
}
