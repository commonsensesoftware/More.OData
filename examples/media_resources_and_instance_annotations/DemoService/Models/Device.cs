namespace More.Examples.Models
{
    using System;

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
    }
}
