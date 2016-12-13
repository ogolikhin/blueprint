// import "angular";
// import "angular-mocks";
// import "rx/dist/rx.lite";
// import {LocalizationServiceMock} from "../../../core/localization/localization.mock";
// import {IProjectMeta} from "./../../../main/models/models";
// import {MetaDataService, IMetaDataService, ProjectMetaData} from "./metadata.svc";
// import {MetaDataServiceMock} from "./metadata.svc.mock";
// import {StatefulArtifactFactoryMock} from "../../artifact-manager/artifact/artifact.factory.mock";

// import {HttpStatusCode} from "../../../core/http/http-status-code";
// import {Enums} from "../../../main/models";


// describe("Metadata -> ", () => {
//     let _$q: ng.IQService;
//     let _$rootScope: ng.IRootScopeService;
//     let factory: StatefulArtifactFactoryMock;
   
//     const mockData = JSON.parse(require("./metadata.mock.json"));

//     beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//         $provide.service("localization", LocalizationServiceMock);
//         $provide.service("metadataService", MetaDataServiceMock);
//     }));
//     beforeEach(inject(($q: ng.IQService, $rootScope: ng.IRootScopeService) => {
//         _$q = $q;
//         _$rootScope = $rootScope;
        
//     }));
    
//     xdescribe("Get Item Type -> ", inject((metaDataService: IMetaDataService) => {
//         beforeEach(inject(( metaDataService: IMetaDataService) => {
//             factory = new StatefulArtifactFactoryMock({
//                 metaDataService: metaDataService
//             });
//         }));
//         it("successful", () => {
//             // Arrange
//             const spyItemType = spyOn(metaDataService, "getArtifactItemType").and
//                 .callFake(() => { return _$q.resolve(); })
//                 ;
//             const artifact = factory.createStatefulArtifact({id: 1});   

//             artifact.metadata.getItemType();

//             //Asserts
//             expect(spyItemType).toHaveBeenCalled();
            

//         });
        
//     }));
   
// });