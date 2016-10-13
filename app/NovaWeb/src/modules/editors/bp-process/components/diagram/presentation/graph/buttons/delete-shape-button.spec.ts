import * as angular from "angular";
import {Button} from "../buttons/button";
import {DeleteShapeButton} from "../buttons/delete-shape-button";
import {DiagramElement} from "../shapes/diagram-element";
import {NodeFactorySettings} from "../shapes/node-factory-settings";

describe("DeleteShapeButton", () => {

    describe("When button is constructed ", () => {

        it("Id should be DS+nodeId", () => {

            //Arrange
            const id = "test";
            const deleteShapeButton: DeleteShapeButton = new DeleteShapeButton(id, 10, 10, null, null, null);

            //Assert
            expect(deleteShapeButton.id).toEqual(`DS${id}`);
        })
    }),

    describe("When NodeFactorySettings are supplied and delete action is enabled ", () => {

        it("DeleteShapeButton is enabled ", () => {

            //Arrange
            const id = "test";
            const clickAction = () => {
                console.log("I clicked")                
            };
            
            const nodeFactorySettings: NodeFactorySettings = new NodeFactorySettings();
            nodeFactorySettings.isDeleteShapeEnabled = true;
            const deleteShapeButton: DeleteShapeButton = new DeleteShapeButton(id, 10, 10, null, nodeFactorySettings, clickAction);

            //Assert
            expect(deleteShapeButton.isEnabled).toEqual(true);
        })
    })

    describe("When NodeFactorySettings are supplied and delete action is false ", () => {

        it("cDeleteShapeButton is disabled ", () => {

            //Arrange
            const id = "test";
            const clickAction = () => {
                console.log("I clicked")                
            };
            const nodeFactorySettings: NodeFactorySettings = new NodeFactorySettings();
            nodeFactorySettings.isDeleteShapeEnabled = false;
            const deleteShapeButton: DeleteShapeButton = new DeleteShapeButton(id, 10, 10, null, nodeFactorySettings, clickAction);

            //Assert
            expect(deleteShapeButton.isEnabled).toEqual(false);
        })
    })

});