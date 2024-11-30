using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ToDoList.Dtos 
{
    public class UserCreateDto
    {
        [Required(ErrorMessage = "名稱是必填的")]
        [StringLength(10, ErrorMessage = "名稱長度不能超過10")]
        public string Name { get ; set; }
        [Required(ErrorMessage = "密碼是必填的")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在 6 到 20 之間")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,}$",
        ErrorMessage = "密碼必須至少包含一個字母和一個數字")]
        public string Password { get; set; }

    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        [ValidateNever]
        public string Status { get; set; }


    }


    public class ListDto 
    {
        public int? Id { get; set; }
        public int? Uid { get; set; }
        public string? Title { get; set; }
        public string? Word { get; set; }
        public string? UserName { get; set; }

        
    }
}
    