using Bhbk.Cli.Identity.Helpers;
using Bhbk.Lib.Core.Cryptography;
using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.DomainModels.Admin;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Providers;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace Bhbk.Cli.Identity.Cmds
{
    public class AdminCmds : ConsoleCommand
    {
        private static IConfigurationRoot _conf;
        private static IJwtContext _jwt;
        private static CmdType _cmdType;
        private static AdminClient _admin = null;
        private static StsClient _sts = null;
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
                var lib = SearchRoots.ByAssemblyContext("libsettings.json");

                _conf = new ConfigurationBuilder()
                    .SetBasePath(lib.DirectoryName)
                    .AddJsonFile(lib.Name, optional: false, reloadOnChange: true)
                    .Build();

                _admin = new AdminClient(_conf, ContextType.Live, new HttpClient());
                _sts = new StsClient(_conf, ContextType.Live, new HttpClient());
                _jwt = new JwtContext(_conf, ContextType.Live, new HttpClient());

                for(int i = 0; i < 20; i++)
                    System.Console.WriteLine(RandomValues.CreateBase64String(32));

                if (_create == false && _destroy == false)
                    throw new ConsoleHelpAsException("Invalid action type.");

                switch (_cmdType)
                {
                    case CmdType.issuer:

                        if (_create)
                        {
                            var issuerName = PromptForInput(CmdType.issuer);
                            var issuerID = Guid.Empty;

                            if (CheckIssuer(issuerName, ref issuerID))
                                Console.WriteLine(Environment.NewLine + "FOUND issuer \"" + issuerName + "\""
                                    + Environment.NewLine + "\tID is " + issuerID.ToString());
                            else
                            {
                                issuerID = CreateIssuer(issuerName);

                                if (issuerID != Guid.Empty)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create issuer \"" + issuerName + "\""
                                        + Environment.NewLine + "\tID is " + issuerID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create issuer \"" + issuerName + "\"");
                            }
                        }
                        else if (_destroy)
                        {
                            var issuerName = PromptForInput(CmdType.issuer);
                            var issuerID = Guid.Empty;

                            if (!CheckIssuer(issuerName, ref issuerID))
                                throw new ConsoleHelpAsException("FAILED find issuer \"" + issuerName + "\"");
                            else
                            {
                                if (DeleteIssuer(issuerID))
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy issuer \"" + issuerName + "\""
                                        + Environment.NewLine + "\tID is " + issuerID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy issuer \"" + issuerName + "\""
                                        + Environment.NewLine + "\tID is " + issuerID.ToString());
                            }
                        }

                        break;

                    case CmdType.client:

                        if (_create)
                        {
                            var issuerName = PromptForInput(CmdType.issuer);
                            var issuerID = Guid.Empty;

                            if (!CheckIssuer(issuerName, ref issuerID))
                                throw new ConsoleHelpAsException("FAILED find issuer \"" + issuerName + "\"");

                            var clientName = PromptForInput(CmdType.client);
                            var clientID = Guid.Empty;

                            if (CheckClient(clientName, ref clientID))
                                Console.WriteLine(Environment.NewLine + "FOUND client \"" + clientName
                                    + Environment.NewLine + "\tID is " + clientID.ToString());
                            else
                            {
                                clientID = CreateClient(issuerID, clientName);

                                if (clientID != Guid.Empty)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create client \"" + clientName + "\""
                                        + Environment.NewLine + "\tID is " + clientID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create client \"" + clientName + "\"");
                            }
                        }
                        else if (_destroy)
                        {
                            var issuerName = PromptForInput(CmdType.issuer);
                            var issuerID = Guid.Empty;

                            if (!CheckIssuer(issuerName, ref issuerID))
                                throw new ConsoleHelpAsException("FAILED find issuer \"" + issuerName + "\"");

                            var clientName = PromptForInput(CmdType.client);
                            var clientID = Guid.Empty;

                            if (!CheckClient(clientName, ref clientID))
                                Console.WriteLine(Environment.NewLine + "FAILED find client \"" + clientName + "\"");
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

                    case CmdType.role:

                        if (_create)
                        {
                            var clientName = PromptForInput(CmdType.client);
                            var clientID = Guid.Empty;

                            if (!CheckClient(clientName, ref clientID))
                                throw new ConsoleHelpAsException("FAILED find client \"" + clientName + "\"");

                            var roleName = PromptForInput(CmdType.role);
                            var roleID = Guid.Empty;

                            if (CheckRole(roleName, ref roleID))
                                Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                                    + Environment.NewLine + "\tID is " + roleID.ToString());
                            else
                            {
                                roleID = CreateRole(clientID, roleName);

                                if (roleID != Guid.Empty)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create role \"" + roleName + "\""
                                        + Environment.NewLine + "\tID is " + roleID.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create role \"" + roleName + "\"");
                            }
                        }
                        else if (_destroy)
                        {
                            var clientName = PromptForInput(CmdType.client);
                            var clientID = Guid.Empty;

                            if (!CheckClient(clientName, ref clientID))
                                throw new ConsoleHelpAsException("FAILED find client \"" + clientName + "\"");

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

                            var loginName = Strings.ApiDefaultLogin;
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
            var result = _admin.Login_AddUserV1(_jwt.AccessToken.RawData, loginID, userID,
                new UserLoginCreate()
                {
                    UserId = userID,
                    LoginId = loginID,
                    LoginProvider = Strings.ApiDefaultLogin,
                    ProviderDisplayName = Strings.ApiDefaultLogin,
                    ProviderKey = Strings.ApiDefaultLoginKey,
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
            var result = _admin.Role_AddToUserV1(_jwt.AccessToken.RawData, roleID, userID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool CheckClient(string client, ref Guid clientID)
        {
            var result = _admin.Client_GetV1(_jwt.AccessToken.RawData, client).Result;

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

        private bool CheckIssuer(string issuer, ref Guid issuerID)
        {
            var result = _admin.Issuer_GetV1(_jwt.AccessToken.RawData, issuer).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == issuer)
                {
                    issuerID = Guid.Parse(content["id"].Value<string>());
                    return true;
                }
            }

            return false;
        }

        private bool CheckLogin(string login, ref Guid loginID)
        {
            var result = _admin.Login_GetV1(_jwt.AccessToken.RawData, login).Result;

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
            var result = _admin.Role_GetV1(_jwt.AccessToken.RawData, role).Result;

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
            var result = _admin.User_GetV1(_jwt.AccessToken.RawData, user).Result;

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

        private Guid CreateClient(Guid issuerID, string client)
        {
            var result = _admin.Client_CreateV1(_jwt.AccessToken.RawData,
                new ClientCreate()
                {
                    IssuerId = issuerID,
                    Name = client,
                    ClientType = "server",
                    Enabled = true,
                }).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == client)
                    return Guid.Parse(content["id"].Value<string>());

                Console.WriteLine(Strings.MsgClientInvalid);
            }

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return Guid.Empty;
        }

        private Guid CreateIssuer(string issuer)
        {
            var result = _admin.Issuer_CreateV1(_jwt.AccessToken.RawData,
                new IssuerCreate()
                {
                    Name = issuer,
                    Enabled = true,
                }).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == issuer)
                    return Guid.Parse(content["id"].Value<string>());

                Console.WriteLine(Strings.MsgIssuerInvalid);
            }

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return Guid.Empty;
        }

        private Guid CreateRole(Guid clientID, string role)
        {
            var result = _admin.Role_CreateV1(_jwt.AccessToken.RawData,
                new RoleCreate()
                {
                    ClientId = clientID,
                    Name = role,
                    Enabled = true,
                }).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["name"].Value<string>() == role)
                    return Guid.Parse(content["id"].Value<string>());

                Console.WriteLine(Strings.MsgRoleInvalid);
            }

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return Guid.Empty;
        }

        private Guid CreateUser(string user)
        {
            var result = _admin.User_CreateV1NoConfirm(_jwt.AccessToken.RawData,
                new UserCreate()
                {
                    Email = user,
                    FirstName = Strings.ApiDefaultFirstName,
                    LastName = Strings.ApiDefaultLastName,
                    PhoneNumber = Strings.ApiDefaultPhone,
                    LockoutEnabled = false,
                    HumanBeing = false,
                    Immutable = false,
                }).Result;

            if (result.IsSuccessStatusCode)
            {
                var content = JObject.Parse(result.Content.ReadAsStringAsync().Result);

                if (content["email"].Value<string>() == user)
                    return Guid.Parse(content["id"].Value<string>());

                Console.WriteLine(Strings.MsgUserInvalid);
            }

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return Guid.Empty;
        }

        private bool DeleteClient(Guid clientID)
        {
            var result = _admin.Client_DeleteV1(_jwt.AccessToken.RawData, clientID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool DeleteIssuer(Guid issuerID)
        {
            var result = _admin.Issuer_DeleteV1(_jwt.AccessToken.RawData, issuerID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool DeleteRole(Guid roleID)
        {
            var result = _admin.Role_DeleteV1(_jwt.AccessToken.RawData, roleID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool DeleteUser(Guid userID)
        {
            var result = _admin.User_DeleteV1(_jwt.AccessToken.RawData, userID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool RemoveRoleFromUser(Guid roleID, Guid userID)
        {
            var result = _admin.Role_RemoveFromUserV1(_jwt.AccessToken.RawData, roleID, userID).Result;

            if (result.IsSuccessStatusCode)
                return true;

            Console.WriteLine(result.RequestMessage
                + Environment.NewLine + result);

            return false;
        }

        private bool SetPassword(Guid userID, string password)
        {
            var result = _admin.User_SetPasswordV1(_jwt.AccessToken.RawData, userID,
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
                case CmdType.issuer:
                    Console.Write(Environment.NewLine + "ENTER issuer name : ");
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
    }
}
