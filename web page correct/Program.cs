using Microsoft.EntityFrameworkCore;
using web_page_correct.school;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddDbContext<SchoolContext>(options => options.UseMySQL())
    .AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();