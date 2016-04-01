﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generation date: 1/14/2016 9:58:39 PM
namespace More.Examples
{
    /// <summary>
    /// There are no comments for Container in the schema.
    /// </summary>
    [global::Microsoft.OData.Client.OriginalNameAttribute("Container")]
    public partial class Container : global::Microsoft.OData.Client.DataServiceContext
    {
        /// <summary>
        /// Initialize a new Container object.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        public Container(global::System.Uri serviceRoot) : 
                base(serviceRoot, global::Microsoft.OData.Client.ODataProtocolVersion.V4)
        {
            this.ResolveName = new global::System.Func<global::System.Type, string>(this.ResolveNameFromType);
            this.ResolveType = new global::System.Func<string, global::System.Type>(this.ResolveTypeFromName);
            this.OnContextCreated();
            this.Format.LoadServiceModel = GeneratedEdmModel.GetInstance;
            this.Format.UseJson();
        }
        partial void OnContextCreated();
        /// <summary>
        /// Since the namespace configured for this service reference
        /// in Visual Studio is different from the one indicated in the
        /// server schema, use type-mappers to map between the two.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        protected global::System.Type ResolveTypeFromName(string typeName)
        {
            global::System.Type resolvedType = this.DefaultResolveType(typeName, "Examples", "More.Examples");
            if ((resolvedType != null))
            {
                return resolvedType;
            }
            resolvedType = this.DefaultResolveType(typeName, "external", "More.External");
            if ((resolvedType != null))
            {
                return resolvedType;
            }
            resolvedType = this.DefaultResolveType(typeName, "my", "More.My");
            if ((resolvedType != null))
            {
                return resolvedType;
            }
            return null;
        }
        /// <summary>
        /// Since the namespace configured for this service reference
        /// in Visual Studio is different from the one indicated in the
        /// server schema, use type-mappers to map between the two.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        protected string ResolveNameFromType(global::System.Type clientType)
        {
            global::Microsoft.OData.Client.OriginalNameAttribute originalNameAttribute = (global::Microsoft.OData.Client.OriginalNameAttribute)global::System.Linq.Enumerable.SingleOrDefault(global::Microsoft.OData.Client.Utility.GetCustomAttributes(clientType, typeof(global::Microsoft.OData.Client.OriginalNameAttribute), true));
            if (clientType.Namespace.Equals("More.Examples", global::System.StringComparison.Ordinal))
            {
                if (originalNameAttribute != null)
                {
                    return string.Concat("Examples.", originalNameAttribute.OriginalName);
                }
                return string.Concat("Examples.", clientType.Name);
            }
            if (clientType.Namespace.Equals("More.External", global::System.StringComparison.Ordinal))
            {
                if (originalNameAttribute != null)
                {
                    return string.Concat("external.", originalNameAttribute.OriginalName);
                }
                return string.Concat("external.", clientType.Name);
            }
            if (clientType.Namespace.Equals("More.My", global::System.StringComparison.Ordinal))
            {
                if (originalNameAttribute != null)
                {
                    return string.Concat("my.", originalNameAttribute.OriginalName);
                }
                return string.Concat("my.", clientType.Name);
            }
            return null;
        }
        /// <summary>
        /// There are no comments for Devices in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("Devices")]
        public global::Microsoft.OData.Client.DataServiceQuery<Device> Devices
        {
            get
            {
                if ((this._Devices == null))
                {
                    this._Devices = base.CreateQuery<Device>("Devices");
                }
                return this._Devices;
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private global::Microsoft.OData.Client.DataServiceQuery<Device> _Devices;
        /// <summary>
        /// There are no comments for Receipts in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("Receipts")]
        public global::Microsoft.OData.Client.DataServiceQuery<Receipt> Receipts
        {
            get
            {
                if ((this._Receipts == null))
                {
                    this._Receipts = base.CreateQuery<Receipt>("Receipts");
                }
                return this._Receipts;
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private global::Microsoft.OData.Client.DataServiceQuery<Receipt> _Receipts;
        /// <summary>
        /// There are no comments for Devices in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        public void AddToDevices(Device device)
        {
            base.AddObject("Devices", device);
        }
        /// <summary>
        /// There are no comments for Receipts in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        public void AddToReceipts(Receipt receipt)
        {
            base.AddObject("Receipts", receipt);
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private abstract class GeneratedEdmModel
        {
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
            private static global::Microsoft.OData.Edm.IEdmModel ParsedModel = LoadModelFromString();
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
            private const string Edmx = @"<edmx:Edmx Version=""4.0"" xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"">
  <edmx:DataServices>
    <Schema Namespace=""Examples"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <EntityType Name=""Device"">
        <Key>
          <PropertyRef Name=""serialNumber"" />
        </Key>
        <Property Name=""serialNumber"" Type=""Edm.String"" Nullable=""false"" />
        <Property Name=""sku"" Type=""Edm.String"" />
        <Property Name=""partNumber"" Type=""Edm.String"" />
        <Property Name=""isActive"" Type=""Edm.Boolean"" Nullable=""false"" />
        <NavigationProperty Name=""receipt"" Type=""Examples.Receipt"" />
      </EntityType>
      <ComplexType Name=""Link"">
        <Property Name=""name"" Type=""Edm.String"" />
        <Property Name=""url"" Type=""Edm.String"" />
      </ComplexType>
      <EntityType Name=""Receipt"" HasStream=""true"">
        <Key>
          <PropertyRef Name=""id"" />
        </Key>
        <Property Name=""id"" Type=""Edm.Guid"" Nullable=""false"" />
        <Property Name=""name"" Type=""Edm.String"" />
        <Property Name=""date"" Type=""Edm.DateTimeOffset"" Nullable=""false"" />
        <Property Name=""receiptNumber"" Type=""Edm.String"" />
        <Property Name=""retailer"" Type=""Edm.String"" />
      </EntityType>
      <EntityContainer Name=""Container"">
        <EntitySet Name=""Devices"" EntityType=""Examples.Device"">
          <NavigationPropertyBinding Path=""receipt"" Target=""Receipts"" />
        </EntitySet>
        <EntitySet Name=""Receipts"" EntityType=""Examples.Receipt"" />
      </EntityContainer>
    </Schema>
    <Schema Namespace=""my"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <Term Name=""lastModified"" Type=""Edm.DateTimeOffset"" AppliesTo=""EntityType ComplexType"" Nullable=""false"" />
      <Term Name=""skuType"" Type=""Edm.String"" AppliesTo=""Property"" Unicode=""false"" />
    </Schema>
    <Schema Namespace=""external"" xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
      <Term Name=""links"" Type=""Collection(Examples.Link)"" AppliesTo=""EntityType ComplexType"" />
    </Schema>
  </edmx:DataServices>
</edmx:Edmx>";
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
            public static global::Microsoft.OData.Edm.IEdmModel GetInstance()
            {
                return ParsedModel;
            }
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
            private static global::Microsoft.OData.Edm.IEdmModel LoadModelFromString()
            {
                global::System.Xml.XmlReader reader = CreateXmlReader(Edmx);
                try
                {
                    return global::Microsoft.OData.Edm.Csdl.EdmxReader.Parse(reader);
                }
                finally
                {
                    ((global::System.IDisposable)(reader)).Dispose();
                }
            }
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
            private static global::System.Xml.XmlReader CreateXmlReader(string edmxToParse)
            {
                return global::System.Xml.XmlReader.Create(new global::System.IO.StringReader(edmxToParse));
            }
        }
    }
    /// <summary>
    /// There are no comments for DeviceSingle in the schema.
    /// </summary>
    [global::Microsoft.OData.Client.OriginalNameAttribute("DeviceSingle")]
    public partial class DeviceSingle : global::Microsoft.OData.Client.DataServiceQuerySingle<Device>
    {
        /// <summary>
        /// Initialize a new DeviceSingle object.
        /// </summary>
        public DeviceSingle(global::Microsoft.OData.Client.DataServiceContext context, string path)
            : base(context, path) {}

        /// <summary>
        /// Initialize a new DeviceSingle object.
        /// </summary>
        public DeviceSingle(global::Microsoft.OData.Client.DataServiceContext context, string path, bool isComposable)
            : base(context, path, isComposable) {}

        /// <summary>
        /// Initialize a new DeviceSingle object.
        /// </summary>
        public DeviceSingle(global::Microsoft.OData.Client.DataServiceQuerySingle<Device> query)
            : base(query) {}

        /// <summary>
        /// There are no comments for Receipt in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("receipt")]
        public global::More.Examples.ReceiptSingle Receipt
        {
            get
            {
                if (!this.IsComposable)
                {
                    throw new global::System.NotSupportedException("The previous function is not composable.");
                }
                if ((this._Receipt == null))
                {
                    this._Receipt = new global::More.Examples.ReceiptSingle(this.Context, GetPath("receipt"));
                }
                return this._Receipt;
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private global::More.Examples.ReceiptSingle _Receipt;
    }
    /// <summary>
    /// There are no comments for Device in the schema.
    /// </summary>
    /// <KeyProperties>
    /// SerialNumber
    /// </KeyProperties>
    [global::Microsoft.OData.Client.Key("serialNumber")]
    [global::Microsoft.OData.Client.EntitySet("Devices")]
    [global::Microsoft.OData.Client.OriginalNameAttribute("Device")]
    public partial class Device : global::Microsoft.OData.Client.BaseEntityType, global::System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Create a new Device object.
        /// </summary>
        /// <param name="serialNumber">Initial value of SerialNumber.</param>
        /// <param name="isActive">Initial value of IsActive.</param>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        public static Device CreateDevice(string serialNumber, bool isActive)
        {
            Device device = new Device();
            device.SerialNumber = serialNumber;
            device.IsActive = isActive;
            return device;
        }
        /// <summary>
        /// There are no comments for Property SerialNumber in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("serialNumber")]
        public string SerialNumber
        {
            get
            {
                return this._SerialNumber;
            }
            set
            {
                this.OnSerialNumberChanging(value);
                this._SerialNumber = value;
                this.OnSerialNumberChanged();
                this.OnPropertyChanged("serialNumber");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private string _SerialNumber;
        partial void OnSerialNumberChanging(string value);
        partial void OnSerialNumberChanged();
        /// <summary>
        /// There are no comments for Property Sku in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("sku")]
        public string Sku
        {
            get
            {
                return this._Sku;
            }
            set
            {
                this.OnSkuChanging(value);
                this._Sku = value;
                this.OnSkuChanged();
                this.OnPropertyChanged("sku");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private string _Sku;
        partial void OnSkuChanging(string value);
        partial void OnSkuChanged();
        /// <summary>
        /// There are no comments for Property PartNumber in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("partNumber")]
        public string PartNumber
        {
            get
            {
                return this._PartNumber;
            }
            set
            {
                this.OnPartNumberChanging(value);
                this._PartNumber = value;
                this.OnPartNumberChanged();
                this.OnPropertyChanged("partNumber");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private string _PartNumber;
        partial void OnPartNumberChanging(string value);
        partial void OnPartNumberChanged();
        /// <summary>
        /// There are no comments for Property IsActive in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("isActive")]
        public bool IsActive
        {
            get
            {
                return this._IsActive;
            }
            set
            {
                this.OnIsActiveChanging(value);
                this._IsActive = value;
                this.OnIsActiveChanged();
                this.OnPropertyChanged("isActive");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private bool _IsActive;
        partial void OnIsActiveChanging(bool value);
        partial void OnIsActiveChanged();
        /// <summary>
        /// There are no comments for Property Receipt in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("receipt")]
        public global::More.Examples.Receipt Receipt
        {
            get
            {
                return this._Receipt;
            }
            set
            {
                this.OnReceiptChanging(value);
                this._Receipt = value;
                this.OnReceiptChanged();
                this.OnPropertyChanged("receipt");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private global::More.Examples.Receipt _Receipt;
        partial void OnReceiptChanging(global::More.Examples.Receipt value);
        partial void OnReceiptChanged();
        /// <summary>
        /// This event is raised when the value of the property is changed
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// The value of the property is changed
        /// </summary>
        /// <param name="property">property name</param>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        protected virtual void OnPropertyChanged(string property)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new global::System.ComponentModel.PropertyChangedEventArgs(property));
            }
        }
    }
    /// <summary>
    /// There are no comments for Link in the schema.
    /// </summary>
    [global::Microsoft.OData.Client.OriginalNameAttribute("Link")]
    public partial class Link : global::System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// There are no comments for Property Name in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("name")]
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this.OnNameChanging(value);
                this._Name = value;
                this.OnNameChanged();
                this.OnPropertyChanged("name");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private string _Name;
        partial void OnNameChanging(string value);
        partial void OnNameChanged();
        /// <summary>
        /// There are no comments for Property Url in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("url")]
        public string Url
        {
            get
            {
                return this._Url;
            }
            set
            {
                this.OnUrlChanging(value);
                this._Url = value;
                this.OnUrlChanged();
                this.OnPropertyChanged("url");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private string _Url;
        partial void OnUrlChanging(string value);
        partial void OnUrlChanged();
        /// <summary>
        /// This event is raised when the value of the property is changed
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// The value of the property is changed
        /// </summary>
        /// <param name="property">property name</param>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        protected virtual void OnPropertyChanged(string property)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new global::System.ComponentModel.PropertyChangedEventArgs(property));
            }
        }
    }
    /// <summary>
    /// There are no comments for ReceiptSingle in the schema.
    /// </summary>
    [global::Microsoft.OData.Client.OriginalNameAttribute("ReceiptSingle")]
    public partial class ReceiptSingle : global::Microsoft.OData.Client.DataServiceQuerySingle<Receipt>
    {
        /// <summary>
        /// Initialize a new ReceiptSingle object.
        /// </summary>
        public ReceiptSingle(global::Microsoft.OData.Client.DataServiceContext context, string path)
            : base(context, path) {}

        /// <summary>
        /// Initialize a new ReceiptSingle object.
        /// </summary>
        public ReceiptSingle(global::Microsoft.OData.Client.DataServiceContext context, string path, bool isComposable)
            : base(context, path, isComposable) {}

        /// <summary>
        /// Initialize a new ReceiptSingle object.
        /// </summary>
        public ReceiptSingle(global::Microsoft.OData.Client.DataServiceQuerySingle<Receipt> query)
            : base(query) {}

    }
    /// <summary>
    /// There are no comments for Receipt in the schema.
    /// </summary>
    /// <KeyProperties>
    /// Id
    /// </KeyProperties>
    [global::Microsoft.OData.Client.Key("id")]
    [global::Microsoft.OData.Client.EntitySet("Receipts")]
    [global::Microsoft.OData.Client.HasStream()]
    [global::Microsoft.OData.Client.OriginalNameAttribute("Receipt")]
    public partial class Receipt : global::Microsoft.OData.Client.BaseEntityType, global::System.ComponentModel.INotifyPropertyChanged
    {
        /// <summary>
        /// Create a new Receipt object.
        /// </summary>
        /// <param name="ID">Initial value of Id.</param>
        /// <param name="date">Initial value of Date.</param>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        public static Receipt CreateReceipt(global::System.Guid ID, global::System.DateTimeOffset date)
        {
            Receipt receipt = new Receipt();
            receipt.Id = ID;
            receipt.Date = date;
            return receipt;
        }
        /// <summary>
        /// There are no comments for Property Id in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("id")]
        public global::System.Guid Id
        {
            get
            {
                return this._Id;
            }
            set
            {
                this.OnIdChanging(value);
                this._Id = value;
                this.OnIdChanged();
                this.OnPropertyChanged("id");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private global::System.Guid _Id;
        partial void OnIdChanging(global::System.Guid value);
        partial void OnIdChanged();
        /// <summary>
        /// There are no comments for Property Name in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("name")]
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this.OnNameChanging(value);
                this._Name = value;
                this.OnNameChanged();
                this.OnPropertyChanged("name");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private string _Name;
        partial void OnNameChanging(string value);
        partial void OnNameChanged();
        /// <summary>
        /// There are no comments for Property Date in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("date")]
        public global::System.DateTimeOffset Date
        {
            get
            {
                return this._Date;
            }
            set
            {
                this.OnDateChanging(value);
                this._Date = value;
                this.OnDateChanged();
                this.OnPropertyChanged("date");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private global::System.DateTimeOffset _Date;
        partial void OnDateChanging(global::System.DateTimeOffset value);
        partial void OnDateChanged();
        /// <summary>
        /// There are no comments for Property ReceiptNumber in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("receiptNumber")]
        public string ReceiptNumber
        {
            get
            {
                return this._ReceiptNumber;
            }
            set
            {
                this.OnReceiptNumberChanging(value);
                this._ReceiptNumber = value;
                this.OnReceiptNumberChanged();
                this.OnPropertyChanged("receiptNumber");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private string _ReceiptNumber;
        partial void OnReceiptNumberChanging(string value);
        partial void OnReceiptNumberChanged();
        /// <summary>
        /// There are no comments for Property Retailer in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("retailer")]
        public string Retailer
        {
            get
            {
                return this._Retailer;
            }
            set
            {
                this.OnRetailerChanging(value);
                this._Retailer = value;
                this.OnRetailerChanged();
                this.OnPropertyChanged("retailer");
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        private string _Retailer;
        partial void OnRetailerChanging(string value);
        partial void OnRetailerChanged();
        /// <summary>
        /// This event is raised when the value of the property is changed
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        public event global::System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// The value of the property is changed
        /// </summary>
        /// <param name="property">property name</param>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "2.4.0")]
        protected virtual void OnPropertyChanged(string property)
        {
            if ((this.PropertyChanged != null))
            {
                this.PropertyChanged(this, new global::System.ComponentModel.PropertyChangedEventArgs(property));
            }
        }
    }
    /// <summary>
    /// Class containing all extension methods
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Get an entity of type global::More.Examples.Device as global::More.Examples.DeviceSingle specified by key from an entity set
        /// </summary>
        /// <param name="source">source entity set</param>
        /// <param name="keys">dictionary with the names and values of keys</param>
        public static global::More.Examples.DeviceSingle ByKey(this global::Microsoft.OData.Client.DataServiceQuery<global::More.Examples.Device> source, global::System.Collections.Generic.Dictionary<string, object> keys)
        {
            return new global::More.Examples.DeviceSingle(source.Context, source.GetKeyPath(global::Microsoft.OData.Client.Serializer.GetKeyString(source.Context, keys)));
        }
        /// <summary>
        /// Get an entity of type global::More.Examples.Device as global::More.Examples.DeviceSingle specified by key from an entity set
        /// </summary>
        /// <param name="source">source entity set</param>
        /// <param name="serialNumber">The value of serialNumber</param>
        public static global::More.Examples.DeviceSingle ByKey(this global::Microsoft.OData.Client.DataServiceQuery<global::More.Examples.Device> source,
            string serialNumber)
        {
            global::System.Collections.Generic.Dictionary<string, object> keys = new global::System.Collections.Generic.Dictionary<string, object>
            {
                { "serialNumber", serialNumber }
            };
            return new global::More.Examples.DeviceSingle(source.Context, source.GetKeyPath(global::Microsoft.OData.Client.Serializer.GetKeyString(source.Context, keys)));
        }
        /// <summary>
        /// Get an entity of type global::More.Examples.Receipt as global::More.Examples.ReceiptSingle specified by key from an entity set
        /// </summary>
        /// <param name="source">source entity set</param>
        /// <param name="keys">dictionary with the names and values of keys</param>
        public static global::More.Examples.ReceiptSingle ByKey(this global::Microsoft.OData.Client.DataServiceQuery<global::More.Examples.Receipt> source, global::System.Collections.Generic.Dictionary<string, object> keys)
        {
            return new global::More.Examples.ReceiptSingle(source.Context, source.GetKeyPath(global::Microsoft.OData.Client.Serializer.GetKeyString(source.Context, keys)));
        }
        /// <summary>
        /// Get an entity of type global::More.Examples.Receipt as global::More.Examples.ReceiptSingle specified by key from an entity set
        /// </summary>
        /// <param name="source">source entity set</param>
        /// <param name="id">The value of id</param>
        public static global::More.Examples.ReceiptSingle ByKey(this global::Microsoft.OData.Client.DataServiceQuery<global::More.Examples.Receipt> source,
            global::System.Guid id)
        {
            global::System.Collections.Generic.Dictionary<string, object> keys = new global::System.Collections.Generic.Dictionary<string, object>
            {
                { "id", id }
            };
            return new global::More.Examples.ReceiptSingle(source.Context, source.GetKeyPath(global::Microsoft.OData.Client.Serializer.GetKeyString(source.Context, keys)));
        }
    }
}
namespace More.My
{
}
namespace More.External
{
}
