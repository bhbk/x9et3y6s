using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Identity.Factories
{
    public class FormatOutput
    {
        public static void Audiences(IUnitOfWork uow, IEnumerable<tbl_Audience> audiences, bool? detail = null)
        {
            foreach (var audience in audiences)
            {
                if (detail.HasValue && detail.Value)
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"  [audience GUID] {audience.Id} [name] {audience.Name}{(!audience.IsDeletable ? " is not deletable" : null)}" +
                    $" [password] {(string.IsNullOrEmpty(audience.PasswordHashPBKDF2) ? "missing" : "present")}" +
                    $" [created] {audience.CreatedUtc.LocalDateTime}");

                if (detail.HasValue && detail.Value)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    var roles = uow.Audiences.GetRolesForAudience(audience.Id).OrderBy(x => x.Name);
                    if (roles.Any())
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** member roles ***");

                        foreach (var role in roles)
                            Console.Out.WriteLine($"    [role GUID] {role.Id} [name] {role.Name}");
                    }

                    var subordinateRoles = uow.Roles.Get(x => x.AudienceId == audience.Id).OrderBy(x => x.Name);
                    if (subordinateRoles.Any())
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** subordinate roles ***");
                        foreach (var role in subordinateRoles)
                            Console.Out.WriteLine($"    [role GUID] {role.Id} [name] {role.Name}");
                    }
                }

                Console.ResetColor();
            }
        }

        public static void Issuers(IUnitOfWork uow, IEnumerable<tbl_Issuer> issuers, bool? detail = null)
        {
            foreach (var issuer in issuers)
            {
                if (detail.HasValue && detail.Value)
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"  [issuer GUID] {issuer.Id} [name] {issuer.Name} {(!issuer.IsDeletable ? "is not deletable " : null)}" +
                    $"[created] {issuer.CreatedUtc.LocalDateTime}");

                if (detail.HasValue && detail.Value)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    var audiences = uow.Issuers.GetAudiencesForIssuer(issuer.Id).OrderBy(x => x.Name);
                    if (audiences.Any())
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** subordinate audiences ***");

                        foreach (var audience in audiences)
                            Console.Out.WriteLine($"    [audience GUID] {audience.Id} [name] {audience.Name}");
                    }
                }

                Console.ResetColor();
            }
        }

        public static void LoginProviders(IUnitOfWork uow, IEnumerable<tbl_LoginProvider> loginProviders, bool? detail = null)
        {
            foreach (var loginProvider in loginProviders)
            {
                if (detail.HasValue && detail.Value)
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"  [login provider GUID] {loginProvider.Id} [name] {loginProvider.Name} {(!loginProvider.IsDeletable ? "is not deletable " : null)}" +
                    $"[created] {loginProvider.CreatedUtc.LocalDateTime}");

                if (detail.HasValue && detail.Value)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    var users = uow.LoginProviders.GetUsersWithLoginProvider(loginProvider.Id).OrderBy(x => x.UserName);
                    if (users.Any())
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** member users ***");

                        foreach (var user in users)
                            Console.Out.WriteLine($"    [user GUID] {user.Id} [name] {user.UserName}");
                    }
                }

                Console.ResetColor();
            }
        }

        public static void Roles(IUnitOfWork uow, IEnumerable<tbl_Role> roles, bool? detail = null)
        {
            foreach (var role in roles)
            {
                if (detail.HasValue && detail.Value)
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"  [role GUID] {role.Id} [name] {role.Name} {(!role.IsDeletable ? "is not deletable " : null)}" +
                    $"[created] {role.CreatedUtc.LocalDateTime}");

                if (detail.HasValue && detail.Value)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    var users = uow.Roles.GetUsersInRole(role.Id).OrderBy(x => x.UserName);
                    if (users.Any())
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** member users ***");

                        foreach (var user in users)
                            Console.Out.WriteLine($"    [user GUID] {user.Id} [name] {user.UserName}");
                    }
                }

                Console.ResetColor();
            }
        }

        public static void Users(IUnitOfWork uow, IEnumerable<tbl_User> users, bool? detail = null)
        {
            foreach (var user in users)
            {
                if (detail.HasValue && detail.Value)
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"  [user GUID] {user.Id} [name] {user.UserName} {(!user.IsDeletable ? "is not deletable " : null)}" +
                    $" [password] {(string.IsNullOrEmpty(user.PasswordHashPBKDF2) ? "missing" : "present")}" +
                    $" [created] {user.CreatedUtc.LocalDateTime}");

                if (detail.HasValue && detail.Value)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    var roles = uow.Users.GetRolesForUser(user.Id).OrderBy(x => x.Name);
                    if (roles.Any())
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** member roles ***");

                        foreach (var role in roles)
                            Console.Out.WriteLine($"    [role GUID] {role.Id} [name] {role.Name}");
                    }
                }

                Console.ResetColor();
            }
        }
    }
}
