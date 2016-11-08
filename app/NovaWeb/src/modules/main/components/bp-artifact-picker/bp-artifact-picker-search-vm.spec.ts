import "angular";
import "angular-mocks";
import {Models, SearchServiceModels} from "../../models";
import {SearchResultVM, SearchResultVMFactory} from "./bp-artifact-picker-search-vm";
import {IProjectService} from "../../../managers/project-manager/";

describe("SearchResultVMFactory", () => {
    let projectService: IProjectService;
    let onSelect: (vm: SearchResultVM<any>, value?: boolean) => boolean;
    let factory: SearchResultVMFactory;

    beforeEach(() => {
        projectService = jasmine.createSpyObj("projectService", ["getFolders", "getArtifacts", "getSubArtifactTree"]) as IProjectService;
        onSelect = jasmine.createSpy("onSelect");
        factory = new SearchResultVMFactory(projectService, onSelect);
    });

    describe("ProjectSearchResultVM", () => {
        it("select calls onSelect", () => {
            // Arrange
            const model = {} as SearchServiceModels.ISearchResult;
            const searchResultVM = factory.createProjectSearchResultVM(model);
            const value = true;

            // Act
            searchResultVM.selected(value);

            // Assert
            expect(onSelect).toHaveBeenCalledWith(searchResultVM, value);
        });

        it("id returns correct result", () => {
            // Arrange
            const model = {} as SearchServiceModels.ISearchResult;
            const searchResultVM = factory.createProjectSearchResultVM(model);

            // Act
            const result = searchResultVM.id;

            // Assert
            expect(result).toEqual("");
        });

        it("iconClass returns correct result", () => {
            // Arrange
            const model = {} as SearchServiceModels.ISearchResult;
            const searchResultVM = factory.createProjectSearchResultVM(model);

            // Act
            const result = searchResultVM.iconClass;

            // Assert
            expect(result).toEqual("icon-project");
        });
    });

    describe("ArtifactSearchResultVM", () => {
        it("select, when selectable, calls onSelect", () => {
            // Arrange
            factory.isItemSelectable = () => true;
            const model = {id: 123, itemId: 123, prefix: "AC", predefinedType: Models.ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
            const searchResultVM = factory.createArtifactSearchResultVM(model);
            const value = true;

            // Act
            searchResultVM.selected(value);

            // Assert
            expect(onSelect).toHaveBeenCalledWith(searchResultVM, value);
        });

        it("select, when not selectable, does not call onSelect", () => {
            // Arrange
            factory.isItemSelectable = () => false;
            const model = {id: 123, itemId: 123, prefix: "AC", predefinedType: Models.ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
            const searchResultVM = factory.createArtifactSearchResultVM(model);
            const value = true;

            // Act
            searchResultVM.selected(value);

            // Assert
            expect(onSelect).not.toHaveBeenCalled();
        });

        it("id returns correct result", () => {
            // Arrange
            const model = {id: 123, itemId: 123, prefix: "AC", predefinedType: Models.ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
            const searchResultVM = factory.createArtifactSearchResultVM(model);

            // Act
            const result = searchResultVM.id;

            // Assert
            expect(result).toEqual("AC123");
        });

        it("iconClass returns correct result", () => {
            // Arrange
            const model = {id: 123, itemId: 123, predefinedType: Models.ItemTypePredefined.Actor} as SearchServiceModels.IItemNameSearchResult;
            const searchResultVM = factory.createArtifactSearchResultVM(model);

            // Act
            const result = searchResultVM.iconClass;

            // Assert
            expect(result).toEqual("icon-actor");
        });
    });
});
