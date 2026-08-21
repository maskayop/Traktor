/**
 * Created by mishchuk on 15.03.2017.
 */
// кусок md5 из cdn
var MD5=function(d){d=unescape(encodeURIComponent(d));result=M(V(Y(X(d),8*d.length)));return result.toLowerCase()};function M(d){for(var _,m="0123456789ABCDEF",f="",r=0;r<d.length;r++)_=d.charCodeAt(r),f+=m.charAt(_>>>4&15)+m.charAt(15&_);return f}function X(d){for(var _=Array(d.length>>2),m=0;m<_.length;m++)_[m]=0;for(m=0;m<8*d.length;m+=8)_[m>>5]|=(255&d.charCodeAt(m/8))<<m%32;return _}function V(d){for(var _="",m=0;m<32*d.length;m+=8)_+=String.fromCharCode(d[m>>5]>>>m%32&255);return _}function Y(d,_){d[_>>5]|=128<<_%32,d[14+(_+64>>>9<<4)]=_;for(var m=1732584193,f=-271733879,r=-1732584194,i=271733878,n=0;n<d.length;n+=16){var h=m,t=f,g=r,e=i;f=md5_ii(f=md5_ii(f=md5_ii(f=md5_ii(f=md5_hh(f=md5_hh(f=md5_hh(f=md5_hh(f=md5_gg(f=md5_gg(f=md5_gg(f=md5_gg(f=md5_ff(f=md5_ff(f=md5_ff(f=md5_ff(f,r=md5_ff(r,i=md5_ff(i,m=md5_ff(m,f,r,i,d[n+0],7,-680876936),f,r,d[n+1],12,-389564586),m,f,d[n+2],17,606105819),i,m,d[n+3],22,-1044525330),r=md5_ff(r,i=md5_ff(i,m=md5_ff(m,f,r,i,d[n+4],7,-176418897),f,r,d[n+5],12,1200080426),m,f,d[n+6],17,-1473231341),i,m,d[n+7],22,-45705983),r=md5_ff(r,i=md5_ff(i,m=md5_ff(m,f,r,i,d[n+8],7,1770035416),f,r,d[n+9],12,-1958414417),m,f,d[n+10],17,-42063),i,m,d[n+11],22,-1990404162),r=md5_ff(r,i=md5_ff(i,m=md5_ff(m,f,r,i,d[n+12],7,1804603682),f,r,d[n+13],12,-40341101),m,f,d[n+14],17,-1502002290),i,m,d[n+15],22,1236535329),r=md5_gg(r,i=md5_gg(i,m=md5_gg(m,f,r,i,d[n+1],5,-165796510),f,r,d[n+6],9,-1069501632),m,f,d[n+11],14,643717713),i,m,d[n+0],20,-373897302),r=md5_gg(r,i=md5_gg(i,m=md5_gg(m,f,r,i,d[n+5],5,-701558691),f,r,d[n+10],9,38016083),m,f,d[n+15],14,-660478335),i,m,d[n+4],20,-405537848),r=md5_gg(r,i=md5_gg(i,m=md5_gg(m,f,r,i,d[n+9],5,568446438),f,r,d[n+14],9,-1019803690),m,f,d[n+3],14,-187363961),i,m,d[n+8],20,1163531501),r=md5_gg(r,i=md5_gg(i,m=md5_gg(m,f,r,i,d[n+13],5,-1444681467),f,r,d[n+2],9,-51403784),m,f,d[n+7],14,1735328473),i,m,d[n+12],20,-1926607734),r=md5_hh(r,i=md5_hh(i,m=md5_hh(m,f,r,i,d[n+5],4,-378558),f,r,d[n+8],11,-2022574463),m,f,d[n+11],16,1839030562),i,m,d[n+14],23,-35309556),r=md5_hh(r,i=md5_hh(i,m=md5_hh(m,f,r,i,d[n+1],4,-1530992060),f,r,d[n+4],11,1272893353),m,f,d[n+7],16,-155497632),i,m,d[n+10],23,-1094730640),r=md5_hh(r,i=md5_hh(i,m=md5_hh(m,f,r,i,d[n+13],4,681279174),f,r,d[n+0],11,-358537222),m,f,d[n+3],16,-722521979),i,m,d[n+6],23,76029189),r=md5_hh(r,i=md5_hh(i,m=md5_hh(m,f,r,i,d[n+9],4,-640364487),f,r,d[n+12],11,-421815835),m,f,d[n+15],16,530742520),i,m,d[n+2],23,-995338651),r=md5_ii(r,i=md5_ii(i,m=md5_ii(m,f,r,i,d[n+0],6,-198630844),f,r,d[n+7],10,1126891415),m,f,d[n+14],15,-1416354905),i,m,d[n+5],21,-57434055),r=md5_ii(r,i=md5_ii(i,m=md5_ii(m,f,r,i,d[n+12],6,1700485571),f,r,d[n+3],10,-1894986606),m,f,d[n+10],15,-1051523),i,m,d[n+1],21,-2054922799),r=md5_ii(r,i=md5_ii(i,m=md5_ii(m,f,r,i,d[n+8],6,1873313359),f,r,d[n+15],10,-30611744),m,f,d[n+6],15,-1560198380),i,m,d[n+13],21,1309151649),r=md5_ii(r,i=md5_ii(i,m=md5_ii(m,f,r,i,d[n+4],6,-145523070),f,r,d[n+11],10,-1120210379),m,f,d[n+2],15,718787259),i,m,d[n+9],21,-343485551),m=safe_add(m,h),f=safe_add(f,t),r=safe_add(r,g),i=safe_add(i,e)}return Array(m,f,r,i)}function md5_cmn(d,_,m,f,r,i){return safe_add(bit_rol(safe_add(safe_add(_,d),safe_add(f,i)),r),m)}function md5_ff(d,_,m,f,r,i,n){return md5_cmn(_&m|~_&f,d,_,r,i,n)}function md5_gg(d,_,m,f,r,i,n){return md5_cmn(_&f|m&~f,d,_,r,i,n)}function md5_hh(d,_,m,f,r,i,n){return md5_cmn(_^m^f,d,_,r,i,n)}function md5_ii(d,_,m,f,r,i,n){return md5_cmn(m^(_|~f),d,_,r,i,n)}function safe_add(d,_){var m=(65535&d)+(65535&_);return(d>>16)+(_>>16)+(m>>16)<<16|65535&m}function bit_rol(d,_){return d<<_|d>>>32-_}

