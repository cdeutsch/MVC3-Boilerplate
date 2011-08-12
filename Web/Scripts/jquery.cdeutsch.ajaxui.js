/// <reference path="jquery.js" />

//add onto the jquery namespace
(function ($) {

    //function myPrivateFunction() { };

    //    ////debugging:
    //    //this is a private static member that is only available in this closure
    //    var instances = 0;
    //    //this is a private static method that can be used internally
    //    function _incrementInstances() {
    //        instances++;
    //    }

    //add helper functions

    $.flashClear = function (selector) {
        if (!selector) {
            selector = '#flashMessage';
        }
        $(selector).hide().html('').attr('class', '');
    };
    $.flashInfo = function (message, selector) {
        $.flashBase('info', message, selector);
    };
    $.flashWarning = function (message, selector) {
        $.flashBase('warning', message, selector);
    };
    $.flashError = function (message, selector) {
        $.flashBase('error', message, selector);
    };
    $.flashBase = function (cssClass, message, selector) {
        if (message) {
            if (!selector) {
                selector = '#flashMessage';
            }
            var jFlash = $(selector);
            if (message) {
                jFlash.html(message);
            }
            jFlash.attr('class', '');
            jFlash.addClass(cssClass);
            jFlash.slideDown('slow');
        }
    };

    $.hideBusy = function () {
        $('#busy').hide();
    };

    $.log = function (msg) {
        if (window.console && window.console.log) {
            window.console.log(msg);
        }
    };

    $(document).ready(function () {
        //hide flash message when clicked.
        $('#flashMessage').live(
            "click",
            function (event) {
                $(this).toggle('highlight');
            }
        );

//        //add busy indicator
//        $('<div id="busy" style="display:none;" />')
//            .ajaxStart(function () { $(this).show(); })
//            .ajaxStop(function () { $(this).hide(); })
//            .appendTo('body');

        //log errors.
        $('body').ajaxError(function handleError(event, xhr, options, err) {
            $.log(event);
            $.log(xhr);
            $.log(options);
            $.log(err);
            $.hideBusy();

            //turn off (make individual calls handle this)
            //$.flashError('Oops, there was an error while communicating with the server.');
        });

    });

})(jQuery);

