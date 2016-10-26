import * as _ from "lodash";

export interface IAppicationError {
    handled?: boolean;
    statusCode?: number;
    errorCode?: number;
    message?: string;
}

export class AppicationError extends Error implements IAppicationError {
    public handled: boolean;
    public statusCode: number;
    public errorCode: number;

    constructor(message?: string | IAppicationError) {
        super();
        if (message instanceof String) {
            this.message = message;
        } else {
            _.assign(this, message);
        }
    }
}