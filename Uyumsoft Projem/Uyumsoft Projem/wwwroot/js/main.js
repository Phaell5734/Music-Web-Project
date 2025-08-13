/* ===================================
--------------------------------------
  SolMusic HTML Template
  Version: 1.0
--------------------------------------
======================================*/

'use strict';

// ➊ COMPONENT INITIALIZERS
function initMixitup() {
    if ($('.playlist-area').length > 0) {
        var containerEl = document.querySelector('.playlist-area');
        mixitup(containerEl);
    }
}

function initSlicknav() {
    $(".main-menu").slicknav({
        appendTo: '.header-section',
        allowParentLinks: true,
        closedSymbol: '<i class="fa fa-angle-right"></i>',
        openedSymbol: '<i class="fa fa-angle-down"></i>'
    });
    $('.slicknav_nav').prepend('<li class="header-right-warp"></li>');
    $('.header-right').clone().prependTo('.slicknav_nav > .header-right-warp');
}

function initSetBg() {
    $('.set-bg').each(function () {
        var bg = $(this).data('setbg');
        $(this).css('background-image', 'url(' + bg + ')');
    });
}

function initHeroSlider() {
    $('.hero-slider').owlCarousel({
        loop: true,
        nav: false,
        dots: true,
        mouseDrag: false,
        animateOut: 'fadeOut',
        animateIn: 'fadeIn',
        items: 1,
        autoplay: true
    });
}

// ➋ PJAX SETUP
function initPjax() {
    if (!$.support.pjax) return;
    // Delegate PJAX for internal links
    $(document).pjax('a:not([data-no-pjax])', '#pjax-container', {
        timeout: 10000
    });
    // After PJAX load, reinitialize components
    $(document).on('pjax:end', function () {
        initMixitup();
        initSlicknav();
        initSetBg();
        initHeroSlider();
        // **restore your global player UI after a PJAX swap**
        if (window.playerRestoreState) {
            window.playerRestoreState();
        }
    });
}

// ➌ PAGE LOAD
$(window).on('load', function () {
    // Preloader fadeOut
    $(".loader").fadeOut();
    $("#preloder").delay(400).fadeOut("slow");
    initMixitup();
});

// ➍ DOCUMENT READY
(function ($) {
    initSlicknav();
    initSetBg();
    initHeroSlider();
    initPjax();
})(jQuery);
