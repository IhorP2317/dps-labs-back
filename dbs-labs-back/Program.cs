using System.Numerics;
using Carter;
using dbs_labs_back.Middlewares;
using dbs_labs_back.Models;
using dbs_labs_back.Utils;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddCarter();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAny", policyBuilder =>
        {
            policyBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    var app = builder.Build();
    app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseCors("AllowAny");
    app.UseHttpsRedirection();
    app.MapCarter();
    app.Run();