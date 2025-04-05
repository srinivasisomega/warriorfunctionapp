using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tempfunctionapp.repository;
using tempfunctionapp.service;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton<IWarriorRepository, WarriorRepository>();
builder.Services.AddSingleton<IExcelService, ExcelService>();

builder.Build().Run();
