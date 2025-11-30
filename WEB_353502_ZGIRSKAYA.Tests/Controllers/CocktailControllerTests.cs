using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;
using WEB_353502_ZGIRSKAYA.UI.Controllers;
using WEB_353502_ZGIRSKAYA.UI.Services.CocktailCategoryService;
using WEB_353502_ZGIRSKAYA.UI.Services.FileService.CocktailService;
using Xunit;

namespace WEB_353502_ZGIRSKAYA.Tests.Controllers
{
    public class CocktailControllerTests
    {
        private readonly ICocktailService _cocktailService;
        private readonly ICategoryService _categoryService;
        private readonly CocktailController _controller;
        private readonly ITempDataDictionary _tempData;

        public CocktailControllerTests()
        {
            _cocktailService = Substitute.For<ICocktailService>();
            _categoryService = Substitute.For<ICategoryService>();
            _controller = new CocktailController(_cocktailService, _categoryService);
            _tempData = Substitute.For<ITempDataDictionary>();

            // Настраиваем необходимые свойства контроллера
            var httpContext = Substitute.For<HttpContext>();
            var controllerContext = new ControllerContext()
            {
                HttpContext = httpContext,
            };
            _controller.ControllerContext = controllerContext;
            _controller.TempData = _tempData;
        }