document.addEventListener('DOMContentLoaded', function () {
    var UNAUTHORIZED_MESSAGE = 'Для того, чтобы добавить комментарий, пожалуйста, авторизуйтесь:';
    var UNAUTHORIZED_MESSAGE_SINGLE_AUTH = 'Для того, чтобы добавить комментарий, пожалуйста, ' +
        '<a href="%href%" rel="nofollow">авторизуйтесь</a>';
    var unexpectedErrText = 'Произошла ошибка, пожалуйста попробуйте позже';
    var btnLoadingClass = 'b-button_state_loading';
    var commentAddBtnAttr = 'data-add-comment';
    var userInfoSelector = '.js-auth_user_info';
    var $body = $(document.body);
    //URL на дром авторизацию
    var myAuthSignUrl = document.getElementById('ajax_auth_form').dataset.myAuthSignUrl;
    //[ALD-1608] Показывать "единую авторизацию"
    var isShowSingleAuth = false;
    var currentUserInfoHtml = '';
    var isMobile = document.documentElement.classList.contains('drom-mobile');
    var defaultOptions = {
        isShowNotification: false,
        isClosable: false,
        isShort: false,
    };

    // Auth on comment form
    $(document).on('click', '[data-toggle-forum-auth]', function (e) {
        e.preventDefault();
        $(e.currentTarget).toggleClass('b-button_active');
        $('[data-forum-auth]').slideToggle();
    });

    $body
        .on(isMobile ? 'touchstart.commentShort' : 'click.commentShort', function (e) {
            $('[data-comment-short="open"]').each(function () {
                var $shortForm = $(this);
                var commentWidget = $shortForm[0].parentElement;
                var $target = $(e.target);
                var $textarea = $shortForm.find('textarea');
                if (
                    commentWidget !== $target.closest('[data-comments-form-widget]')[0] &&
                    !$target.hasClass('dz-hidden-input') &&
                    !$textarea.val() &&
                    !$textarea.is(':focus') &&
                    $shortForm.find('.dz-image-preview').length === 0
                ) {
                    $shortForm[0].dataset.commentShort = 'close';
                    $shortForm.hide();
                    $shortForm.siblings('[data-comments-short-trigger]').show();
                }
            });
        })
        .on('drom.authorized', function (e, userInfoHtml) {
            refreshAuth(userInfoHtml);
            var shortBlocks = document.querySelectorAll('[data-comments-short-trigger]');
            for (var i = 0; i < shortBlocks.length; i++) {
                if (userAccount.nickname && shortBlocks[i].children.length === 1) {
                    var nick = userAccount.nickname;
                    if (userAccount.profile_url) {
                        nick = '<a href="' + userAccount.profile_url + '" class="b-link" target="_blank">' + nick + '</a>';
                    }
                    shortBlocks[i].insertAdjacentHTML('afterbegin', '<div class="b-flex__item" style="padding-right: 40px; flex-shrink: 0;">' + nick + '</div>');
                }
            }
        })
        .on('drom.logout', function () {
            refreshAuth('');
            var shortBlocks = document.querySelectorAll('[data-comments-short-trigger]');
            for (var i = 0; i < shortBlocks.length; i++) {
                if (shortBlocks[i].children.length === 2) {
                    shortBlocks[i].removeChild(shortBlocks[i].children[0]);
                }
            }
        });

    if (typeof loginType !== 'undefined' && typeof userAccount !== 'undefined') {
        showUserInfo(loginType, userAccount);
    }

    var CommentTextStorage = function($el, threadId) {
        this._$el = $el;
        this._threadId = threadId;
        this._key = 'comment-text-' + this._threadId;
        this._isLocalStorageSupported = this._checkLocalStorageSupport();
        this._restoreText();
        this._attachEvents();
    };
    CommentTextStorage.prototype = {
        constructor: CommentTextStorage,
        _$el: null,
        _threadId: null,
        _isLocalStorageSupported: null,
        _key: null,
        _inputEvent: 'keyup',
        _checkLocalStorageSupport: function() {
            try {
                window.localStorage.setItem('___foo', 'bar');
                window.localStorage.removeItem('___foo');
                return true;
            } catch (e) {
                return false;
            }
        },
        _attachEvents: function() {
            this._$el
                .off(this._inputEvent, this._handleInput)
                .on(this._inputEvent, this._handleInput.bind(this));
        },
        _handleInput: function() {
            if (!this._isLocalStorageSupported) return;
            var text = this._$el.val();
            if (!text) {
                this.flush();
                return;
            }
            window.localStorage.setItem(this._key, text);
        },
        _restoreText: function() {
            if (!this._isLocalStorageSupported) return;
            var text = window.localStorage.getItem(this._key);
            if (!text) return;
            this._$el.val(text);
        },
        flush: function() {
            if (!this._isLocalStorageSupported) return;
            window.localStorage.removeItem(this._key);
        },
    };

    onPageLoaded();

    $(document)
        .on('click', '[data-comment-reply]', function () {
            var $link = $(this);
            var replyId = $link.data('comment-reply');
            var $target = $('#comment' + replyId);
            var threadId = $link.closest('[data-comments-thread]').data('comments-thread');
            var options = $.extend({}, defaultOptions, { isClosable: true });
            var $formContainer = initForm(threadId, replyId, options);
            $formContainer.find('[name="source_id"]').val(3);
            $target.after($formContainer);
            var offsetY = isMobile ?
                $formContainer.find('textarea').offset().top - 80 :
                $target.offset().top;
            $('html, body').scrollTop(offsetY);
            $formContainer.find('textarea').trigger('focus');
            setEventListeners($formContainer, options);
        })
        .on('focusin', '[data-comments-short-trigger] input', function () {
            $body.trigger(isMobile ? 'touchstart.commentShort' : 'click.commentShort');
            var $triggerBlock = $(this).closest('[data-comments-short-trigger]');
            var $formPlaceholder = $triggerBlock.closest('[data-comments-form-widget]');
            var $formContainer = $triggerBlock.siblings('[data-comment-short="close"]');
            if (!$formContainer.length) {
                var threadId = $formPlaceholder.data('comments-form-widget');
                var options = $.extend({}, defaultOptions, $formPlaceholder.data('comments-form-widget-options'));
                $formContainer = initForm(threadId, null, options);
                $formContainer.find('[name="source_id"]').val(options.sourceId);
                $triggerBlock.after($formContainer);
                setEventListeners($formContainer, options);
            }
            $formContainer[0].dataset.commentShort = 'open';
            $formContainer.show();
            $formContainer.find('textarea').focus();
            $triggerBlock.hide();
            if (isMobile) {
                $('html, body').scrollTop($formContainer.find('textarea').offset().top - 100);
            }
        });

    $(document).trigger('drom.comments.form.binds.fulfiled');

    function onPageLoaded() {
        /* init inline forms */
        var widgets = $('[data-comments-form-widget]');
        widgets.each(function (i, el) {
            onFormWidgetAppeared($(el));
        });

        // Scroll to form and focus
        if (window.location.hash.search('add_comment') > -1) {
            var $f = $(window.location.hash);
            if ($f.length > 0) {
                $f[0].scrollIntoView(false);
                $f.find('textarea').trigger('focusin');
            }
        }
    }

    function onFormWidgetAppeared($formPlaceholder, options) {
        if ($formPlaceholder.find('[data-comments-short-trigger]').length && document.getElementById('comments_form')) {
            return;
        }
        options = $.extend({}, defaultOptions, $formPlaceholder.data('comments-form-widget-options') || {}, options || {});
        isShowSingleAuth = true === options.isShowSingleAuth;

        var $formContainer = $formPlaceholder.children();
        var threadId = $formPlaceholder.data('comments-form-widget');
        if ($formContainer.length === 0) {
            $formContainer = initForm(threadId, null, options);
            $formPlaceholder.html($formContainer);
        } else {
            $formContainer.data('comments-form-thread', threadId);
            $formContainer.find('[name="thread_id"]').val(threadId);
            $formContainer.find('[name="xurl_uploading_files"]').val(generateXurl());
        }
        setEventListeners($formContainer, options);
    }

    function initForm(threadId, replyId, options) {
        var $formContainer = $('<div class="b-comment-form b-media-cont b-media-cont_no-margin b-media-cont_relative"></div>');
        var $tmp = $($('#comment-form-tpl').html());
        isShowSingleAuth = options.isShowSingleAuth; // [DROM-2989] пробрасываем переменную в глобальную, чтобы кнопка выйти скрылась при раскрытии виджета
        $tmp.find('#comments_form')[0].id = 'comments_form_' + threadId;
        $tmp.find('#comment_text')[0].id = 'comment_text_' + threadId;
        $tmp.find('#comments_go')[0].id = 'comments_go_' + threadId;
        $formContainer.html($tmp);
        $formContainer.data('comments-form-thread', threadId);

        var $addBtn = $formContainer.find('[' + commentAddBtnAttr + ']');

        if (replyId) {
            $('[data-comments-reply-form]').remove();
            var $cancelBtn = $('<span data-cancel-comment-form class="b-link">Передумал отвечать</span>');
            $addBtn.attr(commentAddBtnAttr, replyId);
            $addBtn.children().text('Ответить');
            $addBtn.wrap('<div class="b-random-group b-random-group_margin_r-size-l"></div>');
            $addBtn.parent().append($cancelBtn);
            $formContainer.attr('data-comments-reply-form', '');
            $formContainer.attr('id', 'quoteme' + replyId);
        }

        if (options.isClosable) {
            var $closeBtn = $('<div data-cancel-comment-form class="b-close-btn b-close-btn_pos-abs"></div>');
            $formContainer = $formContainer.prepend($closeBtn);
        }

        $formContainer.find('[name="thread_id"]').val($formContainer.data('comments-form-thread'));
        $formContainer.find('[name="xurl_uploading_files"]').val(generateXurl());
        return $formContainer;
    }

    function resetFormValues($formContainer) {
        $formContainer.find('form')[0].reset();
        $formContainer.find('[name="thread_id"]').val($formContainer.data('comments-form-thread'));
        $formContainer.find('[name="xurl_uploading_files"]').val(generateXurl());
        // refresh drom upload
        var duContainer = $formContainer.find('[data-du-container]')[0];
        if (!duContainer) return;
        if (duContainer.dropzone && typeof DromUpload !== 'undefined') {
            duContainer.dropzone.destroy();
            DromUpload({
                container: duContainer,
                url: '//www.drom.ru/upload_file.php',
                fields: {
                    'xurl_uploading_files': $formContainer.find('[name="xurl_uploading_files"]').val(),
                    'thread_id': $formContainer.data('comments-form-thread')
                },
                minCropBoxWidth: 120,
                minCropBoxHeight: 90,
                enableMain: false,
                enableEdit: false,
                enableLoad: false
            });
        }
    }

    const trackStats = (success) => {
        const statsName = 'add_comment';

        globalGaStats.vaTrack({
            name: statsName,
            event: 'click',
            payload: {
                sending_success: success ? 1 : 0,
            },
        });
    };

    function setEventListeners($formContainer, options) {
        var $textarea = $formContainer.find('[name="comment_text"]');
        var currentThreadID = parseInt($formContainer.find('[name="thread_id"]').val(), 10);
        var storage = null;

        if ($textarea.length && currentThreadID) {
            storage = new CommentTextStorage($textarea, currentThreadID);
        }

        $formContainer.find('[data-cancel-comment-form]').off('click').on('click', function (e) {
            e.preventDefault();
            $formContainer.remove();
        });
        refreshAuth(currentUserInfoHtml, $formContainer);

        $textarea.off('keydown').on('keydown', function (e) {
            if (e.ctrlKey && e.keyCode === 13 && currentUserInfoHtml.length > 0) {
                e.currentTarget.form.submit();
            }
        });
        $formContainer.find('form').off('submit').on('submit', function (ev) {
            ev.preventDefault();
            var $this = $(this);
            var threadId = $this.find('[name="thread_id"]').val();
            var replyId = $this.find('[' + commentAddBtnAttr + ']').attr(commentAddBtnAttr);
            var $thread = $('[data-comments-thread=' + threadId + ']');
            var url = '//www.drom.ru/add_comment_new.php';
            var data = {
                response_type: 'json',
                answer_id: replyId
            };
            var $addBtn = $formContainer.find('[' + commentAddBtnAttr + ']');

            $(userInfoSelector, $formContainer).find('[name]').each(function (idx, field) {
                data[field.name] = field.value;
            });

            data = $this.serialize() + '&' + $.param(data);

            $addBtn.addClass(btnLoadingClass);

            $.ajax({
                url: url,
                type: 'POST',
                method: 'POST',
                data: data,
                xhrFields: {withCredentials: true}
            })
                .done(function (res) {
                    if (res.status && !res.errors.length) {
                        $(document).trigger('drom.comments.form.submit.success', [threadId, res]);
                        resetFormValues($formContainer);
                        if (storage) {
                            storage.flush();
                        }
                        $body.trigger(isMobile ? 'touchstart.commentShort' : 'click.commentShort');
                        if (replyId && res.comment_html.length) {
                            $formContainer.after(res.comment_html);
                            $formContainer.remove();
                        } else if ($thread.length > 0 && res.comment_html.length) {
                            $thread.show().find('[data-comments-list]').append(res.comment_html);
                        }

                        if (options.isShowNotification) {
                            var message = 'Комментарий опубликован!';

                            if (res.comment_url && res.comment_url.length > 0) {
                                message = message +
                                    ' <a data-comment-reply-navigate href="' + res.comment_url +
                                    '" class="b-link">Перейти к комментарию</a>';
                            }

                            showNotification(message, 'success');
                        }

                        trackStats(true);
                    } else if (!res.status && res.errors.length) {
                        refreshErrors($formContainer, res.errors);
                        trackStats(false);
                    } else {
                        showNotification(unexpectedErrText, 'error');
                        trackStats(false);
                    }
                })
                .fail(function () {
                    showNotification(unexpectedErrText, 'error');
                    trackStats(false);
                })
                .always(function () {
                    $addBtn.removeClass(btnLoadingClass);
                });
        });

        var $thread = $formContainer.closest('[data-comments-thread]');
        var isAttach = $thread.length > 0 ?
            $thread.data('is-show-attach-control') :
            options.isShowAttachControl;

        if (isAttach && typeof DromUpload !== 'undefined') {
            $formContainer.find('[data-comments-attach-control]').show();
            var duContainer = $formContainer.find('[data-du-container]')[0];
            if (!duContainer) return;
            if (duContainer.dropzone) {
                duContainer.dropzone.destroy();
            }
            DromUpload({
                container: duContainer,
                url: '//www.drom.ru/upload_file.php',
                fields: {
                    'xurl_uploading_files': $formContainer.find('[name="xurl_uploading_files"]').val(),
                    'thread_id': $formContainer.data('comments-form-thread')
                },
                minCropBoxWidth: 120,
                minCropBoxHeight: 90,
                enableMain: false,
                enableEdit: false,
                enableLoad: false
            });
        }
    }

    function showNotification(message, type) {
        if (typeof DromNotificationGroup === 'undefined') {
            return;
        }
        var $notifContainer = $('[data-notification-cont]').eq(0);
        if (!$notifContainer.length) {
            var $menu = $('.b-menu').eq(0);
            if (!$menu.length) {
                $menu = $('.m-header').eq(0);
            }
            if (!$menu.length) {
                $menu = $('[id=info_div]').eq(0);
            }
            $notifContainer = $(
                '<div data-notification-cont class="b-notifications-group b-notifications-group_relative"></div>'
            );
            $menu.after($notifContainer);
        }
        $notifContainer.empty();

        var notif = DromNotificationGroup.getInstance(
            $notifContainer[0], {useContainerOffset: true}
        );

        var notificationId = 'commentsMessage_' + type;

        notif.createNotification(message, type, notificationId);

        setTimeout(function () {
            notif.removeNotification(notificationId);
        }, 15000);
    }

    function refreshErrors($formContainer, errors, type) {
        type = type || 'red';

        var oldMessages = $formContainer.find('[data-comment-form-message]');

        if (oldMessages.length > 0) {
            oldMessages.remove();
        }

        if (errors.length > 0) {
            var error = errors[errors.length - 1];
            $formContainer.prepend(
                '<div class="' + type + '-notification ' + type + '-notification_show no-margin" ' +
                'data-comment-form-message>' + error + '</div>'
            );
        }
    }

    /**
     * Возвращает сообщение о необходимости авторизации
     * @returns {string}
     */
    function getUnauthorizedMessage() {
        return isShowSingleAuth
            //[ALD-1608] Сообщение со ссылкой на авторизацию
            ? UNAUTHORIZED_MESSAGE_SINGLE_AUTH.replace('%href%', myAuthSignUrl)
            : UNAUTHORIZED_MESSAGE;
    }

    function refreshAuth(html, $formContainer) {
        var $notAuthNotification = $('#cant_add_comment_message', $formContainer);
        var $addBtn = $('[data-add-comment]', $formContainer);
        var $isAuthenticated = !!html;

        currentUserInfoHtml = html;

        $(userInfoSelector, $formContainer).html(currentUserInfoHtml);

        //[ALD-1608] Скрываем иконки вариантов авторизации если единая авторизация и не авторизованы
        $('[data-comments-auth-widget]').toggle(!(isShowSingleAuth && !$isAuthenticated));

        //[ALD-1608] Скрываем кнопку "выйти" если единая авторизация и авторизованы
        if (isShowSingleAuth && $isAuthenticated) {
            $('.b-text_text-block:last-child', userInfoSelector).hide();
        }

        if (currentUserInfoHtml) {
            $("#auth_menu", $formContainer).hide();
            $addBtn
                .removeClass('b-button_locked')
                .attr('disabled', false);
            $notAuthNotification.css({
                'display': 'none',
                'opacity': 0
            });
        } else {
            $("#auth_menu", $formContainer).show();
            $addBtn
                .addClass('b-button_locked')
                .attr('disabled', true);
            $notAuthNotification.css({
                'display': 'block',
                'opacity': 1
            });
        }

        refreshErrors(
            $formContainer && $formContainer.hasClass('b-comment-form')
              ? $formContainer
              : $('.b-comment-form'),
            !currentUserInfoHtml ? [getUnauthorizedMessage()] : [],
            'normal'
        );
    }

    function generateXurl() {
        return MD5((new Date()).toISOString() + parseInt(Math.random() * Math.pow(10, 10)));
    }
});
