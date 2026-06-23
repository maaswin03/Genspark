using LibraryApi.Contexts;
using LibraryApi.Interfaces;
using LibraryApi.Models;
using LibraryApi.Repositories;
using LibraryApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

#region Contexts
builder.Services.AddDbContext<LibraryDbContext>(Options =>
{
    Options.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});
#endregion

#region Repository
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IRepository<int, Member>, MemberRepository>();
#endregion

#region Service
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IMemberService, MemberService>();
#endregion

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseSwagger();

app.MapControllers();

app.Run();

