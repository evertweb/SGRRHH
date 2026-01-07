window.formHelpers = window.formHelpers || {};

window.formHelpers.focusElementById = function (id) {
    if (!id) return;
    var el = document.getElementById(id);
    if (el && typeof el.focus === 'function') {
        try { el.focus(); } catch (e) { }
        try { if (el.select) el.select(); } catch { }
    }
};

window.formHelpers.openCenteredWindow = function (url, windowName, width, height) {
    if (!url) return null;

    var w = Number(width) || 800;
    var h = Number(height) || 600;
    var name = windowName || 'Popup';

    // Multi-monitor aware centering
    var dualScreenLeft = (window.screenLeft !== undefined) ? window.screenLeft : window.screenX;
    var dualScreenTop = (window.screenTop !== undefined) ? window.screenTop : window.screenY;

    var currentWidth = window.innerWidth || document.documentElement.clientWidth || screen.width;
    var currentHeight = window.innerHeight || document.documentElement.clientHeight || screen.height;

    var left = Math.max(0, Math.floor((currentWidth - w) / 2 + dualScreenLeft));
    var top = Math.max(0, Math.floor((currentHeight - h) / 2 + dualScreenTop));

    // Note: do not set `noopener` here because we optionally refresh the opener on close.
    var features = [
        'popup=yes',
        'scrollbars=yes',
        'resizable=no',
        'width=' + w,
        'height=' + h,
        'left=' + left,
        'top=' + top
    ].join(',');

    var newWindow = window.open(url, name, features);
    if (newWindow && typeof newWindow.focus === 'function') {
        try { newWindow.focus(); } catch (e) { }
    }
    return newWindow;
};

window.formHelpers.closeCurrentWindow = function (refreshOpener) {
    try {
        if (refreshOpener && window.opener && !window.opener.closed) {
            window.opener.location.reload();
        }
    } catch (e) { }

    try { window.close(); } catch (e) { }
};