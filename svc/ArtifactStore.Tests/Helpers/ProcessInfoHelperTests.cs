using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Enums;

namespace ArtifactStore.Helpers
{
    [TestClass]
    public class ProcessInfoHelperTests
    {
        [TestMethod]
        public void ProcessInfo_MapUserToSystemType_Successfull()
        {
            //Arrange
            ProcessInfo pi = new ProcessInfo()
            {
                ProcessType = ProcessType.UserToSystemProcess.ToString(),
                ItemId = 1
            };

            ProcessInfoDto piDTO = ProcessInfoMapper.Map(pi);

            Assert.AreEqual(piDTO.ProcessType, ProcessType.UserToSystemProcess);
        }

        [TestMethod]
        public void ProcessInfo_MapArray_Successfull()
        {
            //Arrange
            List<ProcessInfo> pi = new List<ProcessInfo>()
            {
                new ProcessInfo()
                {
                    ProcessType = ProcessType.SystemToSystemProcess.ToString(),
                    ItemId = 1
                },
                new ProcessInfo()
                {
                    ProcessType = ProcessType.UserToSystemProcess.ToString(),
                    ItemId = 2
                },
                new ProcessInfo()
                {
                    ProcessType = ProcessType.BusinessProcess.ToString(),
                    ItemId = 3
                },
            };


            List<ProcessInfoDto> piDTO = ProcessInfoMapper.Map(pi);

            Assert.AreEqual(piDTO.Count, 3);
            Assert.AreEqual(piDTO[0].ProcessType, ProcessType.SystemToSystemProcess);
            Assert.AreEqual(piDTO[1].ProcessType, ProcessType.UserToSystemProcess);
            Assert.AreEqual(piDTO[2].ProcessType, ProcessType.BusinessProcess);
        }

        [TestMethod]
        public void ProcessInfo_IncorrectString_ParsedToNoneType_Successfull()
        {
            //Arrange
            ProcessInfo pi = new ProcessInfo()
            {
                ProcessType = "sefsdfsdfsdf~@!@$@#$",
                ItemId = 1   
            };

            ProcessInfoDto piDTO = ProcessInfoMapper.Map(pi);

            Assert.AreEqual(piDTO.ProcessType, ProcessType.None);
        }
    }

}
