module Storyteller {

    export class StorytellerHeader implements IStorytellerHeader {
        public artifactPathLinks: IArtifactReference[];
        public baseItemTypePredefined: ItemTypePredefined;
        public typePrefix: string;
        public id: number;
        public name: string;
        public isChanged: boolean;
        public showLock: boolean;
        public showLockOpen: boolean;        
        public showDescription: boolean;
        public showSystemStateSwitch: boolean;
        public isUserToSystemProcess: boolean;
        public isReadonly: boolean;
        public artifactPrefix: string;

        private headerDescription: string;
        private descriptionContainsNewLine: boolean;

        public get description(): string {
            return this.headerDescription;
        }

        public set description(value: string) {
            this.headerDescription = value;
            this.descriptionContainsNewLine = this.doesTextContainsNewLine(this.headerDescription);
        }

        public init(typePrefix: string, id: number, name: string, isChanged: boolean, showLock: boolean,
            showLockOpen: boolean, description: string, showDescription: boolean, showSystemStateSwitch: boolean,
            isUserToSystemProcess: boolean, isReadonly: boolean): void {
            this.typePrefix = typePrefix;            
            this.id = id;
            this.name = name;
            this.isChanged = isChanged;
            this.showLock = showLock;
            this.showLockOpen = showLockOpen;
            this.description = description;
            this.showDescription = showDescription && description && description.length > 0;
            this.showSystemStateSwitch = showSystemStateSwitch;
            this.isUserToSystemProcess = isUserToSystemProcess;
            this.isReadonly = isReadonly;
            this.artifactPrefix = (this.typePrefix && this.typePrefix.length > 0) ? this.typePrefix + this.id + ":" : "";
        }

        public doesDescriptionContainNewLine(): boolean {
            return this.descriptionContainsNewLine;
        }

        private doesTextContainsNewLine(text: string): boolean {
            if (text) {
                //use regex to locate new lines in text
                return (text.search(/\r\n|\r|\n/g) >= 0)
            }
            return false;
        }

        public destroy() {
            this.typePrefix = "";
            this.id = 0;
            this.name = "";
            this.isChanged = false;
            this.showLock = false;
            this.showLockOpen = false;
            this.description = "";
            this.showDescription = false;
            this.showSystemStateSwitch = false;
            this.isUserToSystemProcess = false;
            this.isReadonly = false;
            this.artifactPrefix = "";
        }
    }
}