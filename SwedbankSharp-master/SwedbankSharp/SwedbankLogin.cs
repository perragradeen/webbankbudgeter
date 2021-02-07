﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwedbankSharp
{
    public class SwedbankLogin
    {
        private readonly AppData _selectedBank;
        private readonly SwedbankRequester _requester;
        private readonly Action<string> _writeToOutput;

        public SwedbankLogin(BankType bank, Action<string> writeToOutput)
        {
            _selectedBank = BankTypeDefinition.Banks[bank];

            _requester = new SwedbankRequester(GenerateAuthKey(_selectedBank), GenerateDsid(), _selectedBank.UserAgent);
            _writeToOutput = writeToOutput;
        }

        public async Task InitializeMobileBankIdLoginAsync(long personnummer)
        {
            var response = await _requester.PostAsync("identification/bankid/mobile", new JsonSchemas.Login()
            {
                UseEasyLogin = false,
                GenerateEasyLoginId = false,
                UserId = personnummer
            });

            response.EnsureSuccessStatusCode();
        }
        
        public async Task<LoginStatus> VerifyLoginAsync()
        {
            var apiStatus = await _requester.GetAsync<JsonSchemas.LoginStatus>("identification/bankid/mobile/verify");

            if (apiStatus?.Status == "COMPLETE")
            {
                return new LoginStatus()
                {
                    LoggedIn = true,
                    LoginState = "COMPLETE",
                    Swedbank = new Swedbank(_selectedBank, _requester, _writeToOutput)
                };
            }

            return new LoginStatus()
            {
                LoggedIn = false,
                LoginState = apiStatus?.Status
            };
        }

        /// <summary>
        /// Generate authorization key
        /// </summary>
        /// <returns>Auth key</returns>
        private string GenerateAuthKey(AppData bank)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(bank.AppId + ":" + Guid.NewGuid().ToString().ToUpper())); ;
        }

        /// <summary>
        /// Generate dsid for requests
        /// </summary>
        /// <returns>dsid string</returns>
        private string GenerateDsid()
        {
            var dsid = RandomString(8);
            dsid = dsid.Substring(0, 4) + dsid.Substring(4, 4).ToUpper();
            return ShuffleString(dsid);
        }

        /// <summary>
        /// Generate a random string
        /// </summary>
        /// <param name="size">How long of a string?</param>
        /// <returns></returns>
        private string RandomString(int size)
        {
            var builder = new StringBuilder();
            char ch;
            var random = new Random();
            for (var i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString().ToLower();
        }

        /// <summary>
        /// Shuffle all letters in a string
        /// </summary>
        /// <param name="stringToShuffle">String to shuffle</param>
        /// <returns></returns>
        private string ShuffleString(string stringToShuffle)
        {
            if (String.IsNullOrEmpty(stringToShuffle))
            {
                throw new ArgumentNullException("stringToShuffle",
                                                "The stringToShuffle variable must not be null or empty");
            }

            return new string(
                                 stringToShuffle
                                    .OrderBy(character => Guid.NewGuid())
                                    .ToArray()
                            );
        }
    }
}
