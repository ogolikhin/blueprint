using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ArtifactStore.Models;
using ArtifactStore.Repositories.VersionControl;
using ServiceLibrary.Models;
using ServiceLibrary.Models.VersionControl;
using PropertyTypeInfo = System.Tuple<int, ArtifactStore.Models.PropertyTypePredefined>;

namespace ArtifactStore.Helpers
{
    public class PublishEnvironment
    {
        public int RevisionId { get; set; }

        public DateTime Timestamp { get; set; }

        private ISet<int> AffectedArtifactIds { get; set; }

        public void AddAffectedArtifact(int artifactId)
        {
            if (AffectedArtifactIds == null)
            {
                AffectedArtifactIds = new HashSet<int>();
            }

            if (!IsArtifactDeleted(artifactId))
            {
                AffectedArtifactIds.Add(artifactId);
            }
        }

        public ISet<int> GetAffectedArtifacts()
        {
            if (AffectedArtifactIds == null)
            {
                AffectedArtifactIds = new HashSet<int>();
            }
            return AffectedArtifactIds;
        }

        public bool IsArtifactDeleted(int artifactId)
        {
            return DeletedArtifactIds?.Contains(artifactId) ?? false;
        }

        public ISet<int> DeletedArtifactIds { get; set; }
        public bool KeepLock { get; set; }

        public IDictionary<int, SqlItemInfo> ArtifactStates { get; internal set; }

        public IEnumerable<int> FilterByBaseType(IEnumerable<int> artifactIds, ItemTypePredefined baseType)
        {
            foreach (var artifactId in artifactIds)
            {
                SqlItemInfo itemInfo;
                ArtifactStates.TryGetValue(artifactId, out itemInfo);

                if (itemInfo == null || itemInfo.PrimitiveItemTypePredefined != baseType)
                {
                    continue;
                }

                yield return artifactId;
            }
        }

        public ItemTypePredefined GetArtifactBaseType(int artifactId)
        {
            SqlItemInfo itemInfo;
            ArtifactStates.TryGetValue(artifactId, out itemInfo);
            if (itemInfo == null)
            {
                return ItemTypePredefined.None;
            }

            return itemInfo.PrimitiveItemTypePredefined;
        }

        public IDictionary<int, SqlPublishResult> PublishResults { get; private set; }
        public IEnumerable<IPublishRepository> Repositories { get; internal set; }
        public ReuseSensitivityCollector SensitivityCollector { get; internal set; }

        public SqlPublishResult GetPublishResult(int artifactId)
        {
            if (PublishResults == null)
            {
                PublishResults = new Dictionary<int, SqlPublishResult>();
            }

            SqlPublishResult result;
            if (!PublishResults.TryGetValue(artifactId, out result))
            {
                result = new SqlPublishResult(artifactId);
                PublishResults.Add(artifactId, result);
            }

            return result;
        }

        public IEnumerable<SqlPublishResult> GetChangeSqlPublishResults()
        {
            if (PublishResults == null)
                return Enumerable.Empty<SqlPublishResult>();

            return PublishResults
                .Where(pair => pair.Value.Changed)
                .Select(pair => pair.Value);
        }

        internal IEnumerable<int> GetArtifactsMovedAcrossProjects(IEnumerable<int> artifactIds)
        {
            foreach (var artifactId in artifactIds)
            {
                SqlItemInfo itemInfo;
                ArtifactStates.TryGetValue(artifactId, out itemInfo);

                if (itemInfo == null || !itemInfo.MovedAcrossProjects)
                {
                    continue;
                }

                yield return artifactId;
            }
        }
    }

    public class ReuseSensitivityCollector
    {
        public ReuseSensitivityCollector()
        {
            ArtifactModifications = new Dictionary<int, ArtifactModification>();
        }

        public Dictionary<int, ArtifactModification> ArtifactModifications { get; private set; }

        public class ArtifactModification
        {
            public ItemTypeReuseTemplateSetting ArtifactAspects { get; set; }

