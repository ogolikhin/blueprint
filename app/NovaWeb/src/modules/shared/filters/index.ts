import "angular";
import { BpFilesizeFilter } from "./bp-filesize/bp-filesize.filter";

angular.module("bp.filters", [])
    .filter("BpFilesize", BpFilesizeFilter.Factory());
    
export { BpFilesizeFilter } from "./bp-filesize/bp-filesize.filter";
