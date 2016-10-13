export class SafaryGestureHelper {
    public disableGestureSupport($element: ng.IAugmentedJQuery) {
        $element.bind("gesturechange", this.iosPinchZoomHandler);
        $element.bind("gesturestart", this.iosPinchZoomHandler);
        $element.bind("gestureend", this.iosPinchZoomHandler);
    }

    private iosPinchZoomHandler(e: JQueryEventObject) {
        if (e.originalEvent["scale"] !== 1.0) {
            e.stopImmediatePropagation();
        }
    }
}
