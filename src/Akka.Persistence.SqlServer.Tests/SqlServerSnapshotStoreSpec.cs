﻿using System.Configuration;
using Akka.Configuration;
using Akka.Persistence.TestKit.Snapshot;
using Akka.TestKit;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.SqlServer.Tests
{
    public class SqlServerSnapshotStoreSpec : SnapshotStoreSpec
    {
        private static readonly Config SpecConfig;

        static SqlServerSnapshotStoreSpec()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["TestDb"].ConnectionString.Replace(@"\", "\\");

            var specString = @"
                        akka.persistence {
                            publish-plugin-commands = on
                            snapshot-store {
                                plugin = ""akka.persistence.snapshot-store.sql-server""
                                sql-server {
                                    class = ""Akka.Persistence.SqlServer.Snapshot.SqlServerSnapshotStore, Akka.Persistence.SqlServer""
                                    plugin-dispatcher = ""akka.actor.default-dispatcher""
                                    table-name = SnapshotStore
                                    schema-name = dbo
                                    auto-initialize = on
                                    connection-string-name = ""TestDb""
                                }
                            }
                        }";

            SpecConfig = ConfigurationFactory.ParseString(specString);


            //need to make sure db is created before the tests start
            DbUtils.Initialize();
        }

        private readonly TestProbe _senderProbe;

        public SqlServerSnapshotStoreSpec(ITestOutputHelper output)
            : base(SpecConfig, "SqlServerSnapshotStoreSpec", output)
        {
            Initialize();
            _senderProbe = CreateTestProbe();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbUtils.Clean();
        }

        [Fact]
        public void SnapshotStore_should_save_and_overwrite_snapshot_with_same_sequence_number()
        {
            var md = Metadata[4];
            SnapshotStore.Tell(new SaveSnapshot(md, "s-5-modified"), _senderProbe.Ref);
            var md2 = _senderProbe.ExpectMsg<SaveSnapshotSuccess>().Metadata;
            Assert.Equal(md.SequenceNr, md2.SequenceNr);
            SnapshotStore.Tell(new LoadSnapshot(Pid, new SnapshotSelectionCriteria(md.SequenceNr), long.MaxValue), _senderProbe.Ref);
            var result = _senderProbe.ExpectMsg<LoadSnapshotResult>();
            Assert.Equal("s-5-modified", result.Snapshot.Snapshot.ToString());
            Assert.Equal(md.SequenceNr, result.Snapshot.Metadata.SequenceNr);
            // metadata timestamp may have been changed
        }
    }
}