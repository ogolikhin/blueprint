using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AdminStore.Models;
using System.Collections.Generic;

namespace ServiceLibrary.Models
{
    [TestClass]
    public class WorkflowAssignScopeTests
    {
        [TestMethod]
        public void IsEmpty_AllParametersIsOk_ReturnFalse()
        {
            //arrange
            var scope = new WorkflowAssignScope() { AllArtifacts = true, AllProjects = true, ArtifactIds = new List<int> { 1 }, ProjectIds = new List<int> { 1 } };
            var expectedResult = false;

            //act
            var result = scope.IsEmpty();

            //assert
            Assert.AreEqual(expectedResult, result);
        }       

        [TestMethod]
        public void IsEmpty_ArtifactIdsIsEmptyOrNull_ReturnTrue()
        {
            //arrange
            var scope = new WorkflowAssignScope() { AllArtifacts = false, AllProjects = true, ArtifactIds = new List<int> { }, ProjectIds = new List<int> { 1 } };
            var expectedResult = true;

            //act
            var result = scope.IsEmpty();

            //assert
            Assert.AreEqual(expectedResult, result);

            //arrange
            scope.ArtifactIds = null;

            //act
            result = scope.IsEmpty();

            //assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void IsEmpty_ProjectIdsIsEmptyOrNull_ReturnTrue()
        {
            //arrange
            var scope = new WorkflowAssignScope() { AllArtifacts = true, AllProjects = false, ArtifactIds = new List<int> { 1 }, ProjectIds = new List<int> {  } };
            var expectedResult = true;

            //act
            var result = scope.IsEmpty();

            //assert
            Assert.AreEqual(expectedResult, result);

            //arrange
            scope.ProjectIds = null;

            //act
            result = scope.IsEmpty();

            //assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void IsEmpty_ProjectIdsIsEmptyOrNullAndArtifactIdsIsEmptyOrNull_ReturnTrue()
        {
            //arrange
            var scope = new WorkflowAssignScope() { AllArtifacts = false, AllProjects = false, ArtifactIds = new List<int> {  }, ProjectIds = new List<int> { } };
            var expectedResult = true;

            //act
            var result = scope.IsEmpty();

            //assert
            Assert.AreEqual(expectedResult, result);//ProjectIds is empty, ArtifactIds is empty

            //arrange
            scope.ProjectIds = null;

            //act
            result = scope.IsEmpty();

            //assert
            Assert.AreEqual(expectedResult, result);//ProjectIds is null, ArtifactIds is empty

            //arrange
            scope.ArtifactIds = new List<int>{ };

            //act
            result = scope.IsEmpty();

            //assert
            Assert.AreEqual(expectedResult, result);//ProjectIds is null, ArtifactIds is null

            //arrange
            scope.ProjectIds = new List<int> { };

            //act
            result = scope.IsEmpty();

            //assert
            Assert.AreEqual(expectedResult, result);//ProjectIds is empty, ArtifactIds is null
        }

    }
}
