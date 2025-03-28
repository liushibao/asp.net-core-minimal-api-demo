using System.Linq;

namespace WebApi.Services
{
    public class FakeMobVerifier : IMobVerifier
    {
        public async Task<bool> SendSmsCode(string mob, string[] templateParams)
        {
            await Task.Run(() => Console.WriteLine($"fake sms client method called with {mob} {String.Join(",", templateParams)}"));
            return await Task.Run(() => true);
        }

        public async Task<bool> VerifySmsCode(string mob, string code)
        {
            return await Task.Run(() => true); // TODO
        }
    }
}
