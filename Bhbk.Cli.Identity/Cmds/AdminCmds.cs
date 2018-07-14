using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Helpers.FileSystem;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Cli.Identity.Cmds
{
    public class AdminCmds : ConsoleCommand
    {
        private static FileInfo _lib = Search.DefaultPaths("appsettings-lib.json");
        private static JwtSecurityToken _access;
        private static IConfigurationRoot _cb;
        private static CmdType _cmdType;
        private static string _cmdTypeList = string.Join(", ", Enum.GetNames(typeof(CmdType)));
        private static bool _create = false, _destroy = false;

        public AdminCmds()
        {
            IsCommand("admin", "Do things with identity entities...");

            HasOption("c=|create", "Create an entity", arg =>
            {
                _create = true;

                if (!Enum.TryParse<CmdType>(arg, out _cmdType))
                    throw new ConsoleHelpAsException("Invalid entity type. Possible are " + _cmdTypeList);
            });

            HasOption("d=|delete", "Delete an entity", arg =>
            {
                _destroy = true;

                if (!Enum.TryParse<CmdType>(arg, out _cmdType))
                    throw new ConsoleHelpAsException("Invalid entity type. Possible are " + _cmdTypeList);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                _cb = new ConfigurationBuilder()
                    .SetBasePath(_lib.DirectoryName)
                    .AddJsonFile(_lib.Name, optional: false, reloadOnChange: true)
                    .Build();

                if (_create == false && _destroy == false)
                    throw new ConsoleHelpAsException("Invalid action type.");

                switch (_cmdType)
                {
                    case CmdType.client:

                        GetJWT();

                        if (_create)
                        {
                            var clientName = PromptForInput(CmdType.client);
                            var clientID = Guid.Empty;

                            if (CheckClient(clientName, ref clientID))
                                Console.WriteLine(Environment.NewLine + "FOUND client \"" + clientName + "\""
                                    + Environment.NewLine + "\tID is " + clientID.ToString());
                            else
                            {
                                clientID = CreateClient(clientName);

                                if (clientID != Guid.Empty)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create client \"" + clientName + "\""
                                        + Environment.NewLine + "\tID is " + clientID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create client \"" + clientName + "\""
                                        + Environment.NewLine + "\tID is " + clientID.ToString());
                            }
                        }
                        else if (_destroy)
                        {
                            var clientName = PromptForInput(CmdType.client);
                            var clientID = Guid.Empty;

                            if (!CheckClient(clientName, ref clientID))
                                throw new ConsoleHelpAsException("FAILED find client \"" + clientName + "\"");
                            else
                            {
                                if (DeleteClient(clientID))
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy client \"" + clientName + "\""
                                        + Environment.NewLine + "\tID is " + clientID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy client \"" + clientName + "\""
                                        + Environment.NewLine + "\tID is " + clientID.ToString());
                            }
                        }

                        break;

                    case CmdType.audience:

                        GetJWT();

                        if (_create)
                        {
                            var clientName = PromptForInput(CmdType.client);
                            var clientID = Guid.Empty;

                            if (!CheckClient(clientName, ref clientID))
                                throw new ConsoleHelpAsException("FAILED find client \"" + clientName + "\"");

                            var audienceName = PromptForInput(CmdType.audience);
                            var audienceID = Guid.Empty;

                            if (CheckAudience(audienceName, ref audienceID))
                                Console.WriteLine(Environment.NewLine + "FOUND audience \"" + audienceName
                                    + Environment.NewLine + "\tID is " + audienceID.ToString());
                            else
                            {
                                audienceID = CreateAudience(clientID, audienceName);

                                if (audienceID != Guid.Empty)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create audience \"" + audienceName + "\""
                                        + Environment.NewLine + "\tID is " + audienceID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create audience \"" + audienceName + "\""
                                        + Environment.NewLine + "\tID is " + audienceID.ToString());
                            }
                        }
                        else if (_destroy)
                        {
                            var clientName = PromptForInput(CmdType.client);
                            var clientID = Guid.Empty;

                            if (!CheckClient(clientName, ref clientID))
                                throw new ConsoleHelpAsException("FAILED find client \"" + clientName + "\"");

                            var audienceName = PromptForInput(CmdType.audience);
                            var audienceID = Guid.Empty;

                            if (!CheckAudience(audienceName, ref audienceID))
                                Console.WriteLine(Environment.NewLine + "FAILED find audience \"" + audienceName + "\"");
                            else
                            {
                                if (DeleteAudience(audienceID))
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy audience \"" + audienceName + "\""
                                        + Environment.NewLine + "\tID is " + audienceID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy audience \"" + audienceName + "\""
                                        + Environment.NewLine + "\tID is " + audienceID.ToString());
                            }
                        }

                        break;

                    case CmdType.role:

                        GetJWT();

                        if (_create)
                        {
                            var audienceName = PromptForInput(CmdType.audience);
                            var audienceID = Guid.Empty;

                            if (!CheckAudience(audienceName, ref audienceID))
                                throw new ConsoleHelpAsException("FAILED find audience \"" + audienceName + "\"");

                            var roleName = PromptForInput(CmdType.role);
                            var roleID = Guid.Empty;

                            if (CheckRole(roleName, ref roleID))
                                Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                                    + Environment.NewLine + "\tID is " + roleID.ToString());
                            else
                            {
                                roleID = CreateRole(audienceID, roleName);

                                if (roleID != Guid.Empty)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create role \"" + roleName + "\""
                                        + Environment.NewLine + "\tID is " + roleID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create role \"" + roleName + "\""
                                        + Environment.NewLine + "\tID is " + roleID.ToString());
                            }
                        }
                        else if (_destroy)
                        {
                            var audienceName = PromptForInput(CmdType.audience);
                            var audienceID = Guid.Empty;

                            if (!CheckAudience(audienceName, ref audienceID))
                                throw new ConsoleHelpAsException("FAILED find audience \"" + audienceName + "\"");

                            var roleName = PromptForInput(CmdType.role);
                            var roleID = Guid.Empty;

                            if (!CheckRole(roleName, ref roleID))
                                Console.WriteLine(Environment.NewLine + "FAILED find role \"" + roleName + "\"");
                            else
                            {
                                if (DeleteRole(roleID))
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy role \"" + roleName + "\""
                                        + Environment.NewLine + "\tID is " + roleID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy role \"" + roleName + "\""
                                        + Environment.NewLine + "\tID is " + roleID.ToString());
                            }
                        }

                        break;

                    case CmdType.rolemap:

                        GetJWT();

                        if (_create)
                        {
                            var userName = PromptForInput(CmdType.user);
                            var userID = Guid.Empty;

                            if (CheckUser(userName, ref userID))
                                Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                                    + Environment.NewLine + "\tID is " + userID.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find user \"" + userName + "\"");

                            var roleName = PromptForInput(CmdType.role);
                            var roleID = Guid.Empty;

                            if (CheckRole(roleName, ref roleID))
                                Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                                    + Environment.NewLine + "\tID is " + roleID.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find role \"" + roleName + "\"");

                            if (AddUserToRole(roleID, userID))
                                Console.WriteLine(Environment.NewLine + "SUCCESS add role \"" + roleName + "\" to user \"" + userName + "\"");
                            else
                                Console.WriteLine(Environment.NewLine + "FAILED add role \"" + roleName + "\" to user \"" + userName + "\"");
                        }
                        else if (_destroy)
                        {
                            var userName = PromptForInput(CmdType.user);
                            var userID = Guid.Empty;

                            if (CheckUser(userName, ref userID))
                                Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                                    + Environment.NewLine + "\tID is " + userID.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find user \"" + userName + "\"");

                            var roleName = PromptForInput(CmdType.role);
                            var roleID = Guid.Empty;

                            if (CheckRole(roleName, ref roleID))
                                Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                                    + Environment.NewLine + "\tID is " + roleID.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find role \"" + roleName + "\"");

                            if (RemoveUserFromRole(roleID, userID))
                                Console.WriteLine(Environment.NewLine + "SUCCESS remove role \"" + roleName + "\" from user \"" + userName + "\"");
                            else
                                Console.WriteLine(Environment.NewLine + "FAILED remove role \"" + roleName + "\" from user \"" + userName + "\"");
                        }

                        break;

                    case CmdType.user:

                        GetJWT();

                        if (_create)
                        {
                            var userName = PromptForInput(CmdType.user);
                            var userID = Guid.Empty;

                            if (CheckUser(userName, ref userID))
                                Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                                    + Environment.NewLine + "\tID is " + userID.ToString());
                            else
                            {
                                userID = CreateUser(userName);

                                if (userID != Guid.Empty)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create user \"" + userName + "\""
                                        + Environment.NewLine + "\tID is " + userID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create user \"" + userName + "\""
                                        + Environment.NewLine + "\tID is " + userID.ToString());
                            }

                            var loginName = BaseLib.Statics.ApiDefaultLogin;
                            var loginID = Guid.Empty;

                            if (CheckLogin(loginName, ref loginID))
                                Console.WriteLine(Environment.NewLine + "FOUND login \"" + loginName + "\""
                                    + Environment.NewLine + "\tID is " + loginID.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find login \"" + loginName + "\"");

                            if (AddUserToLogin(loginID, userID))
                                Console.WriteLine(Environment.NewLine + "SUCCESS add login \"" + loginName + "\" to user \"" + userName + "\"");
                            else
                                throw new ConsoleHelpAsException("FAILED add login \"" + loginName + "\" to user \"" + userName + "\"");
                        }
                        else if (_destroy)
                        {
                            var userName = PromptForInput(CmdType.user);
                            var userID = Guid.Empty;

                            if (!CheckUser(userName, ref userID))
                                Console.WriteLine(Environment.NewLine + "FAILED find user \"" + userName + "\"");
                            else
                            {
                                if (DeleteUser(userID))
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy user \"" + userName + "\""
                                        + Environment.NewLine + "\tID is " + userID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy user \"" + userName + "\""
                                        + Environment.NewLine + "\tID is \"" + userID.ToString() + "\"");
                            }
                        }

                        break;

                    case CmdType.userpass:

                        GetJWT();

                        if (_create)
                        {
                            var userName = PromptForInput(CmdType.user);
                            var userID = Guid.Empty;

                            if (CheckUser(userName, ref userID))
                                Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                                    + Environment.NewLine + "\tID is " + userID.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find user \"" + userName + "\"");

                            var password = PromptForInput(CmdType.userpass);

                            if (SetPassword(userID, password))
                                Console.WriteLine(Environment.NewLine + "SUCCESS set password for user \"" + userName + "\"");

                            else
                                throw new ConsoleHelpAsException("FAILED set password for user \"" + userName + "\"");
                        }

                        break;

                    default:
                        break;
                }

                return MessageHelper.FondFarewell();
            }
            catch (Exception ex)
            {
                return MessageHelper.AngryFarewell(ex);
            }
        }

        private bool AddUserToLogin(Guid loginID, Guid userID)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        UserId = userID.ToString(),
                        LoginId = loginID.ToString(),
                        LoginProvider = BaseLib.Statics.ApiDefaultLogin,
                        ProviderDisplayName = BaseLib.Statics.ApiDefaultLogin,
                        ProviderKey = BaseLib.Statics.ApiDefaultLoginKey,
                        Enabled = "true",
                    }), Encoding.UTF8, "application/json");

                var response = http.PostAsync(_cb["IdentityApis:AdminPath"] + "/login/v1/" + loginID.ToString() + "/add/" + userID.ToString(), content).Result;

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
        }

        private bool AddUserToRole(Guid roleID, Guid userID)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = http.GetAsync(_cb["IdentityApis:AdminPath"] + "/role/v1/" + roleID.ToString() + "/add/" + userID.ToString()).Result;

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
        }

        private bool CheckAudience(string audience, ref Guid audienceID)
        {
            bool result = false;

            try
            {
                using (var httpHandler = new HttpClientHandler())
                {
                    //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                    httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                    var http = new HttpClient(httpHandler);

                    http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = http.GetAsync(_cb["IdentityApis:AdminPath"] + "/audience/v1/" + audience).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        if (content["name"].Value<string>() == audience)
                        {
                            audienceID = Guid.Parse(content["id"].Value<string>());
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return result;
        }

        private bool CheckClient(string client, ref Guid clientID)
        {
            bool result = false;

            try
            {
                using (var httpHandler = new HttpClientHandler())
                {
                    //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                    httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                    var http = new HttpClient(httpHandler);

                    http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = http.GetAsync(_cb["IdentityApis:AdminPath"] + "/client/v1/" + client).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        if (content["name"].Value<string>() == client)
                        {
                            clientID = Guid.Parse(content["id"].Value<string>());
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return result;
        }

        private bool CheckLogin(string login, ref Guid loginID)
        {
            bool result = false;

            try
            {
                using (var httpHandler = new HttpClientHandler())
                {
                    //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                    httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                    var http = new HttpClient(httpHandler);

                    http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = http.GetAsync(_cb["IdentityApis:AdminPath"] + "/login/v1/" + login).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        if (content["loginProvider"].Value<string>() == login)
                        {
                            loginID = Guid.Parse(content["id"].Value<string>());
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return result;
        }

        private bool CheckRole(string role, ref Guid roleID)
        {
            bool result = false;

            try
            {
                using (var httpHandler = new HttpClientHandler())
                {
                    //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                    httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                    var http = new HttpClient(httpHandler);

                    http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = http.GetAsync(_cb["IdentityApis:AdminPath"] + "/role/v1/" + role).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        if (content["name"].Value<string>() == role)
                        {
                            roleID = Guid.Parse(content["id"].Value<string>());
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return result;
        }

        private bool CheckUser(string user, ref Guid userID)
        {
            bool result = false;

            try
            {
                using (var httpHandler = new HttpClientHandler())
                {
                    //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                    httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                    var http = new HttpClient(httpHandler);

                    http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = http.GetAsync(_cb["IdentityApis:AdminPath"] + "/user/v1/" + user).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                        if (content["email"].Value<string>() == user)
                        {
                            userID = Guid.Parse(content["id"].Value<string>());
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return result;
        }

        private Guid CreateAudience(Guid clientID, string audience)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        ClientId = clientID.ToString(),
                        Name = audience,
                        AudienceType = "server",
                        Enabled = "true",
                    }), Encoding.UTF8, "application/json");

                var response = http.PostAsync(_cb["IdentityApis:AdminPath"] + "/audience/v1", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    if (result["name"].Value<string>() == audience)
                        return Guid.Parse(result["id"].Value<string>());
                }

                return Guid.Empty;
            }
        }

        private Guid CreateClient(string client)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        Name = client,
                        Enabled = "true",
                    }), Encoding.UTF8, "application/json");

                var response = http.PostAsync(_cb["IdentityApis:AdminPath"] + "/client/v1", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    if (result["name"].Value<string>() == client)
                        return Guid.Parse(result["id"].Value<string>());
                }

                return Guid.Empty;
            }
        }

        private Guid CreateRole(Guid audienceID, string role)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        AudienceId = audienceID.ToString(),
                        Name = role,
                        Enabled = "true",
                    }), Encoding.UTF8, "application/json");

                var response = http.PostAsync(_cb["IdentityApis:AdminPath"] + "/role/v1", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    if (result["name"].Value<string>() == role)
                        return Guid.Parse(result["id"].Value<string>());
                }

                return Guid.Empty;
            }
        }

        private Guid CreateUser(string user)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        Email = user,
                        FirstName = BaseLib.Statics.ApiDefaultFirstName,
                        LastName = BaseLib.Statics.ApiDefaultLastName,
                        PhoneNumber = BaseLib.Statics.ApiDefaultPhone,
                        LockoutEnabled = "false",
                        HumanBeing = "false",
                        Immutable = "false",
                    }), Encoding.UTF8, "application/json");

                var response = http.PostAsync(_cb["IdentityApis:AdminPath"] + "/user/v1", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    if (result["email"].Value<string>() == user)
                        return Guid.Parse(result["id"].Value<string>());
                }

                return Guid.Empty;
            }
        }

        private bool DeleteAudience(Guid audienceID)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = http.DeleteAsync(_cb["IdentityApis:AdminPath"] + "/audience/v1/" + audienceID.ToString()).Result;

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
        }

        private bool DeleteClient(Guid clientID)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = http.DeleteAsync(_cb["IdentityApis:AdminPath"] + "/client/v1/" + clientID.ToString()).Result;

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
        }

        private bool DeleteRole(Guid roleID)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = http.DeleteAsync(_cb["IdentityApis:AdminPath"] + "/role/v1/" + roleID.ToString()).Result;

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
        }

        private bool DeleteUser(Guid userID)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = http.DeleteAsync(_cb["IdentityApis:AdminPath"] + "/user/v1/" + userID.ToString()).Result;

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
        }

        private bool RemoveUserFromRole(Guid roleID, Guid userID)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = http.GetAsync(_cb["IdentityApis:AdminPath"] + "/role/v1/" + roleID.ToString() + "/remove/" + userID.ToString()).Result;

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
        }

        private bool SetPassword(Guid userID, string password)
        {
            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:AdminUrl"]);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _access.RawData);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        Id = userID.ToString(),
                        NewPassword = password,
                        NewPasswordConfirm = password,
                    }), Encoding.UTF8, "application/json");

                var response = http.PutAsync(_cb["IdentityApis:AdminPath"] + "/user/v1/" + userID.ToString() + "/reset-password", content).Result;

                if (response.IsSuccessStatusCode)
                    return true;

                return false;
            }
        }

        private string PromptForInput(CmdType cmd)
        {
            switch (cmd)
            {
                case CmdType.audience:
                    Console.Write(Environment.NewLine + "ENTER audience name : ");
                    break;

                case CmdType.client:
                    Console.Write(Environment.NewLine + "ENTER client name : ");
                    break;

                case CmdType.role:
                    Console.Write(Environment.NewLine + "ENTER role name : ");
                    break;

                case CmdType.user:
                    Console.Write(Environment.NewLine + "ENTER user name : ");
                    break;

                case CmdType.userpass:
                    Console.Write(Environment.NewLine + "ENTER password : ");
                    return ConsoleHelper.GetHiddenInput();
            }

            return ConsoleHelper.GetInput();
        }

        private void GetJWT()
        {
            Console.Write("ENTER admin client name or GUID:");
            var clientName = ConsoleHelper.GetInput();

            Console.Write("ENTER admin audience name or GUID:");
            var audienceName = ConsoleHelper.GetInput();

            Console.Write("ENTER admin user name or GUID:");
            var userName = ConsoleHelper.GetInput();

            Console.Write("ENTER admin password:");
            var password = ConsoleHelper.GetHiddenInput();

            using (var httpHandler = new HttpClientHandler())
            {
                //https://stackoverflow.com/questions/38138952/bypass-invalid-ssl-certificate-in-net-core
                httpHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, errors) => { return true; };

                var http = new HttpClient(httpHandler);

                http.BaseAddress = new Uri(_cb["IdentityApis:StsUrl"]);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var post = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string> ("client", clientName),
                    new KeyValuePair<string, string> ("audience", audienceName),
                    new KeyValuePair<string, string> ("user", userName),
                    new KeyValuePair<string, string> ("password", password),
                    new KeyValuePair<string, string> ("grant_type", "password"),
                });

                var response = http.PostAsync(_cb["IdentityApis:StsPath"] + "/oauth/v2/access", post).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                    _access = new JwtSecurityToken((string)result["access_token"]);

                    Console.WriteLine(Environment.NewLine + "SUCCESS getting JWT from STS:\""
                        + _cb["IdentityApis:StsUrl"] + _cb["IdentityApis:StsPath"] + "/oauth/v2/access" + "\""
                        + Environment.NewLine + "\tJWT:\"" + _access.RawData + "\"");
                }
                else
                    throw new ConsoleHelpAsException("FAILED getting JWT from STS:\""
                        + _cb["IdentityApis:StsUrl"] + _cb["IdentityApis:StsPath"] + "/oauth/v2/access" + "\"");
            }
        }
    }
}
