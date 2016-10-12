export class BPAvatar implements ng.IComponentOptions {
    public template: string = require("./bp-avatar.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPAvatarController;
    public bindings: any = {
        icon: "@?",
        name: "@",
        // use this string to generate bg-color
        colorBase: "@?"
    };
}

export class BPAvatarController {
    public static $inject: [string] = ["$log"];

    // bindings vars
    public icon: string;
    public name: string;
    public colorBase: string;

    public background: string;
    public initials: string;
    public initialsColor: string;

    constructor(private $log: ng.ILogService) {
        this.background = this.getAvatarBg(this.colorBase || this.name);
        this.initials = this.getAvatarInitials(this.name);
        this.initialsColor = this.getAvatarInitialsColor(this.colorBase || this.name);
    }

    public getAvatarBg(name: string): string {
        return "#" + this.stringToHex(name);
    }

    public getAvatarInitials(name: string): string {
        let userNames: string[] = name.trim().split(" ");
        let user: string = "";

        // only keep first and last words in the name, remove middle names
        if (userNames && userNames.length > 2) {
            userNames.splice(1, userNames.length - 2);
        }

        userNames.map((value: string) => {
            user += value.charAt(0);
        });

        return user.toUpperCase();
    }

    public getAvatarInitialsColor(name: string): string {
        return this.isDarkColor(this.getAvatarBg(name)) ? "#FFFFFF" : "#000000";
    }

    private stringToHex(str: string): string {
        let hash: number = 0,
            color: string = "";

        // str to hash
        for (let i = 0; i < str.length; hash = str.charCodeAt(i++) + ((hash << 5) - hash)) {
            //fixme: why is this empty
        }

        // int/hash to hex
        for (let i = 0; i < 3; color += ("00" + ((hash >> i++ * 8) & 0xFF).toString(16)).slice(-2)) {
            //fixme: why is this empty

        }


        return color;
    }

    private isDarkColor(color: string): boolean {
        const c = color.substring(1);  // strip #
        const rgb = parseInt(c, 16);   // convert rrggbb to decimal
        const r = (rgb >> 16) & 0xff;  // extract red
        const g = (rgb >> 8) & 0xff;  // extract green
        const b = (rgb >> 0) & 0xff;  // extract blue

        const luma = 0.2126 * r + 0.7152 * g + 0.0722 * b; // per ITU-R BT.709

        return luma < 130;
    }
}
