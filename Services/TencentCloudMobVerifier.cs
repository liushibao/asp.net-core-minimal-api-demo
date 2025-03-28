using System;
using System.Threading.Tasks;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Sms.V20210111;
using TencentCloud.Sms.V20210111.Models;
using WebApi.Models;

namespace WebApi.Services
{
    public class TencentCloudMobVerifier : IMobVerifier
    {
        Credential _cred;
        SmsClient _client;

        public TencentCloudMobVerifier()
        {
            this._cred = new Credential
            {
                SecretId = EnvironmentConfig.Instance.TencentcloudSecretId,
                SecretKey = EnvironmentConfig.Instance.TencentcloudSecretKey
            };
            this._client = new SmsClient(_cred, "ap-guangzhou");
        }


        public async Task<bool> SendSmsCode(string mob, string[] templateParams)
        {
            try
            {
                // 为了保护密钥安全，建议将密钥设置在环境变量中或者配置文件中。
                // 硬编码密钥到代码中有可能随代码泄露而暴露，有安全隐患，并不推荐。
                // 这里采用的是从环境变量读取的方式，需要在环境变量中先设置这两个值。
                SendSmsRequest req = new SendSmsRequest();
                req.PhoneNumberSet = [mob];
                req.SmsSdkAppId = EnvironmentConfig.Instance.TencentcloudSmsAppId;
                req.TemplateId = EnvironmentConfig.Instance.TencentcloudSmsCodeTemplateId;
                req.SignName = EnvironmentConfig.Instance.TencentcloudSmsSignName;
                req.TemplateParamSet = templateParams;
                SendSmsResponse resp = await this._client.SendSms(req);
                if (resp.SendStatusSet[0]?.Code == "Ok")
                    return true;
                else
                {
                    Console.WriteLine(AbstractModel.ToJsonString(resp));
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
        }

        public async Task<bool> VerifySmsCode(string mob, string code)
        {
            return await Task.Run(() => true); // TODO
        }
    }
}