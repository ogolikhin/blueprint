import moment = require("moment");

export class MomentDateTimeFilter {
    static $inject = [];

    static filter() {
        return (value, format) => {
            if (value === null) {
                return "";
            }

            if (!format) {
                format = "MMMM DD, YYYY h:mm:ss a";
            }

            return moment(value).format(format);
        };
    }
}
