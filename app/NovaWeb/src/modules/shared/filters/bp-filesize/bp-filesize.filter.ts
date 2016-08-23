import { ILocalizationService } from  "../../../core/";

export class BpFilesizeFilter {

    public static $inject = [
        "localization"
    ];

    public static Factory() {
        let filter = (localizationService: ILocalizationService) => {
            return (size) => {
                if (isNaN(size)) {
                    size = 0;
                }

                if (size < 1024) {
                    return size + " " + localizationService.get("Filesize_Bytes", "Bytes");
                }

                size /= 1024;

                if (size < 1024) {
                    return size.toFixed(2) + " " + localizationService.get("Filesize_KB", "KB");
                }

                size /= 1024;

                if (size < 1024) {
                    return size.toFixed(2) + " " + localizationService.get("Filesize_MB", "MB");
                }

                size /= 1024;

                if (size < 1024) {
                    return size.toFixed(2) + " " + localizationService.get("Filesize_GB", "GB");
                }

                size /= 1024;

                return size.toFixed(2) + " " + localizationService.get("Filesize_TB", "TB");
            };
        };

        filter.$inject = ["localization"];

        return filter;
    }
}
