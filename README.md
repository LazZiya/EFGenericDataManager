# EFGenericDataManager
- Generic CRUD operations for EF, C#
- All db operations are followed by `.AsNoTracking()`
- All methods are async

### Install
````
PM > Install-Package LazZiya.EFGEnericDataManager
````

Register in startup:
````cs
services.AddScoped<IEFGenericDataManager, EFGenericDataManager<TContext>>();
````
> `TContext` is the application db context (`DbContext`)

Inject:
````cs
public IndexModel : PageModel
{
    private readonly IEFGenericDataManager DataManager;
    
    public IndexModel(IEFGenericDataManager dataManager)
    {
        DataManager = dataManager;
    }
}
````
### Usage

For the samples we will consider the below entity models for demonstration:
````cs
public class Category : IHasId<int>, IDefault, IActive
{
    public int ID { get; set; }
    public string Name { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<Product> Products { get; set; }
}

public class Product : IHasId<int>, IDefault, IActive
{
    public int ID { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    
    public int CategoryID { get; set; }
    public Category Category { get; set; }
}
````
> For some methods the entity must implement all mentioned interfaces as in `Category` and `Product` models.

#### Get

- get an product by its key
````cs
var product = await DataManager.GetAsync<Product, int>(id);
````

- get an entity by expression
````cs
var product = await DataManager.GetAsync<Product>(x => x.Quantity >= 100);
````

- get a category and include list of products
````cs
var includes = new List<Expression<Func<Category, object>>> { };
includes.Add(x => x.Product);
includes.Add(x => ...); // multiple includes are supported

var category = await DataManager.Get<Category>(x => x.ID == id, includes);
````

#### Add

````cs
var product = new Product { ... }
var success = await DataManager.AddAsync<Product>(product);
````

#### Update

````cs
product.Count = 99;
var success = await DataManager.UpdateAsync<Product, int>(product);
````

#### Delete
````cs
var success = await DataManager.DeleteAsync<Product>(product);
````

#### Count
````cs
var count = await DataManager.CountAsync<Product>(x = x.Count <= 10 );
````

#### Set as default

This method will set the previous default item to false and set the new one to true, and will set IsActive to true as well:
````cs
var success = await DataManager.SetAsDefault<Category, int>(categoryID);
````

#### List items

- The result of ListAsync contains two items `(ICollection<T> itemsList, int totalRecords)` this is useful for paging requests, so we can get list of records and count of records with one shot.
````cs
var searchExpressions = new List<Expression<Func<Product, bool>>> { };
searchExpressions.Add(x => x.Quantity > 0);
searchExpressions.Add(x => x.IsActive == true);
searchExpressions.Add(x => ...); // multiple search expressions are supported

var orderByExpressions = new List<OrderByExpression<Product>> { };
orderExpressions.Add(new OrderByExpression<Product> { Expression = x => x.Name, OrderByDir = OrderByDir.Asc });
orderExpressions.Add(new OrderByExpression<Product> { Expression = x => x.Quantity, OrderByDir = OrderByDir.Desc });
orderExpressions.Add(...); // multiple order by expressions are supported

(var productList, var totalRecords) = await DataManager.ListAsync<Product>(start:1, count:10, searchExpressions, orderByExpressions);
````

- In most cases it is recommended to not return a full set of record properties, we can request only necessary fields to be returned in the items list.

````cs
public class ProductSummary
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string CategoryName { get; set; }
}

var searchExpressions = new List<Expression<Func<Product, bool>>> { };
searchExpressions.Add(x => x.Quantity > 0);
searchExpressions.Add(x => x.IsActive == true);
searchExpressions.Add(x => ...); // multiple search expressions are supported

var orderByExpressions = new List<OrderByExpression<Product>> { };
orderExpressions.Add(new OrderByExpression<Product> { Expression = x => x.Name, OrderByDir = OrderByDir.Asc });
orderExpressions.Add(new OrderByExpression<Product> { Expression = x => x.Quantity, OrderByDir = OrderByDir.Desc });
orderExpressions.Add(...); // multiple order by expressions are supported

var includes = new List<Expression<Func<Category, object>>> { };

Expression<Fun<Product, ProductSummary> selectExpression = x => new ProductSummary { ID = x.ID, Name = x.Name, CategoryName = x.Category.Name };

(var productList, var totalRecords) = await DataManager.ListAsync<Product>(start:1, count:10, 
    searchExpressions, 
    orderByExpressions,
    includes,
    selectExpression);
````
