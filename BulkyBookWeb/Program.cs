using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);


// ------ Add services to the container (DI).


//Registering the Identity customized service to the container
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();
//As the default Indentity is not being used the email service has to be registred
builder.Services.AddSingleton<IEmailSender, EmailSender>();

//
builder.Services.AddControllersWithViews();

//Add the DbContext with the string connection registred on appsetings.jason
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefautConnection")));
builder.Services.AddRazorPages();
builder.Services.Configure<FormOptions>(x => x.ValueCountLimit = int.MaxValue);


//DI trought Unity Of Work
//...as the service is added to the scope there is a association between the
//interface and the class declared this way. So when called, the Interface implements
//the class.
builder.Services.AddScoped<IUnityOfWork, UnityOfWork>();

// ------ end of "Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();//wwwroot files

app.UseRouting();
app.UseAuthentication();;

app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
