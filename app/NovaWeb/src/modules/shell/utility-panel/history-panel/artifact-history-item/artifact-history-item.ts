export class ArtifactHistoryItem implements ng.IComponentOptions {
    public template: string = require("./artifact-history-item.html");
    public controller: Function = ArtifactHistoryItemController;
    public bindings: any = {
        artifactInfo: "="
    };
}

export class ArtifactHistoryItemController {
    public static $inject: [string] = ["$log"];
    
    constructor(private $log: ng.ILogService) {
    }

    // Potentially to be used in future US to display included user icon
    // public getAvatarUrl(): string {
    //     let userNames: string[] = this["artifactInfo"].displayName.split(" ");
    //     let user: string = "";

    //     userNames.map( (value: string) => {
    //         user += value[0]; 
    //     });
    //     let color: string = this.stringToHex(this["artifactInfo"].displayName);
    //     let url: string = `https://placeholdit.imgix.net/~text?txtsize=72&bg=${color}&txtclr=ffffff&txt=${user.toUpperCase()}&w=128&h=128&txttrack=3`;

    //     return url;
    // }

    public getAvatarBg(): string {
        return "#" + this.stringToHex(this["artifactInfo"].displayName);
    }
    
    public getAvatarInitials(): string {
        let userNames: string[] = this["artifactInfo"].displayName.trim().split(" ");
        let user: string = "";

        // only keep first and last words in the name, remove middle names
        if (userNames && userNames.length > 2) {
            userNames.splice(1, userNames.length - 2);
        }

        userNames.map( (value: string) => {
            user += value.charAt(0);
        });

        return user.toUpperCase();
    }

    public getAvatarInitialsColor(): string {
        return this.isDarkColor(this.getAvatarBg()) ? "#FFFFFF" : "#000000";
    }

    private stringToHex(str: string): string {
        /* tslint:disable */
        // str to hash
        let hash: number = 0,
            color: string = "";
        for (let i = 0; i < str.length; hash = str.charCodeAt(i++) + ((hash << 5) - hash));

        // int/hash to hex
        for (let i = 0; i < 3; color += ("00" + ((hash >> i++ * 8) & 0xFF).toString(16)).slice(-2));

        return color;
        /* tslint:enable */
    }

    private isDarkColor(color: string): boolean {
        /* tslint:disable */
        const c = color.substring(1);  // strip #
        const rgb = parseInt(c, 16);   // convert rrggbb to decimal
        const r = (rgb >> 16) & 0xff;  // extract red
        const g = (rgb >>  8) & 0xff;  // extract green
        const b = (rgb >>  0) & 0xff;  // extract blue

        const luma = 0.2126 * r + 0.7152 * g + 0.0722 * b; // per ITU-R BT.709

        return luma < 130;
        /* tslint:enable */
    }
}
