// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(function () {
    const tables = document.querySelectorAll('table[data-datatable="true"]');
    tables.forEach(function (el) {
        const $el = $(el);
        if ($el.data('dt-initialized')) return;
        $el.DataTable({
            paging: true,
            searching: true,
            info: true,
            ordering: true,
            language: {
                url: 'https://cdn.datatables.net/plug-ins/1.13.8/i18n/fa.json'
            }
        });
        $el.data('dt-initialized', true);
    });
});
