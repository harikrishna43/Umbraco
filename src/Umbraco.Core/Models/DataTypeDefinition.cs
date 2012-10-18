﻿using System;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Definition of a DataType/PropertyEditor
    /// </summary>
    /// <remarks>
    /// The definition exists as a database reference between an actual DataType/PropertyEditor 
    /// (identified by its control id), its prevalues (configuration) and the named DataType in the backoffice UI.
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DataTypeDefinition : Entity, IAggregateRoot
    {
        private int _parentId;
        private string _name;
        private int _sortOrder;
        private int _level;
        private string _path;
        private int _userId;
        private bool _trashed;
        private Guid _controlId;
        private DataTypeDatabaseType _databaseType;

        public DataTypeDefinition(Guid controlId)
        {
            _controlId = controlId;
        }

        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, string>(x => x.Name);
        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, int>(x => x.ParentId);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, int>(x => x.SortOrder);
        private static readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, int>(x => x.Level);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, string>(x => x.Path);
        private static readonly PropertyInfo UserIdSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, int>(x => x.UserId);
        private static readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, bool>(x => x.Trashed);
        private static readonly PropertyInfo ControlIdSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, Guid>(x => x.ControlId);
        private static readonly PropertyInfo DatabaseTypeSelector = ExpressionHelper.GetPropertyInfo<DataTypeDefinition, DataTypeDatabaseType>(x => x.DatabaseType);

        /// <summary>
        /// Gets or sets the Id of the Parent entity
        /// </summary>
        /// <remarks>Might not be necessary if handled as a relation?</remarks>
        [DataMember]
        public int ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged(ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the name of the current entity
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
        /// Gets or sets the sort order of the content entity
        /// </summary>
        [DataMember]
        public int SortOrder
        {
            get { return _sortOrder; }
            set
            {
                _sortOrder = value;
                OnPropertyChanged(SortOrderSelector);
            }
        }

        /// <summary>
        /// Gets or sets the level of the content entity
        /// </summary>
        [DataMember]
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                OnPropertyChanged(LevelSelector);
            }
        }

        /// <summary>
        /// Gets or sets the path
        /// </summary>
        [DataMember]
        public string Path //Setting this value should be handled by the class not the user
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged(PathSelector);
            }
        }

        /// <summary>
        /// Id of the user who created this entity
        /// </summary>
        [DataMember]
        public int UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged(UserIdSelector);
            }
        }

        /// <summary>
        /// Boolean indicating whether this entity is Trashed or not.
        /// </summary>
        [DataMember]
        public bool Trashed
        {
            get { return _trashed; }
            internal set
            {
                _trashed = value;
                OnPropertyChanged(TrashedSelector);
            }
        }

        /// <summary>
        /// Id of the DataType control
        /// </summary>
        [DataMember]
        public Guid ControlId
        {
            get { return _controlId; }
            private set 
            { 
                _controlId = value;
                OnPropertyChanged(ControlIdSelector);
            }
        }

        /// <summary>
        /// Gets or Sets the DatabaseType for which the DataType's value is saved as
        /// </summary>
        [DataMember]
        public DataTypeDatabaseType DatabaseType
        {
            get { return _databaseType; }
            set 
            { 
                _databaseType = value;
                OnPropertyChanged(DatabaseTypeSelector);
            }
        }
    }
}