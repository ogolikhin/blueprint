using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDefinitions
{
    [EventSource(Name = "BlueprintSys-Blueprint-Blueprint")]
    public sealed class BlueprintEventSource : EventSource
    {
        public static BlueprintEventSource Log = new BlueprintEventSource();

        #region Keywords

        public static class Keywords
        {
            public const EventKeywords Application = (EventKeywords)1L;
            public const EventKeywords DataAccess = (EventKeywords)2L;
            public const EventKeywords UserInterface = (EventKeywords)4L;
            public const EventKeywords General = (EventKeywords)8L;
        }

        #endregion

        #region Tasks

        public static class Tasks
        {
            public const EventTask SessionsWebApi = (EventTask)1;
        }

        #endregion

        #region Opcodes

        public static class Opcodes
        {
            public const EventOpcode Start = (EventOpcode)20;
            public const EventOpcode Finish = (EventOpcode)21;
            public const EventOpcode Error = (EventOpcode)22;
            public const EventOpcode Starting = (EventOpcode)23;

            public const EventOpcode QueryStart = (EventOpcode)30;
            public const EventOpcode QueryFinish = (EventOpcode)31;
            public const EventOpcode QueryNoResults = (EventOpcode)32;

            public const EventOpcode CacheQuery = (EventOpcode)40;
            public const EventOpcode CacheUpdate = (EventOpcode)41;
            public const EventOpcode CacheHit = (EventOpcode)42;
            public const EventOpcode CacheMiss = (EventOpcode)43;
            public const EventOpcode CacheNotEnoughMemory = (EventOpcode)44; 
        }

        #endregion

        [Event(1, Level = EventLevel.Informational)]
        public void Message(string Message)
        {
            WriteEvent(1, Message);
        }

        #region Services

        #endregion

        #region AccessControl

        //LogProvider.Current.WriteEntry(WebApiConfig.ServiceLogSource, "Service starting...", LogEntryType.Information);
        [Event(100, 
            Level = EventLevel.Informational, 
            Task = Tasks.SessionsWebApi, 
            Opcode = Opcodes.Starting,
            Keywords = Keywords.Application)]
        public void SessionsServiceStarting()
        {
            WriteEvent(100);
        }

        //LogProvider.Current.WriteEntry(WebApiConfig.ServiceLogSource, "Service started.", LogEntryType.Information);
        [Event(101, 
            Level = EventLevel.Informational, 
            Task = Tasks.SessionsWebApi, 
            Opcode = Opcodes.Finish,
            Keywords = Keywords.Application)]
        public void SessionsServiceStarted()
        {
            WriteEvent(101);
        }

        //LogProvider.Current.WriteEntry(WebApiConfig.ServiceLogSource, "Not enough memory", LogEntryType.Error);
        [Event(102, 
            Level = EventLevel.Error, 
            Task = Tasks.SessionsWebApi, 
            Opcode = Opcodes.CacheNotEnoughMemory,
            Keywords = Keywords.Application)]
        public void SessionsNotEnoughMemory()
        {
            WriteEvent(102);
        }

        //LogProvider.Current.WriteEntry(WebApiConfig.ServiceLogSource, "Error loading sessions from database.", LogEntryType.Error);
        //[Event(102,
        //    Level = EventLevel.Error,
        //    Task = Tasks.SessionsWebApi,
        //    Opcode = Opcodes.CacheNotEnoughMemory,
        //    Keywords = Keywords.Application + Keywords.DataAccess)]
        //public void SessionsNotEnoughMemory()
        //{
        //    WriteEvent(102);
        //}

        #endregion
    }
}
