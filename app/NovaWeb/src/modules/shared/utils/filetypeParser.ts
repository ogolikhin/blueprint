export class FiletypeParser {
    private static extensionMap = {
            xlsx: "ms-excel",
            xls: "ms-excel",

            doc: "ms-word",
            docx: "ms-word",

            one: "ms-onenote",
            onepkg: "ms-onenote",
            
            ppt: "ms-powerpoint",
            pptx: "ms-powerpoint",
            
            vsd: "ms-visio",
            vsdx: "ms-visio",
            
            pdf: "pdf",
            
            // archive
            zip: "archive",
            rar: "archive",
            "7z": "archive",
            tar: "archive",
            
            // web
            html: "web",
            htm: "web",

            // code
            js: "code",

            // audio
            mp3: "sound",
            m4a: "sound",
            wav: "sound",
            wma: "sound",

            // video
            wmv: "video",
            mp4: "video",
            
            // images
            jpg: "image",
            jpeg: "image",
            png: "image",
            gif: "image",
            svg: "image",
            bmp: "image",
            tif: "image",
            tiff: "image"
        };

    static getFiletypeClass(filename: string): string {
        if (!filename) {
            return "ext-document";
        }

        filename = filename.toLowerCase();
        const fileExt: RegExpMatchArray = filename.match(/([^.]*)$/);

        if (fileExt.length && this.extensionMap[fileExt[0]]) {
            return "ext-" + this.extensionMap[fileExt[0]];
        } else {
            return "ext-document";
        }
    }

    static getFileExtension(filename: string): string {
        if (!filename) {
            return "";
        }

        filename = filename.toLowerCase();
        const fileExt: RegExpMatchArray = filename.match(/([^.]*)$/);

        if (fileExt[0]) {
            return fileExt[0];
        } else {
            return "";
        }
    }
}
