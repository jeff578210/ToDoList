using System;
using System.Collections.Generic;

namespace ToDoList.Models;

public partial class User
{
    public int id { get; set; }

    public string? name { get; set; }

    public string? password { get; set; }

    public string? status { get; set; }
}