            private readonly Lazy<HashSet<PropertyTypeInfo>> _modifiedPropertiesHolder = new Lazy<HashSet<PropertyTypeInfo>>(() => new HashSet<PropertyTypeInfo>());

            public void RegisterArtifactPropertyModification(int propertyTypeId, PropertyTypePredefined predefined)
            {
                _modifiedPropertiesHolder.Value.Add(new PropertyTypeInfo(propertyTypeId, predefined));
            }

            public IEnumerable<PropertyTypeInfo> ModifiedPropertyTypes
            {
                get
                {
                    return _modifiedPropertiesHolder.IsValueCreated
                        ? _modifiedPropertiesHolder.Value
                        : Enumerable.Empty<PropertyTypeInfo>();
                }
            }
        }

        //Reuse Sensitivity

        public void RegisterArtifactPropertyModification(int artifactItemId, int propertyTypeId, PropertyTypePredefined predefined)
        {
            var modifications = GetOrCreateArtifactModifications(artifactItemId);

            modifications.RegisterArtifactPropertyModification(propertyTypeId, predefined);
        }

        public void RegisterArtifactModification(int artifactItemId, ItemTypeReuseTemplateSetting setting)
        {
            var modifications = GetOrCreateArtifactModifications(artifactItemId);

            modifications.ArtifactAspects |= setting;
        }

        private ArtifactModification GetOrCreateArtifactModifications(int artifactItemId)
        {
            ArtifactModification modifications;

            if (ArtifactModifications.TryGetValue(artifactItemId, out modifications))
            {
                return modifications;
            }

            modifications = new ArtifactModification();
            ArtifactModifications.Add(artifactItemId, modifications);

            return modifications;
        }
    }

    [Flags]
    public enum ItemTypeReuseTemplateSetting
    {
        None = 0x0,
        Name = 0x1,
        Description = 0x2,
        ActorImage = 0x4,
        BaseActor = 0x8,
        DocumentFile = 0x10,
        DiagramHeight = 0x20,
        DiagramWidth = 0x40,
        UseCaseLevel = 0x80,
        UIMockupTheme = 0x100,
        UseCaseDiagramShowConditions = 0x200,
        Attachments = 0x400,
        DocumentReferences = 0x800,
        Relationships = 0x1000,
        Subartifacts = 0x2000
    }

    

    public enum UserType
    {
        Group,
        User
    }

    public class UserValue : IEquatable<UserValue>
    {
        private readonly IEnumerable<string> _userLabels;
        private readonly IEnumerable<KeyValuePair<int, UserType>> _userValues;

        public UserValue(IEnumerable<string> userLabels, IEnumerable<KeyValuePair<int, UserType>> userValues)
        {
            _userLabels = userLabels;
            _userValues = userValues;
        }

        public bool Equals(UserValue other)
        {
            if (other == null)
                return false;

            if (_userLabels == null)
            {
                if (other._userLabels != null)
                    return false;

            }
            else
            {
                if (other._userLabels == null)
                    return false;

                return !_userLabels.Except(other._userLabels).Any() && !other._userLabels.Except(_userLabels).Any();
            }

            if (_userValues == null)
            {
                if (other._userValues == null || !other._userValues.Any())
                    return true;
                return false;
            }

            if (other._userValues == null)
            {
                return !_userValues.Any();
            }

            return !_userValues.Except(other._userValues).Any() && !other._userValues.Except(_userValues).Any();
        }
    }

    //public class ChoiceValue : IEquatable<ChoiceValue>
    //{
    //    public ChoiceValue(string customValue, IEnumerable<int> selectedValueIds)
    //    {
    //        CustomValue = customValue;
    //        SelectedValueIds = new HashSet<int>(selectedValueIds ?? Enumerable.Empty<int>());
    //    }

    //    public string CustomValue { get; set; }
    //    public ISet<int> SelectedValueIds { get; set; }

