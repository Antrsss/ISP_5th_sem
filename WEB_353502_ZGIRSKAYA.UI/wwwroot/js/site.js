$(document).ready(function () {

    function loadCocktails(url) {

        $('#cocktail-content').html(`
            <div class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-2">Loading cocktails...</p>
            </div>
        `);

        // Добавляем параметр ?ajax=1, чтобы сервер понял, что это частичный запрос
        if (url.indexOf('?') > -1) url += '&ajax=1';
        else url += '?ajax=1';

        // Используем .load() для подгрузки контента
        // Загружаем только блоки со списком и пагинацией
        $('#cocktail-content').load(url + ' #cocktail-list, .pagination', function (response, status, xhr) {
            if (status === 'error') {
                $('#cocktail-content').html(`
                    <div class="alert alert-danger">
                        <h4>Error loading content</h4>
                        <p>Unable to load cocktails at the moment.</p>
                        <p>Error: ${xhr.statusText}</p>
                    </div>
                `);
            }
        });

        history.pushState(null, '', url.replace('&ajax=1', '').replace('?ajax=1', ''));
    }

    $(document).on('click', '.pagination .page-link', function (e) {
        e.preventDefault();

        const url = $(this).attr('href');
        if (url && url !== '#') {
            loadCocktails(url);
        }
    });

    $(document).on('click', '.dropdown-menu .nav-link', function (e) {
        e.preventDefault();

        var url = $(this).attr('href');
        if (url) {
            loadCocktails(url);

            // Обновляем активную категорию в dropdown
            $('.dropdown-menu .nav-link').removeClass('active');
            $(this).addClass('active');

            // ✅ Меняем только текст кнопки категорий
            var categoryText = $(this).text().trim();
            $('#categoryDropdownButton').text(categoryText);
        }
    });

    $(window).on('popstate', function () {
        loadCocktails(window.location.href);
    });
});