using MediatR;
using Microsoft.EntityFrameworkCore;
using WEB_353502_ZGIRSKAYA.API.Data;
using WEB_353502_ZGIRSKAYA.Domain.Entities;
using WEB_353502_ZGIRSKAYA.Domain.Models;

namespace WEB_353502_ZGIRSKAYA.API.UseCases
{
    public sealed record GetListOfCocktails(
        string? CategoryNormalizedName,
        int PageNo = 1,
        int PageSize = 3
    ) : IRequest<ResponseData<ListModel<Cocktail>>>;

    public class GetListOfCocktailsHandler : IRequestHandler<GetListOfCocktails, ResponseData<ListModel<Cocktail>>>
    {
        private readonly AppDbContext _db;
        private readonly int _maxPageSize = 20;

        public GetListOfCocktailsHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ResponseData<ListModel<Cocktail>>> Handle(GetListOfCocktails request, CancellationToken cancellationToken)
        {
            try
            {
                // Фильтрация по категории
                var query = _db.Cocktails
                    .Include(c => c.Category)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.CategoryNormalizedName))
                {
                    query = query.Where(c => c.Category != null &&
                                           c.Category.NormilisedName == request.CategoryNormalizedName);
                }

                // Валидация параметров пагинации
                var pageSize = Math.Min(request.PageSize, _maxPageSize);
                if (pageSize <= 0) pageSize = 3;

                var pageNo = Math.Max(request.PageNo, 1);

                // Получение общего количества
                var totalCount = await query.CountAsync(cancellationToken);
                var totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;

                // Получение данных для текущей страницы
                var items = await query
                    .OrderBy(c => c.Name)
                    .Skip((pageNo - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var listModel = new ListModel<Cocktail>
                {
                    Items = items,
                    CurrentPage = pageNo,
                    TotalPages = totalPages,
                    PageSize = pageSize
                };

                return ResponseData<ListModel<Cocktail>>.Success(listModel);
            }
            catch (Exception ex)
            {
                return ResponseData<ListModel<Cocktail>>.Error(
                    $"Ошибка при получении списка коктейлей: {ex.Message}");
            }
        }
    }
}