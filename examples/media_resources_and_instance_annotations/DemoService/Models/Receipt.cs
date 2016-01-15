namespace More.Examples.Models
{
    using System;

    public class Receipt
    {
        public Guid Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public DateTimeOffset Date
        {
            get;
            set;
        }

        public string ReceiptNumber
        {
            get;
            set;
        }

        public string Retailer
        {
            get;
            set;
        }

        // note: this property is never transmitted over the wire; see the model configuration
        public Image Image
        {
            get;
            set;
        }
    }
}
