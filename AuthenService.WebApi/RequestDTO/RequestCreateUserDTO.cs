namespace AuthenService.WebApi.RequestDTO
{
    public class RequestCreateUserDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string ConfirmPassword { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Address { get; set; }
        public required string FullName { get; set; }
        public bool? Gender { get; set; }
        public string? Avatar { get; set; }
    }
}
