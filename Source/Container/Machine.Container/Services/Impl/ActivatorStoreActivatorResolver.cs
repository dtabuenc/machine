using System;
using System.Collections.Generic;

using Machine.Container.Model;

namespace Machine.Container.Services.Impl
{
  public class ActivatorStoreActivatorResolver : IActivatorResolver
  {
    #region Logging
    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(typeof(ActivatorStoreActivatorResolver));
    #endregion

    #region IActivatorResolver Members
    public IActivator ResolveActivator(IResolutionServices services, ServiceEntry serviceEntry)
    {
      _log.Info("ResolveActivator: " + serviceEntry);
      if (!services.ActivatorStore.HasActivator(serviceEntry))
      {
        return null;
      }
      IActivator activator = services.ActivatorStore.ResolveActivator(serviceEntry);
      if (activator.CanActivate(services))
      {
        return activator;
      }
      throw new ServiceResolutionException("Unable to activate!");
    }
    #endregion
  }
}