using System;
using System.Collections.Generic;

namespace ToDoList.Models;

public partial class List
{
    public int id { get; set; }

    public int? uid { get; set; }

    public string? title { get; set; }

    public string? word { get; set; }
}
