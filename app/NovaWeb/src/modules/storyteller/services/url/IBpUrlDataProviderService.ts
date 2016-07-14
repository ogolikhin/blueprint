module Storyteller {

    export class VersionInfo implements IVersionInfo {
        constructor(public artifactId: number = null,
            public utcLockedDateTime: Date = null,
            public lockOwnerLogin: string = null,
            public projectId: number = null,
            public versionId: number = null,
            public revisionId: number = null,
            public baselineId: number = null,
            public isVersionInformationProvided: boolean = false,
            public isHeadOrSavedDraftVersion: boolean = false) {
        }
    }

    export interface IBpUrlParsingService {
        getStateParams(): StorytellerStateParams;
    }

    export class StorytellerStateParams {   

        constructor(public id: string = null,
            public lastItemId: string = null,
            public versionId: number = null,
            public revisionId: number = null,
            public baselineId: number = null,
            public readOnly: boolean = null) {

        }
    }
}
