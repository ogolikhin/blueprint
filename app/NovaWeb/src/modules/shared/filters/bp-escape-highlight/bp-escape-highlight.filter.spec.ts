import * as angular from "angular";
import "angular-mocks";
import "../";

describe("bp-escape-highlight filter", () => {
    let $filter;

    beforeEach(angular.mock.module("bp.filters"));

    beforeEach(inject((_$filter_: ng.IFilterService) => {
        $filter = _$filter_;
    }));

    it("should filter plain text", () => {
        // Arrange
        const toFilter = "Lorem ipsum dolor sit tamet";
        const toHighlight = "dolor";

        // Act
        const result = $filter("bpEscapeAndHighlight")(toFilter, toHighlight);

        // Assert
        expect(result).toEqual(`Lorem ipsum <span class="bp-escape-highlight">dolor</span> sit tamet`);
    });

    it("should filter HTML content (filtered token not part of the tag)", () => {
        // Arrange
        const toFilter = "Lorem <strong>ipsum</strong> dolor sit tamet";
        const toHighlight = "dolor";

        // Act
        const result = $filter("bpEscapeAndHighlight")(toFilter, toHighlight);

        // Assert
        expect(result).toEqual(`Lorem &lt;strong&gt;ipsum&lt;&#x2F;strong&gt; <span class="bp-escape-highlight">dolor</span> sit tamet`);
    });

    it("should filter HTML content (filtered token is part of the tag)", () => {
        // Arrange
        const toFilter = "Lorem <strong>ipsum dolor</strong> sit tamet";
        const toHighlight = "dolor";

        // Act
        const result = $filter("bpEscapeAndHighlight")(toFilter, toHighlight);

        // Assert
        expect(result).toEqual(`Lorem &lt;strong&gt;ipsum <span class="bp-escape-highlight">dolor</span>&lt;&#x2F;strong&gt; sit tamet`);
    });

    it("should filter invalid input", () => {
        // Arrange
        const toFilter = null;
        const toHighlight = "unkonwn";

        // Act
        const result = $filter("bpEscapeAndHighlight")(toFilter, toHighlight);

        // Assert
        expect(result).toEqual(``);
    });

    it("should ignore invalid filter", () => {
        // Arrange
        const toFilter = "Lorem ipsum dolor sit tamet";
        const toHighlight = null;

        // Act
        const result = $filter("bpEscapeAndHighlight")(toFilter, toHighlight);

        // Assert
        expect(result).toEqual(`Lorem ipsum dolor sit tamet`);
    });
});
