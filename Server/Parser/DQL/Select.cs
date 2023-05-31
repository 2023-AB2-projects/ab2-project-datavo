﻿using System.Text.RegularExpressions;
using Server.Logging;
using Server.Models.DQL;
using Server.Parser.Actions;
using Server.Server.Cache;
using Server.Server.Requests.Controllers.Parser;
using Server.Server.Responses.Parts;

namespace Server.Parser.DQL;
using TableRows = List<Dictionary<string, Dictionary<string, dynamic>>>;

internal class Select : BaseDbAction
{
    private readonly SelectModel _model;

    public Select(Match match, ParseRequest request)
    {
        _model = SelectModel.FromMatch(match);
    }

    public override void PerformAction(Guid session)
    {
        try
        {
            string databaseName = CacheStorage.Get(session)
                ?? throw new Exception("No database in use!");

            bool hasMissingColumns = _model.Validate(databaseName);

            if (!_model.JoinStatement.ContainsJoin() && hasMissingColumns)
            {
                throw new Exception("Invalid columns specified'");
            }

            TableRows result = new();

            if (_model.WhereStatement.IsEvaluatable())
            {
                result = _model.WhereStatement.EvaluateWithJoin(_model.TableService!, _model.JoinStatement);
            }
            else if (_model.JoinStatement.ContainsJoin())
            {
                Dictionary<string, Dictionary<string, Dictionary<string, dynamic>>> groupedInitialTable = new();

                foreach (var row in _model.FromTable.TableContent!)
                {
                    groupedInitialTable.Add(row.Key, new Dictionary<string, Dictionary<string, dynamic>> { { _model.FromTable.TableName, row.Value } });
                }

                result = _model.JoinStatement!.Evaluate(groupedInitialTable).Select(row => row.Value).ToList();
            }
            else
            {
                result = _model.FromTable!.TableContentValues!
                    .Select(row => new Dictionary<string, Dictionary<string, dynamic>> { { _model.FromTable.TableName, row } })
                    .ToList();
            }

            Logger.Info($"Rows selected: {result.Count}");
            Messages.Add($"Rows selected: {result.Count}");

            Fields = CreateFieldsFromColumns();

            Data = CreateDataFromResult(result);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
            Messages.Add(ex.Message);
        }
    }

    private List<FieldResponse> CreateFieldsFromColumns()
    {
        List<string> selectedColumns = _model.GetSelectedColumns();
        List<FieldResponse> fields = new();
        
        foreach (string column in selectedColumns)
        {
            string[] splittedColumn = column.Split('.');
            string tableName = splittedColumn[0];
            string columnName = splittedColumn[1];

            string inUseNameOfTable = _model.TableService!.GetTableDetailByAliasOrName(tableName).GetTableNameInUse();

            fields.Add(new()
            {
                FieldName = $"{inUseNameOfTable}.{columnName}",
            });
        }

        return fields;
    }

    private List<List<dynamic>> CreateDataFromResult(List<Dictionary<string, Dictionary<string, dynamic>>> filteredTable)
    {
        List<List<dynamic>> result = new();

        foreach (var row in filteredTable)
        {
            List<dynamic> data = new();
            foreach (string nameAssembly in _model.GetSelectedColumns())
            {
                string[] splittedAssembly = nameAssembly.Split('.');
                string tableName = splittedAssembly[0];
                string columnName = splittedAssembly[1];

                data.Add(row[tableName][columnName]);
            }

            result.Add(data);
        }

        return result;
    }
}