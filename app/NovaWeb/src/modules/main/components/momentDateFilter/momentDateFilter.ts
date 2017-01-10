import moment = require("moment");

export class MomentDateFilter {
    static $inject = [];

    static filter() {
        return (value, format) => {
            if (!format) {
                format = "MMMM DD, YYYY";
            }

            return moment(value).format(format);
        };
    }
}
