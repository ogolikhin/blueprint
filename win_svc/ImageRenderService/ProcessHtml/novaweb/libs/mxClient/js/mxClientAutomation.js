var mxCellRendererInitializeShape = mxCellRenderer.prototype.initializeShape;
mxCellRenderer.prototype.initializeShape = function (state) {
    mxCellRendererInitializeShape.apply(this, arguments);

    if (state.shape.node != null && state.cell.id != null) {
        state.shape.node.setAttribute('id', 'shape-' + state.cell.id);
        state.shape.node.setAttribute('name', 'name-' + state.cell.nodeType);
        state.shape.node.setAttribute('edge', 'edge-' + state.cell.edge);
    }
};

var mxCellRendererInitializeLabel = mxCellRenderer.prototype.initializeLabel;
mxCellRenderer.prototype.initializeLabel = function (state) {
    mxCellRendererInitializeLabel.apply(this, arguments);

    if (state.text.node != null && state.cell.id != null) {
        state.text.node.setAttribute('id', 'label-' + state.cell.id);
    }
};

var mxCellRendererinitializeOverlay = mxCellRenderer.prototype.initializeOverlay;
mxCellRenderer.prototype.initializeOverlay = function (state, overlay) {
    mxCellRendererinitializeOverlay.apply(this, arguments);

    if (overlay.node != null && state.cell.id != null) {
        overlay.node.setAttribute('id', 'overlay-' + state.cell.id);
    }
};