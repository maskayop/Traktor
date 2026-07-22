jQuery(document).ready(function ($) {
    /* Радио кнопки в карточке товара */
    $('label.radio').each(function(){
       $(this).wrap("<div class='form-group'><div class='radio radio-primary'></div></div>");
    }); 
    /* Чекбоксы в карточке товара */
    $('.product-field input[type="checkbox"]').each(function(){
        var value = $(this).next().text();
        $(this).next().remove();
        $(this).wrap('<div class="form-group"><div class="checkbox"><label></label></div></div>');
        $(this).after(value);
        $(this).show();
    });
    
    // Стили для кнопки просмотра корзины в модуле корзины
    $('#vmCartModule').mouseover(function(){
        $('.show_cart a').addClass('btn btn-raised btn-primary');
    });
    
    /*Стили для кнопки Подробнее в Блоге*/
    $('.article-intro .readmore-link').addClass('btn btn-raised btn-default');

    /*Стили для страницы аккаунта*/
    $("form#adminForm table.adminForm label").addClass('control-label');
    $("form#adminForm table.adminForm input,form#adminForm table.adminForm select").addClass('form-control');

    /*Стили для страницы восстановления логина и пароля*/
    $('form#user-registration label').addClass('control-label');
    $('form#user-registration input').addClass('form-control');
    $('form#user-registration button').addClass('btn-raised');

    $('.reset-confirm button[type="submit"]').addClass('btn btn-raised btn-primary');
    $('.view-login .login-wrap button[type="submit"]').addClass('btn btn-raised btn-primary');

    //$('form#user-registration .col-sm-9').attr({'class':'col-sm-8'});
    /*Всплывающие подсказки*/
    $('[data-toggle="tooltip"]').tooltip();
    
    /*Страница корзины*/    
    /*$('#shipment_ul label label').each(function(){
        var textShipment = $(this).text();
        $(this).replaceWith(textShipment);
    });
    $('#payment_ul label span.vmpayment').each(function(){
        var textPayment = $(this).text();
        $(this).replaceWith(textPayment);
    });*/   
    
    /*способы доставки*/
    function upShipment(){
        $('#shipment_ul label.opg-width-1-1').each(function(){
            var shipmentInput = $(this).find('input').get(0).outerHTML; // получение тега input 
            var shipmentText = $(this).find('.vmshipment').html(); // получение названия доставки
            $(this).html(shipmentInput + shipmentText);
            $(this).wrap('<div class="form-group"><div class="radio radio-primary"></div></div>');
        });    
    }
    upShipment();
    
    /*способы оплаты*/
    function upPayment(){
        $('#payment_ul > li').each(function(){
            var paymentInput = $(this).find('input').get(0).outerHTML; // получение тега input 
            var paymentText = $(this).find('.vmpayment').html(); // получение названия доставки
            $(this).html('<div class="form-group"><div class="radio radio-primary"><label>' + paymentInput + paymentText + '</label></div></div>');
        });    
    }
    upPayment();
    
    /*поля ввода данных*/
    $('#billto_inputdiv input').each(function(){
        var titleInput = $(this).attr('placeholder');
        var fotInput = $(this).attr('id');
        $(this).before("<label class='col-lg-2 col-md-3 col-sm-3 control-label'></label>");
        $(this).prev('label').attr({'for':fotInput}).html(titleInput);
        $(this).removeAttr('placeholder').addClass('form-control');
    }); 
    /*купон в корзине*/
    $('#coupon_code + span input').addClass('btn btn-raised btn-default');
    
    /*оформление заказа с регистрацией*/
    $('#user_fields_div input').each(function(){
        $(this).wrap('<div class="form-group"></div>');
        var titleInput = $(this).attr('placeholder');
        var fotInput = $(this).attr('id');
        $(this).before("<label class='col-lg-2 col-md-3 col-sm-3 control-label'></label>");
        $(this).prev('label').attr({'for':fotInput}).html(titleInput);
        $(this).removeAttr('placeholder').addClass('form-control');
    });
    $('#logindiv input').each(function(){
        $(this).wrap('<div class="form-group"></div>');
        var titleInput = $(this).attr('placeholder');
        var fotInput = $(this).attr('id');
        $(this).before("<label class='col-lg-2 col-md-3 col-sm-3 control-label'></label>");
        $(this).prev('label').attr({'for':fotInput}).html(titleInput);
        $(this).removeAttr('placeholder').addClass('form-control');
    });
    $('#logindiv .login a').addClass('btn btn-raised btn-primary');
    
    /* Страница благодарности за заказ */
    $('.vm-order-done .vm-button-correct').addClass('btn btn-raised btn-default');
    
    /* Браузеры без поддержки flexbox*/
    if (!Modernizr.flexwrap) {
        $('head').append('<script src="/templates/t3_bs3_blank/local/js/prefixfree.min.js" type="text/javascript"></script>');
    } 
    
    /* Выбор первого изображения в airslider при смене вида каталога */
    var block = $('.vm-trumb-slider');
    if(block){
        $('.product-view a').click(function() {
            var topHeight = $(document).scrollTop();
            //console.log(topHeight);
            $('.slick-dots li:first-child button').trigger('click');
            $('html, body').scrollTop(topHeight);
            return false;
        });
    }
    
    /*
    Переключение табов при просмотре отзывов
    */
    var hash = window.location.hash;
    if(hash == '#tabs-product'){
        $('.tab-home, .tab-content #home').removeClass('active');
        $('.tab-reviews').addClass('active');
        $('.tab-content #reviews').addClass('active in');
        $('.nav-tabs li').click(function() {
            var topHeight = $(document).scrollTop();
            window.location.hash = ''; 
            $('html, body').scrollTop(topHeight);
        });
    } 
    
    /* Кнопка Наверх */
    $('#back-to-top').on('click', function(){
		$("html, body").animate({scrollTop: 0}, 500);
		return false;
	}); 
    
    /*быстрый просмотр */
    $('a.quickview').click(function(event){
        event.preventDefault();
        $.fancybox.showActivity();
        $.fancybox({
            href: $(this).attr('href'),
            type: 'iframe',
            width: 800,
            height: 600,
            autoScale: true,
            centerOnScroll: true,
            onClosed: function(){
                if(localStorage.getItem('addtocart')){
                    jQuery('#vmCartModule').updateVirtueMartCartModule();
                }  
            },
            onStart: function(){
                localStorage.removeItem('addtocart');
                $('#fancybox-content').addClass('load');
            },
            onCleanup: function(){
                $('#fancybox-content').removeClass('load');
            }
        });
      });
    $('html.component input[name="addtocart"]').click(function(){
        localStorage.setItem('addtocart', true);
    });
    
    /**********
    VP One Page
    ***********/
    /*поля контакта*/
    $('.proopc-bt-address .inner').addClass('form-group');
    $('.proopc-bt-address label').addClass('col-lg-2 col-md-3 col-sm-3 control-label');
    $('.proopc-bt-address input, .proopc-bt-address select').addClass('form-control');
    setTimeout(function(){$('.proopc-bt-address .hover-tootip').removeAttr('data-tiptext');}, 1500);
    
    /*кнопка купона*/
    $('#proopc-coupon button').wrap('<div class="btn btn-sm btn-raised btn-default"></div>');
    
    /*удалить всплывающую подсказку у комментрия к заказу*/
    setTimeout(function(){$('.proopc-additional-info .hover-tootip').removeAttr('data-tiptext');}, 1500);
    
    /*чекбокс согласия с условиями обслуживания*/
    //$('.proopc-additional-info input[type="checkbox"]').addClass('');
    $('.proopc-additional-info .cart-tos-group').addClass('checkbox');
    
    /*кнопка подтвердить заказ*/
    $('#proopc-order-submit').wrap('<div class="btn btn-raised btn-primary"></div>');
    /************
    end VP One Page
    ************/
});