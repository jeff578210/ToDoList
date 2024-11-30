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
       options.ExpireTimeSpan = TimeSpan.FromMinutes(20); //�L���ɶ���20����
       options.SlidingExpiration = true; //�p�G�n�J�����ϥΪ̦�����(�Ҧp�o�e�ШD),�h���s�p��L���ɶ�
       options.LoginPath = "/Home"; //���n�J�۰ʾɦܳo�Ӻ��}
   });
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AuthorizeFilter());//�����ʧ@�����q�L�n�J���Ҥ~��ϥ�
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
