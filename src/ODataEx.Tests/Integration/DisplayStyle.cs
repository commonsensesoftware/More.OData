namespace More.Integration
{
    using System;

    public class DisplayStyle
    {
        public DisplayStyle( bool title, int order )
        {
            Title = title;
            Order = order;
        }

        public bool Title
        {
            get;
            set;
        }

        public int Order
        {
            get;
            set;
        }
    }
}
