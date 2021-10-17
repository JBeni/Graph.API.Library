namespace Graph.API.Library.Models
{
    public class User
    {
        public bool AccountEnabled { get; set; }
        public string DisplayName { get; set; }
        public string MailNickname { get; set; }
        public string UserPrincipalName { get; set; }
        public PasswordProfile PasswordProfile { get; set; }
    }
}
