namespace More.Examples.Models
{
    using System;
    using System.Collections.Generic;
    using Web.OData;

    public class Device
    {
        public string SerialNumber
        {
            get;
            set;
        }

        public string PartNumber
        {
            get;
            set;
        }

        public string Sku
        {
            get;
            set;
        }

        public string SkuType
        {
            get;
            set;
        }

        public DateTimeOffset LastModified
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        }

        public Guid? ReceiptId
        {
            get;
            set;
        }

        internal Receipt Receipt
        {
            get;
        }

        public IList<Link> Links
        {
            get;
        } = new List<Link>();
    }
}
