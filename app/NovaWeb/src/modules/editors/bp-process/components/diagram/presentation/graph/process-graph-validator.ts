import {IHashMap} from "../../../../../../main/models/models";
import {IProcessShape, IProcessLink} from "../../../../models/process-models";
import {ProcessShapeType} from "../../../../models/enums";
import {IProcessGraphModel} from "../../viewmodel/process-graph-model";

export class ProcessValidator {
    private addToValidationErrors(error: string, validationErrors: string[]): void {
        if (validationErrors == null) {
            return;
        }

        validationErrors.push(error);
    }

    public isValid(process: IProcessGraphModel, rootScope: any, validationErrors: string[] = null): boolean {
        let shapesMap: IHashMap<IProcessShape[]> = {};
        let sourceMap: IHashMap<number[]> = {};
        let destinationMap: IHashMap<number[]> = {};
        let locationMap: IHashMap<number[]> = {};
        let linkOrderIndexHashMap: IHashMap<IProcessLink> = {};

        if (process == null) {
            this.addToValidationErrors("Process is null", validationErrors);
            return false;
        }

        if (process.shapes == null) {
            this.addToValidationErrors("Process shapes is null", validationErrors);
            return false;
        }

        for (let shape of process.shapes) {
            if (shape == null) {
                this.addToValidationErrors("Process shapes contains a null shape", validationErrors);
                return false;
            }

            let shapeId = shape.id.toString();
            if (shapesMap[shapeId] == null) {
                shapesMap[shapeId] = [shape];
            } else {
                shapesMap[shapeId].push(shape);
            }

            if (shape.propertyValues == null) {
                this.addToValidationErrors(`Shape '${shape.id}' doesn't contain any property values`, validationErrors);
                return false;
            }

            if (shape.propertyValues["x"] == null) {
                this.addToValidationErrors(`Shape '${shape.id}' doesn't contain property value 'x'`, validationErrors);
                return false;
            }

            let x = shape.propertyValues["x"].value;

            if (shape.propertyValues["x"].value < 0) {
                this.addToValidationErrors(`Shape '${shape.id}' has invalid property value 'x': ${shape.propertyValues["x"].value}`, validationErrors);
                return false;
            }

            if (shape.propertyValues["y"] == null) {
                this.addToValidationErrors(`Shape '${shape.id}' doesn't contain property value 'y'`, validationErrors);
                return false;
            }

            let y = shape.propertyValues["y"].value;

            if (shape.propertyValues["y"].value < 0) {
                this.addToValidationErrors(`Shape '${shape.id}' has invalid property value 'y': ${shape.propertyValues["y"].value}`, validationErrors);
                return false;
            }

            let location = `(${x}, ${y})`;
            if (locationMap[location] == null) {
                locationMap[location] = [shape.id];
            } else {
                locationMap[location].push(shape.id);
            }
        }

        for (let key in locationMap) {
            if (locationMap[key].length > 1) {
                this.addToValidationErrors(`Shapes '${locationMap[key].join(", ")}' are overlapping at ${key}`, validationErrors);
                return false;
            }
        }

        if (process.links == null) {
            this.addToValidationErrors("Process links is null", validationErrors);
            return false;
        }

        for (let link of process.links) {
            let sourceId: string = link.sourceId.toString();
            let destinationId: string = link.destinationId.toString();

            if (sourceMap[sourceId] == null) {
                sourceMap[sourceId] = [link.destinationId];
            } else {
                sourceMap[sourceId].push(link.destinationId);
            }

            if (destinationMap[destinationId] == null) {
                destinationMap[destinationId] = [link.sourceId];
            } else {
                destinationMap[destinationId].push(link.sourceId);
            }
            let linkOrderIndexKey = link.sourceId.toString() + ";" + link.orderindex.toString();
            if (!linkOrderIndexHashMap[linkOrderIndexKey]) {
                linkOrderIndexHashMap[linkOrderIndexKey] = link;
            } else {
                let message = rootScope["config"].labels["ST_Duplicate_Link_OrderIndex"];
                message = message.replace("{0}", shapesMap[link.sourceId.toString()][0].name);
                message = message.replace("{1}", link.sourceId);
                message = message.replace("{2}", link.label);
                message = message.replace("{3}", linkOrderIndexHashMap[linkOrderIndexKey].label);
                this.addToValidationErrors(message, validationErrors);
                return false;
            }
        }

        for (let key in shapesMap) {
            if (shapesMap[key].length > 1) {
                this.addToValidationErrors(`Shape '${key}' has ${shapesMap[key].length - 1} duplicates`, validationErrors);
                return false;
            }

            if (!sourceMap.hasOwnProperty(key) && shapesMap[key][0].propertyValues["clientType"].value !== ProcessShapeType.End) {
                this.addToValidationErrors(`Shape '${key}' doesn't have outgoing links`, validationErrors);
                return false;
            }

            if (!destinationMap.hasOwnProperty(key) && shapesMap[key][0].propertyValues["clientType"].value !== ProcessShapeType.Start) {
                this.addToValidationErrors(`Shape '${key}' doesn't have incoming links`, validationErrors);
                return false;
            }
        }

        for (let key in sourceMap) {
            if (!shapesMap.hasOwnProperty(key)) {
                this.addToValidationErrors(`Source '${key}' of a process link doesn't exist`, validationErrors);
                return false;
            }
        }

        for (let key in destinationMap) {
            if (!shapesMap.hasOwnProperty(key)) {
                this.addToValidationErrors(`Destination '${key}' of a process link doesn't exist`, validationErrors);
                return false;
            }
        }

        return true;
    }
}