using BlogSystem.DTO;
using BlogSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogSystem.IBLL
{
    public interface IUserManager
    {
        Task<bool> Register(string email, string password);
        bool Login(string email, string password, out Guid userId);
        Task<bool> ChangePassword(Guid userId, string oldPwd, string newPwd);
        Task<bool> ChangeUserInformation(Guid userId, string nickName, string imagePath);
        Task<bool> ChangeUserNickName(Guid userId, string nickName);
        Task<bool> ChangeUserImage(Guid userId, string imagePath);
        /// <summary>
        /// 返回DTO类型数据
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<UserInformationDto> GetUserByEmail(string email);

        Task<UserInformationDto> GetUserById(Guid id);

        Task<bool> ExistsUser(Guid userId);

        Task<bool> IsFocused(Guid userId, Guid focusUserId);

        Task FocusUser(Guid userId, Guid focusUserId);

        Task UnFocusUser(Guid userId, Guid focusUserId);

        Task<string> ForgetPassword(string token, Guid userId, string email);

        Task<string> ResetPassword(string token, Guid userId, string password);

        Task<List<UserInformationDto>> GetFamousUser(int count);

        Task<int> GetUserDataCount();
    }
}
