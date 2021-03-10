namespace TransactionService.Settings
{
    public class Auth0Settings
    {
        public const string Key = "Auth0";
        public string Authority { get; set; }
        public string Audience { get; set; }
    }
}