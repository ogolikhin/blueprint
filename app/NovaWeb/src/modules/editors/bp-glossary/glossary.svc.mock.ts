import { IGlossaryService, IGlossaryDetails } from "./glossary.svc";

export class GlossaryServiceMock implements IGlossaryService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) { }

    public getGlossary(id: number): ng.IPromise<IGlossaryDetails> {
        const defer = this.$q.defer<IGlossaryDetails>();

        if (id > 0) {
            defer.resolve(GlossaryServiceMock.createGlossary(id));
        } else {
            defer.reject("Error");
        }

        return defer.promise;
    }

    public static createGlossary(id: number): IGlossaryDetails {
        /* tslint:disable:max-line-length */
        const glossary: IGlossaryDetails = {
            id: id,
            terms: [
                {
                    id: 264,
                    name: "fleek",
                    definition: "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">on point</p></div></body></html>",
                    typePrefix: "TR",
                    predefined: 8217
                },
                {
                    id: 386,
                    name: "google",
                    definition: "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">&#x200b;<a href=\"http://www.google.com/\" style=\"color: Blue; text-decoration: underline\"><span style=\"font-family: 'Portable User Interface'; font-size: 11px\">google.com</span></a><span style=\"-c1-editable: true; font-family: 'Portable User Interface'; font-size: 11px; font-style: normal; font-weight: normal; color: Black\">&#x200b;</span></p></div></body></html>",
                    typePrefix: "TR",
                    predefined: 8217
                },
                {
                    id: 382,
                    name: "pokemon",
                    definition: "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">cat thing</p></div></body></html>",
                    typePrefix: "TR",
                    predefined: 8217
                },
                {
                    id: 385,
                    name: "snorlax",
                    definition: "<html><head></head><body style=\"padding: 1px 0px 0px\"><div style=\"padding: 0px\"><p style=\"margin: 0px\">a kind of&nbsp;<span style=\"font-weight: bold\">pokemon</span></p></div></body></html>",
                    typePrefix: "TR",
                    predefined: 8217
                }
            ]
        };

        return glossary;
        /* tslint:enable:max-line-length */
    }
}
