//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Threading.Tasks;
//using AccessControl.Models;
//using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using ServiceLibrary.Repositories;

namespace AccessControl.Repositories
{
    [TestClass]
    public class SqlSessionsRepositoryTests
    {
        //[TestMethod]
        //public async Task GetSession_ReturnsFirstSession()
        //{
        //    // Arrange
        //    var guid = new Guid("12345678901234567890123456789012");
        //    IEnumerable<Session> result = new[] {new Session {SessionId = guid}};
        //    var cxn = new Mock<IDbConnectionWrapper>();
        //    cxn.Setup(c => c.QueryAsync<Session>("GetSession", It.Is<object>(p => guid.Equals(Get(new DynamicParameters(p), "SessionId"))), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure)).Returns(Task.FromResult(result));
        //    var repository = new SqlSessionsRepository(cxn.Object);

        //    // Act
        //    Session session = await repository.GetSession(guid);

        //    // Assert
        //    Assert.AreEqual(result.FirstOrDefault(), session);
        //}

        //private object Get(object parameters, string key)
        //{
        //    return ((SqlMapper.IParameterLookup)new DynamicParameters(parameters))[key];
        //}

        //[TestMethod]
        //public async Task SelectSessions_ReturnsSession()
        //{
        //    // Arrange
        //    int ps = 100;
        //    int pn = 1;
        //    var guid = new Guid("12345678901234567890123456789012");
        //    IEnumerable<Session> result = new[] { new Session { SessionId = guid } };
        //    var cxn = new Mock<IDbConnectionWrapper>();
        //    cxn.Setup(c => c.QueryAsync<Session>("SelectSessions", It.Is<object>(p => ps.Equals(Get(p, "ps")) && pn.Equals(Get(p, "pn"))), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure)).Returns(Task.FromResult(result));
        //    var repository = new SqlSessionsRepository(cxn.Object);

        //    // Act
        //    IEnumerable<Session> sessions = await repository.SelectSessions(ps, pn);

        //    // Assert
        //    CollectionAssert.AreEquivalent(result.ToList(), sessions.ToList());
        //}

        //[TestMethod]
        //public async Task BeginSession_ReturnsNewAndOldSessions()
        //{
        //    // Arrange
        //    int id = 123;
        //    var oldSession = new Guid("12345678901234567890123456789012");
        //    var newSession = new Guid("12345678901234567890123456789013");
        //    var cxn = new Mock<IDbConnectionWrapper>();
        //    cxn.Setup(c => c.ExecuteAsync("BeginSession", It.Is<object>(p => id.Equals(Get(new DynamicParameters(p), "@UserId"))), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure)).Returns(Task.FromResult(1))
        //        .Callback<string, object, IDbTransaction, int?, CommandType>((s, p, t, o, c) => { ((DynamicParameters)p).Add("NewSessionId", newSession);  ((DynamicParameters)p).Add("OldSessionId", oldSession); });
        //    var repository = new SqlSessionsRepository(cxn.Object);

        //    // Act
        //    Guid?[] sessions = await repository.BeginSession(id);

        //    // Assert
        //    Assert.AreEqual(new [] { newSession, oldSession }, sessions);
        //}

        //[TestMethod]
        //public async Task EndSession_CallsEndSessionCorrectly()
        //{
        //    // Arrange
        //    var guid = new Guid("12345678901234567890123456789012");
        //    var cxn = new Mock<IDbConnectionWrapper>(MockBehavior.Strict);
        //    cxn.Setup(c => c.ExecuteAsync("EndSession", It.Is<object>(p => guid.Equals(Get(new DynamicParameters(p), "SessionId"))), It.IsAny<IDbTransaction>(), It.IsAny<int?>(), CommandType.StoredProcedure)).Returns(Task.FromResult(1));
        //    var repository = new SqlSessionsRepository(cxn.Object);

        //    // Act
        //    await repository.EndSession(guid);

        //    // Assert
        //}
    }
}
