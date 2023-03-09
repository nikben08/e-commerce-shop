$(document).ready(function () {
    $(".deleteProduct").click(function () {
        var answer = window.confirm("Ürünü kaldırmak istediğinizden emin misiniz?");
        if (answer) {
            var productId = $(this).data("productid");
            $(this).html('Deleted');
            $.ajax({
                type: "POST",
                url: "/Admin/DeleteProduct",
                dataType: "json",
                data: { "productId": productId },
                success: function (response) {
                    alert('Ürün silindı')
                }
            });
        }
    });

    $(".deleteCategory").click(function () {
        var answer = window.confirm("Bu kategori silinir ise o kategoriye bağlı tüm ürünler silinecektir");
        if (answer) {
            var categoryId = $(this).data("categoryid");
            $(this).html('Deleted');
            $.ajax({
                type: "POST",
                url: "/Admin/DeleteCategory",
                dataType: "json",
                data: { "categoryId": categoryId },
                success: function (response) {
                    alert('Kategori silindı')
                }
            });
        }
    });

    $(".deleteUser").click(function () {
        var userId = $(this).data("userid");
        var numberOfUsers = $(".data-list .deleteUser[value!='Deleted']").length;
        var answer = window.confirm("Kullanıcıyı silmek istediğinizden emin misiniz?");
        if (answer) {
            if (numberOfUsers > 1) {
                $(this).html('Deleted');
                $(this).attr('value', 'Deleted');
                $.ajax({
                    type: "POST",
                    url: "/Admin/DeleteUser",
                    dataType: "json",
                    data: { "userId": userId },
                    success: function (response) {
                        alert('Kullanıcı silindı')
                    }
                });

            } else {
                alert('Kullanıcı silinemez');
            }
        }
    });

    $(".deleteContactForm").click(function () {
        var contactFormId = $(this).data("contactformid");
        $(this).html('Deleted');
        $.ajax({
            type: "POST",
            url: "/Admin/DeleteContactForm",
            dataType: "json",
            data: { "contactFormId": contactFormId },
            success: function (response) {
                alert('İletişim formu silindi')
            }
        });
    });

    $(".deleteQualityCertificate").click(function () {
        var qualityCertificateId = $(this).data("qualitycertificateid");
        $(this).html('Deleted');
        $.ajax({
            type: "POST",
            url: "/Admin/DeleteQualityCertificate",
            dataType: "json",
            data: { "qualityCertificateId": qualityCertificateId },
            success: function (response) {
                alert('Kalite belgesi silindi')
            }
        });
    });

    $(".changeCategory").click(function () {
        var categoryId = $(this).data("categoryid");
        $('<form action="/Admin/EditCategoryDetail"><input type="hidden" name="categoryId" value="' + categoryId + '"></form>').appendTo('body').submit();
    });

    $(".changeProduct").click(function () {
        var productId = $(this).data("productid");
        $('<form action="/Admin/EditProductDetail"><input type="hidden" name="productId" value="' + productId + '"></form>').appendTo('body').submit();
    });

    $(".changeUser").click(function () {
        var userId = $(this).data("userid");
        $('<form action="/Admin/EditUserDetail"><input type="hidden" name="userId" value="' + userId + '"></form>').appendTo('body').submit();
    });

    $(".viewContactForm").click(function () {
        var contactFormId = $(this).data("contactformid");
        $('<form action="/Admin/ViewContactFormDetail"><input type="hidden" name="contactFormId" value="' + contactFormId + '"></form>').appendTo('body').submit();
    });

    $("#addRecommendedProductButton").click(function () {
        var inputGroup = $(".recommendedProductInputGroup");
        $("<div class='row'> \
            <div class='col-md-6'> \
                <div class='input-group mb-3'> \
                    <input type='number' placeholeder='Ürünun Id' name='RecommendedProduct' class='form-control' required> \
                    <div class='input-group-prepend'> \
                        <div class='input-group-text'> \
                            <button type='button' class='recommended_product-delete_button'><i class='fa-solid fa-xmark'></i></button> \
                        </div> \
                    </div> \
                </div> \
            </div> \
          </div>").appendTo(inputGroup);
    });

    $("#addProductParameterButton").click(function () {
        var inputGroup = $(".productParametersInputGroup");
        $("<div class='row mt-2'> \
            <div class='col-md-6'> \
                <div class='row'> \
                    <div class='col-md-5'> \
                        <input type='text' name='ProductParameterName' class='form-control' placeholder='Adı' required> \
                    </div> \
                    <div class='col-md-5'> \
                        <input type='text' name='ProductParameterValue' class='form-control' placeholder='Parametre' required> \
                    </div> \
                    <div class='col-md-1'> \
                        <button type='button' class='product_parameter-delete_button'><i class='fa-solid fa-xmark'></i></button> \
                   </div > \
                </div> \
            </div> \
          </div>").appendTo(inputGroup);
    });

    $("#addHomePageProduct").click(function () {
        var inputGroup = $(".homePageProductsInputGroup");
        $("<div class='row'> \
            <div class='col-md-6'> \
                <div class='input-group mb-3'> \
                    <input type='text' name='HomePageProduct' class='form-control' required > \
                    <div class='input-group-prepend'> \
                        <div class='input-group-text'> \
                            <button type='button' class='home_page_product-delete_button'><i class='fa-solid fa-xmark'></i></button> \
                        </div> \
                    </div> \
                </div> \
            </div> \
          </div>").appendTo(inputGroup);
    });

    $(".homePageProductsInputGroup").on('click', ".home_page_product-delete_button", function () {
        var inputDivs = $(".homePageProductsInputGroup .row").length;
        var productInput = $(this).closest(".row").find("input[name='HomePageProduct']");
        if (!productInput.attr('value') || (inputDivs > 4)) {
            var inputDiv = $(this).closest(".row");
            inputDiv.remove();
        }
    });

    $(".recommendedProductInputGroup").on('click', ".recommended_product-delete_button", function () {
        var inputDivs = $(".recommendedProductInputGroup .row").length;
        var productInput = $(this).closest(".row").find("input[name='RecommendedProduct']");
   
        if (!productInput.attr('value') || inputDivs > 4) {
            var inputDiv = $(this).closest(".row");
            inputDiv.remove();
        }
    });

    $(".productParametersInputGroup").on('click', ".product_parameter-delete_button", function () {
        var inputDivs = $(".productParametersInputGroup .row").length;
        var productInput = $(this).closest(".row").find("input[name='ProductParameter']");

        if (!productInput.attr('value') || inputDivs > 4) {
            var inputDiv = $(this).closest(".row");
            inputDiv.remove();
        }
    });


});