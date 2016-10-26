import {BpFormat} from "./bp-format.filter";

describe("BpFormat.filter", () => {
    it("input with no args returns input", () => {
        // Arrange
        const input = "input";

        // Act
        const result = BpFormat.filter(input);

        // Assert
        expect(result).toEqual("input");
    });

    it("input with single arg replaces placeholder", () => {
        // Arrange
        const input = "in{0}put";

        // Act
        const result = BpFormat.filter(input, "0");

        // Assert
        expect(result).toEqual("in0put");
    });

    it("input with multiple arg replaces all placeholders", () => {
        // Arrange
        const input = "{2} this {3} is {4} a {0} test {1}";

        // Act
        const result = BpFormat.filter(input, "zeroth", "first", "second", "third", "fourth");

        // Assert
        expect(result).toEqual("second this third is fourth a zeroth test first");
    });

    it("placeholders without matching args are not replaced", () => {
        // Arrange
        const input = "{2} this {3} is {4} a {0} test {1}";

        // Act
        const result = BpFormat.filter(input, "zeroth", undefined, "second");

        // Assert
        expect(result).toEqual("second this {3} is {4} a zeroth test {1}");
    });

    it("all instances of the placeholders are replaced", () => {
        // Arrange
        const input = "{0}{0}{1}{1}{0}{1}{0}";

        // Act
        const result = BpFormat.filter(input, "a", "b");

        // Assert
        expect(result).toEqual("aabbaba");
    });

    it("replacements happen simultaneously", () => {
        // Arrange
        const input = "{0}{1}";

        // Act
        const result = BpFormat.filter(input, "{1}", "{0}");

        // Assert
        expect(result).toEqual("{1}{0}");
    });

    it("only correctly formatted placeholders are replaced", () => {
        // Arrange
        const input = "{test} }{ {0{} {}1}";

        // Act
        const result = BpFormat.filter(input, "0", "1");

        // Assert
        expect(result).toEqual("{test} }{ {0{} {}1}");
    });
});