    //    public bool HasPropertyTypeDefinition { get; set; }

    //    public static ChoiceValue CreateForType(bool allowMultiple, bool allowCustomValue, IEnumerable<int> defaultChoiceValues)
    //    {
    //        var choiceValue = new ChoiceValue(null, defaultChoiceValues)
    //        {
    //            AllowMultiple = allowMultiple,
    //            AllowCustomValue = allowCustomValue,
    //            HasPropertyTypeDefinition = true
    //        };

    //        return choiceValue;
    //    }

    //    public bool AllowCustomValue { get; set; }

    //    public bool AllowMultiple
    //    {
    //        get;
    //        set;
    //    }

    //    public void ApplyTypeDefinition(ChoiceValue typeChoiceValue)
    //    {
    //        if (typeChoiceValue == null) throw new ArgumentNullException("typeChoiceValue");

    //        if (!typeChoiceValue.HasPropertyTypeDefinition)
    //            throw new ArgumentException("Value have no type definition", "typeChoiceValue");

    //        AllowCustomValue = typeChoiceValue.AllowCustomValue;
    //        AllowMultiple = typeChoiceValue.AllowMultiple;
    //        HasPropertyTypeDefinition = true;
    //    }

    //    public bool Equals(ChoiceValue other)
    //    {
    //        if (HasPropertyTypeDefinition && other.HasPropertyTypeDefinition)
    //        {
    //            return StrongEquals(other);
    //        }

    //        var isSelectedValuesEquals = IsSelectedValuesEquals(other);
    //        if (!isSelectedValuesEquals)
    //            return false;

    //        bool compareCustomValue = HasPropertyTypeDefinition
    //                                      ? AllowCustomValue
    //                                      : other.HasPropertyTypeDefinition && other.AllowCustomValue;

    //        if (compareCustomValue)
    //        {
    //            return string.Equals(CustomValue, other.CustomValue);
    //        }

    //        return true;
    //    }

    //    private bool IsSelectedValuesEquals(ChoiceValue other)
    //    {
    //        var isSelectedValuesEquals = SelectedValueIds.Count == other.SelectedValueIds.Count
    //                                     && SelectedValueIds.Intersect(other.SelectedValueIds).Count() == SelectedValueIds.Count;
    //        return isSelectedValuesEquals;
    //    }

    //    private bool StrongEquals(ChoiceValue other)
    //    {
    //        if (SelectedValueIds.Count > 0 && other.SelectedValueIds.Count > 0)
    //        {
    //            return IsSelectedValuesEquals(other);
    //        }

    //        if (AllowCustomValue && other.AllowCustomValue)
    //        {
    //            if (CustomValue == null)
    //            {
    //                if (other.CustomValue != null)
    //                    return false;
    //            }
    //            else if (other.CustomValue == null)
    //            {
    //                return false;
    //            }
    //            else // (CustomValue != null && other.CustomValue != null)
    //            {
    //                return string.Equals(CustomValue, other.CustomValue);
    //            }
    //        }
    //        else if (AllowCustomValue && CustomValue != null || other.AllowCustomValue && other.CustomValue != null)
    //        {
    //            return false; // now we cannot compare Custom Value with some selected choice ids
    //        }


    //        return IsSelectedValuesEquals(other);
    //    }
    //}

    //internal static class DBPropertyValueHelper
    //{
    //    internal static string ReadString(object stringValue)
    //    {
    //        if (ReferenceEquals(stringValue, DBNull.Value))
    //            return null;

    //        return (string)stringValue;
    //    }

    //    internal static DateTime? ReadDateTime(object dateTimeValue)
    //    {
    //        if (ReferenceEquals(dateTimeValue, DBNull.Value))
    //            return null;

    //        return (DateTime?)dateTimeValue;
    //    }

    //    internal static decimal? ReadNumber(object dbNumber)
    //    {
    //        if (ReferenceEquals(dbNumber, DBNull.Value))
    //            return null;

