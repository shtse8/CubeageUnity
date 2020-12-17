using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Cubeage
{
    public static class Extensions
    {
        public static T ToEnum<T>(this int value) where T : Enum
        {
            return (T)(object)value;
        }

        public static void Deconstruct<TKey, TValue>(
            this KeyValuePair<TKey, TValue> kvp,
            out TKey key,
            out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
        public static string GetDisplayValue<T>(this T value)
        {
            return value.GetType().GetMember(value.ToString())
                       .First()
                       .GetCustomAttribute<DisplayAttribute>()
                       .Name;
        }
        public static TValue GetValue<T, TValue>(this T target, Expression<Func<T, TValue>> memberLamda)
        {
            if (memberLamda.Body is MemberExpression memberSelectorExpression)
            {
                switch (memberSelectorExpression.Member)
                {
                    case FieldInfo field:
                        return (TValue) field.GetValue(target);
                    case PropertyInfo property:
                        return (TValue) property.GetValue(target, null);
                    default:
                        throw new Exception($"Unsupported Expression Member Type: {memberSelectorExpression.Member.MemberType}");
                }
            } 
            else
            {
                throw new Exception($"Unsupported Expression Body Type");
            }
        }

        public static void SetValue<T, TValue>(this T target, Expression<Func<T, TValue>> memberLamda, TValue value)
        {
            if (memberLamda.Body is MemberExpression memberSelectorExpression)
            {
                switch (memberSelectorExpression.Member)
                {
                    case FieldInfo field:
                        field.SetValue(target, value);
                        break;
                    case PropertyInfo property:
                        property.SetValue(target, value, null);
                        break;
                    default:
                        throw new Exception($"Unsupported Expression Member Type: {memberSelectorExpression.Member.MemberType}");
                }
            }
            else
            {
                throw new Exception($"Unsupported Expression Body Type");
            }
        }
    }

    public static class EnumHelper
    {
        public static int GetValue<T>(this T value) where T : Enum
        {
            return (int)(object)value;
        }

        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