        private void SetupHttpContext(bool isAjaxRequest)
        {
            var httpContext = Substitute.For<HttpContext>();
            var request = Substitute.For<HttpRequest>();
            var headers = new HeaderDictionary();

            if (isAjaxRequest)
            {
                headers.Add("X-Requested-With", "XMLHttpRequest");
            }

            request.Headers.Returns(headers);
            httpContext.Request.Returns(request);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Index_ReturnsErrorView_WhenCocktailListNotSuccessful()
        {
            // Arrange
            var errorMessage = "Ошибка загрузки коктейлей";
            var errorResponse = ResponseData<ListModel<Cocktail>>.Error(errorMessage);
            _cocktailService.GetCocktailListAsync(null, 1).Returns(errorResponse);

            // Act
            var result = await _controller.Index(null, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            _tempData.Received(1)["Error"] = errorMessage;
        }

        [Fact]
        public async Task Index_ReturnsErrorView_WhenCocktailListDataIsNull()
        {
            // Arrange
            var successResponseWithNullData = ResponseData<ListModel<Cocktail>>.Success(null);
            _cocktailService.GetCocktailListAsync(null, 1).Returns(successResponseWithNullData);

            // Act
            var result = await _controller.Index(null, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
            _tempData.Received(1)["Error"] = "Ошибка при загрузке коктейлей";
        }

        [Fact]
        public async Task Index_ReturnsViewWithData_WhenSuccessful_NonAjaxRequest()
        {
            // Arrange
            var cocktails = new ListModel<Cocktail>
            {
                Items = new List<Cocktail>
                {
                    new Cocktail { Id = 1, Name = "Мохито", Description = "Освежающий коктейль", Price = 350 },
                    new Cocktail { Id = 2, Name = "Маргарита", Description = "Классический коктейль", Price = 400 }
                },
                CurrentPage = 1,
                TotalPages = 1
            };

            var categories = new List<CocktailCategory>
            {
                new CocktailCategory { Id = 1, Name = "Алкогольные", NormilisedName = "alcoholic" },
                new CocktailCategory { Id = 2, Name = "Безалкогольные", NormilisedName = "non-alcoholic" }
            };

            var cocktailResponse = ResponseData<ListModel<Cocktail>>.Success(cocktails);
            var categoryResponse = ResponseData<List<CocktailCategory>>.Success(categories);

            _cocktailService.GetCocktailListAsync(null, 1).Returns(cocktailResponse);
            _categoryService.GetCategoryListAsync().Returns(categoryResponse);

            // Act
            var result = await _controller.Index(null, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(cocktails, viewResult.Model);
            Assert.Equal(categories, _controller.ViewData["Categories"]);
        }

        [Fact]
        public async Task Index_ReturnsPartialView_WhenSuccessful_AjaxRequest()
        {
            // Arrange
            var cocktails = new ListModel<Cocktail>
            {
                Items = new List<Cocktail>
                {
                    new Cocktail { Id = 1, Name = "Мохито", Description = "Освежающий коктейль", Price = 350 },
                    new Cocktail { Id = 2, Name = "Маргарита", Description = "Классический коктейль", Price = 400 }
                },
                CurrentPage = 1,
                TotalPages = 1
            };

            var cocktailResponse = ResponseData<ListModel<Cocktail>>.Success(cocktails);
            _cocktailService.GetCocktailListAsync("alcoholic", 1).Returns(cocktailResponse);

            SetupHttpContext(true);

            // Act
            var result = await _controller.Index("alcoholic", 1);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_CocktailListPartial", partialViewResult.ViewName);
            Assert.Equal(cocktails, partialViewResult.Model);
        }

        [Fact]
        public async Task Index_DoesNotLoadCategories_WhenAjaxRequest()
        {
            // Arrange
            var cocktails = new ListModel<Cocktail>
            {
                Items = new List<Cocktail>
                {
                    new Cocktail { Id = 1, Name = "Мохито", Description = "Освежающий коктейль", Price = 350 }
                },
                CurrentPage = 1,
                TotalPages = 1
            };

            var cocktailResponse = ResponseData<ListModel<Cocktail>>.Success(cocktails);
            _cocktailService.GetCocktailListAsync(null, 1).Returns(cocktailResponse);

            SetupHttpContext(true);

            // Act
            var result = await _controller.Index(null, 1);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            await _categoryService.DidNotReceive().GetCategoryListAsync();
            Assert.Null(_controller.ViewData["Categories"]);
        }

        [Fact]
        public async Task Index_WithCategoryParameter_ReturnsCorrectData()
        {
            // Arrange
            var cocktails = new ListModel<Cocktail>
            {
                Items = new List<Cocktail>
                {
                    new Cocktail { Id = 1, Name = "Виски Кола", Description = "Крепкий коктейль", Price = 450 }
                },
                CurrentPage = 1,
                TotalPages = 1
            };

            var categories = new List<CocktailCategory>
            {
                new CocktailCategory { Id = 1, Name = "Алкогольные", NormilisedName = "alcoholic" },
                new CocktailCategory { Id = 2, Name = "Безалкогольные", NormilisedName = "non-alcoholic" }
            };

            var cocktailResponse = ResponseData<ListModel<Cocktail>>.Success(cocktails);
            var categoryResponse = ResponseData<List<CocktailCategory>>.Success(categories);

            _cocktailService.GetCocktailListAsync("alcoholic", 1).Returns(cocktailResponse);
            _categoryService.GetCategoryListAsync().Returns(categoryResponse);

            // Act
            var result = await _controller.Index("alcoholic", 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(cocktails, viewResult.Model);
            Assert.Equal(categories, _controller.ViewData["Categories"]);
        }

        [Fact]
        public async Task Index_HandlesCategoryServiceError_Gracefully()
        {
            // Arrange
            var cocktails = new ListModel<Cocktail>
            {
                Items = new List<Cocktail>
                {
                    new Cocktail { Id = 1, Name = "Мохито", Description = "Освежающий коктейль", Price = 350 }
                },
                CurrentPage = 1,
                TotalPages = 1
            };

            var cocktailResponse = ResponseData<ListModel<Cocktail>>.Success(cocktails);
            var categoryErrorResponse = ResponseData<List<CocktailCategory>>.Error("Ошибка загрузки категорий");

            _cocktailService.GetCocktailListAsync(null, 1).Returns(cocktailResponse);
            _categoryService.GetCategoryListAsync().Returns(categoryErrorResponse);

            // Act
            var result = await _controller.Index(null, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(cocktails, viewResult.Model);
            Assert.Null(_controller.ViewData["Categories"]);
        }
    }
}