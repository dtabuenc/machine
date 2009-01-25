using System.Reflection;

using Machine.Specifications.Model;
using Machine.Specifications.Utility;

namespace Machine.Specifications.Factories
{
  public class SpecificationFactory
  {
    public Specification CreateSpecification(Context context, FieldInfo specificationField)
    {
      bool isIgnored = context.IsIgnored || specificationField.HasAttribute<IgnoreAttribute>();
      It it = (It) specificationField.GetValue(context.Instance);
      string name = specificationField.Name.ReplaceUnderscores().Trim();

      return new Specification(name, it, isIgnored, specificationField);
    }

    public Specification CreateSpecificationMixin(Context rootContext, Context specificationFieldContext, FieldInfo specificationField)
    {
      bool isIgnored = specificationFieldContext.IsIgnored || specificationField.HasAttribute<IgnoreAttribute>();
      It it = (It) specificationField.GetValue(specificationFieldContext.Instance);
      string name = specificationField.Name.ReplaceUnderscores().Trim();

      return new SpecificationMixin(name, it, isIgnored, specificationField, rootContext.Instance, specificationFieldContext.Instance);
    }
  }
}