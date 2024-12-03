namespace Api_Key_Authentication.Model
{
    public class User
    {
        public int Id { get; set; }

        public required string Email { get; set; }

        public required string? Password { get; set; }

        public required string PasswordHash { get; set; }
    }
}