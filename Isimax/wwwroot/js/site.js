$(document).ready(function () {

    if ($(window).width() < 768) {
        $('.desktop_language-select').remove();
    } else {
        $('.mobile_language-select').remove();
    }

    $(".content-block").css("background-image", "url('/uploads/sliderImage1.jpg')");
    let i = 2;
    setInterval(function () {
        $(".content-block").css("background-image", "url('/uploads/sliderImage" + i + ".jpg')");
        if (i == 3) {
            i = 1;
        } else {
            i++;
        }
    }, 3000);


    var prevScrollpos = window.pageYOffset;
    window.onscroll = function () {
        var currentScrollPos = window.pageYOffset;
        if ((prevScrollpos > currentScrollPos) || $(window).scrollTop() == 0) {
            $(".hamburger-menu").css("top", "0px");
            $(".menu__btn").css("display", "block");
        } else if ((prevScrollpos < currentScrollPos) && $(window).scrollTop() != 0) {
            $(".hamburger-menu").css("top", "-60px");
            $(".menu__btn").css("display", "none");
        } 
        prevScrollpos = currentScrollPos;
    }

    var clickTracker = 1;
    $(".menu__btn").click(function () {
        if (clickTracker == 1) {
            $('html, body').css({
                overflow: 'hidden',
                height: '100%'
            });
            clickTracker++;
        } else {
            $('html, body').css({
                overflow: 'auto',
                height: 'auto'
            });
            clickTracker--;
        }
    });


    var myCarousel = document.querySelector('#carouselExampleIndicators')
    var carousel = new bootstrap.Carousel(myCarousel, {
        interval: 2000,
        wrap: true
    })


});