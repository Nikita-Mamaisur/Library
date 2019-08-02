using System;
using System.Collections.Generic;
using Terrasoft.Core;
using Terrasoft.Core.DB;
using Terrasoft.Core.Entities;

namespace ClassLibrary
{
    public class ValueHandler
    {
        private UserConnection _userConnection;

        public ValueHandler(UserConnection uc)
        {
            _userConnection = uc;
        }

        public string GetName(Guid contactId)
        {
            var query = new Select(_userConnection)
                    .Column("Name")
                .From("Contact")
                .Where("Id")
                     .IsEqual(Column.Parameter(contactId)) as Select;

            var contactName = query.ExecuteScalar<string>();

            return contactName;
        }
        public string GetEntry(Guid EntryId)
        {
            var query = new Select(_userConnection)
                    .Column("UsrName")
                    .Column("UsrNotes")
                    .Column("UsrDate2")
                    .Column("UsrDate3")
                    .Column("UsrString2")
                    .Column("UsrString3")
                    .Column("UsrString4")
                    .Column("UsrString5")
                    .Column("UsrFloat2")
                    .Column("UsrTypeCarId")
                    .Column("UsrBoolean2")
                .From("UsrNewCar")
                .Where("Id")
                    .IsEqual(Column.Parameter(EntryId)) as Select;


            var newEntry = new InsertSelect(_userConnection)
                .Into("UsrNewCar")
                    .Set("UsrName")
                    .Set("UsrNotes")
                    .Set("UsrDate2")
                    .Set("UsrDate3")
                    .Set("UsrString2")
                    .Set("UsrString3")
                    .Set("UsrString4")
                    .Set("UsrString5")
                    .Set("UsrFloat2")
                    .Set("UsrTypeCarId")
                    .Set("UsrBoolean2")
                .FromSelect(query);

            newEntry.Execute();


            var DateNewEntry = new Select(_userConnection)
                    .Column("CreatedOn")
                .From("UsrNewCar")
                .OrderByDesc("CreatedOn") as Select;
                var maxDate = DateNewEntry.ExecuteScalar<DateTime>();

            var selectEntryWithMaxDate = new Select(_userConnection)
                    .Column("Id")
                .From("UsrNewCar")
                .Where("CreatedOn")
                    .IsEqual(Column.Parameter(maxDate)) as Select;
            var idNewEntry = selectEntryWithMaxDate.ExecuteScalar<string>();

            var selectDetail = new Select(_userConnection)
                    .Column("UsrContactId")
                    .Column(Column.Const(idNewEntry))
                .From("Usrseller")
                .Where("UsrSoldCarId")
                    .IsEqual(Column.Parameter(EntryId)) as Select;

            var insertEntry = new InsertSelect(_userConnection)
                .Into("Usrseller")
                    .Set("UsrContactId", "UsrSoldCarId")
                .FromSelect(selectDetail);
            insertEntry.Execute();
            return string.Empty;
        }
        public string GetEntryESM(Guid EntryId)
        {
            var gistManager = _userConnection.EntitySchemaManager.GetInstanceByName("UsrNewCar");
            var entityDetail = _userConnection.EntitySchemaManager.GetInstanceByName("Usrseller");
            var randomId =  Guid.NewGuid();
            var fieldFrom = gistManager.CreateEntity(_userConnection);
            var fieldTo = gistManager.CreateEntity(_userConnection);

            bool result = fieldFrom.FetchFromDB(EntryId);
            if (result)
            {
                fieldTo.SetColumnValue("Id", randomId);
                fieldTo.SetColumnValue("UsrName", fieldFrom.GetColumnValue("UsrName"));
                fieldTo.SetColumnValue("UsrNotes", fieldFrom.GetColumnValue("UsrNotes"));
                fieldTo.SetColumnValue("UsrDate2", fieldFrom.GetColumnValue("UsrDate2"));
                fieldTo.SetColumnValue("UsrDate3", fieldFrom.GetColumnValue("UsrDate3"));
                fieldTo.SetColumnValue("UsrString2", fieldFrom.GetColumnValue("UsrString2"));
                fieldTo.SetColumnValue("UsrString3", fieldFrom.GetColumnValue("UsrString3"));
                fieldTo.SetColumnValue("UsrString4", fieldFrom.GetColumnValue("UsrString4"));
                fieldTo.SetColumnValue("UsrString5", fieldFrom.GetColumnValue("UsrString5"));
                fieldTo.SetColumnValue("UsrFloat2", fieldFrom.GetColumnValue("UsrFloat2"));
                fieldTo.SetColumnValue("UsrTypeCarId", fieldFrom.GetColumnValue("UsrTypeCarId"));
                fieldTo.SetColumnValue("UsrBoolean2", fieldFrom.GetColumnValue("UsrBoolean2"));
                fieldTo.Save();
            }

            EntitySchemaManager esqManager = _userConnection.EntitySchemaManager;
            var esqResult = new EntitySchemaQuery(esqManager, "Usrseller");
            var colName = esqResult.AddColumn("UsrContact.Id");
            var filter = esqResult.CreateFilterWithParameters(FilterComparisonType.Equal,
                                                                "UsrSoldCar.Id", EntryId);
                   esqResult.Filters.Add(filter);
            var entities = esqResult.GetEntityCollection(_userConnection);
            foreach (var item in entities)
            {
                var sellerDetail = entityDetail.CreateEntity(_userConnection);
                sellerDetail.SetColumnValue("Id", Guid.NewGuid());
                sellerDetail.SetColumnValue("UsrContactId", item.GetColumnValue(colName.Name));
                sellerDetail.SetColumnValue("UsrSoldCarId", randomId);
                sellerDetail.Save();
            }
            return string.Empty;
        }
    }
}
