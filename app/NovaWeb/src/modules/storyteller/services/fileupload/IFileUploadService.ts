module Storyteller {
    export interface IFileUploadService {
        uploadToFileStore(file: any, expirationDate: Date): ng.IPromise<IFileResult>;
    }
}
