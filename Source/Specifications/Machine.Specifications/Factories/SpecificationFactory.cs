using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Machine.Specifications.Model;
using Machine.Specifications.Utility;

namespace Machine.Specifications.Factories
{
  public class SpecificationFactory
  {
    public Specification CreateSpecification(Context context, FieldInfo specificationField)
    {
      bool isIgnored = context.IsIgnored || specificationField.HasAttribute<IgnoreAttribute>();
      It it = (It)specificationField.GetValue(context.Instance);
      string name = specificationField.Name.ReplaceUnderscores().Trim();

      return new Specification(name, it, isIgnored, specificationField);
    }


  }
}