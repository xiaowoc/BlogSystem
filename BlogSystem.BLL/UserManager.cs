using BlogSystem.DAL;
using BlogSystem.DTO;
using BlogSystem.IBLL;
using BlogSystem.IDAL;
using BlogSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace BlogSystem.BLL
{
    public class UserManager : IUserManager
    {
        public async Task<bool> ChangePassword(Guid userId, string oldPwd, string newPwd)
        {
            using (IUserService userSvc = new UserService())
            {
                User user = await userSvc.GetAll().FirstAsync(m => m.Id == userId && m.Password == oldPwd);
                if (user != null)
                {
                    user.Password = newPwd;
                    await userSvc.EditAsync(user);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<bool> ChangeUserInformation(Guid userId, string siteName, string imagePath)
        {
            using (IUserService userSvc = new UserService())
            {
                var user = await userSvc.GetAll().FirstAsync(m => m.Id == userId);
                if (user != null)
                {
                    user.SiteName = siteName;
                    user.ImagePath = imagePath;
                    await userSvc.EditAsync(user);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public async Task<UserInformationDto> GetUserByEmail(string email)
        {
            using (IUserService userSvc = new UserService())
            {
                if (await userSvc.GetAll().AnyAsync(m => m.Email == email))
                {
                    return await userSvc.GetAll().Where(m => m.Email == email).Select(m =>
                    new UserInformationDto()
                    {
                        Id = m.Id,
                        Email = m.Email,
                        FansCount = m.FansCount,
                        FocusCount = m.FocusCount,
                        ImagePath = m.ImagePath,
                        SiteName = m.SiteName
                    }).FirstAsync();
                }
                else
                {
                    return null;
                }
            }
        }

        public bool Login(string email, string password, out Guid userId)
        {
            using (IUserService userSvc = new UserService())
            {
                var user = userSvc.GetAll().FirstOrDefaultAsync(m => m.Email == email && m.Password == password);
                user.Wait();
                var data = user.Result;
                //以上三句来替代await，异步转同步
                if (data == null)
                {
                    userId = Guid.Empty;
                    return false;
                }
                else
                {
                    userId = data.Id;
                    return true;
                }
            }
        }

        public async Task<bool> Register(string email, string password)
        {
            using (IUserService userSvc = new UserService())
            {
                UserInformationDto user = await GetUserByEmail(email);
                if (user == null)//如果没有相同邮箱则通过
                {
                    await userSvc.CreatAsync(new User() { Email = email, Password = password, SiteName = "小破站", ImagePath = "default.png" });
                    return true;
                }
                else//有相同邮箱则不可
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 通过id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<UserInformationDto> GetUserById(Guid id)
        {
            using (IUserService userSvc = new UserService())
            {
                if (await userSvc.GetAll().AnyAsync(m => m.Id == id))
                {
                    return await userSvc.GetAll().Where(m => m.Id == id).Select(m =>
                    new UserInformationDto()
                    {
                        Id = m.Id,
                        Email = m.Email,
                        FansCount = m.FansCount,
                        FocusCount = m.FocusCount,
                        ImagePath = m.ImagePath,
                        SiteName = m.SiteName
                    }).FirstAsync();
                }
                else
                {
                    throw new ArgumentException("id不存在");
                }
            }
        }

        /// <summary>
        /// 判断用户是否存在
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <returns></returns>
        public async Task<bool> ExistsUser(Guid userId)
        {
            using (IUserService userSve = new UserService())
            {
                return await userSve.GetAll().AnyAsync(m => m.Id == userId);
            }
        }

        //public async Task<UserDto> GetUserDetails(Guid id)
        //{
        //    UserDto user = new UserDto();
        //    using (IUserService userSvc = new UserService())
        //    {
        //      UserDto userDto=  await userSvc.GetAll().Where(m => m.Id == id).Select(m =>new UserDto() 
        //        {  
        //          CreateTime = m.CreatTime,
        //            Email = m.Email,
        //            FansCount = m.FansCount,
        //            FoucusCount = m.FoucusCount,
        //            ImagePath = m.ImagePath,
        //            SiteName = m.SiteName,
        //            UserId = m.Id
        //        }).FirstOrDefaultAsync();
        //    }
        //    using (ArticleService articleSvc=new ArticleService())
        //    {
        //       List<Article> articles = articleSvc.GetAll().Where(m => m.UserId == id).ToList();
        //        foreach (Article article in articles)
        //        {

        //        }
        //    }
        //}

        /// <summary>
        /// 是否已经关注
        /// </summary>
        /// <param name="userId">关注者ID</param>
        /// <param name="focusUserId">被关注者ID</param>
        /// <returns></returns>
        public async Task<bool> IsFocused(Guid userId, Guid focusUserId)
        {
            using (IFanService fanSvc = new FanService())
            {
                return await fanSvc.GetAll().Where(m => m.UserId == userId && m.FocusUserId == focusUserId).AnyAsync();
            }
        }

        /// <summary>
        /// 关注用户
        /// </summary>
        /// <param name="userId">关注者ID</param>
        /// <param name="focusUserId">被关注者ID</param>
        /// <returns></returns>
        public async Task FocusUser(Guid userId, Guid focusUserId)
        {
            int userFansCount;
            int userFocusCount;
            int focusUserFansCount;
            int focusUserFocusCount;
            using (IFanService fanSvc = new FanService())
            {
                Fans fans = new Fans
                {
                    UserId = userId,
                    FocusUserId = focusUserId
                };
                await fanSvc.CreatAsync(fans);
                userFocusCount = await fanSvc.GetAll().Where(m => m.UserId == userId).CountAsync();//用户关注数量
                focusUserFocusCount = await fanSvc.GetAll().Where(m => m.UserId == focusUserId).CountAsync();//被关注者关注数量
                userFansCount = await fanSvc.GetAll().Where(m => m.FocusUserId == userId).CountAsync();//用户粉丝数量
                focusUserFansCount = await fanSvc.GetAll().Where(m => m.FocusUserId == focusUserId).CountAsync();//被关注者粉丝数量
            }
            using (IUserService userSve = new UserService())
            {
                User user = await userSve.GetOneByIdAsync(userId);
                user.FansCount = userFansCount;
                user.FocusCount = userFocusCount;
                await userSve.EditAsync(user);
                User focusUser = await userSve.GetOneByIdAsync(focusUserId);
                focusUser.FansCount = focusUserFansCount;
                focusUser.FocusCount = focusUserFocusCount;
                await userSve.EditAsync(focusUser);
            }
        }

        /// <summary>
        /// 取消关注用户
        /// </summary>
        /// <param name="userId">关注者ID</param>
        /// <param name="focusUserId">被关注者ID</param>
        /// <returns></returns>
        public async Task UnFocusUser(Guid userId, Guid focusUserId)
        {
            int userFansCount;
            int userFocusCount;
            int focusUserFansCount;
            int focusUserFocusCount;
            using (IFanService fanSvc = new FanService())
            {
                Fans fan = await fanSvc.GetAll().Where(m => m.UserId == userId && m.FocusUserId == focusUserId).FirstOrDefaultAsync();
                await fanSvc.DeleteAsync(fan);
                userFocusCount = await fanSvc.GetAll().Where(m => m.UserId == userId).CountAsync();//用户关注数量
                focusUserFocusCount = await fanSvc.GetAll().Where(m => m.UserId == focusUserId).CountAsync();//被关注者关注数量
                userFansCount = await fanSvc.GetAll().Where(m => m.FocusUserId == userId).CountAsync();//用户粉丝数量
                focusUserFansCount = await fanSvc.GetAll().Where(m => m.FocusUserId == focusUserId).CountAsync();//被关注者粉丝数量
            }
            using (IUserService userSve = new UserService())
            {
                User user = await userSve.GetOneByIdAsync(userId);
                user.FansCount = userFansCount;
                user.FocusCount = userFocusCount;
                await userSve.EditAsync(user);
                User focusUser = await userSve.GetOneByIdAsync(focusUserId);
                focusUser.FansCount = focusUserFansCount;
                focusUser.FocusCount = focusUserFocusCount;
                await userSve.EditAsync(focusUser);
            }
        }

        public async Task<string> ForgetPassword(string token, Guid userId,string email)
        {
            string modelError = null;
            using (IResetPasswordService resetPasswordSvc = new ResetPasswordService())
            {
                ResetPassword resetPassword = new ResetPassword();
                if (await resetPasswordSvc.GetAll().AnyAsync(m => m.UserId == userId))
                {
                    resetPassword = await resetPasswordSvc.GetAll().Where(m => m.UserId == userId && m.IsSuccess == false).OrderByDescending(m => m.CreatTime).FirstAsync();
                }
                else
                {
                    resetPassword = null;
                }
                //获取最新的一条reset password，找不到有效的或者间隔大于5分钟才可申请
                if (resetPassword == null || DateTime.Now >= resetPassword.CreatTime.AddMinutes(5))
                {
                    //在reset password中添加数据 token
                    ResetPassword data = new ResetPassword
                    {
                        Email = email,
                        Token = token,
                        UserId = userId
                    };
                    await resetPasswordSvc.CreatAsync(data);
                    return modelError;
                }
                else
                {
                    modelError = "不可频繁申请，请在" + resetPassword.CreatTime.AddMinutes(5) + "后再重试";
                    return modelError;
                }
            }
        }

        public async Task<string> ResetPassword(string token, Guid userId, string password)
        {
            string modelError = null;
            using (IResetPasswordService resetPasswordSvc = new ResetPasswordService())
            {
                ResetPassword resetPassword = new ResetPassword();
                if (await resetPasswordSvc.GetAll().AnyAsync(m => m.UserId == userId))
                {
                    resetPassword = await resetPasswordSvc.GetAll().Where(m => m.UserId == userId && m.Token == token).OrderByDescending(m => m.CreatTime).FirstAsync();
                }
                else
                {
                    resetPassword = null;
                }
                //查找reset password中是否有对应的token而且对应的id是否一致 issuccess是否已经成功 时间是否到期
                if (resetPassword != null)
                {
                    if (resetPassword.IsSuccess == false)
                    {
                        if (DateTime.Now >= resetPassword.ExpireTime)
                        {
                            modelError = "该链接已过期，请重新申请！";
                            return modelError;
                        }
                        else
                        {
                            //一致的话reset password issuccess修改为成功
                            resetPassword.IsSuccess = true;
                            await resetPasswordSvc.EditAsync(resetPassword);
                            //user表用户密码修改
                            using (IUserService userSvc = new UserService())
                            {
                                User user = await userSvc.GetAll().FirstAsync(m => m.Id == userId && m.Email == resetPassword.Email);
                                if (user != null)
                                {
                                    user.Password = password;
                                    await userSvc.EditAsync(user);
                                    return modelError;
                                }
                                else
                                {
                                    modelError = "找不到用户信息，修改密码失败！";
                                    return modelError;
                                }
                            }
                        }
                    }
                    else
                    {
                        modelError = "该链接已被使用，如未成功修改请重新申请忘记密码！";
                        return modelError;
                    }
                }
                else
                {
                    modelError = "token信息不正确";
                    return modelError;
                }
            }
        }
    }
}