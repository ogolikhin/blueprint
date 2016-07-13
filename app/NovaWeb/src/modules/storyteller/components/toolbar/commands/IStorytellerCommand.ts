module Storyteller {
    export interface IStorytellerCommand {
        canExecute: (elements?: Array<IDiagramNode>) => boolean;
        execute: (elements?: Array<IDiagramNode>) => void;
    }
}