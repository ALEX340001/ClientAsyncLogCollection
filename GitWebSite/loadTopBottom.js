// Универсальный loadTopBottom для корня и вложенных папок

// Определяем базовый путь для ссылок (ведет в корень сайта)
const path = window.location.pathname;
// Если мы в папке generator, то поднимаемся на уровень выше
const basePath = path.includes('/generator/') ? '../' : './';

// Резервное меню (ссылки строятся через basePath)
const fallbackNav = `
<a href="${basePath}index.html">Начало</a>
<div class="dropdown">
    <a href="#" class="dropbtn">Руководство ▾</a>
    <div class="dropdown-content">
        <a href="${basePath}configuration.html">Конфигурация</a>
        <a href="${basePath}cli.html">CLI и команды</a>
    </div>
</div>
<div class="dropdown">
    <a href="#" class="dropbtn">Разработка ▾</a>
    <div class="dropdown-content">
        <a href="${basePath}developer.html">Архитектура</a>
        <a href="${basePath}logging.html">Логирование</a>
        <a href="${basePath}dependencies.html">Зависимости</a>
        <a href="${basePath}generator/generator.html">Генератор конфигурации</a>
    </div>
</div>
<div class="dropdown">
    <a href="#" class="dropbtn">О проекте ▾</a>
    <div class="dropdown-content">
        <a href="${basePath}license.html">Лицензия</a>
    </div>
</div>
`;

const fallbackFooter = `
<footer>
    ClientAsyncLogCollection · Гибкий сборщик журналов Windows<br>
    Документация актуальна на 08.06.2026
</footer>
`;

function loadComponent(id, url, fallback) {
    fetch(url)
        .then(response => {
            if (!response.ok) throw new Error('Статус ' + response.status);
            return response.text();
        })
        .then(html => {
            document.getElementById(id).innerHTML = html;
        })
        .catch(err => {
            console.warn('Не удалось загрузить ' + url + ', используется резервный HTML.', err);
            document.getElementById(id).innerHTML = fallback;
        });
}

// Обработчик гамбургера для мобильных (добавляем динамически)
setTimeout(() => {
    const nav = document.getElementById('nav-placeholder');
    if (!nav) return;
    if (!nav.querySelector('.menu-toggle')) {
        const toggleBtn = document.createElement('a');
        toggleBtn.className = 'menu-toggle';
        toggleBtn.innerHTML = '☰';
        toggleBtn.href = '#';
        toggleBtn.addEventListener('click', function(e) {
            e.preventDefault();
            nav.classList.toggle('responsive');
        });
        nav.prepend(toggleBtn);
    }
}, 100);

// Загрузка при DOMContentLoaded
document.addEventListener('DOMContentLoaded', function() {
    // Для папки generator путь к nav.html другой
    const navUrl = path.includes('/generator/') ? '../nav.html' : 'nav.html';
    loadComponent('nav-placeholder', navUrl, fallbackNav);
    loadComponent('footer-placeholder', 'footer.html', fallbackFooter);
});