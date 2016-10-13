export class SystemTaskShape extends mxCylinder {

    public redrawPath(path, x, y, w, h, isForeground) {
        //path.setFillColor("#DDDDDD");

        path.moveTo(0, h - 10);
        path.lineTo(0, 0);
        path.lineTo(w, 0);
        path.lineTo(w, h - 10);
        path.lineTo(w / 2 + 10, h - 10);
        path.curveTo(w / 2 + 10, h - 10, w / 2 + 1, h - 9, w / 2, h);
        path.curveTo(w / 2, h, w / 2 - 1, h - 9, w / 2 - 10, h - 10);
        path.lineTo(0, h - 10);
    }
}

export class NodeShapes {
    public static register(graph: MxGraph) {
        mxCellRenderer.registerShape("systemTask", SystemTaskShape);
    }
}
