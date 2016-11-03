using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Sitecore.Reference.Storefront.Infrastructure
{
	public static class ServiceCollectionExtensions
	{
		public static void AddMvcControllersInCurrentAssembly(this IServiceCollection serviceCollection)
		{
		    AddTypesImplementingInCurrentAssembly<IController>(serviceCollection);
		}

        public static void AddTypesImplementingInCurrentAssembly<T>(this IServiceCollection serviceCollection)
        {
            AddTypesImplementing<T>(serviceCollection, Assembly.GetCallingAssembly());
        }

        public static void AddTypesImplementing<T>(this IServiceCollection serviceCollection, params string[] assemblyFilters)
		{
			var assemblyNames = new HashSet<string>(assemblyFilters.Where(filter => !filter.Contains('*')));
			var wildcardNames = assemblyFilters.Where(filter => filter.Contains('*')).ToArray();

			var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
			{
				var nameToMatch = assembly.GetName().Name;
				if (assemblyNames.Contains(nameToMatch)) return true;

				return wildcardNames.Any(wildcard => IsWildcardMatch(nameToMatch, wildcard));
			})
			.ToArray();

            AddTypesImplementing<T>(serviceCollection, assemblies);
		}

		public static void AddTypesImplementing<T>(this IServiceCollection serviceCollection, params Assembly[] assemblies)
		{
			var types = GetTypesImplementing<T>(assemblies);

			foreach (var typeToRegister in types)
			{
				serviceCollection.AddTransient(typeToRegister);
			}
		}

        public static Type[] GetTypesImplementing<T>(params Assembly[] assemblies)
		{
			if (assemblies == null || assemblies.Length == 0)
			{
				return new Type[0];
			}

			var targetType = typeof(T);

			return assemblies
				.Where(assembly => !assembly.IsDynamic)
				.SelectMany(GetExportedTypes)
				.Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition && targetType.IsAssignableFrom(type))
				.ToArray();
		}

		private static IEnumerable<Type> GetExportedTypes(Assembly assembly)
		{
			try
			{
				return assembly.GetExportedTypes();
			}
			catch (NotSupportedException)
			{
				// A type load exception would typically happen on an Anonymously Hosted DynamicMethods
				// Assembly and it would be safe to skip this exception.
				return Type.EmptyTypes;
			}
			catch (ReflectionTypeLoadException ex)
			{
				// Return the types that could be loaded. Types can contain null values.
				return ex.Types.Where(type => type != null);
			}
			catch (Exception ex)
			{
				// Throw a more descriptive message containing the name of the assembly.
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,	"Unable to load types from assembly {0}. {1}", assembly.FullName, ex.Message), ex);
			}
		}

		/// <summary>
		/// Checks if a string matches a wildcard argument (using regex)
		/// </summary>
		private static bool IsWildcardMatch(string input, string wildcards)
		{
			return Regex.IsMatch(input, "^" + Regex.Escape(wildcards).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
		}


	}
}