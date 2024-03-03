namespace BlogDotNet8.Services.Interfaces
{
    public interface IBlogEmailSender 
    {
        Task SendContactEmailAsync(string emailFrom, string name, string subject, string htmlMessage);

    }
}
