module Shell {
    /**
     * Draws indicator for mxCell
     */
    export class IconRack {

        private image: mxImageShape;

        private imageSrc: string;

        private clickHandler;

        constructor(imageSrc: string, clickHandler?: any) {
            this.imageSrc = imageSrc;
            this.clickHandler = clickHandler;
        }

        public draw(state: MxCellState) {
            if (state == null) {
                throw new Error("Argument 'state' can not be null");
            }
            if (this.image == null) {
                this.image = this.createImageShape(state.view.getOverlayPane());
                this.attachClickHandler();
            }
            
            this.image.bounds = this.getInfoIconRackBounds(state);
            this.image.scale = state.view.getScale();
            this.image.redraw();
            this.getNode().style.cursor = "pointer";
        }

        private createImageShape(container: HTMLElement) {
            var bounds = Review.MxFactory.rectangle(0, 0, 16, 16);
            var image = new mxImageShape(bounds, this.imageSrc);
            image.dialect = mxConstants.DIALECT_SVG;
            image.preserveImageAspect = false;
            image.init(container);
            return image;
        }

        private getInfoIconRackBounds(state: MxCellState) {
            var oldScale = this.image.scale;
            var w = this.image.bounds.width / oldScale;
            var h = this.image.bounds.height / oldScale;
            var s = state.view.getScale();

            return Review.MxFactory.rectangle(state.x + state.width - w * s, state.y - h * s - 5, w * s, h * s);
        }

        private attachClickHandler() {
            this.detachClickHandler();
            mxEvent.addListener(this.getNode(), "click", this.clickHandler);
        }

        private detachClickHandler() {
            if (this.clickHandler != null) {
                if (this.image != null) {
                    mxEvent.removeListener(this.getNode(), "click", this.clickHandler);
                }
            }
        }

        private getNode() {
            return (<any>this.image).innerNode || this.image.node;
        }

        public destroy() {
            this.detachClickHandler();
            if (this.image != null) {
                this.image.destroy();
                this.image = null;
            }
            this.clickHandler = null;
        }
    }
} 