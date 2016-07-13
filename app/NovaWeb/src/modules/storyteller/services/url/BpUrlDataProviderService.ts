module Storyteller {

    /**
     * Url Data Provider Service:
     * 
     * Helps in parsing url to obtain version and read-only information
     * 
     */

    export class BpUrlParsingService implements IBpUrlParsingService {

        public static $inject = ["$state"];

        constructor(private $state: ng.ui.IState) {
        }

        public getStateParams(): StorytellerStateParams {
            var storytellerStateParams = new StorytellerStateParams();

            const idParam = this.$state.params["id"];
            let lastId;

            if (typeof idParam === "number") {
                lastId = idParam;

            } else if (idParam) {
                const arrayOfIds = idParam.split("/").filter(id => id != "");
                lastId = arrayOfIds[arrayOfIds.length - 1];
            }

            const versionId = Number(this.$state.params["versionId"]);
            const revisionId = Number(this.$state.params["revisionId"]);
            const baselineId = Number(this.$state.params["baselineId"]);
            const readOnlyStateParam: string = this.$state.params["readOnly"];
            let readOnly: boolean = null;
            if (readOnlyStateParam && (readOnlyStateParam === "true" || readOnlyStateParam === "1")) {
                readOnly = true;
            }

            storytellerStateParams.id = idParam;
            storytellerStateParams.lastItemId = lastId ? lastId : null;
            storytellerStateParams.versionId = isNaN(versionId) ? null : versionId;
            storytellerStateParams.revisionId = isNaN(revisionId) ? null : revisionId;
            storytellerStateParams.baselineId = isNaN(baselineId) ? null : baselineId;
            storytellerStateParams.readOnly = readOnly;
            return storytellerStateParams;
            
        }
    }

    angular.module("Storyteller").service("bpUrlParsingService", BpUrlParsingService);
}
