﻿using Machine.Container.Services;

namespace Machine.Migrations.Services
{
  public interface IMigratorContainerFactory
  {
    IHighLevelContainer CreateAndPopulateContainer(IConfiguration configuration);
  }
}