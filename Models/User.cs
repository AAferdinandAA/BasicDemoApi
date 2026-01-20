// 引用字段映射的命名空间
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BasicsDemoApi.Models;

// 映射表名
[Table("users")]
public class User
{
    [Column("id")] 
    [Key] // 标记主键
    public int Id { get; set; }

    [Column("name")] 
    [Required] // 非空约束
    public string Name { get; set; } = string.Empty;

    [Column("age")] 
    public int? Age { get; set; }

    [Column("createtime")] 
    public DateTime CreateTime { get; set; }
}