import "angular";

import {PropertyDescriptorBuilder, IPropertyDescriptor, IPropertyDescriptorBuilder} from "./propertyDescriptorBuilder.service";
import {PropertyDescriptorBuilderMock} from "./propertyDescriptorBuilder.service.mock";

export const EditorServices = angular.module("editor.services", [])
    .service("propertyDescriptorBuilder", PropertyDescriptorBuilder)
    .name;

export {IPropertyDescriptor, IPropertyDescriptorBuilder, PropertyDescriptorBuilderMock}