import {Button} from "../buttons/button";
import {DiagramElement} from "../shapes/diagram-element";
import {NodeFactorySettings} from "../shapes/node-factory-settings";

export class DeleteShapeButton extends Button {
    constructor(nodeId: string,
                width: number,
                height: number,
                tooltip: string = null,
                nodeFactorySettings: NodeFactorySettings = null,
                clickAction?: any) {

        super(`DS${nodeId}`, width, height, null);        


        this.setNeutralImage(this.getImageSource("delete-neutral.svg"));
        this.setActiveImage(this.getImageSource("delete-active.svg"));
        this.setHoverImage(this.getImageSource("delete-hover.svg"));
        this.setDisabledImage(this.getImageSource("delete-mute.svg"));

        if (nodeFactorySettings && nodeFactorySettings.isDeleteShapeEnabled) {
            this.isEnabled = true;
            if (clickAction) {
                this.setClickAction(clickAction);
            }            

        } else {
            this.disable();            
        }

        this.setTooltip(tooltip);

    }
}
