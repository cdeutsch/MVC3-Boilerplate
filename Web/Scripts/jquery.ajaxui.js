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

    $.flashClear = function () {
        $('#flash').hide().html('').attr('class', '');
    };
    $.flashInfo = function (message) {
        $.flashBase('info', message);
    };
    $.flashWarning = function (message) {
        $.flashBase('warning', message);
    };
    $.flashError = function (message) {
        $.flashBase('error', message);
    };
    $.flashBase = function (cssClass, message) {
        if (message) {
            var jFlash = $('#flash');
            jFlash.html(message);
            jFlash.attr('class', '');
            jFlash.addClass(cssClass);
            jFlash.slideDown('slow');
        }
    };

    $.hideBusy = function () {
        $('#busy').hide();
    };

    $(document).ready(function () {
        //hide flash message when clicked.
        $('#flash').live(
            "click",
            function (event) {
                $('#flash').toggle('highlight');
            }
        );

        //add busy indicator
        $('<div id="busy" style="display:none;" />')
            .ajaxStart(function () { $(this).show(); })
            .ajaxStop(function () { $(this).hide(); })
            .appendTo('body');

        //log errors.
        $('body').ajaxError(function handleError(event, xhr, options, err) {
            console.log(event);
            console.log(xhr);
            console.log(options);
            console.log(err);

            $.flashError('Oops, there was an error while communicating with the server.');
        });

    });

})(jQuery);

