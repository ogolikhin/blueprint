import moment = require("moment");
export class MomentDateFilter {
    static $inject = [];
    static filter() {
        return (value, format) => {
            return moment(value).format(format).toString();
        };
    }
}
