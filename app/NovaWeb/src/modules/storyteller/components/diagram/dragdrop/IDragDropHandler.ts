module Storyteller {
    export interface IDragDropHandler {
        moveCell: MxCell;
        createDragPreview();
        reset();
        isValidDropSource(dropSource: MxCell);
        highlightDropTarget(me);
        dispose();
    };

}