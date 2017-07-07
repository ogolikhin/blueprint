using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.Models;
using ArtifactStore.Models.Reuse;
using ServiceLibrary.Models;

namespace ArtifactStore.Helpers
{
    public interface IReuseSystemPropertiesMap
    {
        IEnumerable<PropertyTypePredefined> GetPropertyTypePredefined(ReconcileProperty reconcileProperty,
            ItemTypePredefined baseItemTypePredefined);

        IList<PropertyTypePredefined> GetAssociatedPropertyTypePredefined(ItemTypePredefined baseItemTypePredefined);
        IList<ReconcileProperty> GetAssociatedReconcileProperties(ItemTypePredefined baseItemTypePredefined);
        IList<ReconcileProperty> GetNotDefinedReconcileProperties(ItemTypePredefined baseItemTypePredefined);

        bool IsInternalSystemProperty(PropertyTypePredefined propertyTypePredefined,
            ItemTypePredefined itemTypePredefined);

        ReconcileProperty GetCorrespondingReconcileProperty(PropertyTypePredefined propertyTypePredefined, ItemTypePredefined artifacTypePredefined);
    }

    public class ReuseSystemPropertiesMap : IReuseSystemPropertiesMap
    {
        public static ReuseSystemPropertiesMap Instance { get; } = Init(new[]
        {
            AllTypesEntry(ReconcileProperty.Name, PropertyTypePredefined.Name),

            Entry(ReconcileProperty.DiagramWidth, ItemTypePredefined.GenericDiagram, PropertyTypePredefined.Width),
            Entry(ReconcileProperty.DiagramHeight, ItemTypePredefined.GenericDiagram, PropertyTypePredefined.Height),

            Entry(ReconcileProperty.DiagramWidth, ItemTypePredefined.BusinessProcess, PropertyTypePredefined.Width),
            Entry(ReconcileProperty.DiagramHeight, ItemTypePredefined.BusinessProcess, PropertyTypePredefined.Height),

            Entry(ReconcileProperty.DiagramWidth, ItemTypePredefined.UIMockup, PropertyTypePredefined.Width),
            Entry(ReconcileProperty.DiagramHeight, ItemTypePredefined.UIMockup, PropertyTypePredefined.Height),
            Entry(ReconcileProperty.UIMockupTheme, ItemTypePredefined.UIMockup, PropertyTypePredefined.Theme),

            Entry(ReconcileProperty.DiagramWidth, ItemTypePredefined.DomainDiagram, PropertyTypePredefined.Width),
            Entry(ReconcileProperty.DiagramHeight, ItemTypePredefined.DomainDiagram, PropertyTypePredefined.Height),

            Entry(ReconcileProperty.DiagramWidth, ItemTypePredefined.Storyboard, PropertyTypePredefined.Width),
            Entry(ReconcileProperty.DiagramHeight, ItemTypePredefined.Storyboard, PropertyTypePredefined.Height),

            Entry(ReconcileProperty.DiagramWidth, ItemTypePredefined.UseCaseDiagram, PropertyTypePredefined.Width),
            Entry(ReconcileProperty.DiagramHeight, ItemTypePredefined.UseCaseDiagram, PropertyTypePredefined.Height),
            Entry(ReconcileProperty.UseCaseDiagramShowConditions, ItemTypePredefined.UseCaseDiagram, PropertyTypePredefined.RawData),

            Entry(ReconcileProperty.DocumentFileName, ItemTypePredefined.Document, PropertyTypePredefined.RawData),

            MultipleEntry(ReconcileProperty.ActorImageName, ItemTypePredefined.Actor, PropertyTypePredefined.RawData, PropertyTypePredefined.Image),

            Entry(ReconcileProperty.UseCaseLevel, ItemTypePredefined.UseCase, PropertyTypePredefined.UseCaseLevel),
        },
            new[]
            {
                Entry(ReconcileProperty.ActorBase, ItemTypePredefined.Actor, PropertyTypePredefined.None),
            });

        private Dictionary<MapKey, IEnumerable<PropertyTypePredefined>> _typePredefineds;
        private Dictionary<MapKey, IEnumerable<PropertyTypePredefined>> _typeNonDefinedReconciliationProperties;

        private class MapKey : Tuple<ReconcileProperty, ItemTypePredefined?>
        {
            public MapKey(ReconcileProperty item1, ItemTypePredefined? item2)
                : base(item1, item2)
            {
            }
        }

        private static ReuseSystemPropertiesMap Init(IEnumerable<KeyValuePair<MapKey, IEnumerable<PropertyTypePredefined>>> mapping, IEnumerable<KeyValuePair<MapKey, IEnumerable<PropertyTypePredefined>>> nonDefinedMapping)
        {
            var map = new ReuseSystemPropertiesMap
            {
                _typePredefineds = new Dictionary<MapKey, IEnumerable<PropertyTypePredefined>>(),
                _typeNonDefinedReconciliationProperties = new Dictionary<MapKey, IEnumerable<PropertyTypePredefined>>()
            };

            mapping.ForEach(p => map._typePredefineds.Add(p.Key, p.Value));
            nonDefinedMapping.ForEach(p => map._typeNonDefinedReconciliationProperties.Add(p.Key, p.Value));
            return map;
        }