    //        return ToDecimal((byte[])dbNumber);
    //    }

    //    static decimal? ToDecimal(byte[] value)
    //    {
    //        if (value == null)
    //        {
    //            return null;
    //        }
    //        int[] bits = { BitConverter.ToInt32(value, 0), BitConverter.ToInt32(value, 4), BitConverter.ToInt32(value, 8), BitConverter.ToInt32(value, 12) };
    //        return new decimal(bits);
    //    }

    //    internal static IEnumerable<int> ReadChoiceValues(object stringValue)
    //    {
    //        if (ReferenceEquals(stringValue, DBNull.Value) || string.IsNullOrEmpty(stringValue as string))
    //            yield break;

    //        var indexes = ((string)stringValue).Split(',');

    //        foreach (var s in indexes)
    //        {
    //            int id;
    //            if (int.TryParse(s, out id))
    //            {
    //                yield return id;
    //            }
    //        }
    //    }

    //    internal static int? ReadInt(object dbInteger)
    //    {
    //        if (ReferenceEquals(dbInteger, DBNull.Value))
    //            return null;

    //        return (int)dbInteger;
    //    }

    //    internal static long? ReadLong(object dbLong)
    //    {
    //        if (ReferenceEquals(dbLong, DBNull.Value))
    //            return null;

    //        return (long)dbLong;
    //    }

    //    public static bool ReadBoolean(object dbBool, bool defaultValue = false)
    //    {
    //        if (ReferenceEquals(dbBool, DBNull.Value))
    //            return defaultValue;

    //        return (bool)dbBool;
    //    }

    //    public static IEnumerable<KeyValuePair<int, UserType>> ReadUserValue(object lineSeparatedValues)
    //    {
    //        if (ReferenceEquals(lineSeparatedValues, DBNull.Value))
    //            return null;

    //        return ToKeyValuePairs((string)lineSeparatedValues);
    //    }

    //    public static IEnumerable<string> ReadUserLabel(object lineSeparatedValues)
    //    {
    //        if (ReferenceEquals(lineSeparatedValues, DBNull.Value))
    //            return null;

    //        return ToStringValues((string)lineSeparatedValues);
    //    }

    //    internal const char NewLine = '\n';
    //    internal const char GroupPrefix = 'g';

    //    static IEnumerable<KeyValuePair<int, UserType>> ToKeyValuePairs(string stringValue)
    //    {
    //        if (string.IsNullOrEmpty(stringValue))
    //        {
    //            return Enumerable.Empty<KeyValuePair<int, UserType>>();
    //        }
    //        List<KeyValuePair<int, UserType>> keyValuePairs = new List<KeyValuePair<int, UserType>>();
    //        foreach (string keyValuePair in stringValue.Split(new char[] { NewLine }, StringSplitOptions.RemoveEmptyEntries))
    //        {
    //            if (keyValuePair[0] == GroupPrefix)
    //            {
    //                keyValuePairs.Add(new KeyValuePair<int, UserType>(int.Parse(keyValuePair.Substring(1),
    //                    NumberStyles.None, NumberFormatInfo.InvariantInfo), UserType.Group));
    //            }
    //            else
    //            {
    //                keyValuePairs.Add(new KeyValuePair<int, UserType>(int.Parse(keyValuePair,
    //                    NumberStyles.None, NumberFormatInfo.InvariantInfo), UserType.User));
    //            }
    //        }
    //        return keyValuePairs;
    //    }


    //    static IEnumerable<string> ToStringValues(string stringValue)
    //    {
    //        if (string.IsNullOrEmpty(stringValue))
    //        {
    //            return Enumerable.Empty<string>();
    //        }
    //        List<string> stringValues = new List<string>();
    //        using (StringReader stringReader = new StringReader(stringValue))
    //        {
    //            string line;
    //            while ((line = stringReader.ReadLine()) != null)
    //            {
    //                stringValues.Add(line);
    //            }
    //        }
    //        return stringValues;
    //    }
    //}
}