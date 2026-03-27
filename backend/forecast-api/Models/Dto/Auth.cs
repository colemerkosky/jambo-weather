namespace forecast_api.Models.Dto
{
    public record AuthRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public record AuthResponse
    {
        public required string Token { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public DateTime ExpiresAtUtc { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiresAtUtc { get; set; }
    }

    public record RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }
}