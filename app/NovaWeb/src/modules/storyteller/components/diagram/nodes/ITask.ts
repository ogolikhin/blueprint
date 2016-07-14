module Storyteller {
    export interface ITask extends IDiagramNode {
        persona: string;
        description: string;
        associatedArtifact: any;
        activateButton(itemFlag: ItemIndicatorFlags): void;
    }
}
