﻿using System;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions;

namespace Umbraco.Core.Persistence
{
    public static class PetaPocoExtensions
    {
        internal delegate void CreateTableEventHandler(string tableName, Database db, TableCreationEventArgs e);

        internal static event CreateTableEventHandler NewTable;

        public static void CreateTable<T>(this Database db)
           where T : new()
        {
            var tableType = typeof(T);
            CreateTable(db, false, tableType);
        }

        public static void CreateTable<T>(this Database db, bool overwrite)
           where T : new()
        {
            var tableType = typeof(T);
            CreateTable(db, overwrite, tableType);
        }

        public static void CreateTable(this Database db, bool overwrite, Type modelType)
        {
            var tableDefinition = DefinitionFactory.GetTableDefinition(modelType);
            var tableName = tableDefinition.TableName;

            string createSql = SyntaxConfig.SqlSyntaxProvider.ToCreateTableStatement(tableDefinition);
            string createPrimaryKeySql = SyntaxConfig.SqlSyntaxProvider.ToCreatePrimaryKeyStatement(tableDefinition);
            var foreignSql = SyntaxConfig.SqlSyntaxProvider.ToCreateForeignKeyStatements(tableDefinition);
            var indexSql = SyntaxConfig.SqlSyntaxProvider.ToCreateIndexStatements(tableDefinition);

#if DEBUG
            Console.WriteLine(createSql);
            Console.WriteLine(createPrimaryKeySql);
            foreach (var sql in foreignSql)
            {
                Console.WriteLine(sql);
            }
            foreach (var sql in indexSql)
            {
                Console.WriteLine(sql);
            }
#endif

            var tableExist = db.TableExist(tableName);
            if (overwrite && tableExist)
            {
                db.DropTable(tableName);
            }

            if (!tableExist)
            {
                int created = db.Execute(new Sql(createSql));

                if (NewTable != null)
                {
                    var e = new TableCreationEventArgs();
                    NewTable(tableName, db, e);
                }

                //TODO Figure out how to deal with base data before/after db and constraint creation
                //Possibly add an internal task to trigger the data creation prior to creating constraints?
                
                if(!string.IsNullOrEmpty(createPrimaryKeySql))
                    db.Execute(new Sql(createPrimaryKeySql));
                
                foreach (var sql in foreignSql)
                {
                    int createdFk = db.Execute(new Sql(sql));
                }
                foreach (var sql in indexSql)
                {
                    int createdIndex = db.Execute(new Sql(sql));
                }
            }
        }

        public static void DropTable<T>(this Database db)
            where T : new()
        {
            Type type = typeof (T);
            var tableNameAttribute = type.FirstAttribute<TableNameAttribute>();
            if (tableNameAttribute == null)
                throw new Exception(
                    string.Format(
                        "The Type '{0}' does not contain a TableNameAttribute, which is used to find the name of the table to drop. The operation could not be completed.",
                        type.Name));

            string tableName = tableNameAttribute.Value;
            DropTable(db, tableName);
        }

        public static void DropTable(this Database db, string tableName)
        {
            var sql = new Sql(string.Format("DROP TABLE {0}", SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(tableName)));
            db.Execute(sql);
        }

        public static bool TableExist(this Database db, string tableName)
        {
            return SyntaxConfig.SqlSyntaxProvider.DoesTableExist(db, tableName);
        }

        public static void Initialize(this Database db)
        {
            NewTable += PetaPocoExtensions_NewTable;

            var creation = new DatabaseCreation(db);
            creation.InitializeDatabaseSchema();
        }

        static void PetaPocoExtensions_NewTable(string tableName, Database db, TableCreationEventArgs e)
        {
            var baseDataCreation = new BaseDataCreation(db);
            baseDataCreation.InitializeBaseData(tableName);
        }
    }

    internal class TableCreationEventArgs : System.ComponentModel.CancelEventArgs{}
}