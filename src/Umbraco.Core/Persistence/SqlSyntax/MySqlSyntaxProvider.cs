﻿using System;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Static class that provides simple access to the MySql SqlSyntax Providers singleton
    /// </summary>
    internal static class MySqlSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return MySqlSyntaxProvider.Instance; } }
    }

    /// <summary>
    /// Represents an SqlSyntaxProvider for MySql
    /// </summary>
    internal class MySqlSyntaxProvider : SqlSyntaxProviderBase<MySqlSyntaxProvider>
    {
        public static MySqlSyntaxProvider Instance = new MySqlSyntaxProvider();

        private MySqlSyntaxProvider()
        {
            DefaultStringLength = 255;
            StringLengthColumnDefinitionFormat = StringLengthUnicodeColumnDefinitionFormat;
            StringColumnDefinition = string.Format(StringLengthColumnDefinitionFormat, DefaultStringLength);

            AutoIncrementDefinition = "AUTO_INCREMENT";
            IntColumnDefinition = "int(11)";
            BoolColumnDefinition = "tinyint(1)";
            DateTimeColumnDefinition = "TIMESTAMP";
            TimeColumnDefinition = "time";
            DecimalColumnDefinition = "decimal(38,6)";
            GuidColumnDefinition = "char(36)";
            
            InitColumnTypeMap();

            DefaultValueFormat = "DEFAULT '{0}'";
        }

        public override bool DoesTableExist(Database db, string tableName)
        {
            db.OpenSharedConnection();
            var result =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_NAME = @TableName AND " +
                "TABLE_SCHEMA = @TableSchema", new { TableName = tableName, TableSchema = db.Connection.Database });

            return result > 0;
        }

        public override bool SupportsClustered()
        {
            return true;
        }

        public override bool SupportsIdentityInsert()
        {
            return false;
        }

        public override string GetQuotedTableName(string tableName)
        {
            return string.Format("`{0}`", tableName);
        }

        public override string GetQuotedColumnName(string columnName)
        {
            return string.Format("`{0}`", columnName);
        }

        public override string GetQuotedName(string name)
        {
            return string.Format("`{0}`", name);
        }

        public override string GetSpecialDbType(SpecialDbTypes dbTypes)
        {
            if (dbTypes == SpecialDbTypes.NCHAR)
            {
                return "CHAR";
            }
            else if (dbTypes == SpecialDbTypes.NTEXT)
                return "LONGTEXT";

            return "NVARCHAR";
        }

        public override string Format(TableDefinition table)
        {
            string primaryKey = string.Empty;
            var columnDefinition = table.Columns.FirstOrDefault(x => x.IsPrimaryKey);
            if (columnDefinition != null && columnDefinition.PrimaryKeyColumns.Contains(",") == false)
            {
                string columns = string.IsNullOrEmpty(columnDefinition.PrimaryKeyColumns)
                                 ? GetQuotedColumnName(columnDefinition.Name)
                                 : string.Join(", ", columnDefinition.PrimaryKeyColumns
                                                                     .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(GetQuotedColumnName));

                primaryKey = string.Format(", \nPRIMARY KEY {0} ({1})", columnDefinition.IsIndexed ? "CLUSTERED" : "NONCLUSTERED", columns);
            }

            var statement = string.Format(CreateTable, GetQuotedTableName(table.Name), Format(table.Columns), primaryKey);

            return statement;
        }

        public override string Format(IndexDefinition index)
        {
            string name = string.IsNullOrEmpty(index.Name)
                                  ? string.Format("IX_{0}_{1}", index.TableName, index.ColumnName)
                                  : index.Name;

            string columns = index.Columns.Any()
                                 ? string.Join(",", index.Columns.Select(x => GetQuotedColumnName(x.Name)))
                                 : GetQuotedColumnName(index.ColumnName);

            return string.Format(CreateIndex,
                                 GetQuotedName(name),
                                 GetQuotedTableName(index.TableName), 
                                 columns);
        }

        public override string Format(ForeignKeyDefinition foreignKey)
        {
            return string.Format(CreateForeignKeyConstraint,
                                 GetQuotedTableName(foreignKey.ForeignTable),
                                 GetQuotedColumnName(foreignKey.ForeignColumns.First()),
                                 GetQuotedTableName(foreignKey.PrimaryTable),
                                 GetQuotedColumnName(foreignKey.PrimaryColumns.First()),
                                 FormatCascade("DELETE", foreignKey.OnDelete),
                                 FormatCascade("UPDATE", foreignKey.OnUpdate));
        }

        public override string FormatPrimaryKey(TableDefinition table)
        {
            return string.Empty;
        }

        protected override string FormatConstraint(ColumnDefinition column)
        {
            return string.Empty;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? AutoIncrementDefinition : string.Empty;
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            if (column.DefaultValue == null)
                return string.Empty;

            // see if this is for a system method
            if (column.DefaultValue is SystemMethods)
            {
                string method = FormatSystemMethods((SystemMethods)column.DefaultValue);
                if (string.IsNullOrEmpty(method))
                    return string.Empty;

                return string.Format(DefaultValueFormat, method);
            }

            if (column.DefaultValue.ToString().ToLower().Equals("getdate()".ToLower()))
                return "DEFAULT CURRENT_TIMESTAMP";

            return string.Format(DefaultValueFormat, column.DefaultValue);
        }

        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            return string.Empty;
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return "NEWID()";
                case SystemMethods.NewSequentialId:
                    return "NEWSEQUENTIALID()";
                case SystemMethods.CurrentDateTime:
                    return "GETDATE()";
                case SystemMethods.CurrentUTCDateTime:
                    return "GETUTCDATE()";
            }

            return null;
        }

        public override string DeleteDefaultConstraint
        {
            get
            {
                throw new NotSupportedException("Default constraints are not supported in MySql");
            }
        }

        public override string AlterColumn { get { return "ALTER TABLE {0} MODIFY COLUMN {1}"; } }

        //CREATE TABLE {0} ({1}) ENGINE = INNODB versus CREATE TABLE {0} ({1}) ENGINE = MYISAM ?
        public override string CreateTable { get { return "CREATE TABLE {0} ({1}{2})"; } }

        public override string CreateIndex { get { return "CREATE INDEX {0} ON {1} ({2})"; } }

        public override string CreateForeignKeyConstraint { get { return "ALTER TABLE {0} ADD FOREIGN KEY ({1}) REFERENCES {2} ({3}){4}{5}"; } }

        public override string DeleteConstraint { get { return "ALTER TABLE {0} DROP {1}{2}"; } }

        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

        public override string RenameColumn { get { return "ALTER TABLE {0} CHANGE {1} {2}"; } }
    }
}