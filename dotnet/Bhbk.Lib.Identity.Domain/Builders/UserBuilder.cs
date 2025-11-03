using Bhbk.Lib.Cryptography.Entropy;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Domain.Configuration;
using Bhbk.Lib.Identity.Models.Admin;
using System;
using System.Collections.Generic;

namespace Bhbk.Lib.Identity.Domain.Builders
{
    public class UserBuilder : EntityBuilderBase<UserBuilder, tbl_User>
    {
        private string _userName;
        private string _email;
        private string _firstName;
        private string _lastName;
        private string _phoneNumber;
        private string _password;
        private bool _isHumanBeing = true;
        private bool _isLockedOut = false;
        private bool _isDeletable = true;
        private bool _emailConfirmed = true;
        private bool _passwordConfirmed = true;
        private bool _phoneNumberConfirmed = true;
        private readonly List<string> _roleNames = new List<string>();

        public UserBuilder WithUserName(string userName)
        {
            _userName = userName;
            _email = _email ?? userName;
            return Self;
        }

        public UserBuilder WithEmail(string email)
        {
            _email = email;
            return Self;
        }

        public UserBuilder WithName(string firstName, string lastName)
        {
            _firstName = firstName;
            _lastName = lastName;
            return Self;
        }

        public UserBuilder WithPhoneNumber(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return Self;
        }

        public UserBuilder WithPassword(string password)
        {
            _password = password;
            return Self;
        }

        public UserBuilder IsHumanBeing(bool isHuman = true)
        {
            _isHumanBeing = isHuman;
            return Self;
        }

        public UserBuilder IsLockedOut(bool lockedOut = true)
        {
            _isLockedOut = lockedOut;
            return Self;
        }

        public UserBuilder IsDeletable(bool deletable = true)
        {
            _isDeletable = deletable;
            return Self;
        }

        public UserBuilder EmailConfirmed(bool confirmed = true)
        {
            _emailConfirmed = confirmed;
            return Self;
        }

        public UserBuilder PasswordConfirmed(bool confirmed = true)
        {
            _passwordConfirmed = confirmed;
            return Self;
        }

        public UserBuilder PhoneNumberConfirmed(bool confirmed = true)
        {
            _phoneNumberConfirmed = confirmed;
            return Self;
        }

        public UserBuilder WithRole(string roleName)
        {
            _roleNames.Add(roleName);
            return Self;
        }

        public UserBuilder AsAdmin()
        {
            _roleNames.Add("Identity.Admins");
            _roleNames.Add("Alert.Admins");
            return Self;
        }

        public UserBuilder AsUser()
        {
            _roleNames.Add("Identity.Users");
            _roleNames.Add("Alert.Users");
            return Self;
        }

        public UserBuilder AsViewer()
        {
            _roleNames.Add("Identity.Viewers");
            _roleNames.Add("Alert.Viewers");
            return Self;
        }

        public UserBuilder WithDefaults()
        {
            var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
            _userName = $"testuser-{suffix}@test.local";
            _email = _userName;
            _firstName = "Test-" + AlphaNumeric.CreateString(4);
            _lastName = "User-" + AlphaNumeric.CreateString(4);
            _phoneNumber = NumberAs.CreateString(11);
            _password = "TestP@ss" + AlphaNumeric.CreateString(4) + "!";
            return Self;
        }

        public UserBuilder WithProductionAdminDefaults()
        {
            _userName = "admin@local";
            _email = _userName;
            _firstName = "Administrator";
            _lastName = "User";
            _password = "pa$$word01!";
            return Self;
        }

        public UserBuilder WithProductionUserDefaults()
        {
            _userName = "user@local";
            _email = _userName;
            _firstName = "Normal";
            _lastName = "User";
            _password = "pa$$word02!";
            return Self;
        }

        public UserBuilder FromTestSettings(TestUserSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _userName = settings.UserName;
            _email = settings.UserName;
            _phoneNumber = settings.PhoneNumber;
            _password = settings.PasswordCurrent;
            _firstName = "Test";
            _lastName = "User";
            return Self;
        }

        public override tbl_User Build()
        {
            if (string.IsNullOrEmpty(_userName))
                throw new InvalidOperationException("UserName is required");

            return Map.Map<tbl_User>(new UserV1
            {
                UserName = _userName,
                Email = _email ?? _userName,
                FirstName = _firstName ?? "FirstName",
                LastName = _lastName ?? "LastName",
                PhoneNumber = _phoneNumber,
                IsHumanBeing = _isHumanBeing,
                IsLockedOut = _isLockedOut,
                IsDeletable = _isDeletable,
                EmailConfirmed = _emailConfirmed,
                PasswordConfirmed = _passwordConfirmed,
                PhoneNumberConfirmed = _phoneNumberConfirmed,
            });
        }

        public string GetPassword() => _password ?? "DefaultP@ssword1!";

        public string GetUserName() => _userName;

        public IReadOnlyList<string> GetRoleNames() => _roleNames.AsReadOnly();
    }
}
