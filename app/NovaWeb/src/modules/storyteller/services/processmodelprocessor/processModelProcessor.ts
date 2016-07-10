module Storyteller {
    
   
    export interface IProcessModelProcessor {
        processModelBeforeSave(model: IProcess): IProcess;
    }

    export class ProcessModelProcessor implements IProcessModelProcessor {

        public processModelBeforeSave(model: IProcess): IProcess {
            const procModel: IProcess = new ProcessModel(
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
            var processLinks: IProcessLink[] = this.copyLinks(model);
            procModel.links.push.apply(procModel.links, processLinks);

            //copy shapes and remove mergeShapes
            var processShapes: IProcessShape[] = this.copyShapes(model);
            procModel.shapes.push.apply(procModel.shapes, processShapes);

            //copy property values
            var procModelPropValue: IHashMapOfPropertyValues = this.copyPropertyValues(model.propertyValues);
            procModel.propertyValues = procModelPropValue; 

            // copy decision branch destination links
            var processDestinationLinks: IProcessLink[] = this.copyDecisionBranchDestinationLinks(model.decisionBranchDestinationLinks);
            procModel.decisionBranchDestinationLinks.push.apply(procModel.decisionBranchDestinationLinks, processDestinationLinks);

            return procModel;
        }

        private copyLinks(model: IProcess): Array<IProcessLink> {
            var processLinks = new Array<IProcessLink>();
            if (model && model.links) {
                var linksLength = model.links.length;
                for (var linkCounter = 0; linkCounter < linksLength; linkCounter++) {

                    var value = <IProcessLinkModel>model.links[linkCounter];
                    if (!value) {
                        processLinks.push(angular.copy(model.links[linkCounter]));
                        continue;
                    }

                    var sourceNode = <IDiagramNode>value.sourceNode;
                    var destinationNode = <IDiagramNode>value.destinationNode;

                    if (sourceNode != null && sourceNode.model.baseItemTypePredefined == ItemTypePredefined.None) {
                        continue;
                    }
                    else if (destinationNode != null && destinationNode.model.baseItemTypePredefined == ItemTypePredefined.None) {
                        var nextNodes = destinationNode.getNextNodes();
                        if (nextNodes) {
                            value.destinationId = destinationNode.getNextNodes()[0].model.id;
                        }
                        var copyLink: IProcessLink = {
                            destinationId: value.destinationId,
                            label: value.label,
                            orderindex: value.orderindex,
                            sourceId: value.sourceId
                        };
                        processLinks.push(copyLink);
                    }
                    else {
                        var copyLink: IProcessLink = {
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

        private copyShapes(model: IProcess): Array<IProcessShape> {

            var shapes: IProcessShape[] = new Array<IProcessShape>();

            if (model && model.shapes) {
                for (var shapeCounter = 0; shapeCounter < model.shapes.length; shapeCounter++) {

                    var value: IProcessShape = model.shapes[shapeCounter];
                    if (value.baseItemTypePredefined === ItemTypePredefined.None) {
                        continue;
                    }

                    var newShape: IProcessShape = {
                        associatedArtifact: value.associatedArtifact,
                        baseItemTypePredefined: value.baseItemTypePredefined,
                        id: value.id,
                        name: value.name,
                        parentId: value.parentId,
                        projectId: value.projectId,
                        typePrefix: value.typePrefix,
                        propertyValues: null
                    };

                    var newPropertyValues: IHashMapOfPropertyValues = this.copyPropertyValues(value.propertyValues);
                    newShape.propertyValues = newPropertyValues;

                    shapes.push(newShape);
                }
            }

            return shapes;
        }

        private copyPropertyValues(propertyValues: IHashMapOfPropertyValues): IHashMapOfPropertyValues {
            var result: IHashMapOfPropertyValues = {};

            if (propertyValues) {
                for (var key in propertyValues) {
                    if (propertyValues.hasOwnProperty(key)) {
                        var keyName: string = key;
                        var actualKeyName: string = keyName[0].toUpperCase() + keyName.substring(1);
                        result[actualKeyName] = angular.copy(propertyValues[key]);
                    }
                }
            }

            return result;
        }

        private copyArtifactPathLinks(links: IArtifactReference[]): IArtifactReference[] {
            var result = new Array<IArtifactReference>();

            if (links) {
                for (var linkCounter = 0; linkCounter < links.length; linkCounter++) {
                    result.push(angular.copy(links[linkCounter]));
                }
            }

            return result;
        }

        private copyDecisionBranchDestinationLinks(links: IProcessLink[]): IProcessLink[] {
            var copies: IProcessLink[] = new Array<IProcessLink>();

            if (links) {
                for (var i in links) {
                    var original = links[i];
                    var copy = new ProcessLinkModel(null, original.sourceId, original.destinationId, original.orderindex, original.label);
                    copies.push(copy);
                }
            }

            return copies;
        }
    }

    var app = angular.module("Storyteller");
    app.service("processModelProcessor", ProcessModelProcessor);
}