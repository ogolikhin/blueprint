module nova {
    export class Reverse {
        constructor(input: string, uppercase: boolean) {
            input = input || "";

            let out = "";
            for (var i = 0; i < input.length; i++) {
                out = input.charAt(i) + out;
            }
            // conditional based on optional argument
            if (uppercase) {
                out = out.toUpperCase();
            }
            return out;
        }
    }
}
