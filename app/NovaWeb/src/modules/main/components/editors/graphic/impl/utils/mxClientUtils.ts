interface MxCell {
    getLabel();
}

mxSvgCanvas2D.prototype.getBaseUrl = () => {
    var href = window.location.href;
    var hash = href.indexOf("#");

    if (hash > 0) {
        href = href.substring(0, hash);
    }

    return href;
};