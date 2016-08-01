﻿//-----------------------------------------------------------------------
// <copyright file="SqlServerJournalQuerySpec.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2016 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2016 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using Akka.Configuration;
using Akka.Persistence.Sql.TestKit;
using Xunit;
using Xunit.Abstractions;

namespace Akka.Persistence.SqlServer.Tests
{
    [Collection("SqlServerSpec")]
    public class SqlServerJournalQuerySpec : SqlJournalQuerySpec
    {
        private static readonly Config SpecConfig;

        static SqlServerJournalQuerySpec()
        {
            var specString = @"
                    akka.persistence {
                        publish-plugin-commands = on
                        journal {
                            plugin = ""akka.persistence.journal.sql-server""
                            sql-server {
                                class = ""Akka.Persistence.SqlServer.Journal.SqlServerJournal, Akka.Persistence.SqlServer""
                                plugin-dispatcher = ""akka.actor.default-dispatcher""
                                table-name = EventJournal
                                schema-name = dbo
                                auto-initialize = on
                                connection-string-name = ""TestDb""
                            }
                        }
                    } " + TimestampConfig("akka.persistence.journal.sql-server");

            SpecConfig = ConfigurationFactory.ParseString(specString);


            //need to make sure db is created before the tests start
            DbUtils.Initialize();
        }

        public SqlServerJournalQuerySpec(ITestOutputHelper output)
            : base(SpecConfig, "SqlServerJournalQuerySpec", output)
        {
            Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            DbUtils.Clean();
        }
    }
}
