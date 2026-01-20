using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BasicsDemoApi.Data;
using BasicsDemoApi.Models;

[Route("api/[controller]")]
[ApiController]
public class LearningController: ControllerBase
{
    // 注入DbContext
    private readonly AppDbContext _dbContext;

    // 构造函数注入
    public LearningController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // 1. GET: api/learning/user?id=xxx（从PgSQL查询用户）
    [HttpGet("user")]
    public async Task<IActionResult> GetUserById([FromQuery] int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                code = 400,
                message = "用户ID不能为空且必须为正整数",
                data = (object?)null
            });
        }

        // 从数据库查询用户
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return Ok(new
            {
                code = 404,
                message = "用户不存在",
                data = (object?)null
            });
        }

        return Ok(new
        {
            code = 200,
            message = "获取用户信息成功",
            data = user
        });
    }

    // 2. 新增：POST api/learning/user（添加用户到PgSQL）
    [HttpPost("user")]
    public async Task<IActionResult> AddUser([FromBody] UserAddRequest request)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            return BadRequest(new
            {
                code = 400,
                message = "用户名不能为空",
                data = (object?)null
            });
        }

        // 创建用户实体
        var newUser = new User
        {
            Name = request.Name,
            Age = request.Age,
            CreateTime = DateTime.Now
        };

        // 添加到数据库
        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync(); // 保存更改

        return Ok(new
        {
            code = 200,
            message = "用户添加成功",
            data = newUser
        });
    }

}

// 新增：添加用户的请求模型
public class UserAddRequest
{
    public string? Name { get; set; }
    public int? Age { get; set; }
}

// LoginRequest保留不变
public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}