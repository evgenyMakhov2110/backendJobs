namespace WebApplication1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }  // "USERS" или "ADMIN"
        public string Password { get; set; }  // "USERS12345" или "ADMINISTRATOR"
        public string Role { get; set; }      // "users" или "admin"
    }
}
