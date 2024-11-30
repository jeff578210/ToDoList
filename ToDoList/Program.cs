using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ToDoListContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("ToDoListDatabase")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
   .AddCookie(options =>
   {
       options.ExpireTimeSpan = TimeSpan.FromMinutes(20); //過期時間為20分鐘
       options.SlidingExpiration = true; //如果登入期間使用者有活動(例如發送請求),則重新計算過期時間
       options.LoginPath = "/Home"; //未登入自動導至這個網址
   });
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AuthorizeFilter());//全部動作都須通過登入驗證才能使用
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapDefaultControllerRoute();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
