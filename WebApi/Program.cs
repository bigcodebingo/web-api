// создается билдер веб приложения

using Dapper;
using WebApi.DAL;

var builder = WebApplication.CreateBuilder(args);

DefaultTypeMap.MatchNamesWithUnderscores = true;
builder.Services.AddScoped<UnitOfWork>();

// зависимость, которая автоматически подхватывает все контроллеры в проекте
builder.Services.AddControllers();
// добавляем swagger
builder.Services.AddSwaggerGen();

// собираем билдер в приложение
var app = builder.Build();

// добавляем 2 миддлвари для обработки запросов в сваггер
app.UseSwagger();
app.UseSwaggerUI();

// добавляем миддлварю для роутинга в нужный контроллер
app.MapControllers();

// вместо *** должен быть путь к проекту Migrations
// по сути в этот момент будет происходить накатка миграций на базу
Migrations.Program.Main(Array.Empty<string>());

// код до вызова builder.Services.AddControllers():
builder.Services.Configure<DbSettings>(builder.Configuration.GetSection(nameof(DbSettings)));

// запускам приложение
app.Run();