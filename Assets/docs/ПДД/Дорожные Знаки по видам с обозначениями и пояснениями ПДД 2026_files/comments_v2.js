// User reviews cache
var cache_user_reviews = {};

// Get comment id from hash
function getCommentIdFromHash(str, marker) {
    if (str) {
        var reg = new RegExp('\\#' + marker + '(\\d+)', '');
        var result = str.match(reg);
        if (result && result[1]) {
            return result[1];
        }
    }
    return 0;
}

// Find comment in page
function findComment(marker) {
    var commentId = getCommentIdFromHash(window.location.hash, marker);
    var $pagination = $('.b-pagination');
    var response = $.Deferred();
    var commentExistsOnThePage = $('#comment' + commentId).length > 0;
    var isAllCommentsPage = window.location.pathname.indexOf('/all.html') >= 0;

    if (($pagination.length > 0 || isAllCommentsPage) && commentId && !commentExistsOnThePage) {
        $.ajax({
            url: '//www.drom.ru/find_comment.php',
            dataType: 'script',
            data: {
                hasQuoteMe: (marker === 'quoteme' ? 1 : 0),
                comment_id: commentId,
                current_page: isAllCommentsPage ? -1 : $pagination.data('current-page'),
                comments_on_the_page: (isAllCommentsPage ? $('.b-pagination-config') : $pagination).data('per-page')
            },
            xhrFields: {withCredentials: true},
            crossDomain: true,
            timeout: 1000,
            success: function (response) {
                if (response && response.url) {
                    window.location = response.url;
                }
            }
        });
    } else if (commentId && commentExistsOnThePage) {
        response.resolve();
    }

    return response;
}

// Open reply form
function quoteme(marker) {
    var commentId = getCommentIdFromHash(window.location.hash, marker);
    if (commentId) {
        findComment(marker).then(function () {
            window.location.hash = '#comment' + commentId;
            $("[data-comment-reply=" + commentId + "]").trigger('click');
        });
    }
}

