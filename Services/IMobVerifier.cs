namespace WebApi.Services
{
    public interface IMobVerifier
    {
        Task<bool> SendSmsCode(string mob, string[] templateParams);
        Task<bool> VerifySmsCode(string mob, string code);
    }
}