        public IEnumerable<PropertyTypePredefined> GetPropertyTypePredefined(ReconcileProperty reconcileProperty,
            ItemTypePredefined baseItemTypePredefined)
        {
            IEnumerable<PropertyTypePredefined> propertyTypePredefined;
            if (_typePredefineds.TryGetValue(new MapKey(reconcileProperty, baseItemTypePredefined), out propertyTypePredefined))
                return propertyTypePredefined;

            if (_typePredefineds.TryGetValue(new MapKey(reconcileProperty, null), out propertyTypePredefined))
                return propertyTypePredefined;

            return Enumerable.Empty<PropertyTypePredefined>();
        }

        public IList<PropertyTypePredefined> GetAssociatedPropertyTypePredefined(ItemTypePredefined baseItemTypePredefined)
        {
            var result = new List<PropertyTypePredefined>();
            foreach (ReconcileProperty reconcileProperty in Enum.GetValues(typeof(ReconcileProperty)))
            {
                if (reconcileProperty == ReconcileProperty.None)
                {
                    continue;
                }
                IEnumerable<PropertyTypePredefined> propertyTypePredefined;
                if (_typePredefineds.TryGetValue(new MapKey(reconcileProperty, baseItemTypePredefined), out propertyTypePredefined))
                    result.AddRange(propertyTypePredefined);

                if (_typePredefineds.TryGetValue(new MapKey(reconcileProperty, null), out propertyTypePredefined))
                    result.AddRange(propertyTypePredefined);
            }
            return result;
        }

        public IList<ReconcileProperty> GetAssociatedReconcileProperties(ItemTypePredefined baseItemTypePredefined)
        {
            var result = new List<ReconcileProperty>();
            foreach (ReconcileProperty reconcileProperty in Enum.GetValues(typeof(ReconcileProperty)))
            {
                if (reconcileProperty == ReconcileProperty.None)
                {
                    continue;
                }
                IEnumerable<PropertyTypePredefined> propertyTypePredefined;
                if (_typePredefineds.TryGetValue(new MapKey(reconcileProperty, baseItemTypePredefined), out propertyTypePredefined))
                    result.Add(reconcileProperty);

                if (_typePredefineds.TryGetValue(new MapKey(reconcileProperty, null), out propertyTypePredefined))
                    result.Add(reconcileProperty);
            }
            return result;
        }

        public IList<ReconcileProperty> GetNotDefinedReconcileProperties(ItemTypePredefined baseItemTypePredefined)
        {
            var result = new List<ReconcileProperty>();
            foreach (ReconcileProperty reconcileProperty in Enum.GetValues(typeof(ReconcileProperty)))
            {
                if (reconcileProperty == ReconcileProperty.None)
                {
                    continue;
                }
                IEnumerable<PropertyTypePredefined> propertyTypePredefined;
                if (_typeNonDefinedReconciliationProperties.TryGetValue(new MapKey(reconcileProperty, baseItemTypePredefined), out propertyTypePredefined))
                    result.Add(reconcileProperty);

                if (_typeNonDefinedReconciliationProperties.TryGetValue(new MapKey(reconcileProperty, null), out propertyTypePredefined))
                    result.Add(reconcileProperty);
            }
            return result;
        }

        public bool IsInternalSystemProperty(PropertyTypePredefined propertyTypePredefined,
            ItemTypePredefined itemTypePredefined)
        {
            if (propertyTypePredefined == PropertyTypePredefined.Description)
                return false;

            return
                _typePredefineds.Where(pair => pair.Value.Contains(propertyTypePredefined))
                    .Select(pair => pair.Key)
                    .All(mapKey => mapKey.Item2 != null && mapKey.Item2 != itemTypePredefined);
        }

        private static KeyValuePair<MapKey, IEnumerable<PropertyTypePredefined>> AllTypesEntry(
            ReconcileProperty reconcileProperty, PropertyTypePredefined propertyTypePredefined)
        {
            return new KeyValuePair<MapKey, IEnumerable<PropertyTypePredefined>>(new MapKey(reconcileProperty, null), Enumerable.Repeat(propertyTypePredefined, 1));
        }

        private static KeyValuePair<MapKey, IEnumerable<PropertyTypePredefined>> Entry(
            ReconcileProperty reconcileProperty, ItemTypePredefined itemTypePredefined, PropertyTypePredefined propertyTypePredefined)
        {
            return new KeyValuePair<MapKey, IEnumerable<PropertyTypePredefined>>(new MapKey(reconcileProperty, itemTypePredefined), Enumerable.Repeat(propertyTypePredefined, 1));
        }

        private static KeyValuePair<MapKey, IEnumerable<PropertyTypePredefined>> MultipleEntry(
            ReconcileProperty reconcileProperty,
            ItemTypePredefined itemTypePredefined,
            params PropertyTypePredefined[] propertyTypePredefineds)
        {
            return new KeyValuePair<MapKey, IEnumerable<PropertyTypePredefined>>(new MapKey(reconcileProperty, itemTypePredefined), propertyTypePredefineds);
        }

        public ReconcileProperty GetCorrespondingReconcileProperty(PropertyTypePredefined propertyTypePredefined, ItemTypePredefined artifacTypePredefined)
        {
            if (propertyTypePredefined == PropertyTypePredefined.Name)
                return ReconcileProperty.Name;

            return _typePredefineds
                .Where(pair => pair.Key.Item2 == artifacTypePredefined && pair.Value.Contains(propertyTypePredefined))
                .Select(pair => pair.Key.Item1)
                .FirstOrDefault();
        }
    }
}