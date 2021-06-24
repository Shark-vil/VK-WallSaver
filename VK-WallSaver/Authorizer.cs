using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VK_WallSaver.Extensions;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace VK_WallSaver
{
    public class Authorizer
    {
        private static string TokenFilePath = Path.Combine(Program.ApplicationPath, ".token");

        public static bool Default(VkApi api)
        {
            // BUG: Fix exception when authorizing user
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            // ----------------------------------------

            bool HasLogged = Token(api);

            if (!HasLogged)
                HasLogged = Login(api);

            return HasLogged;
        }

        public static bool Login(VkApi api)
        {
            EConsole.Print("Does your account have 2-step authentication?");
            bool IsTwoStepAuthorization = EConsole.HasYes();

            EConsole.Print("Enter user login:");
            string UserLogin = Console.ReadLine();

            EConsole.Print("Enter user password:");
            string UserPassword = Console.ReadLine();

            var AuthParams = new ApiAuthParams
            {
                ApplicationId = Config.Settings.ApplicationId,
                Login = UserLogin,
                Password = UserPassword,
                Settings = Settings.Wall | Settings.Groups,
            };

            if (IsTwoStepAuthorization)
                AuthParams.TwoFactorAuthorization = () =>
                {
                    EConsole.Print("Enter the verification code to login:");
                    return Console.ReadLine();
                };

            try
            {
                api.Authorize(AuthParams);
            }
            catch (Exception ex)
            {
                EConsole.Warning("An exception was thrown during authorization:");
                EConsole.Error(ex);
                EConsole.ClickToContinue();
            }

            if (api.IsAuthorized)
                SaveToken(api.Token);

            return api.IsAuthorized;
        }

        public static bool Token(VkApi api)
        {
            string Token = ReadToken();
            if (Token != null)
            {
                var AuthParams = new ApiAuthParams
                {
                    AccessToken = Token
                };

                try
                {
                    api.Authorize(AuthParams);
                }
                catch(Exception ex)
                {
                    EConsole.Warning("An exception was thrown during authorization:");
                    EConsole.Error(ex);
                    EConsole.ClickToContinue();
                }

                return api.IsAuthorized;
            }
            return false;
        }

        private static void SaveToken(string Token) => File.WriteAllText(TokenFilePath, Token);

        private static string ReadToken()
        {
            if (File.Exists(TokenFilePath))
                return File.ReadAllText(TokenFilePath);
            return null;
        }
    }
}
