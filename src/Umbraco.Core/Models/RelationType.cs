using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a RelationType
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class RelationType : Entity, IAggregateRoot
    {
        private string _name;
        private string _alias;
        private bool _isBidrectional;
        private Guid _parentObjectType;
        private Guid _childObjectType;

        public RelationType(Guid childObjectType, Guid parentObjectType, string @alias)
        {
            _childObjectType = childObjectType;
            _parentObjectType = parentObjectType;
            _alias = alias;
        }

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<RelationType, string>(x => x.Name);
        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<RelationType, string>(x => x.Alias);
        private static readonly PropertyInfo IsBidirectionalSelector = ExpressionHelper.GetPropertyInfo<RelationType, bool>(x => x.IsBidirectional);
        private static readonly PropertyInfo ParentObjectTypeSelector = ExpressionHelper.GetPropertyInfo<RelationType, Guid>(x => x.ParentObjectType);
        private static readonly PropertyInfo ChildObjectTypeSelector = ExpressionHelper.GetPropertyInfo<RelationType, Guid>(x => x.ChildObjectType);

        /// <summary>
        /// Gets or sets the Name of the RelationType
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(NameSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Alias of the RelationType
        /// </summary>
        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set
            {
                _alias = value;
                OnPropertyChanged(AliasSelector);
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the RelationType is Bidirectional (true) or Parent to Child (false)
        /// </summary>
        [DataMember]
        public bool IsBidirectional
        {
            get { return _isBidrectional; }
            set
            {
                _isBidrectional = value;
                OnPropertyChanged(IsBidirectionalSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Parents object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid ParentObjectType
        {
            get { return _parentObjectType; }
            set
            {
                _parentObjectType = value;
                OnPropertyChanged(ParentObjectTypeSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Childs object type id
        /// </summary>
        /// <remarks>Corresponds to the NodeObjectType in the umbracoNode table</remarks>
        [DataMember]
        public Guid ChildObjectType
        {
            get { return _childObjectType; }
            set
            {
                _childObjectType = value;
                OnPropertyChanged(ChildObjectTypeSelector);
            }
        }
    }
}