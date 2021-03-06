﻿using System;
using System.Diagnostics;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface IMigrationController
    {
        void MigrateToLatest(string connectionString, MigrationType migrationType, Action<NzbDroneMigrationBase> beforeMigration);
    }

    public class MigrationController : IMigrationController
    {
        private readonly IAnnouncer _announcer;

        public MigrationController(IAnnouncer announcer)
        {
            _announcer = announcer;
        }

        public void MigrateToLatest(string connectionString, MigrationType migrationType, Action<NzbDroneMigrationBase> beforeMigration)
        {
            var sw = Stopwatch.StartNew();

            _announcer.Heading("Migrating " + connectionString);

            var assembly = Assembly.GetExecutingAssembly();

            var migrationContext = new RunnerContext(_announcer)
                {
                    Namespace = "NzbDrone.Core.Datastore.Migration",
                    ApplicationContext = new MigrationContext(migrationType, beforeMigration)
                };

            var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };
            var factory = new NzbDroneSqliteProcessorFactory();
            var processor = factory.Create(connectionString, _announcer, options);
            var runner = new MigrationRunner(assembly, migrationContext, processor);
            runner.MigrateUp(true);

            sw.Stop();

            _announcer.ElapsedTime(sw.Elapsed);
        }
    }
}
