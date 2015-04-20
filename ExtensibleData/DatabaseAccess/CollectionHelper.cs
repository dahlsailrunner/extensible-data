using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using Pocos;

namespace DatabaseAccess
{
    public class CollectionHelper
    {
        public static List<T> BuildCollection<T>(Type type, DataTable table)
        {
            var ret = new List<T>();

            //Get all the properties in Entity Class
            var props = type.GetProperties();

            foreach (DataRow row in table.Rows)
            {
                //Create new instance of Entity
                var entity = Activator.CreateInstance<T>();

                //Set all properties from the column names
                //NOTE: This assumes your column names are the same name as your class property names
                foreach (var col in props)
                {
                    if (!table.Columns.Contains(col.Name))
                        continue;
                    try
                    {
                        if (!col.PropertyType.FullName.StartsWith("System"))
                        {
                            if (string.IsNullOrEmpty(row[col.Name].ToString()))
                                continue;

                            var outputType = col.PropertyType.UnderlyingSystemType;
                            var handler = new XmlSerializer(outputType);
                            using (var reader = new StringReader(row[col.Name].ToString()))
                                col.SetValue(entity, handler.Deserialize(reader));
                        }
                        else
                        {
                            if (row[col.Name].Equals(DBNull.Value))
                            {
                                col.SetValue(entity, null);
                                continue;
                            }

                            if (col.PropertyType == typeof (bool))
                            {
                                // straight-up bool types
                                col.SetValue(entity, (row[col.Name].ToString() == "1" || row[col.Name].ToString() == bool.TrueString));
                            }
                            else if (col.PropertyType == typeof (int) || col.PropertyType == typeof (short) ||
                                     col.PropertyType == typeof (long) || col.PropertyType == typeof (double) ||
                                     col.PropertyType == typeof (decimal))
                            {
                                // numeric types (non-nullable)
                                col.SetValue(entity, string.IsNullOrEmpty(row[col.Name].ToString())
                                    ? 0
                                    : Convert.ChangeType(row[col.Name].ToString(), col.PropertyType)
                                    , null);
                            }
                            else
                            {
                                if (col.PropertyType.Name.StartsWith("Nullable"))
                                {
                                    var colType = Nullable.GetUnderlyingType(col.PropertyType);
                                    if (colType == typeof (int) || colType == typeof (short) ||
                                        colType == typeof (long) ||
                                        colType == typeof (double) || colType == typeof (decimal))
                                    {
                                        if (row[col.Name].Equals(DBNull.Value))
                                            // numeric nullables
                                            col.SetValue(entity, null, null);
                                        else
                                            col.SetValue(entity,
                                                Convert.ChangeType(row[col.Name].ToString(), colType), null);
                                    }
                                    else if (colType == typeof (bool))
                                        col.SetValue(entity, row[col.Name].ToString() == "1");
                                    else
                                    {
                                        //non-numeric nullables
                                        col.SetValue(entity,
                                            Convert.ChangeType(row[col.Name].ToString(),
                                                Nullable.GetUnderlyingType(col.PropertyType)), null);
                                    }
                                }
                                else
                                {
                                    col.SetValue(entity, col.PropertyType.IsEnum
                                        ? Enum.ToObject(col.PropertyType, Convert.ToInt32(row[col.Name].ToString()))
                                        // enum logic
                                        : Convert.ChangeType(row[col.Name].ToString(), col.PropertyType), null);
                                    // non-nullable, non-numeric, non-enums
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format("Failed building collection. Setting Property{0} to value {1}", col.Name, row[col.Name]), ex);
                    }
                }

                ret.Add(entity);
            }

            return ret;
        }

        public static void SetPropertyFromExtensibleFieldValue(object objectWithPropToSet, PropertyInfo propToSet, string fieldValue)
        {
            try
            {
                if (propToSet.PropertyType == typeof(bool)) // special handling for bools -- doesn't like string to bool implicit conversion
                {
                    // straight-up bool types
                    propToSet.SetValue(objectWithPropToSet, fieldValue == "1");
                }
                else if (propToSet.PropertyType == typeof(int) || propToSet.PropertyType == typeof(short) ||
                         propToSet.PropertyType == typeof(long) || propToSet.PropertyType == typeof(float) ||
                         propToSet.PropertyType == typeof(double) || propToSet.PropertyType == typeof(decimal))
                {
                    // numeric types (non-nullable)
                    propToSet.SetValue(objectWithPropToSet, string.IsNullOrEmpty(fieldValue)
                                                                    ? 0
                                                                    : Convert.ChangeType(fieldValue, propToSet.PropertyType), null);
                }
                else
                {
                    if (propToSet.PropertyType.Name.StartsWith("Nullable"))
                    {
                        var type = Nullable.GetUnderlyingType(propToSet.PropertyType);
                        if (string.IsNullOrEmpty(fieldValue))
                        {
                            // type is nullable and value we are setting is null (or empty) and underlying type is not string                            
                            propToSet.SetValue(objectWithPropToSet, (type == typeof(string) ? fieldValue : null), null);
                        }
                        else
                        {
                            // nullable type with actual value
                            propToSet.SetValue(objectWithPropToSet,
                                type.IsEnum ? Enum.ToObject(type, Convert.ToInt32(fieldValue)) : Convert.ChangeType(fieldValue, type), null);
                        }
                    }
                    else
                    {
                        // non-nullable type should have actual value and not be empty string
                        propToSet.SetValue(objectWithPropToSet,
                            propToSet.PropertyType.IsEnum ? Enum.ToObject(propToSet.PropertyType, Convert.ToInt32(fieldValue)) :
                                                            Convert.ChangeType(fieldValue, propToSet.PropertyType), null); // non-nullable, non-numeric, non-enums
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed setting property from attribute value. Setting Property [{0}] from attribute value [{1}]", propToSet.Name, fieldValue), ex);
            }
        }

        public static void UpdateObjectFromExtensibleData(List<ExtensibleDataItem> dataFields, object contact)
        {
            foreach (var prop in contact.GetType().GetProperties())
            {
                var attr = (DataFieldAttribute)Attribute.GetCustomAttribute(prop, typeof(DataFieldAttribute));
                if (attr != null)
                {
                    var dataItem = dataFields.FirstOrDefault(a => a.FieldName == attr.FieldName);
                    if (dataItem != null)
                        SetPropertyFromExtensibleFieldValue(contact, prop, dataItem.FieldValue);
                }
            }
        }
        public static string GetExtensibleDataAsXml(object objectWithExtensibleData)
        {
            var gotSomeXml = false;
            XElement results = null;
            var props = objectWithExtensibleData.GetType().GetProperties();
            foreach (var prop in props)
            {
                var fldAttr = (DataFieldAttribute)Attribute.GetCustomAttribute(prop, typeof(DataFieldAttribute));
                if (fldAttr == null) continue;

                if (!gotSomeXml)
                {
                    results = new XElement("ExtensibleFields");
                    gotSomeXml = true;
                }

                var val = prop.GetValue(objectWithExtensibleData);

                if (val == null)
                    val = "";
                else if (val is Enum)
                    val = ((int)val).ToString(CultureInfo.InvariantCulture);
                else if (val is bool)
                    val = ((bool)val) ? "1" : "0";
                else if (val is DateTime)
                    val = ((DateTime)val).ToShortDateString();

                results.Add(new XElement("DataElement", new XElement("FieldName", fldAttr.FieldName),
                                                        new XElement("FieldValue", val.ToString())));
            }
            return results == null ? "" : results.ToString();
        }
    }
}
