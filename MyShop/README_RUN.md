# ?? MyShop Project - Running Guide (API + Desktop App)

## ?? Prerequisites
- .NET 8 SDK
- Visual Studio 2022 (or VS Code)
- Windows 10/11 (for WinUI 3 app)

---

## ?? How to Run the Project

### **Option 1: Run from Visual Studio 2022**

#### Step 1: Start the API (MyShopAPI)
1. Right-click on the **MyShopAPI** project in Solution Explorer
2. Select **"Set as Startup Project"**
3. Press **F5** or click the green **Start** button
4. The API will run at:
   - HTTP: `http://localhost:5002`
   - HTTPS: `https://localhost:7024`
5. Swagger UI will open at: `http://localhost:5002/swagger`

#### Step 2: Start the Desktop App (MyShop)
1. **Keep the API running** (IMPORTANT!)
2. Right-click on the **MyShop** project in Solution Explorer
3. Select **"Set as Startup Project"**
4. Press **Ctrl+F5** to run without debugging (or F5 to debug)
5. The desktop application will launch

#### Step 3: Connect and Test
1. Click the **"?? Load Products from API"** button
2. If successful, you should see 5 products displayed
3. If there's an error, verify that the API is running

---

### **Option 2: Run from Terminal/Command Line**

#### Start the API
```bash
cd "L:\3-HK2\Win\final project\MyShopAPI"
dotnet run
```
**Result:** API runs at `http://localhost:5002`

#### Start the Desktop App (in a new terminal)
```bash
cd "L:\3-HK2\Win\final project\MyShop"
dotnet run
```

---

### **Option 3: Run Both Projects Simultaneously (Multiple Startup Projects)**

1. Right-click on the **Solution** in Solution Explorer
2. Select **"Configure Startup Projects..."**
3. Choose **"Multiple startup projects"**
4. Set:
   - **MyShopAPI**: Action = **Start**
   - **MyShop**: Action = **Start**
5. Click **OK**
6. Press **F5** - Both projects will start together!

---

## ?? API Endpoints

### Products API
- **GET** `/api/products` - Get all products
- **GET** `/api/products/{id}` - Get product by ID
- **GET** `/api/products/category/{category}` - Get products by category
- **POST** `/api/products` - Create a new product
- **PUT** `/api/products/{id}` - Update a product
- **DELETE** `/api/products/{id}` - Delete a product

### Test API in Browser
```
http://localhost:5002/api/products
```

### Test API with Swagger
```
http://localhost:5002/swagger
```

---

## ??? Architecture & Connection Flow

```
???????????????????????????????????????????????
?   MyShop Desktop App (WinUI 3)              ?
?   ???????????????????????????????????????   ?
?   ? MainWindow.xaml (View)              ?   ?
?   ?      ?                              ?   ?
?   ? MainViewModel (ViewModel)           ?   ?
?   ?      ?                              ?   ?
?   ? DataService (Service)               ?   ?
?   ?      ?                              ?   ?
?   ? HttpClient                          ?   ?
?   ???????????????????????????????????????   ?
???????????????????????????????????????????????
         ?
         ? HTTP GET http://localhost:5002/api/products
         ?
???????????????????????????????????????????????
?   MyShopAPI (ASP.NET Core Web API)          ?
?   ???????????????????????????????????????   ?
?   ? Program.cs                          ?   ?
?   ?   • CORS Configuration              ?   ?
?   ?   • Request Logging Middleware      ?   ?
?   ?      ?                              ?   ?
?   ? ProductsController                  ?   ?
?   ?   • GET /api/products               ?   ?
?   ?   • CRUD operations                 ?   ?
?   ?      ?                              ?   ?
?   ? Product Model                       ?   ?
?   ? In-memory List<Product>             ?   ?
?   ???????????????????????????????????????   ?
?                                             ?
?   Returns JSON Response                     ?
???????????????????????????????????????????????
```

---

## ?? Configuration Details

### API Configuration (MyShopAPI)
**File:** `Properties/launchSettings.json`
```json
"applicationUrl": "http://localhost:5002;https://localhost:7024"
```

**CORS Policy:** Configured in `Program.cs`
- Allows connections from any origin (suitable for development)
- Required for WPF/WinUI desktop apps to connect to localhost API

### Desktop App Configuration (MyShop)
**File:** `Services/DataService.cs`
```csharp
private const string BaseUrl = "http://localhost:5002";
```

**API Endpoint Construction:**
- Base URL: `http://localhost:5002`
- API Path: `/api/products`
- Full URL: `http://localhost:5002/api/products` ?

---

## ?? Common Errors & Solutions

### ? Error: "Unable to connect to API"
**Cause:** API is not running or running on wrong port

**Solutions:**
1. Verify the API is running
2. Check the port in `DataService.cs`:
   ```csharp
   private const string BaseUrl = "http://localhost:5002";
   ```
3. Check the port in `MyShopAPI/Properties/launchSettings.json`
4. Check firewall settings

### ? Error: "Failed to load products" with no server logs
**Cause:** CORS not configured or URL path incorrect

**Solutions:**
1. Verify CORS is configured in `Program.cs`:
   ```csharp
   builder.Services.AddCors(options => { ... });
   app.UseCors("AllowAll");
   ```
2. Check API endpoint URL construction:
   - Should be: `http://localhost:5002/api/products` ?
   - NOT: `http://localhost:5002/api/api/products` ?

### ? Error: "This type of CollectionView does not support changes"
**Cause:** ObservableCollection threading issue

