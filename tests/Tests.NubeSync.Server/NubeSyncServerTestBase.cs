using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NubeSync.Server;
using NubeSync.Server.Data;

namespace Tests.NubeSync.Server
{
    public class NubeSyncServerTestBase
    {
        protected TestContext Context;
        protected static DateTimeOffset UpdatedEarly = new DateTimeOffset(2020, 6, 25, 6, 0, 0, TimeSpan.Zero);
        protected static DateTimeOffset UpdatedMid = new DateTimeOffset(2020, 6, 25, 12, 0, 0, TimeSpan.Zero);
        protected static DateTimeOffset UpdatedLate = new DateTimeOffset(2020, 6, 25, 18, 0, 0, TimeSpan.Zero);

        protected List<TestItem> Items = new List<TestItem>
        {
            new TestItem { Id = "1", Name = "Name1" },
            new TestItem { Id = "2", Name = "Name2" },
        };

        protected List<NubeOperation> NewOperations = new List<NubeOperation>
        {
            new NubeOperation
            {
                Id = "Op100",
                ItemId = "1",
                InstallationId = "InstallationId",
                Type = OperationType.Added,
                TableName = "TestItem",
                UserId = "User",
            },
            new NubeOperation
            {
                Id = "Op101",
                ItemId = "1",
                InstallationId = "InstallationId",
                Type = OperationType.Modified,
                TableName = "TestItem",
                UserId = "User",
                Property = "Name",
                OldValue = null,
                Value = "Name0",
            },
            new NubeOperation
            {
                Id = "Op200",
                ItemId = "1",
                InstallationId = "InstallationId",
                Type = OperationType.Modified,
                TableName = "TestItem",
                UserId = "User",
                Property = "Name",
                OldValue = "Name0",
                Value = "Name1",
            },
            new NubeOperation
            {
                Id = "Op300",
                ItemId = "2",
                InstallationId = "InstallationId2",
                Type = OperationType.Added,
                TableName = "TestItem",
                UserId = "User",
            },
            new NubeOperation
            {
                Id = "Op400",
                ItemId = "2",
                InstallationId = "InstallationId2",
                Type = OperationType.Modified,
                TableName = "TestItem",
                UserId = "User",
                Property = "Name",
                OldValue = null,
                Value = "Name2",
            }
        };

        protected List<NubeOperation> ProcessedOperations = new List<NubeOperation>
        {
            new NubeOperation 
            { 
                Id = "Op100",
                ItemId = "1", 
                InstallationId = "InstallationId",
                ProcessingType = ProcessingType.Processed,
                Type = OperationType.Added,
                TableName = "TestItem",
                UserId = "User",
                ServerUpdatedAt = UpdatedEarly
            },
            new NubeOperation
            {
                Id = "Op101",
                ItemId = "1",
                InstallationId = "InstallationId",
                ProcessingType = ProcessingType.Processed,
                Type = OperationType.Modified,
                TableName = "TestItem",
                UserId = "User",
                ServerUpdatedAt = UpdatedMid,
                Property = "Name",
                OldValue = null,
                Value = "Name0",
            },
            new NubeOperation
            {
                Id = "Op200",
                ItemId = "1",
                InstallationId = "InstallationId",
                ProcessingType = ProcessingType.Processed,
                Type = OperationType.Modified,
                TableName = "TestItem",
                UserId = "User",
                ServerUpdatedAt = UpdatedMid,
                Property = "Name",
                OldValue = "Name0",
                Value = "Name1",
            },
            new NubeOperation
            {
                Id = "Op300",
                ItemId = "2",
                InstallationId = "InstallationId2",
                ProcessingType = ProcessingType.Processed,
                Type = OperationType.Added,
                TableName = "TestItem",
                UserId = "User",
                ServerUpdatedAt = UpdatedEarly
            },
            new NubeOperation
            {
                Id = "Op400",
                ItemId = "2",
                InstallationId = "InstallationId2",
                ProcessingType = ProcessingType.Processed,
                Type = OperationType.Modified,
                TableName = "TestItem",
                UserId = "User",
                ServerUpdatedAt = UpdatedMid,
                Property = "Name",
                OldValue = null,
                Value = "Name2",
            }
        };

        protected OperationService Service;

        public NubeSyncServerTestBase()
        {
            Service = new OperationService();

            var options = new DbContextOptionsBuilder<TestContext>()
               .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
               .Options;

            Context = new TestContext(options);
            Context.AddRange(Items);
            Context.AddRange(ProcessedOperations);
            Context.SaveChanges();
        }
    }
}