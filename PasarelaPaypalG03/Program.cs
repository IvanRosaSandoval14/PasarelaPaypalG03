using PasarelaPaypalG03.Services;

var builder = WebApplication.CreateBuilder(args);

// Registrar los servicios
builder.Services.AddHttpClient<PayPalService>(); // Registra el servicio de PayPal
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
      pattern: "{controller=Home}/{action=Index}");
/*pattern: "{controller=Payment}/{action=CreateOrder}/{id?}");*/ // Cambi� la ruta predeterminada para que vaya a PaymentController
app.Run();
