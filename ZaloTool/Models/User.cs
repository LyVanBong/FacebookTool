using System;

namespace ZaloTool.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; }
    public string Password { get; set; }
}