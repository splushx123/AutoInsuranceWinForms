namespace AutoInsuranceWinForms
{
    public enum UserRole
    {
        Administrator,
        Manager,
        Adjuster
    }

    public sealed class UserAccount
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public UserRole Role { get; set; }
    }
}
