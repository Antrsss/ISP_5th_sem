// AJAX настройки для всех запросов
$(document).ajaxSend(function (event, xhr) {
    xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
});

// Функция для загрузки контента через AJAX
function loadContent(url, container) {
    $(container).html(`
        <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p class="mt-2">Loading...</p>
        </div>
    `);

    return $(container).load(url + ' ' + container + ' > *', function (response, status, xhr) {
        if (status === "error") {
            $(container).html(`
                <div class="alert alert-danger">
                    <h4>Error loading content</h4>
                    <p>Please try again later.</p>
                </div>
            `);
        }
    });
}

// Инициализация при загрузке документа
$(document).ready(function () {
    // Обработка кликов по пагинации
    $(document).on('click', '.pagination .page-link:not(.disabled)', function (e) {
        e.preventDefault();

        var url = $(this).attr('href');
        if (url && url !== '#') {
            loadContent(url, '#cocktail-content');
            history.pushState(null, '', url);
        }
    });

    // Обработка кликов по категориям
    $(document).on('click', '.dropdown-menu .nav-link', function (e) {
        e.preventDefault();

        var url = $(this).attr('href');
        if (url) {
            loadContent(url, '#cocktail-content');

            // Обновляем активную категорию
            $('.dropdown-menu .nav-link').removeClass('active');
            $(this).addClass('active');

            // Обновляем текст кнопки
            var categoryText = $(this).text().trim();
            $('.dropdown-toggle').text(categoryText);

            history.pushState(null, '', url);
        }
    });

    // Обработка истории браузера
    $(window).on('popstate', function () {
        loadContent(window.location.href, '#cocktail-content');
    });
});