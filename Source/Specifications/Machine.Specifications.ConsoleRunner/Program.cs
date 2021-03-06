using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Machine.Specifications.ConsoleRunner.Properties;
using Machine.Specifications.Reporting;
using Machine.Specifications.Runner;
using Machine.Specifications.Runner.Impl;

namespace Machine.Specifications.ConsoleRunner
{
  public class Program
  {

    [STAThread]
    [LoaderOptimization(LoaderOptimization.MultiDomain)]
    public static void Main(string[] args)
    {
      var program = new Program(new DefaultConsole());
      ExitCode exitCode = program.Run(args);

      Environment.Exit((int)exitCode);
    }

    readonly IConsole _console;

    public Program(IConsole console)
    {
      _console = console;
    }

    public ExitCode Run(string[] arguments)
    {
      ExceptionReporter reporter = new ExceptionReporter(_console);
      var runListener = new RunListener(_console);
      
      List<ISpecificationRunListener> listeners = new List<ISpecificationRunListener>();
      
      try
      {

        Options options = new Options();
        if (!options.ParseArguments(arguments))
        {
          _console.WriteLine(Resources.UsageStatement);
          return ExitCode.Failure;
        }

        if (!IsHtmlPathUnspecifiedOrSpecifiedAndValid(options, listeners))
        {
          _console.WriteLine("Invalid html path:" + options.HtmlPath);
          _console.WriteLine(Resources.UsageStatement);
          return ExitCode.Failure;
        }

        if (!options.Silent)
          listeners.Add(runListener);
        
        if (options.AssemblyFiles.Count == 0)
        {
          _console.WriteLine(Resources.UsageStatement);
          return ExitCode.Failure;
        }

        var listener = new AggregateRunListener(listeners);
        
        ISpecificationRunner specificationRunner = new AppDomainRunner(listener, options.GetRunOptions());
        List<Assembly> assemblies = new List<Assembly>();
        foreach (string assemblyName in options.AssemblyFiles)
        {
          if (!File.Exists(assemblyName))
          {
            throw NewException.MissingAssembly(assemblyName);
          }

          Assembly assembly = Assembly.LoadFrom(assemblyName);
          assemblies.Add(assembly);
        }

        specificationRunner.RunAssemblies(assemblies);
      }
      catch(Exception ex)
      {
        reporter.ReportException(ex);
        return ExitCode.Error;
      }

      if (runListener.FailureOccured)
      {
        return ExitCode.Failure;
      }

      return ExitCode.Success;
    }

    public bool IsHtmlPathUnspecifiedOrSpecifiedAndValid(Options options, List<ISpecificationRunListener> listeners)
    {
      if (!options.HtmlPath.Equals(string.Empty))
      {
        var reportingListener = new GenerateHtmlReportRunListener(options.HtmlPath, options.ShowTimeInformation);
        listeners.Add(reportingListener);
        return true;
      }
      return true;
    }
  }
}