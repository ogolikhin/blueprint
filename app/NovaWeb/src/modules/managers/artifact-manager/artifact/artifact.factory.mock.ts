import {IProcess, IProcessShape} from "../../../editorsModule/bp-process/models/process-models";
import {StatefulProcessArtifact} from "../../../editorsModule/bp-process/process-artifact";
import {Models} from "../../../main/models";
import {ItemTypePredefined} from "../../../main/models/item-type-predefined";
import {IStatefulArtifact, StatefulArtifact, StatefulProcessSubArtifact} from "../artifact";
import {IStatefulSubArtifact, StatefulSubArtifact} from "../sub-artifact";
import {IStatefulArtifactFactory} from "./";

export interface IStatefulArtifactFactoryMock extends IStatefulArtifactFactory {
    populateStatefulProcessWithProcessModel(statefulArtifact: StatefulProcessArtifact, process: IProcess);
}

export class StatefulArtifactFactoryMock implements IStatefulArtifactFactoryMock {
    public createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        if (artifact.predefinedType === ItemTypePredefined.Process) {
            return new StatefulProcessArtifact(artifact, null);
        }
        return new StatefulArtifact(artifact, null);
    }

    public createStatefulArtifactFromId(artifactId: number): ng.IPromise<IStatefulArtifact> {
        return null;
    }

    public createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact {
        return new StatefulSubArtifact(artifact, subArtifact, null);
    }

    public createStatefulProcessSubArtifact(artifact: IStatefulArtifact, subArtifact: IProcessShape): StatefulProcessSubArtifact {
        return new StatefulProcessSubArtifact(artifact, subArtifact, null);
    }

    public populateStatefulProcessWithProcessModel(statefulArtifact: StatefulProcessArtifact, process: IProcess) {
        statefulArtifact.links = process.links;
        statefulArtifact.decisionBranchDestinationLinks = process.decisionBranchDestinationLinks;
        statefulArtifact.propertyValues = process.propertyValues;

        const statefulSubArtifacts = [];
        statefulArtifact.shapes = [];

        for (const shape of process.shapes) {
            const statefulShape = new StatefulProcessSubArtifact(statefulArtifact, shape, null);
            const specialProperties = [];
            for (const propertyValue in shape.propertyValues) {
                const property = shape.propertyValues[propertyValue];
                if (property.typePredefined === Models.PropertyTypePredefined.Label ||
                    property.typePredefined === Models.PropertyTypePredefined.AssociatedArtifact ||
                    property.typePredefined === Models.PropertyTypePredefined.ImageId) {
                    const newProperty = {
                        propertyTypeId: property.typeId,
                        propertyTypeVersionId: -1,
                        propertyTypePredefined: property.typePredefined,
                        isReuseReadOnly: false,
                        value: property.value
                    };
                    specialProperties.push(newProperty);
                }
            }
            const associatedArtifactProperty = {
                propertyTypeId: Models.PropertyTypePredefined.AssociatedArtifact,
                propertyTypeVersionId: -1,
                propertyTypePredefined: Models.PropertyTypePredefined.AssociatedArtifact,
                isReuseReadOnly: false,
                value: shape.associatedArtifact ? shape.associatedArtifact.id : null
            };

            const personaReferenceProperty = {
                propertyTypeId: Models.PropertyTypePredefined.PersonaReference,
                propertyTypeVersionId: -1,
                propertyTypePredefined: Models.PropertyTypePredefined.PersonaReference,
                isReuseReadOnly: false,
                value: shape.personaReference ? shape.personaReference.id : null
            };
            specialProperties.push(associatedArtifactProperty);
            specialProperties.push(personaReferenceProperty);
            statefulShape.specialProperties.initialize(specialProperties);
            statefulArtifact.shapes.push(statefulShape);
            statefulSubArtifacts.push(statefulShape);
        }

        statefulArtifact.subArtifactCollection.initialise(statefulSubArtifacts);
    }
}
