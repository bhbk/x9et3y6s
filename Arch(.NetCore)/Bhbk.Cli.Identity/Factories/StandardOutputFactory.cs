using Bhbk.Lib.Identity.Data_EF6.Infrastructure;
using Bhbk.Lib.Identity.Data_EF6.Models;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bhbk.Cli.Identity.Factories
{
    public class StandardOutputFactory
    {
        public static void Audiences(IUnitOfWork uow, IEnumerable<E_Audience> audiences, bool? detail = null)
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
                    $" [password] {(string.IsNullOrEmpty(audience.PasswordHashPBKDF2) ? "present" : "missing")}" +
                    $" [created] {audience.CreatedUtc.LocalDateTime}");

                if (detail.HasValue && detail.Value)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    var roleList = audience.AudienceRoles.Select(t => t.RoleId).ToList();
                    if (roleList.Count > 0)
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** member roles ***");

                        var roles = uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<E_Role>()
                            .Where(x => roleList.Any(y => y == x.Id)).ToLambda())
                            .OrderBy(x => x.Name);

                        foreach (var role in roles.OrderBy(x => x.Name))
                            Console.Out.WriteLine($"    [role GUID] {role.Id} [name] {role.Name}");
                    }

                    if (audience.Roles.Count > 0)
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** subordinate roles ***");
                        foreach (var role in audience.Roles.OrderBy(x => x.Name))
                            Console.Out.WriteLine($"    [role GUID] {role.Id} [name] {role.Name}");
                    }
                }

                Console.ResetColor();
            }
        }

        public static void Issuers(IUnitOfWork uow, IEnumerable<E_Issuer> issuers, bool? detail = null)
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

                    if (issuer.Audiences.Count > 0)
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** subordinate audiences ***");

                        foreach (var audience in issuer.Audiences.OrderBy(x => x.Name))
                            Console.Out.WriteLine($"    [audience GUID] {audience.Id} [name] {audience.Name}");
                    }
                }

                Console.ResetColor();
            }
        }

        public static void Logins(IUnitOfWork uow, IEnumerable<E_Login> logins, bool? detail = null)
        {
            foreach (var login in logins)
            {
                if (detail.HasValue && detail.Value)
                {
                    Console.Out.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                    Console.ForegroundColor = ConsoleColor.White;

                Console.Out.WriteLine($"  [login GUID] {login.Id} [name] {login.Name} {(!login.IsDeletable ? "is not deletable " : null)}" +
                    $"[created] {login.CreatedUtc.LocalDateTime}");

                if (detail.HasValue && detail.Value)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    var userList = login.UserLogins.Select(t => t.UserId).ToList();
                    if (userList.Count > 0)
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** member users ***");

                        var users = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<E_User>()
                            .Where(x => userList.Any(y => y == x.Id)).ToLambda())
                            .OrderBy(x => x.UserName);

                        foreach (var user in users)
                            Console.Out.WriteLine($"    [user GUID] {user.Id} [name] {user.UserName}");
                    }
                }

                Console.ResetColor();
            }
        }

        public static void Roles(IUnitOfWork uow, IEnumerable<E_Role> roles, bool? detail = null)
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

                    var userList = role.UserRoles.Select(t => t.UserId).ToList();
                    if (userList.Count > 0)
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** member users ***");

                        var users = uow.Users.Get(QueryExpressionFactory.GetQueryExpression<E_User>()
                            .Where(x => userList.Any(y => y == x.Id)).ToLambda())
                            .OrderBy(x => x.UserName);

                        foreach (var user in users)
                            Console.Out.WriteLine($"    [user GUID] {user.Id} [name] {user.UserName}");
                    }
                }

                Console.ResetColor();
            }
        }

        public static void Users(IUnitOfWork uow, IEnumerable<E_User> users, bool? detail = null)
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
                    $" [password] {(string.IsNullOrEmpty(user.PasswordHashPBKDF2) ? "present" : "missing")}" +
                    $" [created] {user.CreatedUtc.LocalDateTime}");

                if (detail.HasValue && detail.Value)
                {
                    Console.ForegroundColor = ConsoleColor.White;

                    var roleList = user.UserRoles.Select(t => t.RoleId).ToList();
                    if (roleList.Count > 0)
                    {
                        Console.Out.WriteLine();
                        Console.Out.WriteLine($"  *** member roles ***");

                        var roles = uow.Roles.Get(QueryExpressionFactory.GetQueryExpression<E_Role>()
                            .Where(x => roleList.Any(y => y == x.Id)).ToLambda())
                            .OrderBy(x => x.Name);

                        foreach (var role in roles)
                            Console.Out.WriteLine($"    [role GUID] {role.Id} [name] {role.Name}");
                    }
                }

                Console.ResetColor();
            }
        }
    }
}
