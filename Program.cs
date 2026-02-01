using Microsoft.EntityFrameworkCore; 
using BasicDemoApi.Data; // 引入你自己的DbContext命名空间

var builder = WebApplication.CreateBuilder(args);

// 添加控制器
builder.Services.AddControllers();

// 配置Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 关键：注册PgSQL的DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // 读取连接字符串
    var connectionString = builder.Configuration.GetConnectionString("PgSqlConnection");
    options.UseNpgsql(connectionString);
});

// 跨域配置
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 开发环境启用Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseCors("AllowAll"); // 跨域
app.UseAuthorization();

app.MapControllers();

app.Run();