using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BasicDemoApi.Data;
using BasicDemoApi.Models;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    // 注入DbContext
    private readonly AppDbContext _dbContext;

    // 构造函数注入DbContext
    public UserController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // 1. 查询: api/learning/user?id=xxx（从PgSQL查询用户）
    [HttpGet("getUserInfo")]
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
    [HttpPost("addUser")]
    public async Task<IActionResult> AddUser([FromBody] UserAddRequest request)
    {
        if (request == null)
        {
            return BadRequest(new
            {
                code = 400,
                message = "请求参数不能为空",
                data = (object?)null
            });
        }

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
            Gender = request.Gender,
            Phone = request.Phone,
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

    //统计用户总数
    [HttpGet("userCount/total")]
    public async Task<IActionResult> GetTotalUserCount()
    {
        var total = await _dbContext.Users.CountAsync();
        return Ok(new
        {
            code = 200,
            message = "统计用户总数成功",
            data = new { totalUserCount = total }
        });
    }

    [HttpGet("userCount/gender")]
    public async Task<IActionResult> GetCountByGender()
    {
        var genderCount = _dbContext.Users
            .GroupBy(u => u.Gender)
            .Select(g => new
            {
                gender = g.Key ?? "-",
                count = g.Count()
            }).ToListAsync();
        return Ok(new
        {
            code = 200,
            message = "按性别统计用户数量成功",
            data = genderCount
        });
    }

    [HttpGet("userCount/avgAge")]
    public async Task<IActionResult> GetAvgAge()
    {
        var avgAge = await _dbContext.Users
            .Where(u => u.Age.HasValue)
            .AverageAsync(u => (double)u.Age!);

        var avgAgeRounded = Math.Round(avgAge, 2);

        return Ok(new
        {
            code = 200,
            message = "统计用户平均年龄成功",
            data = new { averageAge = avgAgeRounded }
        });
    }
}

// 新增：添加用户的请求模型
public class UserAddRequest
{
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
}

public class UserUpateRequest
{
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
}

public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}