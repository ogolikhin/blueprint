export class FiletypeParser {
    private static extensionMap = {
            // extensions
            xlsx: "ms-excel",
            xls: "ms-excel",

            // word document
            doc: "ms-word",
            xdoc: "ms-word",

            onenote: "ms-onenote",
            ppt: "ms-powerpoint",
            visio: "ms-visio",
            pdf: "pdf",
            
            // archive
            zip: "archive",
            rar: "archive",
            "7z": "archive",
            
            // generics
            // web: "web",
            // code: "code",
            // document: "document",

            // sound
            mp3: "sound",
            m4a: "sound",

            // videos
            wmv: "video",
            mp4: "video",
            
            // images
            jpg: "image",
            png: "image",
            gif: "image"
        };

    static getFiletypeClass(filename: string): string {
        if (!filename) {
            return "ext-document";
        }

        const fileExt: RegExpMatchArray = filename.match(/([^.]*)$/);

        if (fileExt.length && this.extensionMap[fileExt[0]]) {
            return "ext-" + this.extensionMap[fileExt[0]];
        } else {
            return "ext-document";
        }
    }
}
