﻿using Entities.IdentityUsers;
using Entities.Observer;
using Microsoft.AspNet.Identity.EntityFramework;
using SpacePro.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SpacePro.Controllers.HelperClasses
{
    public static class ApplicationUserHelper
    {
       
        #region SubOfTheMonth
        public static async Task<WinnersDto> GetSubOfTheMonth(this IEnumerable<ApplicationUser> appUsers, IEnumerable<IdentityRole> roles,ApplicationUserManager userManager)
        {
            
            var users = GetUsers(appUsers,roles);
            var count = users.Count();
            if ( !IsSubCountZero(count))
            {
                UserObserver userObserver = new UserObserver();
                var randomIndex = GetRandomIndex(count);
                foreach (var user in users)
                {
                    user.IsWinnerSub = false;
                }
                var winner = GetWinner(users, randomIndex);
                winner.IsWinnerSub = true;
                await userObserver.Notify(users, userManager);
                return new WinnersDto(winner.Id,winner.UserName,winner.UserImage != null ? winner.UserImage.Url : @"\Template\sash\assets\images\users\2.jpg");
            }
            return null;
        }

        public static bool IsLastDay(DateTime date) => date.Day == DateTime.DaysInMonth(date.Year, date.Month);
        public static bool IsSubCountZero(int count) => count == 0;
        public static ApplicationUser GetWinner(IEnumerable<ApplicationUser> subs, int randomIndex)
        {
            return subs.Where((x, i) => i == randomIndex).FirstOrDefault();
        }
        public static int GetRandomIndex(int count)
        {
            Random random = new Random();
            return random.Next(count);
        }
        //this requires finding the roles name to string in order to work
        public static IEnumerable<ApplicationUser> GetUsers(IEnumerable<ApplicationUser> users,IEnumerable<IdentityRole> roles)
        {
            var userRole = roles.Single(x=>x.Name=="User");
            return users.Where(x=>x.Roles.Any(s=>userRole.Id==s.RoleId));
        }
        #endregion

        #region AuthorOfTheMonth
        public static WinnersDto GetAuthorOfTheMonth(this IEnumerable<ApplicationUser> users, IEnumerable<IdentityRole> roles)
        {
            var authors = GetAuthors(users,roles);
            if (!IsSubCountZero(authors.Count()))
            {
                return GetAuthorWithMostLikes(authors);
            }
            return null;
        }
        //this requires finding the roles name to string in order to work
        public static IEnumerable<ApplicationUser> GetAuthors(IEnumerable<ApplicationUser> users,IEnumerable<IdentityRole> roles)
        {
            var authors = roles.Select(x => x.Name == "Author" ? x.Id : null).Where(x => x != null);
            return users.Where(x => x.Roles.Any(s =>authors.Contains(s.RoleId)));
        }
        public static WinnersDto GetAuthorWithMostLikes(IEnumerable<ApplicationUser> authors)
        {
            var author= authors.Where(x => x.UserPosts.Select(s => new { s.UserPostId }).Count() > 0)
                .Select(x => new { auth = x, likes = x.UserPosts.OrderByDescending(z => z.PostLikes.Count()).First().PostLikes.Count() })
                .OrderByDescending(x => x.likes).First().auth;
            return new WinnersDto(author.Id, author.UserName, author.UserImage != null ? author.UserImage.Url : @"\Template\sash\assets\images\users\1.jpg");

        }
        #endregion

        public static IEnumerable<object> AddUserImageIfNull(this List<ApplicationUser> users)
        {
            return users.Select(x => new { x.UserName, x.Id, UserImage = x.UserImage == null ? "/Template/sash/assets/images/faces/1.jpg" : x.UserImage.Url });
        }
    }
}