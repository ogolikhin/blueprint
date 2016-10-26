export class BpFormat {
    public static $inject = [];

    public static factory() {
        let filter = () => this.filter;
        filter.$inject = BpFormat.$inject;
        return filter;
    }

    /// Replaces placeholders "{0}", "{1}", etc. in input strings with corresponding arguments. For
    /// example, if Review.review.startDate = new Date(2015, 0, 1) then
    ///
    ///     {{'The review was created on {0}' | bpFormat:(Review.review.startDate | date:'medium')} }
    ///
    /// outputs: "The review was created on Jan 1, 2015 12:00:00 AM" in en-CA locale. If the
    /// corresponding argument is undefined, the placeholder is not replaced. There is no syntax
    /// for escaping braces; if you need to output a valid placeholder string literally omit the
    /// corresponding argument or pass undefined.
    public static filter = (input: string, ...args: any[]) => {
        if (typeof input === "string") {
            return input.replace(/{(\d+)}/g, (match, number) =>
                typeof args[number] === "undefined" ? match : args[number]);
        }
        return input;
    }
}