**Solution:** Already handled in code (Clear and Add items individually)

### ? Build Errors
**Solutions:**
```bash
# Clean solution
dotnet clean

# Restore packages
dotnet restore

# Rebuild
dotnet build
```

---

## ?? Project Dependencies

### MyShopAPI
- `Microsoft.AspNetCore.OpenApi` - OpenAPI/Swagger support
- `Swashbuckle.AspNetCore` - Swagger UI

### MyShop (Desktop App)
- `Microsoft.WindowsAppSDK` (1.6.241114003) - WinUI 3 framework
- `CommunityToolkit.Mvvm` (8.3.2) - MVVM helpers
- `Microsoft.Extensions.DependencyInjection` (9.0.0) - DI container
- `System.Net.Http.Json` - Built-in .NET 8 HTTP JSON extensions

---

## ?? Complete Data Flow

1. **User Action**: Click "Load Products from API" button
2. **UI ? ViewModel**: `LoadProductsCommand` is executed
3. **ViewModel ? Service**: Calls `DataService.GetProductsAsync()`
4. **Service ? API**: 
   - Creates HTTP GET request
   - URL: `http://localhost:5002/api/products`
5. **API Processing**:
   - CORS validation (allows request)
   - Request logged in console
   - `ProductsController.GetAll()` executed
   - Returns JSON array of products
6. **Response Processing**:
   - JSON deserialized to `List<Product>`
   - Response status logged
7. **Service ? ViewModel**: Returns product list
8. **ViewModel ? UI**: 
   - Clears `ObservableCollection<Product>`
   - Adds items one by one
9. **UI Rendering**: GridView displays products with data binding

---

## ? Features Implemented

### Desktop App Features:
- ? Load products from REST API
- ? Display products in responsive grid layout
- ? Loading indicator with progress ring
- ? Error handling with InfoBar notifications
- ? Mica backdrop (Windows 11 modern style)
- ? MVVM pattern with Dependency Injection
- ? Async/await data operations
- ? ObservableCollection for reactive UI

### API Features:
- ? RESTful API design
- ? In-memory data storage (5 sample products)
- ? Swagger/OpenAPI documentation
- ? Full CRUD operations support
- ? Structured logging with ILogger
- ? CORS enabled for cross-origin requests
- ? Request/Response logging middleware
- ? Exception handling

---

## ?? Testing the Application

### Manual Testing Steps:
1. **Start API** and verify Swagger UI works
2. **Test in Browser**: Navigate to `http://localhost:5002/api/products`
3. **Expected Response**:
   ```json
   [
     {
       "id": 1,
       "name": "Laptop Dell XPS 13",
       "price": 1299.99,
       "stock": 15,
       "category": "Electronics",
       ...
     },
     ...
   ]
   ```
4. **Start Desktop App** and click "Load Products from API"
5. **Verify** 5 products are displayed in the grid
6. **Check API Console** for request logs:
   ```
   info: MyShopAPI.Program[0]
         Incoming request: GET /api/products
   info: MyShopAPI.Controllers.ProductsController[0]
         Getting all products
   info: MyShopAPI.Program[0]
         Response status: 200
   ```

### Testing API Endpoints with Swagger:
1. Navigate to `http://localhost:5002/swagger`
2. Expand `GET /api/products`
3. Click "Try it out" ? "Execute"
4. Verify 200 OK response with product data

---

## ?? Next Steps (Potential Extensions)

### Short-term Enhancements:
1. **Database Integration**: Replace in-memory data with SQL Server/SQLite
2. **Product CRUD UI**: Add Create/Update/Delete functionality in desktop app
3. **Search & Filter**: Add search box and category filters
4. **Image Upload**: Support product image uploads

### Medium-term Features:
5. **Authentication**: Implement JWT authentication
6. **User Roles**: Add admin/customer role management
7. **Shopping Cart**: Implement cart functionality
8. **Responsive Design**: Add adaptive layouts for different window sizes

### Long-term Goals:
9. **Order Management**: Full order processing system
10. **Payment Integration**: Add payment gateway support
11. **Reports & Analytics**: Sales reports and dashboards
12. **Multi-platform**: Port to MAUI for cross-platform support

---

## ?? Important Notes

- **API Ports**: 
  - HTTP: `5002`
  - HTTPS: `7024`
- **Startup Order**: API must be running before launching the desktop app
- **Sample Data**: 5 pre-loaded products (Laptop, iPhone, Headphones, Watch, iPad)
- **CORS**: Enabled in API to allow desktop app connections
- **Logging**: Request/response logging enabled for debugging
- **Threading**: UI updates handled correctly with ObservableCollection
- **Error Handling**: Comprehensive try-catch blocks with user-friendly messages

---

## ?? Learning Resources

- [WinUI 3 Documentation](https://docs.microsoft.com/windows/apps/winui/winui3/)
- [ASP.NET Core Web API](https://docs.microsoft.com/aspnet/core/web-api/)
- [MVVM Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)
- [Dependency Injection in .NET](https://docs.microsoft.com/dotnet/core/extensions/dependency-injection)

---

## ?? Contributing

This is a learning project demonstrating:
- Modern .NET 8 development
- WinUI 3 desktop applications
- ASP.NET Core Web API
- MVVM architectural pattern
- Dependency Injection
- RESTful API consumption

---

**Happy Coding! ??**

---

*Last Updated: 2024 - .NET 8 with WinUI 3*
