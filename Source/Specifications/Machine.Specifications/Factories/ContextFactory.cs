using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Machine.Specifications.Model;
using Machine.Specifications.Utility;

namespace Machine.Specifications.Factories
{
  public class ContextFactory
  {
    readonly SpecificationFactory _specificationFactory;

    public ContextFactory()
    {
      _specificationFactory = new SpecificationFactory();
    }

    public Context CreateContextFrom(object instance, FieldInfo fieldInfo)
    {
      if (fieldInfo.FieldType == typeof(It))
      {
        return CreateContextFrom(instance, new[] {fieldInfo});
      }
      else
      {
        return CreateContextFrom(instance);
      }
    }

    public Context CreateContextFrom(object instance)
    {
      var type = instance.GetType();
      var fieldInfos = type.GetPrivateOrInheritedFieldsOfType<It>();

      return CreateContextFrom(instance, fieldInfos);
    }

    Context CreateContextFrom(object instance, IEnumerable<FieldInfo> acceptedSpecificationFields)
    {
      var type = instance.GetType();
	  var fieldInfos = type.GetPrivateOrInheritedFields();
      List<FieldInfo> itFieldInfos = new List<FieldInfo>();

      var contextClauses = ExtractPrivateFieldValues<Establish>(instance);
      contextClauses.Reverse();

      var cleanupClauses = ExtractPrivateFieldValues<Cleanup>(instance);

      var becauses = ExtractPrivateFieldValues<Because>(instance);

      if (becauses.Count > 1)
      {
        throw new SpecificationUsageException("There can only be one Because clause.");
      }

      var concern = ExtractSubject(type);
      var isSetupForEachSpec = IsSetupForEachSpec(type);

      var isIgnored = type.HasAttribute<IgnoreAttribute>();
      var tags = ExtractTags(type);
      var context = new Context(type, instance, contextClauses, becauses.FirstOrDefault(), cleanupClauses, concern, isIgnored, tags, isSetupForEachSpec);

      foreach (FieldInfo info in fieldInfos)
      {
        if (acceptedSpecificationFields.Contains(info) &&
          info.FieldType == typeof(It))
        {
          itFieldInfos.Add(info);
        }
      }

      CreateSpecifications(itFieldInfos, context);

      return context;
    }

    static IEnumerable<Tag> ExtractTags(Type type)
    {
      var extractor = new AttributeTagExtractor();
      return extractor.ExtractTags(type);
    }

    static bool IsSetupForEachSpec(Type type)
    {
      return type.GetCustomAttributes(typeof(SetupForEachSpecification), true).Any();
    }

    static Subject ExtractSubject(Type type)
    {
      var attributes = type.GetCustomAttributes(typeof(SubjectAttribute), true);

      if (attributes.Length > 1)
        throw new SpecificationUsageException("Cannot have more than one Subject on a Context");

      if (attributes.Length == 0) return null;

      var attribute = (SubjectAttribute)attributes[0];

      return new Subject(attribute.SubjectType, attribute.SubjectText);
    }

    void CreateSpecifications(IEnumerable<FieldInfo> itFieldInfos, Context context)
    {
      foreach (var itFieldInfo in itFieldInfos)
      {
        Specification specification = _specificationFactory.CreateSpecification(context, itFieldInfo);
        context.AddSpecification(specification);
      }
    }

    static List<T> ExtractPrivateFieldValues<T>(object instance)
    {
      var delegates = new List<T>();
      var type = instance.GetType();
      while (type != null)
      {
		var fields = type.GetPrivateFieldsWith(typeof(T));

        if (fields.Count() > 1)
        {
          throw new SpecificationUsageException(String.Format("You cannot have more than one {0} clause in {1}", typeof(T).Name, type.FullName));
        }

        var field = fields.FirstOrDefault();

        if (field != null) 
        { 
          T val = (T)field.GetValue(instance);
          delegates.Add(val);
        }

        type = type.BaseType;
      }

      return delegates;
    }
  }
}