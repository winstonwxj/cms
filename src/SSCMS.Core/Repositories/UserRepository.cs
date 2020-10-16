﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Datory;
using SqlKata;
using SSCMS.Core.Utils;
using SSCMS.Enums;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Core.Repositories
{
    public partial class UserRepository : IUserRepository
    {
        private readonly Repository<User> _repository;
        private readonly ICacheManager<bool> _cacheManager;
        private readonly IConfigRepository _configRepository;

        public UserRepository(ISettingsManager settingsManager, ICacheManager<bool> cacheManager, IConfigRepository configRepository)
        {
            _repository = new Repository<User>(settingsManager.Database, settingsManager.Redis);
            _cacheManager = cacheManager;
            _configRepository = configRepository;
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;
        public List<TableColumn> TableColumns => _repository.TableColumns;

        private async Task<(bool success, string errorMessage)> InsertValidateAsync(string userName, string email, string mobile, string password, string ipAddress)
        {
            var config = await _configRepository.GetAsync();
            if (await IsIpAddressCachedAsync(ipAddress))
            {
                return (false, $"同一IP在{config.UserRegistrationMinMinutes}分钟内只能注册一次");
            }
            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }
            if (password.Length < config.UserPasswordMinLength)
            {
                return (false, $"密码长度必须大于等于{config.UserPasswordMinLength}");
            }
            if (!PasswordRestrictionUtils.IsValid(password, config.UserPasswordRestriction))
            {
                return (false, $"密码不符合规则，请包含{config.UserPasswordRestriction.GetDisplayName()}");
            }
            if (string.IsNullOrEmpty(userName))
            {
                return (false, "用户名为空，请填写用户名");
            }
            if (!string.IsNullOrEmpty(userName) && await IsUserNameExistsAsync(userName))
            {
                return (false, "用户名已被注册，请更换用户名");
            }
            if (!IsUserNameCompliant(userName.Replace("@", string.Empty).Replace(".", string.Empty)))
            {
                return (false, "用户名包含不规则字符，请更换用户名");
            }
            
            if (!string.IsNullOrEmpty(email) && await IsEmailExistsAsync(email))
            {
                return (false, "电子邮件地址已被注册，请更换邮箱");
            }
            if (!string.IsNullOrEmpty(mobile) && await IsMobileExistsAsync(mobile))
            {
                return (false, "手机号码已被注册，请更换手机号码");
            }

            return (true, string.Empty);
        }

        private async Task<(User user, string errorMessage)> UpdateValidateAsync(User user)
        {
            if (user == null || user.Id <= 0)
            {
                return (null, "用户不存在");
            }

            var entity = await GetByUserIdAsync(user.Id);
            user.UserName = entity.UserName;
            user.Password = entity.Password;
            user.PasswordFormat = entity.PasswordFormat;
            user.PasswordSalt = entity.PasswordSalt;

            if (entity.Mobile != user.Mobile && !string.IsNullOrEmpty(user.Mobile) && await IsMobileExistsAsync(user.Mobile))
            {
                return (null, "手机号码已存在");
            }

            if (entity.Email != user.Email && !string.IsNullOrEmpty(user.Email) && await IsEmailExistsAsync(user.Email))
            {
                return (null, "邮箱地址已存在");
            }

            return (entity, string.Empty);
        }

        public async Task<(User user, string errorMessage)> InsertAsync(User user, string password, string ipAddress)
        {
            var config = await _configRepository.GetAsync();
            if (!config.IsUserRegistrationAllowed)
            {
                return (null, "对不起，系统已禁止新用户注册！");
            }

            user.Checked = config.IsUserRegistrationChecked;
            if (StringUtils.IsMobile(user.UserName) && string.IsNullOrEmpty(user.Mobile))
            {
                user.Mobile = user.UserName;
            }

            var (success, errorMessage) = await InsertValidateAsync(user.UserName, user.Email, user.Mobile, password, ipAddress);
            if (!success)
            {
                return (null, errorMessage);
            }

            var passwordSalt = GenerateSalt();
            password = EncodePassword(password, PasswordFormat.Encrypted, passwordSalt);
            user.LastActivityDate = DateTime.Now;
            user.LastResetPasswordDate = DateTime.Now;

            user.Id = await InsertWithoutValidationAsync(user, password, PasswordFormat.Encrypted, passwordSalt);

            await CacheIpAddressAsync(ipAddress);

            return (user, string.Empty);
        }

        private async Task<int> InsertWithoutValidationAsync(User user, string password, PasswordFormat passwordFormat, string passwordSalt)
        {
            user.LastActivityDate = DateTime.Now;
            user.LastResetPasswordDate = DateTime.Now;

            user.Password = password;
            user.PasswordFormat = passwordFormat;
            user.PasswordSalt = passwordSalt;

            user.Id = await _repository.InsertAsync(user);

            return user.Id;
        }

        public async Task<(bool success, string errorMessage)> IsPasswordCorrectAsync(string password)
        {
            var config = await _configRepository.GetAsync();
            if (string.IsNullOrEmpty(password))
            {
                return (false, "密码不能为空");
            }
            if (password.Length < config.UserPasswordMinLength)
            {
                return (false, $"密码长度必须大于等于{config.UserPasswordMinLength}");
            }
            if (!PasswordRestrictionUtils.IsValid(password, config.UserPasswordRestriction))
            {
                return (false, $"密码不符合规则，请包含{config.UserPasswordRestriction.GetDisplayName()}");
            }
            return (true, string.Empty);
        }

        public async Task<(bool success, string errorMessage)> UpdateAsync(User user)
        {
            var (entity, errorMessage) = await UpdateValidateAsync(user);
            if (entity == null)
            {
                return (false, errorMessage);
            }

            await _repository.UpdateAsync(user, Q.CachingRemove(GetCacheKeysToRemove(entity)));
            return (true, string.Empty);
        }

        private async Task UpdateLastActivityDateAndCountOfFailedLoginAsync(User user)
        {
            if (user == null) return;

            user.LastActivityDate = DateTime.Now;
            user.CountOfFailedLogin += 1;

            await _repository.UpdateAsync(Q
                .Set(nameof(User.LastActivityDate), user.LastActivityDate)
                .Set(nameof(User.CountOfFailedLogin), user.CountOfFailedLogin)
                .Where(nameof(User.Id), user.Id)
                .CachingRemove(GetCacheKeysToRemove(user))
            );
        }

        public async Task UpdateLastActivityDateAndCountOfLoginAsync(User user)
        {
            if (user == null) return;

            user.LastActivityDate = DateTime.Now;
            user.CountOfLogin += 1;
            user.CountOfFailedLogin = 0;

            await _repository.UpdateAsync(Q
                .Set(nameof(User.LastActivityDate), user.LastActivityDate)
                .Set(nameof(User.CountOfLogin), user.CountOfLogin)
                .Set(nameof(User.CountOfFailedLogin), user.CountOfFailedLogin)
                .Where(nameof(User.Id), user.Id)
                .CachingRemove(GetCacheKeysToRemove(user))
            );
        }

        public async Task UpdateLastActivityDateAsync(User user)
        {
            if (user == null) return;

            user.LastActivityDate = DateTime.Now;

            await _repository.UpdateAsync(Q
                .Set(nameof(User.LastActivityDate), user.LastActivityDate)
                .Where(nameof(User.Id), user.Id)
                .CachingRemove(GetCacheKeysToRemove(user))
            );
        }

        private static string EncodePassword(string password, PasswordFormat passwordFormat, string passwordSalt)
        {
            var retVal = string.Empty;

            if (passwordFormat == PasswordFormat.Clear)
            {
                retVal = password;
            }
            else if (passwordFormat == PasswordFormat.Hashed)
            {
                var src = Encoding.Unicode.GetBytes(password);
                var buffer2 = Convert.FromBase64String(passwordSalt);
                var dst = new byte[buffer2.Length + src.Length];
                byte[] inArray = null;
                Buffer.BlockCopy(buffer2, 0, dst, 0, buffer2.Length);
                Buffer.BlockCopy(src, 0, dst, buffer2.Length, src.Length);
                var algorithm = HashAlgorithm.Create("SHA1");
                if (algorithm != null) inArray = algorithm.ComputeHash(dst);

                if (inArray != null) retVal = Convert.ToBase64String(inArray);
            }
            else if (passwordFormat == PasswordFormat.Encrypted)
            {
                retVal = TranslateUtils.EncryptStringBySecretKey(password, passwordSalt);

                //var des = new DesEncryptor
                //{
                //    InputString = password,
                //    EncryptKey = passwordSalt
                //};
                //des.DesEncrypt();

                //retVal = des.OutString;
            }
            return retVal;
        }

        private static string DecodePassword(string password, PasswordFormat passwordFormat, string passwordSalt)
        {
            var retVal = string.Empty;
            if (passwordFormat == PasswordFormat.Clear)
            {
                retVal = password;
            }
            else if (passwordFormat == PasswordFormat.Hashed)
            {
                throw new Exception("can not decode hashed password");
            }
            else if (passwordFormat == PasswordFormat.Encrypted)
            {
                retVal = TranslateUtils.DecryptStringBySecretKey(password, passwordSalt);

                //var des = new DesEncryptor
                //{
                //    InputString = password,
                //    DecryptKey = passwordSalt
                //};
                //des.DesDecrypt();

                //retVal = des.OutString;
            }
            return retVal;
        }

        private static string GenerateSalt()
        {
            var data = new byte[0x10];
            new RNGCryptoServiceProvider().GetBytes(data);
            return Convert.ToBase64String(data);
        }

        public async Task<(bool success, string errorMessage)> ChangePasswordAsync(int userId, string password)
        {
            var config = await _configRepository.GetAsync();
            if (password.Length < config.UserPasswordMinLength)
            {
                return (false, $"密码长度必须大于等于{config.UserPasswordMinLength}");
            }
            if (!PasswordRestrictionUtils.IsValid(password, config.UserPasswordRestriction))
            {
                return (false, $"密码不符合规则，请包含{config.UserPasswordRestriction.GetDisplayName()}");
            }

            var passwordSalt = GenerateSalt();
            password = EncodePassword(password, PasswordFormat.Encrypted, passwordSalt);
            await ChangePasswordAsync(userId, PasswordFormat.Encrypted, passwordSalt, password);
            return (true, string.Empty);
        }

        private async Task ChangePasswordAsync(int userId, PasswordFormat passwordFormat, string passwordSalt, string password)
        {
            var user = await GetByUserIdAsync(userId);
            if (user == null) return;

            user.LastResetPasswordDate = DateTime.Now;

            await _repository.UpdateAsync(Q
                .Set(nameof(User.Password), password)
                .Set(nameof(User.PasswordFormat), passwordFormat.GetValue())
                .Set(nameof(User.PasswordSalt), passwordSalt)
                .Set(nameof(User.LastResetPasswordDate), user.LastResetPasswordDate)
                .Where(nameof(User.Id), user.Id)
                .CachingRemove(GetCacheKeysToRemove(user))
            );
        }

        public async Task CheckAsync(IList<int> userIds)
        {
            var cacheKeys = new List<string>();
            foreach (var userId in userIds)
            {
                var user = await GetByUserIdAsync(userId);
                cacheKeys.AddRange(GetCacheKeysToRemove(user));
            }

            await _repository.UpdateAsync(Q
                .Set(nameof(User.Checked), true)
                .WhereIn(nameof(User.Id), userIds)
                .CachingRemove(cacheKeys.ToArray())
            );
        }

        public async Task LockAsync(IList<int> userIds)
        {
            var cacheKeys = new List<string>();
            foreach (var userId in userIds)
            {
                var user = await GetByUserIdAsync(userId);
                cacheKeys.AddRange(GetCacheKeysToRemove(user));
            }

            await _repository.UpdateAsync(Q
                .Set(nameof(User.Locked), true)
                .WhereIn(nameof(User.Id), userIds)
                .CachingRemove(cacheKeys.ToArray())
            );
        }

        public async Task UnLockAsync(IList<int> userIds)
        {
            var cacheKeys = new List<string>();
            foreach (var userId in userIds)
            {
                var user = await GetByUserIdAsync(userId);
                cacheKeys.AddRange(GetCacheKeysToRemove(user));
            }

            await _repository.UpdateAsync(Q
                .Set(nameof(User.Locked), false)
                .Set(nameof(User.CountOfFailedLogin), 0)
                .WhereIn(nameof(User.Id), userIds)
                .CachingRemove(cacheKeys.ToArray())
            );
        }

        public async Task<bool> IsUserNameExistsAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return false;

            return await _repository.ExistsAsync(Q.Where(nameof(User.UserName), userName));
        }

        private static bool IsUserNameCompliant(string userName)
        {
            if (userName.IndexOf("　", StringComparison.Ordinal) != -1 || userName.IndexOf(" ", StringComparison.Ordinal) != -1 || userName.IndexOf("'", StringComparison.Ordinal) != -1 || userName.IndexOf(":", StringComparison.Ordinal) != -1 || userName.IndexOf(".", StringComparison.Ordinal) != -1)
            {
                return false;
            }
            return DirectoryUtils.IsDirectoryNameCompliant(userName);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            var exists = await IsUserNameExistsAsync(email);
            if (exists) return true;

            return await _repository.ExistsAsync(Q.Where(nameof(User.Email), email));
        }

        public async Task<bool> IsMobileExistsAsync(string mobile)
        {
            if (string.IsNullOrEmpty(mobile)) return false;

            var exists = await IsUserNameExistsAsync(mobile);
            if (exists) return true;

            return await _repository.ExistsAsync(Q.Where(nameof(User.Mobile), mobile));
        }

        public async Task<List<int>> GetUserIdsAsync(bool isChecked)
        {
            return await _repository.GetAllAsync<int>(Q
                .Where(nameof(User.Checked), isChecked)
                .OrderByDesc(nameof(User.Id))
            );
        }

        public bool CheckPassword(string password, bool isPasswordMd5, string dbPassword, PasswordFormat passwordFormat, string passwordSalt)
        {
            var decodePassword = DecodePassword(dbPassword, passwordFormat, passwordSalt);
            if (isPasswordMd5)
            {
                return password == AuthUtils.Md5ByString(decodePassword);
            }
            return password == decodePassword;
        }

        public async Task<(User user, string userName, string errorMessage)> ValidateAsync(string account, string password, bool isPasswordMd5)
        {
            if (string.IsNullOrEmpty(account))
            {
                return (null, null, "账号不能为空");
            }
            if (string.IsNullOrEmpty(password))
            {
                return (null, null, "密码不能为空");
            }

            var user = await GetByAccountAsync(account);

            if (string.IsNullOrEmpty(user?.UserName))
            {
                return (null, null, "帐号或密码错误");
            }

            if (!user.Checked)
            {
                return (null, user.UserName, "此账号未审核，无法登录");
            }

            if (user.Locked)
            {
                return (null, user.UserName, "此账号被锁定，无法登录");
            }

            var config = await _configRepository.GetAsync();

            if (config.IsUserLockLogin)
            {
                if (user.CountOfFailedLogin > 0 && user.CountOfFailedLogin >= config.UserLockLoginCount)
                {
                    var lockType = TranslateUtils.ToEnum(config.UserLockLoginType, LockType.Hours);
                    if (lockType == LockType.Forever)
                    {
                        return (null, user.UserName, "此账号错误登录次数过多，已被永久锁定");
                    }
                    if (lockType == LockType.Hours && user.LastActivityDate.HasValue)
                    {
                        var ts = new TimeSpan(DateTime.Now.Ticks - user.LastActivityDate.Value.Ticks);
                        var hours = Convert.ToInt32(config.UserLockLoginHours - ts.TotalHours);
                        if (hours > 0)
                        {
                            return (null, user.UserName, $"此账号错误登录次数过多，已被锁定，请等待{hours}小时后重试");
                        }
                    }
                }
            }

            var userEntity = await _repository.GetAsync(user.Id);

            if (!CheckPassword(password, isPasswordMd5, userEntity.Password, userEntity.PasswordFormat, userEntity.PasswordSalt))
            {
                await UpdateLastActivityDateAndCountOfFailedLoginAsync(user);
                return (null, user.UserName, "帐号或密码错误");
            }

            return (user, user.UserName, string.Empty);
        }

        public Dictionary<DateTime, int> GetTrackingDictionary(DateTime dateFrom, DateTime dateTo, string xType)
        {
            var analysisType = TranslateUtils.ToEnum(xType, AnalysisType.Day);
            var dict = new Dictionary<DateTime, int>();

            var builder = new StringBuilder();
            builder.Append($" AND CreateDate >= {SqlUtils.GetComparableDate(Database.DatabaseType, dateFrom)}");
            builder.Append($" AND CreateDate < {SqlUtils.GetComparableDate(Database.DatabaseType, dateTo)}");

            string sqlString = $@"
SELECT COUNT(*) AS AddNum, AddYear, AddMonth, AddDay FROM (
    SELECT {SqlUtils.GetDatePartYear(Database.DatabaseType, "CreateDate")} AS AddYear, {SqlUtils.GetDatePartMonth(Database.DatabaseType, "CreateDate")} AS AddMonth, {SqlUtils.GetDatePartDay(Database.DatabaseType, "CreateDate")} AS AddDay 
    FROM {TableName} 
    WHERE {SqlUtils.GetDateDiffLessThanDays(Database.DatabaseType, "CreateDate", 30.ToString())} {builder}
) DERIVEDTBL GROUP BY AddYear, AddMonth, AddDay ORDER BY AddYear, AddMonth, AddDay
";//添加日统计

            if (analysisType == AnalysisType.Month)
            {
                sqlString = $@"
SELECT COUNT(*) AS AddNum, AddYear, AddMonth FROM (
    SELECT {SqlUtils.GetDatePartYear(Database.DatabaseType, "CreateDate")} AS AddYear, {SqlUtils.GetDatePartMonth(Database.DatabaseType, "CreateDate")} AS AddMonth 
    FROM {TableName} 
    WHERE {SqlUtils.GetDateDiffLessThanMonths(Database.DatabaseType, "CreateDate", 12.ToString())} {builder}
) DERIVEDTBL GROUP BY AddYear, AddMonth ORDER BY AddYear, AddMonth
";//添加月统计
            }
            else if (analysisType == AnalysisType.Year)
            {
                sqlString = $@"
SELECT COUNT(*) AS AddNum, AddYear FROM (
    SELECT {SqlUtils.GetDatePartYear(Database.DatabaseType, "CreateDate")} AS AddYear
    FROM {TableName} 
    WHERE {SqlUtils.GetDateDiffLessThanYears(Database.DatabaseType, "CreateDate", 10.ToString())} {builder}
) DERIVEDTBL GROUP BY AddYear ORDER BY AddYear
";//添加年统计
            }

            using (var connection = _repository.Database.GetConnection())
            {
                using (var rdr = connection.ExecuteReader(sqlString))
                {
                    while (rdr.Read())
                    {
                        var accessNum = rdr.IsDBNull(0) ? 0 : rdr.GetInt32(0);
                        if (analysisType == AnalysisType.Day)
                        {
                            var year = rdr.GetValue(1);
                            var month = rdr.GetValue( 2);
                            var day = rdr.GetValue(3);
                            var dateTime = TranslateUtils.ToDateTime($"{year}-{month}-{day}");
                            dict.Add(dateTime, accessNum);
                        }
                        else if (analysisType == AnalysisType.Month)
                        {
                            var year = rdr.GetValue(1);
                            var month = rdr.GetValue(2);

                            var dateTime = TranslateUtils.ToDateTime($"{year}-{month}-1");
                            dict.Add(dateTime, accessNum);
                        }
                        else if (analysisType == AnalysisType.Year)
                        {
                            var year = rdr.GetValue(1);
                            var dateTime = TranslateUtils.ToDateTime($"{year}-1-1");
                            dict.Add(dateTime, accessNum);
                        }
                    }
                    rdr.Close();
                }
            }

            return dict;
        }

        public async Task<int> GetCountAsync()
        {
            return await _repository.CountAsync();
        }

        private Query GetQuery(bool? state, int groupId, int dayOfLastActivity, string keyword, string order)
        {
            var query = Q.NewQuery();

            if (state.HasValue)
            {
                query.Where(nameof(User.Checked), state.Value);
            }

            if (dayOfLastActivity > 0)
            {
                var dateTime = DateTime.Now.AddDays(-dayOfLastActivity);
                query.WhereDate(nameof(User.LastActivityDate), ">=", dateTime);
            }

            if (groupId > -1)
            {
                if (groupId > 0)
                {
                    query.Where(nameof(User.GroupId), groupId);
                }
                else
                {
                    query.Where(q => q
                        .Where(nameof(User.GroupId), 0)
                        .OrWhereNull(nameof(User.GroupId))
                    );
                }
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                var like = $"%{keyword}%";
                query.Where(q => q
                    .WhereLike(nameof(User.UserName), like)
                    .OrWhereLike(nameof(User.Email), like)
                    .OrWhereLike(nameof(User.Mobile), like)
                    .OrWhereLike(nameof(User.DisplayName), like)
                );
            }

            if (!string.IsNullOrEmpty(order))
            {
                if (StringUtils.EqualsIgnoreCase(order, nameof(User.UserName)))
                {
                    query.OrderBy(nameof(User.UserName));
                }
                else
                {
                    query.OrderByDesc(order);
                }
            }
            else
            {
                query.OrderByDesc(nameof(User.Id));
            }

            return query;
        }

        public async Task<int> GetCountAsync(bool? state, int groupId, int dayOfLastActivity, string keyword)
        {
            var query = GetQuery(state, groupId, dayOfLastActivity, keyword, string.Empty);
            return await _repository.CountAsync(query);
        }

        public async Task<List<User>> GetUsersAsync(bool? state, int groupId, int dayOfLastActivity, string keyword, string order, int offset, int limit)
        {
            var query = GetQuery(state, groupId, dayOfLastActivity, keyword, order);
            query.Offset(offset).Limit(limit);

            return await _repository.GetAllAsync(query);
        }

        public async Task<bool> IsExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }

        public async Task<User> DeleteAsync(int userId)
        {
            var user = await GetByUserIdAsync(userId);

            await _repository.DeleteAsync(userId, Q.CachingRemove(GetCacheKeysToRemove(user)));

            return user;
        }
    }
}

