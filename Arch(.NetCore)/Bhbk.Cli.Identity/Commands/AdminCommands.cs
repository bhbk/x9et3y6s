using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.Identity.Data.Primitives;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace Bhbk.Cli.Identity.Commands
{
    public class AdminCommands : ConsoleCommand
    {
        private static CommandTypes _cmdType;
        private static IAdminService _service;
        private static JwtSecurityToken _jwt;
        private static string _cmdTypeList = string.Join(", ", Enum.GetNames(typeof(CommandTypes)));
        private static bool _create = false, _destroy = false;

        public AdminCommands()
        {
            IsCommand("admin", "Do things with identity entities...");

            HasOption("c=|create", "Create an entity", arg =>
            {
                _create = true;

                if (!Enum.TryParse<CommandTypes>(arg, out _cmdType))
                    throw new ConsoleHelpAsException("Invalid entity type. Possible are " + _cmdTypeList);
            });

            HasOption("d=|delete", "Delete an entity", arg =>
            {
                _destroy = true;

                if (!Enum.TryParse<CommandTypes>(arg, out _cmdType))
                    throw new ConsoleHelpAsException("Invalid entity type. Possible are " + _cmdTypeList);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            IssuerModel issuer = null;
            ClientModel client = null;
            LoginModel login = null;
            RoleModel role = null;
            UserModel user = null;
            string issuerName = string.Empty;
            string clientName = string.Empty;
            string loginName = string.Empty;
            string roleName = string.Empty;
            string userName = string.Empty;

            try
            {
                var file = SearchRoots.ByAssemblyContext("clisettings.json");

                var conf = (IConfiguration)new ConfigurationBuilder()
                    .SetBasePath(file.DirectoryName)
                    .AddJsonFile(file.Name, optional: false, reloadOnChange: true)
                    .Build();

                _service = new AdminService(conf);
                _jwt = _service.Jwt;

                if (_create == false && _destroy == false)
                    throw new ConsoleHelpAsException("Invalid action type.");

                switch (_cmdType)
                {
                    case CommandTypes.issuer:

                        issuerName = PromptForInput(CommandTypes.issuer);
                        issuer = _service.Issuer_GetV1(issuerName).Result;

                        if (_create)
                        {

                            if (issuer != null)
                                Console.WriteLine(Environment.NewLine + "FOUND issuer \"" + issuer.Name + "\""
                                    + Environment.NewLine + "\tID is " + issuer.Id.ToString());
                            else
                            {
                                issuer = _service.Issuer_CreateV1(new IssuerCreate()
                                {
                                    Name = issuerName,
                                    Enabled = true,
                                }).Result;

                                if (issuer.Id != Guid.Empty)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create issuer \"" + issuerName + "\""
                                        + Environment.NewLine + "\tID is " + issuer.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create issuer \"" + issuerName + "\"");
                            }
                        }
                        else if (_destroy)
                        {
                            if (issuer == null)
                                throw new ConsoleHelpAsException("FAILED find issuer \"" + issuerName + "\"");
                            else
                            {
                                if (_service.Issuer_DeleteV1(issuer.Id).Result)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy issuer \"" + issuerName + "\""
                                        + Environment.NewLine + "\tID is " + issuer.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy issuer \"" + issuerName + "\""
                                        + Environment.NewLine + "\tID is " + issuer.Id.ToString());
                            }
                        }

                        break;

                    case CommandTypes.client:

                        issuerName = PromptForInput(CommandTypes.issuer);
                        issuer = _service.Issuer_GetV1(issuerName).Result;

                        if (issuer == null)
                            throw new ConsoleHelpAsException("FAILED find issuer \"" + issuerName + "\"");

                        clientName = PromptForInput(CommandTypes.client);
                        client = _service.Client_GetV1(clientName).Result;

                        if (_create)
                        {
                            if (client != null)
                                Console.WriteLine(Environment.NewLine + "FOUND client \"" + clientName
                                    + Environment.NewLine + "\tID is " + client.Id.ToString());
                            else
                            {
                                client = _service.Client_CreateV1(new ClientCreate()
                                {
                                    IssuerId = issuer.Id,
                                    Name = clientName,
                                    ClientType = ClientType.user_agent.ToString(),
                                    Enabled = true,
                                }).Result;

                                if (client != null)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create client \"" + clientName + "\""
                                        + Environment.NewLine + "\tID is " + client.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create client \"" + clientName + "\"");
                            }
                        }
                        else if (_destroy)
                        {
                            if (client == null)
                                Console.WriteLine(Environment.NewLine + "FAILED find client \"" + clientName + "\"");
                            else
                            {
                                if (_service.Client_DeleteV1(client.Id).Result)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy client \"" + clientName + "\""
                                        + Environment.NewLine + "\tID is " + client.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy client \"" + clientName + "\""
                                        + Environment.NewLine + "\tID is " + client.Id.ToString());
                            }
                        }

                        break;

                    case CommandTypes.login:

                        loginName = PromptForInput(CommandTypes.login);
                        login = _service.Login_GetV1(loginName).Result;

                        if (_create)
                        {
                            if (login != null)
                                Console.WriteLine(Environment.NewLine + "FOUND login \"" + loginName
                                    + Environment.NewLine + "\tID is " + login.Id.ToString());
                            else
                            {
                                login = _service.Login_CreateV1(new LoginCreate()
                                {
                                    Name = loginName,
                                    Enabled = true,
                                }).Result;

                                if (login != null)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create login \"" + loginName + "\""
                                        + Environment.NewLine + "\tID is " + login.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create login \"" + loginName + "\"");
                            }
                        }
                        else if (_destroy)
                        {
                            if (login == null)
                                Console.WriteLine(Environment.NewLine + "FAILED find login \"" + loginName + "\"");
                            else
                            {
                                if (_service.Login_DeleteV1(login.Id).Result)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy login \"" + loginName + "\""
                                        + Environment.NewLine + "\tID is " + login.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy login \"" + loginName + "\""
                                        + Environment.NewLine + "\tID is " + login.Id.ToString());
                            }
                        }

                        break;

                    case CommandTypes.role:

                        clientName = PromptForInput(CommandTypes.client);
                        client = _service.Client_GetV1(clientName).Result;

                        if (client == null)
                            throw new ConsoleHelpAsException("FAILED find client \"" + clientName + "\"");

                        roleName = PromptForInput(CommandTypes.role);
                        role = _service.Role_GetV1(roleName).Result;

                        if (_create)
                        {
                            if (role != null)
                                Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                                    + Environment.NewLine + "\tID is " + role.Id.ToString());
                            else
                            {
                                role = _service.Role_CreateV1(new RoleCreate()
                                {
                                    ClientId = client.Id,
                                    Name = roleName,
                                    Enabled = true,
                                }).Result;

                                if (role != null)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create role \"" + roleName + "\""
                                        + Environment.NewLine + "\tID is " + role.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create role \"" + roleName + "\"");
                            }
                        }
                        else if (_destroy)
                        {
                            if (role == null)
                                Console.WriteLine(Environment.NewLine + "FAILED find role \"" + roleName + "\"");
                            else
                            {
                                if (_service.Role_DeleteV1(role.Id).Result)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy role \"" + roleName + "\""
                                        + Environment.NewLine + "\tID is " + role.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy role \"" + roleName + "\""
                                        + Environment.NewLine + "\tID is " + role.Id.ToString());
                            }
                        }

                        break;

                    case CommandTypes.rolemap:

                        userName = PromptForInput(CommandTypes.user);
                        user = _service.User_GetV1(userName).Result;

                        if (user != null)
                            Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                                + Environment.NewLine + "\tID is " + user.Id.ToString());
                        else
                            throw new ConsoleHelpAsException("FAILED find user \"" + userName + "\"");

                        roleName = PromptForInput(CommandTypes.role);
                        role = _service.Role_GetV1(roleName).Result;

                        if (role != null)
                            Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                                + Environment.NewLine + "\tID is " + role.Id.ToString());
                        else
                            throw new ConsoleHelpAsException("FAILED find role \"" + roleName + "\"");

                        if (_create)
                        {
                            if (_service.User_AddToRoleV1(user.Id, role.Id).Result)
                                Console.WriteLine(Environment.NewLine + "SUCCESS add role \"" + roleName + "\" to user \"" + userName + "\"");
                            else
                                Console.WriteLine(Environment.NewLine + "FAILED add role \"" + roleName + "\" to user \"" + userName + "\"");
                        }
                        else if (_destroy)
                        {
                            if (_service.User_RemoveFromRoleV1(user.Id, role.Id).Result)
                                Console.WriteLine(Environment.NewLine + "SUCCESS remove role \"" + roleName + "\" from user \"" + userName + "\"");
                            else
                                Console.WriteLine(Environment.NewLine + "FAILED remove role \"" + roleName + "\" from user \"" + userName + "\"");
                        }

                        break;

                    case CommandTypes.user:

                        userName = PromptForInput(CommandTypes.user);
                        user = _service.User_GetV1(userName).Result;

                        if (_create)
                        {
                            if (user != null)
                                Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                                    + Environment.NewLine + "\tID is " + user.Id.ToString());
                            else
                            {
                                user = _service.User_CreateV1(new UserCreate()
                                {
                                    Email = userName,
                                    FirstName = Constants.ApiDefaultAdminUserFirstName,
                                    LastName = Constants.ApiDefaultAdminUserLastName,
                                    PhoneNumber = Constants.ApiDefaultAdminUserPhone,
                                    LockoutEnabled = false,
                                    HumanBeing = false,
                                    Immutable = false,
                                }).Result;

                                if (user != null)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create user \"" + userName + "\""
                                        + Environment.NewLine + "\tID is " + user.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create user \"" + userName + "\"");
                            }

                            loginName = Constants.ApiDefaultLogin;
                            login = _service.Login_GetV1(loginName).Result;

                            if (login != null)
                                Console.WriteLine(Environment.NewLine + "FOUND login \"" + loginName + "\""
                                    + Environment.NewLine + "\tID is " + login.Id.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find login \"" + loginName + "\"");

                            if (_service.User_AddToLoginV1(user.Id, login.Id).Result)
                                Console.WriteLine(Environment.NewLine + "SUCCESS add login \"" + loginName + "\" to user \"" + userName + "\"");
                            else
                                throw new ConsoleHelpAsException("FAILED add login \"" + loginName + "\" to user \"" + userName + "\"");
                        }
                        else if (_destroy)
                        {
                            if (user != null)
                                Console.WriteLine(Environment.NewLine + "FAILED find user \"" + userName + "\"");
                            else
                            {
                                if (_service.User_DeleteV1(user.Id).Result)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy user \"" + userName + "\""
                                        + Environment.NewLine + "\tID is " + user.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy user \"" + userName + "\""
                                        + Environment.NewLine + "\tID is \"" + user.Id.ToString() + "\"");
                            }
                        }

                        break;

                    case CommandTypes.userpass:

                        userName = PromptForInput(CommandTypes.user);
                        user = _service.User_GetV1(userName).Result;

                        if (_create)
                        {
                            if (user != null)
                                Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                                    + Environment.NewLine + "\tID is " + user.Id.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find user \"" + userName + "\"");

                            var password = PromptForInput(CommandTypes.userpass);

                            if (_service.User_SetPasswordV1(user.Id,
                                new UserAddPassword()
                                {
                                    UserId = user.Id,
                                    NewPassword = password,
                                    NewPasswordConfirm = password,
                                }).Result)
                                Console.WriteLine(Environment.NewLine + "SUCCESS set password for user \"" + userName + "\"");
                            else
                                throw new ConsoleHelpAsException("FAILED set password for user \"" + userName + "\"");
                        }

                        break;

                    default:
                        break;
                }

                return StandardOutput.FondFarewell();
            }
            catch (Exception ex)
            {
                return StandardOutput.AngryFarewell(ex);
            }
        }

        private string PromptForInput(CommandTypes cmd)
        {
            switch (cmd)
            {
                case CommandTypes.issuer:
                    Console.Write(Environment.NewLine + "ENTER issuer name : ");
                    break;

                case CommandTypes.client:
                    Console.Write(Environment.NewLine + "ENTER client name : ");
                    break;

                case CommandTypes.role:
                    Console.Write(Environment.NewLine + "ENTER role name : ");
                    break;

                case CommandTypes.user:
                    Console.Write(Environment.NewLine + "ENTER user name : ");
                    break;

                case CommandTypes.userpass:
                    Console.Write(Environment.NewLine + "ENTER user password : ");
                    return StandardInput.GetHiddenInput();
            }

            return StandardInput.GetInput();
        }
    }
}
