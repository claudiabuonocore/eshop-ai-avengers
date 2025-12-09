using eShop.WebApp.Components;
using eShop.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.AddApplicationServices();

// Reviews API typed client
var reviewsApiUrl = builder.Configuration["ReviewsApiUrl"]
    ?? Environment.GetEnvironmentVariable("REVIEWS_API_URL")
    ?? "http://reviews-api";
builder.Services.AddHttpClient<eShop.WebApp.Services.ReviewService>(client =>
{
    client.BaseAddress = new Uri(reviewsApiUrl);
});
builder.Services.AddScoped<eShop.WebApp.Services.IReviewService, eShop.WebApp.Services.ReviewService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAntiforgery();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.MapForwarder("/product-images/{id}", "https+http://catalog-api", "/api/catalog/items/{id}/pic");

app.Run();
