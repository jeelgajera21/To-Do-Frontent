namespace To_Do_UI.Models
{
    public class UserModel
    {
        public int? UserID { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserLoginRequest
    {
        public string UsernameOrEmail { get; set; }
        public string PasswordHash { get; set; }
    }

    public class UserLoginResponse
    {
        public int UserID { get; set; }
        public string Name { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string? Token { get; set; }
        public bool? LoginStatus { get; set; }
    }
}
