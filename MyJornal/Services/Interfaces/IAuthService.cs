namespace JournalApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<bool> IsPinSetAsync();
        Task<bool> SetPinAsync(string pin);
        Task<bool> VerifyPinAsync(string pin);
    }
}
