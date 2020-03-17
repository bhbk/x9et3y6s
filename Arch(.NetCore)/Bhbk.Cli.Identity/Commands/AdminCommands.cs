using Bhbk.Lib.CommandLine.IO;
using Bhbk.Lib.Common.FileSystem;
using Bhbk.Lib.DataState.Interfaces;
using Bhbk.Lib.DataState.Models;
using Bhbk.Lib.Identity.Grants;
using Bhbk.Lib.Identity.Models.Admin;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives;
using Bhbk.Lib.Identity.Primitives.Enums;
using Bhbk.Lib.Identity.Services;
using ManyConsole;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Identity.Commands
{
    public class AdminCommands : ConsoleCommand
    {
        private static CommandTypes _cmdType;
        private static IAdminService _service;
        private static string _cmdTypeList = string.Join(", ", Enum.GetNames(typeof(CommandTypes)));
        private static bool _create = false, _destroy = false, _list = false;

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

            HasOption("l=|list", "List entities", arg =>
            {
                _list = true;

                if (!Enum.TryParse<CommandTypes>(arg, out _cmdType))
                    throw new ConsoleHelpAsException("Invalid entity type. Possible are " + _cmdTypeList);
            });
        }

        public override int Run(string[] remainingArguments)
        {
            IssuerV1 issuer = null;
            AudienceV1 audience = null;
            LoginV1 login = null;
            RoleV1 role = null;
            UserV1 user = null;
            string issuerName = string.Empty;
            string audienceName = string.Empty;
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
                _service.Grant = new ResourceOwnerGrantV2(conf);

                if (_create == false && _destroy == false && _list == false)
                    throw new ConsoleHelpAsException("Invalid action type.");

                switch (_cmdType)
                {
                    case CommandTypes.issuer:

                        if (_create)
                        {
                            issuerName = PromptForInput(CommandTypes.issuer);
                            issuer = _service.Issuer_GetV1(issuerName).Result;

                            if (issuer != null)
                                Console.WriteLine(Environment.NewLine + "FOUND issuer \"" + issuer.Name + "\""
                                    + Environment.NewLine + "\tID is " + issuer.Id.ToString());
                            else
                            {
                                issuer = _service.Issuer_CreateV1(new IssuerV1()
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
                            issuerName = PromptForInput(CommandTypes.issuer);
                            issuer = _service.Issuer_GetV1(issuerName).Result;

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
                        else if (_list)
                        {
                            var issuers = _service.Issuer_GetV1(new DataStateV1()
                            {
                                Sort = new List<IDataStateSort>()
                                {
                                    new DataStateV1Sort() { Field = "name", Dir = "asc" }
                                },
                                Skip = 0,
                                Take = 100
                            }).Result;

                            foreach (var issuerEntry in issuers.Data.OrderBy(x => x.Name))
                                Console.WriteLine($"\t{issuerEntry.Name} [{issuerEntry.Id}]");
                        }

                        break;

                    case CommandTypes.audience:

                        if (_create)
                        {
                            issuerName = PromptForInput(CommandTypes.issuer);
                            issuer = _service.Issuer_GetV1(issuerName).Result;

                            if (issuer == null)
                                throw new ConsoleHelpAsException("FAILED find issuer \"" + issuerName + "\"");

                            try
                            {
                                audienceName = PromptForInput(CommandTypes.audience);
                                audience = _service.Audience_GetV1(audienceName).Result;
                            }
                            catch (Exception) { }

                            if (audience != null)
                                Console.WriteLine(Environment.NewLine + "FOUND audience \"" + audienceName
                                    + Environment.NewLine + "\tID is " + audience.Id.ToString());
                            else
                            {
                                audience = _service.Audience_CreateV1(new AudienceV1()
                                {
                                    IssuerId = issuer.Id,
                                    Name = audienceName,
                                    Enabled = true,
                                }).Result;

                                if (audience != null)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS create audience \"" + audienceName + "\""
                                        + Environment.NewLine + "\tID is " + audience.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED create audience \"" + audienceName + "\"");
                            }
                        }
                        else if (_destroy)
                        {
                            issuerName = PromptForInput(CommandTypes.issuer);
                            issuer = _service.Issuer_GetV1(issuerName).Result;

                            if (issuer == null)
                                throw new ConsoleHelpAsException("FAILED find issuer \"" + issuerName + "\"");

                            try
                            {
                                audienceName = PromptForInput(CommandTypes.audience);
                                audience = _service.Audience_GetV1(audienceName).Result;
                            }
                            catch (Exception) { }

                            if (audience == null)
                                Console.WriteLine(Environment.NewLine + "FAILED find audience \"" + audienceName + "\"");
                            else
                            {
                                if (_service.Audience_DeleteV1(audience.Id).Result)
                                    Console.WriteLine(Environment.NewLine + "SUCCESS destroy audience \"" + audienceName + "\""
                                        + Environment.NewLine + "\tID is " + audience.Id.ToString());
                                else
                                    throw new ConsoleHelpAsException("FAILED destroy audience \"" + audienceName + "\""
                                        + Environment.NewLine + "\tID is " + audience.Id.ToString());
                            }
                        }
                        else if (_list)
                        {
                            var audiences = _service.Audience_GetV1(new DataStateV1()
                            {
                                Sort = new List<IDataStateSort>()
                                {
                                    new DataStateV1Sort() { Field = "name", Dir = "asc" }
                                },
                                Skip = 0,
                                Take = 100
                            }).Result;

                            foreach (var audienceEntry in audiences.Data.OrderBy(x => x.Name))
                                Console.WriteLine($"\t{audienceEntry.Name} [{audienceEntry.Id}]");
                        }

                        break;

                    case CommandTypes.login:

                        if (_create)
                        {
                            try
                            {
                                loginName = PromptForInput(CommandTypes.login);
                                login = _service.Login_GetV1(loginName).Result;
                            }
                            catch (Exception) { }

                            if (login != null)
                                Console.WriteLine(Environment.NewLine + "FOUND login \"" + loginName
                                    + Environment.NewLine + "\tID is " + login.Id.ToString());
                            else
                            {
                                login = _service.Login_CreateV1(new LoginV1()
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
                            try
                            {
                                loginName = PromptForInput(CommandTypes.login);
                                login = _service.Login_GetV1(loginName).Result;
                            }
                            catch (Exception) { }

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
                        else if (_list)
                        {
                            var logins = _service.Login_GetV1(new DataStateV1()
                            {
                                Sort = new List<IDataStateSort>()
                                {
                                    new DataStateV1Sort() { Field = "name", Dir = "asc" }
                                },
                                Skip = 0,
                                Take = 100
                            }).Result;

                            foreach (var loginEntry in logins.Data.OrderBy(x => x.Name))
                                Console.WriteLine($"\t{loginEntry.Name} [{loginEntry.Id}]");
                        }

                        break;

                    case CommandTypes.role:

                        if (_create)
                        {
                            audienceName = PromptForInput(CommandTypes.audience);
                            audience = _service.Audience_GetV1(audienceName).Result;

                            if (audience == null)
                                throw new ConsoleHelpAsException("FAILED find audience \"" + audienceName + "\"");

                            try
                            {
                                roleName = PromptForInput(CommandTypes.role);
                                role = _service.Role_GetV1(roleName).Result;
                            }
                            catch (Exception) { }

                            if (role != null)
                                Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                                    + Environment.NewLine + "\tID is " + role.Id.ToString());
                            else
                            {
                                role = _service.Role_CreateV1(new RoleV1()
                                {
                                    AudienceId = audience.Id,
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
                            audienceName = PromptForInput(CommandTypes.audience);
                            audience = _service.Audience_GetV1(audienceName).Result;

                            if (audience == null)
                                throw new ConsoleHelpAsException("FAILED find audience \"" + audienceName + "\"");

                            try
                            {
                                roleName = PromptForInput(CommandTypes.role);
                                role = _service.Role_GetV1(roleName).Result;
                            }
                            catch (Exception) { }

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
                        else if (_list)
                        {
                            var roles = _service.Role_GetV1(new DataStateV1()
                            {
                                Sort = new List<IDataStateSort>()
                                {
                                    new DataStateV1Sort() { Field = "name", Dir = "asc" }
                                },
                                Skip = 0,
                                Take = 100
                            }).Result;

                            foreach (var roleEntry in roles.Data.OrderBy(x => x.Name))
                                Console.WriteLine($"\t{roleEntry.Name} [{roleEntry.Id}]");
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

                        if (_create)
                        {
                            roleName = PromptForInput(CommandTypes.role);
                            role = _service.Role_GetV1(roleName).Result;

                            if (role != null)
                                Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                                    + Environment.NewLine + "\tID is " + role.Id.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find role \"" + roleName + "\"");

                            if (_service.User_AddToRoleV1(user.Id, role.Id).Result)
                                Console.WriteLine(Environment.NewLine + "SUCCESS add role \"" + roleName + "\" to user \"" + userName + "\"");
                            else
                                Console.WriteLine(Environment.NewLine + "FAILED add role \"" + roleName + "\" to user \"" + userName + "\"");
                        }
                        else if (_destroy)
                        {
                            roleName = PromptForInput(CommandTypes.role);
                            role = _service.Role_GetV1(roleName).Result;

                            if (role != null)
                                Console.WriteLine(Environment.NewLine + "FOUND role \"" + roleName + "\""
                                    + Environment.NewLine + "\tID is " + role.Id.ToString());
                            else
                                throw new ConsoleHelpAsException("FAILED find role \"" + roleName + "\"");

                            if (_service.User_RemoveFromRoleV1(user.Id, role.Id).Result)
                                Console.WriteLine(Environment.NewLine + "SUCCESS remove role \"" + roleName + "\" from user \"" + userName + "\"");
                            else
                                Console.WriteLine(Environment.NewLine + "FAILED remove role \"" + roleName + "\" from user \"" + userName + "\"");
                        }
                        else if (_list)
                        {
                            var roles = _service.User_GetRolesV1(user.UserName).Result;

                            foreach (var roleEntry in roles.OrderBy(x => x.Name))
                                Console.WriteLine($"\t{roleEntry.Name} [{roleEntry.Id}]");
                        }

                        break;

                    case CommandTypes.user:

                        if (_create)
                        {
                            try
                            {
                                userName = PromptForInput(CommandTypes.user);
                                user = _service.User_GetV1(userName).Result;
                            }
                            catch (Exception) { }

                            if (user != null)
                                Console.WriteLine(Environment.NewLine + "FOUND user \"" + userName + "\""
                                    + Environment.NewLine + "\tID is " + user.Id.ToString());
                            else
                            {
                                Console.Write(Environment.NewLine + "ENTER first name : ");
                                var firstName = StandardInput.GetInput();

                                Console.Write(Environment.NewLine + "ENTER last name : ");
                                var lastName = StandardInput.GetInput();

                                user = _service.User_CreateV1NoConfirm(new UserV1()
                                {
                                    UserName = userName,
                                    Email = userName,
                                    FirstName = firstName,
                                    LastName = lastName,
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

                            loginName = Constants.DefaultLogin;
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
                            try
                            {
                                userName = PromptForInput(CommandTypes.user);
                                user = _service.User_GetV1(userName).Result;
                            }
                            catch (Exception) { }

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
                        else if (_list)
                        {
                            var users = _service.User_GetV1(new DataStateV1() 
                            {
                                Sort = new List<IDataStateSort>()
                                {
                                    new DataStateV1Sort() { Field = "userName", Dir = "asc" }
                                },
                                Skip = 0,
                                Take = 100
                            }).Result;

                            foreach (var userEntry in users.Data.OrderBy(x => x.UserName))
                            {
                                Console.WriteLine($"  {userEntry.UserName} [{userEntry.Id}]");

                                var roles = _service.User_GetRolesV1(userEntry.Id.ToString()).Result;

                                foreach(var roleEntry in roles.OrderBy(x => x.Name))
                                    Console.WriteLine($"    {roleEntry.Name} [{roleEntry.Id}]");

                                Console.WriteLine();
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
                                new PasswordAddV1()
                                {
                                    EntityId = user.Id,
                                    NewPassword = password,
                                    NewPasswordConfirm = password,
                                }).Result)
                                Console.WriteLine(Environment.NewLine + "SUCCESS set password for user \"" + userName + "\"");
                            else
                                throw new ConsoleHelpAsException("FAILED set password for user \"" + userName + "\"");
                        }
                        else if (_list)
                        {
                            throw new ConsoleHelpAsException("NO SUPPORT to list password for \"" + userName + "\""
                                + Environment.NewLine + "\tID is \"" + user.Id.ToString() + "\"");
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
                    return StandardInput.GetInput();

                case CommandTypes.audience:
                    Console.Write(Environment.NewLine + "ENTER audience name : ");
                    return StandardInput.GetInput();

                case CommandTypes.role:
                    Console.Write(Environment.NewLine + "ENTER role name : ");
                    return StandardInput.GetInput();

                case CommandTypes.user:
                    Console.Write(Environment.NewLine + "ENTER user name : ");
                    return StandardInput.GetInput();

                case CommandTypes.userpass:
                    Console.Write(Environment.NewLine + "ENTER user password : ");
                    return StandardInput.GetHiddenInput();
            }

            throw new InvalidOperationException();
        }
    }
}
