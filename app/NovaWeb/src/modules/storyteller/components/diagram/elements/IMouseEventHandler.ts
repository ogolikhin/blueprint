module Storyteller {

    export interface IMouseEventHandler {

        onMouseEnter(sender, evt);

        onMouseLeave(sender, evt);

        onMouseDown(sender, evt);

        onMouseUp(sender, evt);
    }
}
