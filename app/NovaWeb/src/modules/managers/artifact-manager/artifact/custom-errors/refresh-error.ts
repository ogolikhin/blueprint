import {ApplicationError} from "../../../../core/error/applicationError";

export class RefreshError extends ApplicationError {    
    constructor(error: ApplicationError | Error) {
        super(error);
        if (error instanceof ApplicationError) {
            this.handled = error.handled;
            this.statusCode = error.statusCode;
            this.errorCode = error.errorCode;
            this.errorContent = error.errorContent;
        }
        this.stack = error.stack;
        this.message = error.message;
    }
}