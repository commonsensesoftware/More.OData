namespace More.Integration
{
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
    }
}