$(function () {
    findComment('comment');
    quoteme('quoteme');
    $(document).one('drom.comments.form.binds.fulfiled', function () {
        quoteme('quoteme');
    });

    // Insert author reviews signature
    var signTimerId;
    $(window).on('scroll', function () {
        clearTimeout(signTimerId);
        signTimerId = setTimeout(function () {
            var scrollTop = $(window).scrollTop();
            var windowHeight = $(window).height();

            $('[data-comments-thread]').each(function () {
                var $block = $(this);

                // stop loop
                if (($block.offset().top) > (scrollTop + windowHeight)) {
                    return false;
                }

                var $newReplies = $('[data-new-replies][data-thread-id=' + $block.data('comments-thread') + ']');

                $block.find('.b-comment').each(function () {
                    var $comment = $(this);
                    var cOffTop = $comment.offset().top;
                    // continue loop
                    if (scrollTop > cOffTop) {
                        return;
                    }
                    // stop loop
                    if (scrollTop + (windowHeight / 2) < cOffTop) {
                        return false;
                    }

                    // comment in window
                    $comment.find('.user_reviews').each(function () {
                        var $obj = $(this);
                        var id = $obj.attr('id');
                        var signatureId = $obj.attr('data-signature-source-id');

                        if (id) {
                            cache_user_reviews[id] = $obj.html();
                        } else if (signatureId && !$obj.text().length) {
                            if (!cache_user_reviews[signatureId]) {
                                cache_user_reviews[signatureId] = $('#' + signatureId).html();
                            }
                            $obj.html(cache_user_reviews[signatureId]);
                        }
                    });
                    var $viewedSticker = $comment.find('[data-comment-viewed]');
                    if ($viewedSticker.length > 0) {
                        $viewedSticker.removeAttr('data-comment-viewed');
                        $.ajax({
                            url: '//www.drom.ru/process_ajax_request.php',
                            data: {
                                mode: 'comment_mark_viewed',
                                comment_id: $comment.data('comment')
                            },
                            xhrFields: {withCredentials: true},
                            crossDomain: true
                        })
                            .done(function () {
                                $viewedSticker
                                    .removeClass('b-comment__sticker_theme_unviewed')
                                    .addClass('b-comment__sticker_theme_viewed')
                                    .text('Просмотрен');

                                if ($newReplies.data('new-replies')) {
                                    $newReplies.data('new-replies').count--;
                                }

                                // if ($newReplies.data('new-replies').count === 0) {
                                // if ($newReplies.data('new-replies').count === 0) {
                                //     $newReplies.find('[data-message]')
                                //         .addClass('b-comment__sticker_theme_viewed')
                                //         .text('Вы просмотрели все комментарии')
                                // }
                            });
                    }
                });
            });
        }, 200);
    });

    /** Toggle replies @ comment */
    $(document).on('click', '[data-comment-toggle-replies]', function (ev) {
        var $this = $(this);
        $('[data-comment-replies=' + $this.data('comment-toggle-replies') + ']').toggle();
        $this.toggleClass('b-button_active');
    });

    // Voting
    $(document).on('click', '[data-comments-vote]', function (ev) {
        ev.preventDefault();
        var $container = $(this);
        var $btn = $(ev.target);
        var $btnCont = $btn.closest('[data-comments-vote-type]');
        var options = $container.data('commentsVote');
        var $thread = $btn.closest('[data-comments-thread]');

        if (!options || !options.can_vote || $btn.attr('data-comments-vote-trigger') === undefined) {
            return;
        }

        var data = {
            mode: 'ivote_comment',
            result_type: 'json',
            thread_id: $container.closest('[data-comments-thread]').data('commentsThread'),
            comment_id: options.id,
            ivote_mark: $btnCont.data('commentsVoteType'),
            hash: $thread.data('comments-thread-hash')
        };

        $.ajax({
            data: data,
            dataType: 'json',
            type: 'GET',
            url: '//www.drom.ru/process_ajax_request.php',
            xhrFields: {withCredentials: true},
            crossDomain: true
        })
            .done(function (res) {
                updateCounters(res.positive_count, res.negative_count);
                if (!res.can_vote) {
                    lockButtons();
                }
            })
            .fail(function (xhr) {
                // TODO: error log
                console.log(xhr);
            })
            .always(function () {
                // TODO: preloader
            });

        function lockButtons() {
            $container.removeAttr('data-comments-vote');
            $container.find('[data-comments-vote-type]').addClass('b-comment__vote_disabled');
        }

        function updateCounters(pos, neg) {
            pos = parseInt(pos, 10);
            neg = parseInt(neg, 10);

            var $pos = $container.find('[data-comments-vote-type="1"]');
            var $neg = $container.find('[data-comments-vote-type="2"]');
            var textNodeSel = '[data-comments-vote-text]';
            var prevalingClassName = 'b-comment__vote_prevailing';

            $container.find('[data-comments-vote-type]').removeClass(prevalingClassName);

            if (pos > neg) {
                $pos.addClass(prevalingClassName);
            } else if (neg > pos) {
                $neg.addClass(prevalingClassName);
            }

            if (pos > 0) {
                $pos.find(textNodeSel).text(pos);
            } else {
                $pos.hide();
            }

            if (neg > 0) {
                $neg.find(textNodeSel).text(neg);
            } else {
                $neg.hide();
            }
        }
    });

    // Show hide quoteme
    $(document).on('click', '.b-comment__answer-toggler', function (e) {
        e.preventDefault();
        var $toggler = $(this);
        if ($toggler.prop('disabled')) {
            return;
        }
        var hash = $toggler.data('answer-hash');
        if (hash) {
            $toggler.prop('disabled', true);
            $toggler.parent().addClass('loading');
            $.ajax({
                url: '//www.drom.ru/process_ajax_request.php',
                data: {
                    mode: 'get_comment',
                    comment_hash: hash
                },
                xhrFields: {withCredentials: true},
                crossDomain: true
            })
                .done(function (res) {
                    if (res.status) {
                        $toggler.prev().html(res.data.comment_text_html);
                        $toggler.data('answer-hash', '');
                    }
                })
                .always(function () {
                    $toggler.prop('disabled', false);
                    $toggler.parent().removeClass('loading');
                    $toggler.parent().toggleClass('active');
                });
        } else {
            $toggler.parent().toggleClass('active');
            if (!$toggler.parent().hasClass('active')) {
                var offsetY = $toggler.parent().offset().top - 20;
                if ($('html').hasClass('drom-mobile')) {
                    offsetY -= 60;
                }
                $('html, body').scrollTop(offsetY);
            }
        }
    });
});
