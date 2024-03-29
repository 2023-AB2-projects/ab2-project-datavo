﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Server.Enums;

namespace Server.Models.Catalog;

[XmlType("Attribute")]
[Serializable]
public class Field
{
    [XmlIgnore]
    [Required(ErrorMessage = "Field must belong to a table!")]
    public string Table { get; set; }

    [XmlAttribute]
    [Required(ErrorMessage = "Field must have a type!")]
    public DataTypes Type { get; set; }

    [XmlAttribute]
    [Required(ErrorMessage = "Field must have a name!")]
    public string Name { get; set; }

    [XmlAttribute]
    [DefaultValue(value: -1)]
    public int IsNull { get; set; }

    [XmlAttribute]
    [DefaultValue(value: 0)]
    public int Length { get; set; }

    [XmlIgnore] public bool? IsPrimaryKey { get; set; }

    [XmlIgnore] public ForeignKey? ForeignKey { get; set; }

    [XmlIgnore] public bool? IsUnique { get; set; }

    public static Field FromMatch(Match match, string tableName)
    {
        var type = (DataTypes)Enum.Parse(typeof(DataTypes), GetTypeString(match.Groups["Type"].Value),
            ignoreCase: true);

        Field field = new()
        {
            Name = match.Groups["FieldName"].Value,
            Type = type,
            Table = tableName,
            IsPrimaryKey = !string.IsNullOrEmpty(match.Groups["PrimaryKey"]?.Value),
            IsUnique = !string.IsNullOrEmpty(match.Groups["Unique"]?.Value),
            IsNull = -1,
        };

        if (field.Type == DataTypes.Varchar)
        {
            field.Length = int.Parse(match.Groups["Length"].Value);
        }

        if (!string.IsNullOrEmpty(match.Groups["ForeignKey"]?.Value))
        {
            var refTables = match.Groups["ForeignTable"].Captures;
            var refAttributes = match.Groups["ForeignColumn"].Captures;

            List<Reference> references = new();

            for (int i = 0; i < refTables.Count && i < refAttributes.Count; ++i)
            {
                references.Add(new Reference
                {
                    ReferenceTableName = refTables[i].Value,
                    ReferenceAttributeName = refAttributes[i].Value,
                });
            }

            field.ForeignKey = new ForeignKey
            {
                AttributeName = field.Name,
                References = references,
            };
        }

        return field;
    }

    private static string GetTypeString(string type)
    {
        if (type.Contains("int"))
        {
            return "int";
        }

        if (type.Contains("float"))
        {
            return "float";
        }

        if (type.Contains("bit"))
        {
            return "bit";
        }

        if (type.Contains("date"))
        {
            return "date";
        }

        return "varchar";
    }
}