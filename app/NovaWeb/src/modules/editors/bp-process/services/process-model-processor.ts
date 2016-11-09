import * as ProcessModels from "../models/process-models";
import { IDiagramNode } from "../components/diagram/presentation/graph/models/process-graph-interfaces";

export interface IProcessModelProcessor {
    processModelBeforeSave(model: ProcessModels.IProcess): ProcessModels.IProcess;
}

export class ProcessModelProcessor implements IProcessModelProcessor {

    public processModelBeforeSave(model: ProcessModels.IProcess): ProcessModels.IProcess {
        const procModel: ProcessModels.IProcess = new ProcessModels.ProcessModel(
            model.id,
            model.name,
            model.typePrefix,
            model.projectId,
            model.baseItemTypePredefined,
            [],
            [],
            null,
            []
        );

        //Copy links
        const processLinks: ProcessModels.IProcessLink[] = this.copyLinks(model);
        procModel.links.push.apply(procModel.links, processLinks);

        //copy shapes and remove mergeShapes
        const processShapes: ProcessModels.IProcessShape[] = this.copyShapes(model);
        procModel.shapes.push.apply(procModel.shapes, processShapes);

        //copy property values
        let procModelPropValue: ProcessModels.IHashMapOfPropertyValues = this.copyPropertyValues(model.propertyValues);
        procModel.propertyValues = procModelPropValue;

        // copy decision branch destination links
        const processDestinationLinks: ProcessModels.IProcessLink[] = this.copyDecisionBranchDestinationLinks(model.decisionBranchDestinationLinks);
        procModel.decisionBranchDestinationLinks.push.apply(procModel.decisionBranchDestinationLinks, processDestinationLinks);

        return procModel;
    }

    private copyLinks(model: ProcessModels.IProcess): Array<ProcessModels.IProcessLink> {
        const processLinks = new Array<ProcessModels.IProcessLink>();
        if (model && model.links) {
            let linksLength = model.links.length;
            for (let linkCounter = 0; linkCounter < linksLength; linkCounter++) {

                let value = <ProcessModels.IProcessLinkModel>model.links[linkCounter];
                if (!value) {
                    processLinks.push(angular.copy(model.links[linkCounter]));
                    continue;
                }

                let sourceNode = <IDiagramNode>value.sourceNode;
                let destinationNode = <IDiagramNode>value.destinationNode;

                if (sourceNode != null && sourceNode.model.baseItemTypePredefined === ProcessModels.ItemTypePredefined.None) {
                    continue;
                }
                else if (destinationNode != null && destinationNode.model.baseItemTypePredefined === ProcessModels.ItemTypePredefined.None) {
                    let nextNodes = destinationNode.getNextNodes();
                    if (nextNodes) {
                        value.destinationId = destinationNode.getNextNodes()[0].model.id;
                    }
                    let copyLink: ProcessModels.IProcessLink = {
                        destinationId: value.destinationId,
                        label: value.label,
                        orderindex: value.orderindex,
                        sourceId: value.sourceId
                    };
                    processLinks.push(copyLink);
                }
                else {
                    let copyLink: ProcessModels.IProcessLink = {
                        destinationId: value.destinationId,
                        label: value.label,
                        orderindex: value.orderindex,
                        sourceId: value.sourceId
                    };
                    processLinks.push(copyLink);
                }
            }
        }
        return processLinks;
    }

    private copyShapes(model: ProcessModels.IProcess): Array<ProcessModels.IProcessShape> {

        const shapes: ProcessModels.IProcessShape[] = new Array<ProcessModels.IProcessShape>();

        if (model && model.shapes) {
            for (let shapeCounter = 0; shapeCounter < model.shapes.length; shapeCounter++) {

                let value: ProcessModels.IProcessShape = model.shapes[shapeCounter];
                if (value.baseItemTypePredefined === ProcessModels.ItemTypePredefined.None) {
                    continue;
                }

                let newShape: ProcessModels.IProcessShape = {
                    associatedArtifact: value.associatedArtifact,
                    personaReference: value.personaReference,
                    baseItemTypePredefined: value.baseItemTypePredefined,
                    id: value.id,
                    name: value.name,
                    parentId: value.parentId,
                    projectId: value.projectId,
                    typePrefix: value.typePrefix,
                    propertyValues: null
                };

                let newPropertyValues: ProcessModels.IHashMapOfPropertyValues = this.copyPropertyValues(value.propertyValues);
                newShape.propertyValues = newPropertyValues;

                shapes.push(newShape);
            }
        }

        return shapes;
    }

    private copyPropertyValues(propertyValues: ProcessModels.IHashMapOfPropertyValues): ProcessModels.IHashMapOfPropertyValues {
        let result: ProcessModels.IHashMapOfPropertyValues = {};

        if (propertyValues) {
            for (let key in propertyValues) {
                if (propertyValues.hasOwnProperty(key)) {
                    let keyName: string = key;
                    let actualKeyName: string = keyName[0].toUpperCase() + keyName.substring(1);
                    result[actualKeyName] = angular.copy(propertyValues[key]);
                }
            }
        }

        return result;
    }

    private copyArtifactPathLinks(links: ProcessModels.IArtifactReference[]): ProcessModels.IArtifactReference[] {
        let result = new Array<ProcessModels.IArtifactReference>();

        if (links) {
            for (let linkCounter = 0; linkCounter < links.length; linkCounter++) {
                result.push(angular.copy(links[linkCounter]));
            }
        }

        return result;
    }

    private copyDecisionBranchDestinationLinks(links: ProcessModels.IProcessLink[]): ProcessModels.IProcessLink[] {
        let copies: ProcessModels.IProcessLink[] = new Array<ProcessModels.IProcessLink>();

        if (links) {
            for (let i in links) {
                let original = links[i];
                let copy = new ProcessModels.ProcessLinkModel(null, original.sourceId, original.destinationId, original.orderindex, original.label);
                copies.push(copy);
            }
        }

        return copies;
    }
}