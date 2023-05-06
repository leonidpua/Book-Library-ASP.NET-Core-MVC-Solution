﻿using BookLibrary.Storage.Contexts;
using BookLibrary.Storage.Exceptions;
using BookLibrary.Storage.Models.Account;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace BookLibrary.Storage.Repositories
{
    public class AccountRepository
    {
        private readonly SessionRepository sessionRepository;

        public AccountRepository(SessionRepository sessionRepository)
        {
            this.sessionRepository = sessionRepository;
        }

        public int Login(string sessionId, string login, string password)
        {
            var inLogin = new SqlParameter
            {
                ParameterName = "Login",
                Value = login,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var inPassword = new SqlParameter
            {
                ParameterName = "Password",
                Value = password,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var outResult = new SqlParameter
            {
                ParameterName = "Result",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output
            };

            var sql = "exec LoginAccount @Login, @Password, @Result OUT";
            using (var dbContext = new BookLibraryContext())
            {
                _ = dbContext.Database.ExecuteSqlRaw(sql, inLogin, inPassword, outResult);
            }
            if (int.TryParse(outResult.Value.ToString(), out int accountId))
                if (accountId > 0)
                {
                    switch (sessionRepository.CheckSessionExpiration(sessionId))
                    {
                        case null:
                            if (!sessionRepository.RegisterSession(accountId, sessionId))
                                return 0;
                            break;
                        case true:
                            throw new SessionExpirationException("Application and DB session expiration time conflict.");
                    }
                    return accountId;
                }
            return 0;
        }

        public bool Logout(string sessionId)
        {
            var inSessionId = new SqlParameter
            {
                ParameterName = "SessionId",
                Value = sessionId,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var inCloseDate = new SqlParameter
            {
                ParameterName = "CloseDate",
                Value = DateTime.Now,
                DbType = System.Data.DbType.DateTime,
                Direction = System.Data.ParameterDirection.Input
            };
            var outResult = new SqlParameter
            {
                ParameterName = "Result",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output
            };
            var sql = "exec CloseSession @SessionId, @CloseDate, @Result OUT";
            using (var dbContext = new BookLibraryContext())
            {
                _ = dbContext.Database.ExecuteSqlRaw(sql, inSessionId, inCloseDate, outResult);
            }
            if (int.TryParse(outResult.Value.ToString(), out int result))
                if (result > 0)
                {
                    return true;
                }
            return false;
        }

        public int Register(string sessionId, string login, string password, string firstName, string lastName, string email)
        {
            var inLogin = new SqlParameter
            {
                ParameterName = "Login",
                Value = login,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var inPassword = new SqlParameter
            {
                ParameterName = "Password",
                Value = password,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var inFirstName = new SqlParameter
            {
                ParameterName = "FirstName",
                Value = firstName,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var inLastName = new SqlParameter
            {
                ParameterName = "LastName",
                Value = lastName,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var inEmail = new SqlParameter
            {
                ParameterName = "Email",
                Value = email,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var outResult = new SqlParameter
            {
                ParameterName = "Result",
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Output
            };

            var sql = "exec RegisterAccount @Login, @Password, @FirstName, @LastName, @Email, @Result OUT";
            using (var dbContext = new BookLibraryContext())
            {
                _ = dbContext.Database.ExecuteSqlRaw(sql, inLogin, inPassword, inFirstName, inLastName, inEmail, outResult);
            }
            if (int.TryParse(outResult.Value.ToString(), out int accountId))
                if (accountId == -1) return accountId;
            switch (sessionRepository.CheckSessionExpiration(sessionId))
            {
                case null:
                    if (!sessionRepository.RegisterSession(accountId, sessionId))
                        return 0;
                    break;
                case true:
                    throw new SessionExpirationException("Application and DB session expiration time conflict.");
            }
            return accountId;
        }

        public DisplayUserModel GetUser(int userId)
        {
            var inAccountId = new SqlParameter
            {
                ParameterName = "AccountId",
                Value = userId,
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Input
            };
            var outLogin = new SqlParameter
            {
                ParameterName = "Login",
                DbType = System.Data.DbType.String,
                Size = 32,
                Direction = System.Data.ParameterDirection.Output
            };
            var outFirstName = new SqlParameter
            {
                ParameterName = "FirstName",
                DbType = System.Data.DbType.String,
                Size = 32,
                Direction = System.Data.ParameterDirection.Output
            };
            var outLastName = new SqlParameter
            {
                ParameterName = "LastName",
                DbType = System.Data.DbType.String,
                Size = 32,
                Direction = System.Data.ParameterDirection.Output
            };
            var outEmail = new SqlParameter
            {
                ParameterName = "Email",
                DbType = System.Data.DbType.String,
                Size = 32,
                Direction = System.Data.ParameterDirection.Output
            };
            var sql = "exec GetUser @AccountId, @Login OUT, @FirstName OUT, @LastName OUT, @Email OUT";
            using (var dbContext = new BookLibraryContext())
            {
                _ = dbContext.Database.ExecuteSqlRaw(sql, inAccountId, outLogin, outFirstName, outLastName, outEmail);
            }
            if (!string.IsNullOrEmpty(outLogin.Value.ToString()))
            {
                return new DisplayUserModel
                {
                    Login = outLogin.Value.ToString(),
                    FirstName = outFirstName.Value.ToString(),
                    LastName = outLastName.Value.ToString(),
                    Email = outEmail.Value.ToString()
                };
            }
            return null;

        }

        public bool ChangeAccountPassword(int accountId, string accountPassword, string newAccountPassword)
        {
            var inAccountId = new SqlParameter
            {
                ParameterName = "AccountId",
                Value = accountId,
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Input
            };
            var inPassword = new SqlParameter
            {
                ParameterName = "Password",
                Value = accountPassword,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var inNewPassword = new SqlParameter
            {
                ParameterName = "NewPassword",
                Value = newAccountPassword,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var outResult = new SqlParameter
            {
                ParameterName = "Result",
                DbType = System.Data.DbType.Boolean,
                Direction = System.Data.ParameterDirection.Output
            };
            var sql = "exec ChangeAccountPassword @AccountId, @Password, @NewPassword, @Result OUT";
            using (var dbContext = new BookLibraryContext())
            {
                _ = dbContext.Database.ExecuteSqlRaw(sql, inAccountId, inPassword, inNewPassword, outResult);
            }
            bool.TryParse(outResult.Value.ToString(), out bool result);

            return result;
        }

        public bool DeleteAccount(int accountId, string accountPassword)
        {
            var inAccountId = new SqlParameter
            {
                ParameterName = "AccountId",
                Value = accountId,
                DbType = System.Data.DbType.Int32,
                Direction = System.Data.ParameterDirection.Input
            };
            var inPassword = new SqlParameter
            {
                ParameterName = "Password",
                Value = accountPassword,
                DbType = System.Data.DbType.String,
                Direction = System.Data.ParameterDirection.Input
            };
            var outResult = new SqlParameter
            {
                ParameterName = "Result",
                DbType = System.Data.DbType.Boolean,
                Direction = System.Data.ParameterDirection.Output
            };
            var sql = "exec DeleteAccount @AccountId, @Password, @Result OUT";
            using (var dbContext = new BookLibraryContext())
            {
                _ = dbContext.Database.ExecuteSqlRaw(sql, inAccountId, inPassword, outResult);
            }
            bool.TryParse(outResult.Value.ToString(), out bool result);

            return result;

        }
    }
}
