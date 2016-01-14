# More.OData
Provides more extensions for OData v4.0 endpoints using ASP.NET Web API.

This project provides and simplifies the use of advanced OData features.
The current set of features include:

* Media Resources
* Instance Annotations

## Media Resources
Entities that represent media resources are a first-class concept in OData,
but exposing them correctly in Web API requires a lot of developer work
using the out-of-box experience. The extensions provided by this project
allow to you define media resources using the model builder.

### Setup and Configuration

Assume you have the following model:

```C#
public class Receipt
{
    public int ReceiptId { get; set; }
    public string Name { get; set; }
    public string Retailer { get; set; }
    public string ImageType { get; set; }
    public long ImageSize { get; set; }
    public string ImagePath { get; set; }
}
```

We can now define the entity as a media resource within a Web API
configuration as follows:

```C#
public static void Configure( HttpConfiguration configuration )
{
    var builder = new ODataConventionModelBuilder();
    var receipt = builder.EntitySet<Receipt>( "Receipts" ).EntityType;
    
    // specify the property that will provide the content type;
    // this is the minimum requirement
    receipt.MediaType( r => r.ImageType );

    // our model might also include other properties that are
    // required in the service and persisted (ex: EF), but are
    // not part of the model nor returned over the wire
    receipt.Ignore( r => r.ImagePath );
    receipt.Ignore( r => r.ImageSize );

    var model = builder.GetEdmModel();

    // this enables the needed serialization features
    configuration.EnableMediaResources();

    configuration.MapODataServiceRoute( "odata", "api", model );
}
```

### Implementation

Given this model and configuration, we can now implement the service as a
**ReceiptsController**. The following examples assume we have some
*repository* to retrieve receipts and a *file system* to open images.

```C#
// GET ~/receipts(<guid>)/$value
[HttpGet]
[ODataRoute( "({key})/$value" )]
public async Task<IHttpActionResult> GetValue( [FromODataUri] Guid key )
{
    var receipt = await repository.GetSingleAsync( r => r.Id == key );

    if ( receipt == null )
        return NotFound();

    var file = await fileSystem.TryGetFileAsync( receipt.ImagePath );

    if ( file == null )
        return NotFound();

    var stream = await file.OpenReadAsync();

    // this will return a partial stream with status 206 if the client
    // specified the HTTP Range header; otherwise status 200 is returned
    return this.SuccessOrPartialContent( stream, receipt.ImageType );
}
```

Although the **SuccessOrPartialContent** enables easy support for partial
streams, how does a client know the length of the stream? Most REST services
implement this behavior by supporting the HEAD verb. In the following example,
when a client sends the HEAD verb, the server responds with the Content-Type
and Content-Length without any content.

Web API requires content in order to return content-related HTTP headers.
The **OkWithContentHeaders** extension method is a shortcut to return
an **EmptyStream**, which defines the media type and length, but no actual
content.

```C#
// HEAD ~/receipts(<guid>)/$value
[HttpHead]
[ODataRoute( "({key})/$value" )]
public async Task<IHttpActionResult> HeadValue( [FromODataUri] Guid key )
{
    var receipt = await repository.GetSingleAsync( r => r.Id == key );

    if ( receipt == null || !string.IsNullOrEmpty( receipt.ImagePath ) )
        return NotFound();

    return this.OkWithContentHeaders( receipt.ImageSize, receipt.ImageType );
}
```

This example does not demostrate uploading a media resource, but this is easily
achieved using the appropriate HTTP verb and mapping the route to an action.

### Advanced Configuration
The **MediaType** extension method returns a **MediaTypeConfiguration&lt;T&gt;**
object that can be used to further configure the media resource. This
configuration object allows you to define your own link factory methods for the
media resource's read and edit links. Links are read-only by default, but
if you want to make a media resource read-write using the default link factory,
you can use the **IsReadWrite** method. You can also use the configuration
object to define a factory method for media resouce entity tag generation,
which is invoked as each OData entry is created.

## Instance Annotations
OData instance annotations allow you to provide an annotation to virtually any
part of your entity model. While this is a powerful capability and is supported
by the default Web API OData serializers, there is no support to define
annotations in your entity model.

This project provides extensions to define and expose instance annotations for
the following parts of your entity model:

* Entity sets (e.g. OData feed)
* Entities
* Entity properties
* Complex values
* Complex value properties

An instance annotation can be a primitive or complex value. An instance
annotation can also be a sequence of primitive or complex values. The
extensions provided support all of these capabilities.

### Setup and Configuration

Assume you have the following model:

```C#
public class Device
{
    public string SerialNumber { get; set; }
    public string Name { get; set; }
    public string Sku { get; set; }
    public string SkuType { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public IList<Link> Links { get; set; } = new List<Link>();
}
```

We can now define the entity with instance annotations in a Web API
configuration as follows:

```C#
public static void Configure( HttpConfiguration configuration )
{
    var builder = new ODataConventionModelBuilder();
    var device = builder.EntitySet<Device>( "Devices" );
    var device = devices.EntityType;

    // an entity set doesn't have properties we can use to define
    // an expression for. we must use string names in these cases.
    devices.HasAnnotation( "Version", 42.0 );

    device.HasKey( d => d.SerialNumber );

    // define a primitive entity instance annotation
    device.HasAnnotation( d => d.LastModified );

    // define a complex value entity instance annotation
    // note: service authors might do this for HATEOS links that are remote
    // and not represented by navigation properties, functions, or actions
    device.HasComplexAnnotations( d => d.Links );

    // define a primitive instance annotation for an entity property
    device.HasAnnotation( d => SkuType ).ForProperty( d => d.Sku );

    // note: the annotations must be applied to the constructed EDM model
    // this method calls GetEdmModel and then ApplyAnnotations(model)
    var model = builder.GetEdmModelWithAnnotations();

    // this enables the needed serialization features
    configuration.EnableInstanceAnnotations();

    configuration.MapODataServiceRoute( "odata", "api", model );
}
```

Whenever a property is defined as an annotation, it is automatically ignored
by the entity model. Ignored properties are never serialized in results.
Furthermore, annotation content is only returned when a client specifies
the **odata.include-annotations** preference in the HTTP Prefer header that
matches one or more instance annotations.

If the model builder invokes the **EnableLowerCamelCase** method, then
the namespace and name of instance annotations will honor this setting and
will appear appropriately cased in results.

### Advanced Configuration
Each of the **HasAnnotation**, **HasAnnotations**, **HasComplexAnnotation**,
and **HasComplexAnnotations** methods return a configuration object that
can be used to further configure the instance annotation. Typically, the only
information that would be changed is the annotation namespace and name.

The namespace for an annotation defaults to the namespace of the declaring
entity. The name of an annotation defaults to the name of the defining
property expression. Complex value annotations also return the configuration
for the complex type that can be used to configure its attributes; however,
the complex type can still be accessed via the standard **ComplexType&lt;T&gt;**
method of the model builder.

### Limitations
There are a few limitations when using instance annotations:

* The full fidelity of features will not work unless the
  **ODataConventionModelBuilder** is used.
* Instance annotations are not returned when the result of a query is a
  projection (e.g. **$select**). There is currently no way to access the
  annotation value in this scenario.
* Instance annotations sent by the client to the server cannot be accessed. The current
  OData deserializers for Web API do not support client-specified annotations.