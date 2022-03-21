﻿using Entities.IdentityUsers;
using Microsoft.AspNet.Identity;
using MyDatabase;
using Persistance_UnitOfWork;
using SpacePro.Models;
using SpacePro.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SpacePro.Controllers.AppUsersContollers
{
    public class AppUserController : Controller
    {
        private readonly UnitOfWork unitOfWork;
        public AppUserController()
        {
            unitOfWork = new UnitOfWork(new ApplicationDbContext());
        }

        public async Task<ActionResult> UserProfile()
        {
            Task<string> task = new Task<string>(User.Identity.GetUserId);
            task.Start();
            var userId = await task;

            var user =await unitOfWork.ApplicationUsers.GetUserWithImages(userId);

            return View(user);
        }

        [HttpGet]
        public async Task<ActionResult> AnyUserProfile(string id)
        {
            var user = await unitOfWork.ApplicationUsers.GetUserWithImages(id);

            return View("UserProfile",user);
        }

        [HttpPost]
        public async Task<ActionResult> AddUserImage(HttpPostedFileBase image)
        {
            var userId = User.Identity.GetUserId();

            var user =await unitOfWork.ApplicationUsers.GetUserWithImages(userId);

            if (user.UserImage != null)
            {
                DeleteImageFromFolder(user.UserImage.Name);
            }

            if (image != null)
            {
                image.SaveAs(Server.MapPath("/Content/UserImages/" + image.FileName));

                if (user.UserImage != null)
                {
                    unitOfWork.UserImages.Remove(user.UserImage);
                }

                UserImage userImage = new UserImage();

                userImage.Name = image.FileName;
                userImage.Url = "/Content/UserImages/" + image.FileName;
                userImage.AlternativeText = (image.FileName).Split('.').First();
                userImage.ApplicationUser = user;
                unitOfWork.UserImages.Add(userImage);

                user.UserImage = userImage;
                unitOfWork.ApplicationUsers.ModifyEntity(user);

                unitOfWork.Complete();
            }
            return RedirectToAction("UserProfile");
        }

        public async Task<ActionResult> EditProfile()
        {
            var userId = User.Identity.GetUserId();

            var user =await unitOfWork.ApplicationUsers.GetUser(userId);

            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> EditProfile(EditUserDto editUserDto)
        {
            var userId = User.Identity.GetUserId();
            var user = await unitOfWork.ApplicationUsers.GetUser(userId);
            user.FirstName = editUserDto.FirstName;
            user.LastName = editUserDto.LastName;
            user.PhoneNumber = editUserDto.PhoneNumber;
            user.Email = editUserDto.Email;
            user.AboutMe = editUserDto.AboutMe;
            user.DateOfBirth = editUserDto.DateOfBirth;
            user.Work = editUserDto.Work;
            user.Education = editUserDto.Education;

            unitOfWork.ApplicationUsers.ModifyEntity(user);
            unitOfWork.Complete();

            return RedirectToAction("UserProfile");
        }

        public async Task<ActionResult> GetUserImage()
        {
            if (User.Identity.GetUserId()==null)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }

            var userId = User.Identity.GetUserId();

            if ((await unitOfWork.ApplicationUsers.GetUserWithImages(userId)).UserImage == null)
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }

            var imgId = (await unitOfWork.ApplicationUsers
                .GetUserWithImages(userId)).UserImage.UserImageId;

            var userImg = unitOfWork.UserImages
                .Find(x => x.UserImageId == imgId)
                .Select(x=> new {x.Url,x.AlternativeText})
                .FirstOrDefault();

            return Json( new { data = userImg },JsonRequestBehavior.AllowGet);
        }

        private void DeleteImageFromFolder(string imageName)
        {
            var filePath = Server.MapPath("/Content/UserImages/" + imageName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}