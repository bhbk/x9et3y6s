using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Primitives.Enums;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.Cli.Identity.Cmds
{
    public class AdminCmds : ConsoleCommand
    {
        private static FileInfo _lib = Search.DefaultPaths("appsettings-lib.json");
        private static IConfigurationRoot _conf;
        private static CmdType _cmdType;
        private static JwtSecurityToken _access;
        private static S2SClient _client;
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
                _conf = new ConfigurationBuilder()
                    .SetBasePath(_lib.DirectoryName)
                    .AddJsonFile(_lib.Name, optional: false, reloadOnChange: true)
                    .Build();

                _client = new S2SClient(_conf, ContextType.Live);

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
                                    throw new ConsoleHelpAsException("FAILED create client \"" + clientName + "\"");
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
                                    throw new ConsoleHelpAsException("FAILED create audience \"" + audienceName + "\"");
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
                                    throw new ConsoleHelpAsException("FAILED create role \"" + roleName + "\"");
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

                            if (RemoveRoleFromUser(roleID, userID))
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
                                    throw new ConsoleHelpAsException("FAILED create user \"" + userName + "\"");
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
            var result = _client.AdminLoginAddUserV1(_access, loginID, userID,
                new UserLoginCreate()
                {
                    UserId = userID,
                    LoginId = loginID,
                    LoginProvider = BaseLib.Statics.ApiDefaultLogin,
                    ProviderDisplayName = BaseLib.Statics.ApiDefaultLogin,
                    ProviderKey = BaseLib.Statics.ApiDefaultLoginKey,
                    Enabled = true,
                }).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool AddUserToRole(Guid roleID, Guid userID)
        {
            var result = _client.AdminRoleAddToUserV1(_access, roleID, userID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool CheckAudience(string audience, ref Guid audienceID)
        {
            var result = _client.AdminAudienceGetV1(_access, audience).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == audience)
                {
                    audienceID = Guid.Parse(content["id"].Value<string>());
                    return true;
                }
            }

            return false;
        }

        private bool CheckClient(string client, ref Guid clientID)
        {
            var result = _client.AdminClientGetV1(_access, client).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == client)
                {
                    clientID = Guid.Parse(content["id"].Value<string>());
                    return true;
                }
            }

            return false;
        }

        private bool CheckLogin(string login, ref Guid loginID)
        {
            var result = _client.AdminLoginGetV1(_access, login).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["loginProvider"].Value<string>() == login)
                {
                    loginID = Guid.Parse(content["id"].Value<string>());
                    return true;
                }
            }

            return false;
        }

        private bool CheckRole(string role, ref Guid roleID)
        {
            var result = _client.AdminRoleGetV1(_access, role).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == role)
                {
                    roleID = Guid.Parse(content["id"].Value<string>());
                    return true;
                }
            }

            return false;
        }

        private bool CheckUser(string user, ref Guid userID)
        {
            var result = _client.AdminUserGetV1(_access, user).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["email"].Value<string>() == user)
                {
                    userID = Guid.Parse(content["id"].Value<string>());
                    return true;
                }
            }

            return false;
        }

        private Guid CreateAudience(Guid clientID, string audience)
        {
            var result = _client.AdminAudienceCreateV1(_access,
                new AudienceCreate()
                {
                    ClientId = clientID,
                    Name = audience,
                    AudienceType = "server",
                    Enabled = true,
                }).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == audience)
                    return Guid.Parse(content["id"].Value<string>());

                Console.WriteLine(BaseLib.Statics.MsgAudienceInvalid);
            }

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return Guid.Empty;
        }

        private Guid CreateClient(string client)
        {
            var result = _client.AdminClientCreateV1(_access,
                new ClientCreate()
                {
                    Name = client,
                    Enabled = true,
                }).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == client)
                    return Guid.Parse(content["id"].Value<string>());

                Console.WriteLine(BaseLib.Statics.MsgClientInvalid);
            }

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return Guid.Empty;
        }

        private Guid CreateRole(Guid audienceID, string role)
        {
            var result = _client.AdminRoleCreateV1(_access,
                new RoleCreate()
                {
                    AudienceId = audienceID,
                    Name = role,
                    Enabled = true,
                }).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == role)
                    return Guid.Parse(content["id"].Value<string>());

                Console.WriteLine(BaseLib.Statics.MsgRoleInvalid);
            }

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return Guid.Empty;
        }

        private Guid CreateUser(string user)
        {
            var result = _client.AdminUserCreateV1NoConfirm(_access,
                new UserCreate()
                {
                    Email = user,
                    FirstName = BaseLib.Statics.ApiDefaultFirstName,
                    LastName = BaseLib.Statics.ApiDefaultLastName,
                    PhoneNumber = BaseLib.Statics.ApiDefaultPhone,
                    LockoutEnabled = false,
                    HumanBeing = false,
                    Immutable = false,
                }).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["email"].Value<string>() == user)
                    return Guid.Parse(content["id"].Value<string>());

                Console.WriteLine(BaseLib.Statics.MsgUserInvalid);
            }

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return Guid.Empty;
        }

        private bool DeleteAudience(Guid audienceID)
        {
            var result = _client.AdminAudienceDeleteV1(_access, audienceID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool DeleteClient(Guid clientID)
        {
            var result = _client.AdminClientDeleteV1(_access, clientID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool DeleteRole(Guid roleID)
        {
            var result = _client.AdminRoleDeleteV1(_access, roleID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool DeleteUser(Guid userID)
        {
            var result = _client.AdminUserDeleteV1(_access, userID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool RemoveRoleFromUser(Guid roleID, Guid userID)
        {
            var result = _client.AdminRoleRemoveFromUserV1(_access, roleID, userID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool SetPassword(Guid userID, string password)
        {
            var result = _client.AdminUserSetPasswordV1(_access,
                new UserAddPassword()
                {
                    Id = userID,
                    NewPassword = password,
                    NewPasswordConfirm = password,
                }).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
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
            var clientName = _conf["IdentityLogin:ClientName"];
            Console.WriteLine("USED admin client name or GUID: " + clientName);

            var audienceName = _conf["IdentityLogin:AudienceName"];
            Console.WriteLine("USED admin audience name or GUID: " + audienceName);

            var userName = _conf["IdentityLogin:UserName"];
            Console.WriteLine("USED admin user name or GUID: " + userName);

            var password = _conf["IdentityLogin:UserPass"];
            Console.WriteLine("USED admin password: " + password);

            /*
            Console.WriteLine("ENTER admin client name or GUID:");
            var clientName = ConsoleHelper.GetInput();

            Console.WriteLine("ENTER admin audience name or GUID:");
            var audienceName = ConsoleHelper.GetInput();

            Console.WriteLine("ENTER admin user name or GUID:");
            var userName = ConsoleHelper.GetInput();

            Console.WriteLine("ENTER admin password:");
            var password = ConsoleHelper.GetHiddenInput();
            */

            var result = _client.StsAccessTokenV2(clientName, new List<string> { audienceName }, userName, password).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                _access = new JwtSecurityToken((string)content["access_token"]);

                Console.WriteLine(Environment.NewLine + "SUCCESS getting JWT from STS:\""
                    + _conf["IdentityStsUrls:BaseApiUrl"] + _conf["IdentityStsUrls:BaseApiPath"] + "/oauth/v2/access" + "\""
                    + Environment.NewLine + "\tJWT:\"" + _access.RawData + "\"");
            }
            else
                throw new ConsoleHelpAsException("FAILED getting JWT from STS:\""
                    + _conf["IdentityStsUrls:BaseApiUrl"] + _conf["IdentityStsUrls:BaseApiPath"] + "/oauth/v2/access" + "\"");
        }
    }
}
