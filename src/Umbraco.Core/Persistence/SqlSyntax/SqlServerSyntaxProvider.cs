﻿using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Static class that provides simple access to the Sql Server SqlSyntax Providers singleton
    /// </summary>
    internal static class SqlServerSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return SqlServerSyntaxProvider.Instance; } }
    }

    /// <summary>
    /// Represents an SqlSyntaxProvider for Sql Server
    /// </summary>
    internal class SqlServerSyntaxProvider : SqlSyntaxProviderBase<SqlServerSyntaxProvider>
    {
        public static SqlServerSyntaxProvider Instance = new SqlServerSyntaxProvider();

        private SqlServerSyntaxProvider()
        {
            StringLengthColumnDefinitionFormat = StringLengthUnicodeColumnDefinitionFormat;
            StringColumnDefinition = string.Format(StringLengthColumnDefinitionFormat, DefaultStringLength);

            AutoIncrementDefinition = "IDENTITY(1,1)";
            StringColumnDefinition = "VARCHAR(8000)";
            GuidColumnDefinition = "UniqueIdentifier";
            RealColumnDefinition = "FLOAT";
            BoolColumnDefinition = "BIT";
            DecimalColumnDefinition = "DECIMAL(38,6)";
            TimeColumnDefinition = "TIME"; //SQLSERVER 2008+
            BlobColumnDefinition = "VARBINARY(MAX)";

            InitColumnTypeMap();
        }

        public override string GetQuotedTableName(string tableName)
        {
            return string.Format("[{0}]", tableName);
        }

        public override string GetQuotedColumnName(string columnName)
        {
            return string.Format("[{0}]", columnName);
        }

        public override string GetQuotedName(string name)
        {
            return string.Format("[{0}]", name);
        }

        public override bool DoesTableExist(Database db, string tableName)
        {
            var result =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName",
                                       new { TableName = tableName });

            return result > 0;
        }

        public override string FormatColumnRename(string tableName, string oldName, string newName)
        {
            return string.Format(RenameColumn, tableName, oldName, newName);
        }

        public override string FormatTableRename(string oldName, string newName)
        {
            return string.Format(RenameTable, oldName, newName);
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
        }

        private static string GetIdentityString(ColumnDefinition column)
        {
            return "IDENTITY(1,1)";
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
                return "DECLARE @default sysname, @sql nvarchar(max);\r\n\r\n" +
                    "-- get name of default constraint\r\n" +
                    "SELECT @default = name\r\n" +
                    "FROM sys.default_constraints\r\n" +
                    "WHERE parent_object_id = object_id('{0}')\r\n" + "" +
                    "AND type = 'D'\r\n" + "" +
                    "AND parent_column_id = (\r\n" + "" +
                    "SELECT column_id\r\n" +
                    "FROM sys.columns\r\n" +
                    "WHERE object_id = object_id('{0}')\r\n" +
                    "AND name = '{1}'\r\n" +
                    ");\r\n\r\n" +
                    "-- create alter table command to drop contraint as string and run it\r\n" +
                    "SET @sql = N'ALTER TABLE {0} DROP CONSTRAINT ' + @default;\r\n" +
                    "EXEC sp_executesql @sql;";
            }
        }

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }

        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

        public override string RenameColumn { get { return "sp_rename '{0}.{1}', '{2}', 'COLUMN'"; } }

        public override string RenameTable { get { return "sp_rename '{0}', '{1}'"; } }
    }
}