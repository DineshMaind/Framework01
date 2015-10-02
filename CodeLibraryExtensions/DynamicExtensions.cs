//-----------------------------------------------------------------------
// <copyright file="DynamicExtensions.cs" company="Personal">
//     Copyright (c) for personal and commercial use. All rights reserved.
// </copyright>
// <author>Dinesh Maind</author>
//-----------------------------------------------------------------------

namespace CodeLibrary.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Script.Serialization;

    /// <summary>
    /// Extends the System.String class to convert a json object to an expando object.
    /// </summary>
    public static class DynamicExtensions
    {
        /// <summary>
        /// Converts the Json string into expando object.
        /// </summary>
        /// <param name="jsonString">The json string.</param>
        /// <returns>Returns the dynamic object.</returns>
        public static dynamic AsExpandoObject(this string jsonString)
        {
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;
            var jsonObject = (new JavaScriptSerializer()).DeserializeObject(jsonString);
            var dict = jsonObject as IDictionary<string, object>;
            var list = jsonObject as object[];

            if (dict != null)
            {
                AddToExpandoObject(expandoObject, dict);
            }
            else if (list != null)
            {
                AddToExpandoObject(expandoObject, list);
            }

            return expandoObject;
        }

        /// <summary>
        /// Converts to the specific object of type TType.
        /// </summary>
        /// <typeparam name="TType">The type to be converted.</typeparam>
        /// <param name="expandoObject">The expando object.</param>
        /// <returns>Returns an object of type TType.</returns>
        public static TType ToObject<TType>(this IDictionary<string, object> expandoObject) where TType : new()
        {
            var obj = new TType();
            var properties = typeof(TType).GetProperties();

            Parallel.ForEach(properties, property =>
            {
                if (expandoObject.Any(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    KeyValuePair<string, object> item = expandoObject.First(x => x.Key.Equals(property.Name, StringComparison.InvariantCultureIgnoreCase));
                    var propertyType = property.PropertyType;
                    propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    object value = Convert.ChangeType(item.Value, propertyType);
                    property.SetValue(obj, value, null);
                }
            });

            return obj;
        }

        /// <summary>
        /// Adds to the expando object.
        /// </summary>
        /// <param name="expandoObject">The expando object.</param>
        /// <param name="itemKey">The item key of type System.String.</param>
        /// <param name="itemValue">The item value of type System.Object.</param>
        private static void AddToExpandoObject(IDictionary<string, object> expandoObject, string itemKey, object itemValue)
        {
            var result = new ExpandoObject() as IDictionary<string, object>;
            var list = itemValue as object[];
            var dict = itemValue as IDictionary<string, object>;

            if (dict != null)
            {
                AddToExpandoObject(result, dict);
                expandoObject.Add(itemKey, result);
            }
            else if (list != null)
            {
                AddToExpandoObject(result, list);
                expandoObject.Add(itemKey, result);
            }
            else
            {
                expandoObject.Add(itemKey, itemValue);
            }
        }

        /// <summary>
        /// Adds to the expando object.
        /// </summary>
        /// <param name="expandoObject">The expando object.</param>
        /// <param name="objDict">The dictionary object of type IDictionary.</param>
        private static void AddToExpandoObject(IDictionary<string, object> expandoObject, IDictionary<string, object> objDict)
        {
            Parallel.ForEach(objDict, item =>
            {
                AddToExpandoObject(expandoObject, item.Key, item.Value);
            });
        }

        /// <summary>
        /// Adds to the expando object.
        /// </summary>
        /// <param name="expandoObject">The expando object.</param>
        /// <param name="objList">The array of System.Object.</param>
        private static void AddToExpandoObject(IDictionary<string, object> expandoObject, object[] objList)
        {
            Parallel.For(0, objList.Length, x =>
            {
                AddToExpandoObject(expandoObject, "item" + x.ToString(), objList[x]);
            });
        }
    }
}