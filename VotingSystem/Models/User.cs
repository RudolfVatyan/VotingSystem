namespace VotingSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }  
        public string? Role { get; set; }
        public string? WalletAddress { get; set; } 
        public bool HasVoted { get; set; }
    }
}
