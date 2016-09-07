import { IArtifactAttachmentsService, IArtifactAttachmentsResultSet } from "./artifact-attachments.svc";

export class ArtifactAttachmentsMock implements IArtifactAttachmentsService {
    public static $inject = ["$q"];

    public artifactHistory;

    constructor(private $q: ng.IQService) { }

    public getArtifactAttachments(artifactId: number, subArtifactId?: number, addDrafts?: boolean): ng.IPromise<IArtifactAttachmentsResultSet> {
        const deferred = this.$q.defer<IArtifactAttachmentsResultSet>();
        const artifactAttachments: IArtifactAttachmentsResultSet = {
            artifactId: 306,
            subartifactId: null,
            attachments: [
                {
                userId: 1,
                userName: "admin",
                fileName: "test.png",
                attachmentId: 1093,
                uploadedDate: "2016-06-23T14:54:27.273Z"
                },
                {
                userId: 1,
                userName: "admin",
                fileName: "bug.wmv",
                attachmentId: 1097,
                uploadedDate: "2016-06-27T15:13:39.12Z"
                },
                {
                userId: 1,
                userName: "admin",
                fileName: "Allegro from Duet in C Major.mp3",
                attachmentId: 1098,
                uploadedDate: "2016-06-27T15:16:33.463Z"
                },
                {
                userId: 1,
                userName: "admin",
                fileName: "adventure_time.jpg",
                attachmentId: 1099,
                uploadedDate: "2016-06-27T15:17:27.433Z"
                },
                {
                userId: 1,
                userName: "admin",
                fileName: "open311ApiGetServiceRequestsReadme.doc",
                attachmentId: 1100,
                uploadedDate: "2016-06-27T15:17:27.433Z"
                },
                {
                userId: 1,
                userName: "admin",
                fileName: "demo.zip",
                attachmentId: 1101,
                uploadedDate: "2016-06-27T15:17:27.433Z"
                },
                {
                userId: 1,
                userName: "admin",
                fileName: "nova.tslint.json",
                attachmentId: 1103,
                uploadedDate: "2016-06-27T22:06:25.217Z"
                }
            ],
            documentReferences: [
                {
                artifactName: "acc-wizard.d.ts",
                artifactId: 258,
                userId: 1,
                userName: "admin",
                itemTypePrefix: "DOC",
                referencedDate: "2016-06-23T14:54:27.273Z"
                },
                {
                artifactName: "document with no attachment",
                artifactId: 357,
                userId: 1,
                userName: "admin",
                itemTypePrefix: "DOC",
                referencedDate: "2016-06-27T21:27:57.67Z"
                },
                {
                artifactName: "a very long document name without an attachment that will need to be displayed",
                artifactId: 358,
                userId: 1,
                userName: "admin",
                itemTypePrefix: "DOC",
                referencedDate: "2016-06-27T22:00:07.873Z"
                }
            ]
        };

        deferred.resolve(artifactAttachments);
        return deferred.promise;
    }
}
