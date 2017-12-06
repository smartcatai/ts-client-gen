using System;
using System.Reflection;
using System.Resources;

namespace TSClientGen
{
    /// <summary>
    /// Вспомогательный класс для упрощения работы с ресурсами
    /// </summary>
    internal static class ResxHelper
    {
        public static ResourceManager GetResourceManager(this Type resxType)
        {
            var resourceManagerProperty = resxType.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			
            if (resourceManagerProperty == null)
                throw new ArgumentException(
                    $"Parameter must have static property ResourceManager (type {resxType.FullName})",
                    nameof(resxType));
			
            if (resourceManagerProperty.PropertyType != typeof(ResourceManager))
                throw new ArgumentException(
                    "Static property ResourceManager has unexpected type (type {resxType.FullName})",
                    nameof(resxType));
			
            return (ResourceManager) resourceManagerProperty.GetValue(null);;
        }
    }
}