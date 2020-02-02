﻿using System;
using System.Collections.Generic;
using Coverlet.Core.Abstracts;
using Coverlet.Core.Helpers;
using MonoDevelop.Projects;
using CoverletCoverage = Coverlet.Core.Coverage;

namespace CodeCoverage.Coverage
{
  class CoverletCoverageProvider : ICoverageProvider
  {
    Dictionary<Tuple<Project, ConfigurationSelector>, CoverletCoverage> projectCoverageMap;
    readonly ILogger logger;
    readonly FileSystem fileSystem;
    readonly InstrumentationHelper instrumentationHelper;

    public CoverletCoverageProvider(ILoggingService log)
    {
      logger = new LoggingServiceCoverletLogger(log);
      fileSystem = new FileSystem();
      instrumentationHelper = new InstrumentationHelper(new ProcessExitHandler(), new RetryHelper(), fileSystem);
      projectCoverageMap = new Dictionary<Tuple<Project, ConfigurationSelector>, CoverletCoverage>();
    }

    public void Prepare(Project testProject, ConfigurationSelector configuration)
    {
      var unitTestDll = testProject.GetOutputFileName(configuration).ToString();
      var coverage = new CoverletCoverage(unitTestDll,
          new string[0],
          new string[0], // Include directories.
          new string[0],
          new string[0],
          new string[0],
          false, // Include test assembly.
          false, // Single hit.
          null, // Merge with.
          false, // Use source link.
          logger,
          instrumentationHelper,
          fileSystem);
      coverage.PrepareModules();
      projectCoverageMap[new Tuple<Project, ConfigurationSelector>(testProject, configuration)] = coverage;
    }

    public ICoverageResults GetCoverage(Project testProject, ConfigurationSelector configuration)
    {
      if (!projectCoverageMap.TryGetValue(new Tuple<Project, ConfigurationSelector>(testProject, configuration), out CoverletCoverage coverage))
        return null;

      var results = coverage.GetCoverageResult();
      return new CoverletCoverageResults(results);
    }
  }
}
