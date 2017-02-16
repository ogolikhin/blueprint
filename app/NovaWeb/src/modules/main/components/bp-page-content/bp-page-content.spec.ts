import "angular";
import "angular-mocks";
import "angular-sanitize";
import "rx";
import "./";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {NavigationServiceMock} from "../../../commonModule/navigation/navigation.service.mock";
import {Models} from "../../../main";
import {IStatefulArtifactFactory, StatefulArtifactFactory} from "../../../managers/artifact-manager";
import {ISelectionManager, SelectionManager} from "../../../managers/selection-manager/selection-manager";
import {ComponentTest} from "../../../util/component.test";
import {ItemTypePredefined} from "../../models/itemTypePredefined.enum";
import {PageContentCtrl} from "./bp-page-content";
import {IMainBreadcrumbService} from "./mainbreadcrumb.svc";
import {MainBreadcrumbServiceMock} from "./mainbreadcrumb.svc.mock";
import {SelectionManagerMock} from "../../../managers/selection-manager/selection-manager.mock";
import {StatefulArtifactFactoryMock} from "../../../managers/artifact-manager/artifact/artifact.factory.mock";
import {DialogServiceMock} from "../../../shared/widgets/bp-dialog/bp-dialog.mock";
import {ProjectExplorerServiceMock} from "../bp-explorer/project-explorer.service.mock";

describe("Component BPPageContent", () => {

    let vm: PageContentCtrl;

    beforeEach(angular.mock.module("ui.router"));
    beforeEach(angular.mock.module("bp.components.pagecontent"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("mainbreadcrumbService", MainBreadcrumbServiceMock);
        $provide.service("selectionManager", SelectionManagerMock);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("navigationService", NavigationServiceMock);
        $provide.service("dialogService", DialogServiceMock);
        $provide.service("projectExplorerService", ProjectExplorerServiceMock);
    }));

    let directiveTest: ComponentTest<PageContentCtrl>;
    let template = `<pagecontent></pagecontent>`;

    beforeEach(() => {
        directiveTest = new ComponentTest<PageContentCtrl>(template, "pagecontent");
        vm = directiveTest.createComponent({});
    });

    it("should be visible by default", () => {

        //Arrange
        directiveTest.createComponent({});

        //Assert
        expect(directiveTest.element.find("bp-breadcrumb").length).toBe(1);
    });

    it("should load breadcrumb for a selected artifact",
        inject(($rootScope: ng.IRootScopeService,
            selectionManager: ISelectionManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService) => {

            //Arrange
            mainbreadcrumbService.breadcrumbLinks = [];
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "My"});

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBeGreaterThan(0);
        }));

    it("should not load breadcrumb when selected artifact is null",
        inject(($rootScope: ng.IRootScopeService,
            selectionManager: ISelectionManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService) => {

            //Arrange
            mainbreadcrumbService.breadcrumbLinks = [];
            const spy = spyOn(mainbreadcrumbService, "reloadBreadcrumbs");

            //Act
            selectionManager.setArtifact(null);
            $rootScope.$digest();

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBe(0);
            expect(spy).not.toHaveBeenCalled();
        }));

    it("should not load breadcrumb when selected artifact is same as current artifact",
        inject(($rootScope: ng.IRootScopeService,
            selectionManager: ISelectionManager,
            statefulArtifactFactory: IStatefulArtifactFactory,
            mainbreadcrumbService: IMainBreadcrumbService) => {

            //Arrange
            const subArtifactModel = {
                id: 32,
                name: "SubArtifact",
                prefix: "SA",
                predefinedType: ItemTypePredefined.PROShape
            } as Models.ISubArtifact;
            mainbreadcrumbService.breadcrumbLinks = [];
            const artifact = statefulArtifactFactory.createStatefulArtifact({id: 22, name: "Artifact", prefix: "My"});
            const subArtifact = statefulArtifactFactory.createStatefulSubArtifact(artifact, subArtifactModel);

            //Act
            selectionManager.setArtifact(artifact);
            $rootScope.$digest();
            const spy = spyOn(mainbreadcrumbService, "reloadBreadcrumbs");
            selectionManager.setSubArtifact(subArtifact);

            // Assert
            const breadcrumbs = mainbreadcrumbService.breadcrumbLinks;
            expect(breadcrumbs).toBeDefined();
            expect(breadcrumbs.length).toBe(1);
            expect(spy).not.toHaveBeenCalled();
        }));
});
