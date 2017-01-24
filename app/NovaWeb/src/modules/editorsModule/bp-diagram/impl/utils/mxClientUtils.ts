// tslint:disable-next-line: interface-name
interface MxCell {
    getLabel();
}

mxSvgCanvas2D.prototype.getBaseUrl = () => {
    let href = window.location.href;
    const hash = href.indexOf("#");

    if (hash > 0) {
        href = href.substring(0, hash);
    }

    return href;
};