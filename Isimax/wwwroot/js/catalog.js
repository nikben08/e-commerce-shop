$(document).ready(function () {

    let items = document.querySelectorAll('.carousel .carousel-item')

    items.forEach((el) => {
        const minPerSlide = 4
        let next = el.nextElementSibling
        for (var i = 1; i < minPerSlide; i++) {
            if (!next) {
                // wrap carousel by using first child
                next = items[0]
            }
            let cloneChild = next.cloneNode(true)
            el.appendChild(cloneChild.children[0])
            next = next.nextElementSibling
        }
    })


    $("#showCategoriesButton").click(function () {
        $(".mobile-side-bar-menu-categories").slideToggle();
    });


    function set_sort_select() {
        var query = get_query();
        var sort = query.Sort;
        if (sort) {
            $("#sort option[value='" + sort[0] + "']").prop("selected", true);
        }

        var page = query.Page;
        if (page) {
            $("#pageNumber").val(page);
        } else {
            $("#pageNumber").val('1');
        }
    }

    set_sort_select();

    function get_query() {
        var url = document.location.href;
        var qs = url.substring(url.indexOf('?') + 1).split('&');
        for (var i = 0, result = {}; i < qs.length; i++) {
            qs[i] = qs[i].split('=');
            result[qs[i][0]] = decodeURIComponent(qs[i][1]);
        }
        return result;
    }

    $(".category-name").click(function () {
        var categoryId = $(this).data("categoryid");
        var sort = $("#sort").val();

        $("<form action=''>\
            <input name='CategoryId' value='" + categoryId + "' >\
            <input name='Sort' value='" + sort + "' >\
            <input name='Page' value='1' >\
        </form>").appendTo('body').submit();
    });


    $("#sort").on('change', function () {
        var query = get_query();
        var categoryId = query.CategoryId;
        var sort = $(this).val();
        $("<form action=''>\
            <input name='CategoryId' value='" + categoryId + "' >\
            <input name='Sort' value='" + sort + "' >\
            <input name='Page' value='1' >\
        </form>").appendTo('body').submit();
    });

    $(".fa-caret-left").click(function () {
        var query = get_query();

        var page = query.Page;
        if (page) {
            page = parseInt(query.Page) - 1;
        }

        var categoryId = query.CategoryId;
        if (categoryId) {
            categoryId = query.CategoryId;
        } else {
            categoryId = 1;
        }

        var sort = query.Sort;
        if (sort) {
            sort = query.Sort;
        } else {
            sort = 1;
        }

        $("<form action=''>\
            <input name='CategoryId' value='" + categoryId + "' >\
            <input name='Sort' value='" + sort + "' >\
            <input name='Page' value='" + page + "' >\
        </form>").appendTo('body').submit();
    });

    $(".fa-caret-right").click(function () {
        var query = get_query();

        var page = query.Page;
        if (page) {
            page = parseInt(query.Page) + 1;
        } else {
            page = 2;
        }

        var categoryId = query.CategoryId;
        if (categoryId) {
            categoryId = query.CategoryId;
        } else {
            categoryId = 1;
        }

        var sort = query.Sort;
        if (sort) {
            sort = query.Sort;
        } else {
            sort = 1;
        }

        $("<form action=''>\
            <input name='CategoryId' value='" + categoryId + "' >\
            <input name='Sort' value='" + sort + "' >\
            <input name='Page' value='" + page + "' >\
        </form>").appendTo('body').submit();
    });
});