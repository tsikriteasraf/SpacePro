﻿using MyDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Entities.BroadClasses;
using SpacePro.Models.Dtos;
using Persistance_UnitOfWork;
using Entities.IdentityUsers;
using Microsoft.AspNet.Identity;

namespace SpacePro.Controllers.BroadsControllers
{
    public class ArticleController : Controller
    {
      private readonly IUnitOfWork _unitOfWork;

        public ArticleController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public ActionResult ShowArticles()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetAllArticles()
        {
            var article = _unitOfWork.Articles
                            .GetArticlesFullModel().Select(x => new {
                                x.ArticleId,
                                x.Title,
                                x.ShortDescription,
                                x.PostDate,
                                x.PostLikes,
                                x.AuthorName,
                                Category = x.ArticleCategory.Title,
                                Url = x.ArticleImage.Url,
                                AltText = x.ArticleImage.AlternativeText
                            });

            return Json(article, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult CreateArticles()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateNewArticle(CreateArticleDto articleDto, HttpPostedFileBase image)
        {
            if (image != null)
            {
                image.SaveAs(Server.MapPath("/Content/ArticlesImages/" + image.FileName));
            }

            ArticleImage articleImage = new ArticleImage();
            articleImage.Name = image.FileName;
            articleImage.Url = "/Content/ArticlesImages/" + image.FileName;
            articleImage.AlternativeText = (image.FileName).Split('.')[0];
            _unitOfWork.ArticleImages.Add(articleImage);

            Article article = new Article();
            article.Title = articleDto.Title;
            article.ShortDescription = articleDto.ShortDescription;
            article.FullDescription = articleDto.FullDescription;
            article.AuthorName = articleDto.AuthorName;
            article.PostLikes = 0;
            article.PostDate = DateTime.Now;
            article.ArticleCategoryId = articleDto.ArticleCategoryId;
            article.ArticleImage = articleImage;
            _unitOfWork.Articles.Add(article);

            _unitOfWork.Complete();

            return RedirectToAction("ShowArticles");
        }

        [HttpPost]
        public ActionResult EditArticle(int? articleId, CreateArticleDto articleDto, HttpPostedFileBase image)
        {
            var article = _unitOfWork.Articles.GetArticlesWithImage().FirstOrDefault(x => x.ArticleId == articleId);

            if (image != null)
            {
                image.SaveAs(Server.MapPath("/Content/ArticlesImages/" + image.FileName));

                ArticleImage articleImage = new ArticleImage();
                articleImage.Name = image.FileName;
                articleImage.Url = "/Content/ArticlesImages/" + image.FileName;
                articleImage.AlternativeText = (image.FileName).Split('.')[0];
                _unitOfWork.ArticleImages.Add(articleImage);
                _unitOfWork.ArticleImages.Remove(article.ArticleImage);

                article.ArticleImage = articleImage;
            }

            article.Title = articleDto.Title;
            article.ShortDescription = articleDto.ShortDescription;
            article.FullDescription = articleDto.FullDescription;
            article.AuthorName = articleDto.AuthorName;
            article.PostDate = DateTime.Now;
            article.ArticleCategoryId = articleDto.ArticleCategoryId;

            _unitOfWork.Articles.ModifyEntity(article);
            _unitOfWork.Complete();

            return RedirectToAction("ShowArticles");
        }

        [HttpDelete]
        public ActionResult DeleteArticle(int? articleId, int? imageId)
        {
            var article = _unitOfWork.Articles.Get((int)articleId);
            var articleImage = _unitOfWork.ArticleImages.Get((int)imageId);
            var imageName = articleImage.Name;

            _unitOfWork.ArticleImages.Remove(articleImage);
            _unitOfWork.Articles.Remove(article);

            _unitOfWork.Complete();

            DeleteImageFromFolder(imageName);

            return Json("/Article/ShowArticles",JsonRequestBehavior.AllowGet);
        }

        private void DeleteImageFromFolder(string imageName)
        {
            var filePath = Server.MapPath("/Content/ArticlesImages/" + imageName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [HttpGet]
        public ActionResult GetArticleDetails(int? id)
        {
            var article = _unitOfWork.Articles.GetArticlesFullModel().FirstOrDefault(x=>x.ArticleId==id);
            return View(article);
        }

        [HttpGet]
        public JsonResult GiveLike(int? articleId)
        {   
            var article = _unitOfWork.Articles.Get((int)articleId);
            var likedUserId = User.Identity.GetUserId();
            var userNotYetLikedThisArticle = _unitOfWork.ArticleLikes.Find(x => x.LikedUser == likedUserId && x.ArticleId == article.ArticleId).Count() == 0;

            if (userNotYetLikedThisArticle)
            {
                ArticleLike articleLike = new ArticleLike();
                articleLike.LikedUser = _unitOfWork.ApplicationUsers.Find(x => x.Id == likedUserId).FirstOrDefault().Id;
                articleLike.ArticleId = article.ArticleId;
                _unitOfWork.ArticleLikes.Add(articleLike);
                article.ArticleLikes.Add(articleLike);
                article.PostLikes++;
                _unitOfWork.Complete();
            }  
            return Json(article.PostLikes, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult RemoveLike(int? articleId)
        {
            var article = _unitOfWork.Articles.Get((int)articleId);
            var likedUserId = User.Identity.GetUserId();

            var articleLike = _unitOfWork.ArticleLikes.Find(x => x.LikedUser == likedUserId && x.ArticleId == articleId).FirstOrDefault();
            _unitOfWork.ArticleLikes.Remove(articleLike);
            article.PostLikes--;
            _unitOfWork.Complete();

            return Json(article.PostLikes, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
