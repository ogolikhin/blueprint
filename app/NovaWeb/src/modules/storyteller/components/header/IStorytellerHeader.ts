module Storyteller {
    export interface IStorytellerHeader {
        artifactPathLinks: IArtifactReference[];
        baseItemTypePredefined: ItemTypePredefined;
        typePrefix: string;
        id: number;
        name: string;
        isChanged: boolean;
        showLock: boolean;
        showLockOpen: boolean;
        description: string;
        showDescription: boolean;
        showSystemStateSwitch: boolean;
        isUserToSystemProcess: boolean;
        isReadonly: boolean;
        artifactPrefix: string;

        init(typePrefix: string, id: number, name: string, isChanged: boolean, showLock: boolean,
            showLockOpen: boolean, description: string, showDescription: boolean, showSystemStateSwitch: boolean,
            isUserToSystemProcess: boolean, isReadonly: boolean): void;

        doesDescriptionContainNewLine(): boolean;

        destroy():void;
    }
}