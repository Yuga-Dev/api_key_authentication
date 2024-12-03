namespace Api_Key_Authentication.Model
{
    public class ApiKey
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public required string KeyHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; }
    }
}