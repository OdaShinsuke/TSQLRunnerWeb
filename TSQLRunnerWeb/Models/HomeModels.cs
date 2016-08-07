using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Xsl;
using Dapper;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace TSQLRunnerWeb.Models {
  public class IndexModel {
    [DataType(DataType.MultilineText)]
    [Required]
    public string Query { get; set; }
    public ResultModel Result { get; set; }

    private ScriptDomService ScriptDom { get; set; } = new ScriptDomService();

    public async Task<IEnumerable<ValidationResult>> RunQuery() {
      var parsed = ScriptDom.ParseSql(Query);
      if (parsed.Item2.Any()) {
        return parsed.Item2
          .Select(e => new ValidationResult($"行:{e.Line} {e.Column}文字目 {e.Message}"));
      }

      var script = parsed.Item1;

      if (script.Batches.Count != 1) {
        return new ValidationResult[] { new ValidationResult("not support 複数バッチ") };
      }
      var batch = script.Batches[0];

      Result = new ResultModel();
      var vrs = new List<ValidationResult>();

      try {
        using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString)) {
          conn.Open();
          //foreach (var statement in batch.Statements) {
            await RunSelectQuery(conn, batch);
          //}
        }
      } catch (Exception e) {
        vrs.Add(new ValidationResult(e.Message));
      }

      return vrs.AsEnumerable();
    }
    public void GenerateExecPlan(string stylesheetPath) {
      if (Result?.ExecPlan == null) return;

      Result.ExecPlan.TransformXmlToHtml(stylesheetPath);
    }

    private async Task RunQuery(IDbConnection conn, TSqlFragment statement) {
      var select = statement as SelectStatement;
      if (select != null) {
        await RunSelectQuery(conn, select);
        return;
      }
      var cnt = await conn.ExecuteAsync(ScriptDom.ToQueryString(statement));

      Result.Message = $"{cnt} 行処理しました。\r\n";
    }
    private async Task RunSelectQuery(IDbConnection conn, TSqlFragment statement) {
      string planXml;
      var q = ScriptDom.ToQueryString(statement);
      try {
        await conn.ExecuteAsync("set showplan_xml on");
        planXml = await conn.ExecuteScalarAsync<string>(q);
      } finally {
        await conn.ExecuteAsync("set showplan_xml off");
      }

      var rows = await conn.QueryAsync(q);
      Result = new ResultModel {
        ExecPlan = new ExecPlanModel { ExecPlanXml = planXml }, 
        QueryResult = rows.Select(r => {
          var kvps = r as ICollection<KeyValuePair<string, object>>;
          var row = new RowModel();
          row.Value = kvps.Select(kvp => {
            return new CellModel {
              ColumnName = kvp.Key.ToExprString(),
              Value = kvp.Value.ToFormatString()
            };
          }).ToList();

          row.RenameColumn();
          return row;
        }).ToArray()
      };
    }
  }
  public class ResultModel {
    public IEnumerable<RowModel> QueryResult { get; set; }
    public string Message { get; set; }
    public ExecPlanModel ExecPlan { get; set; }
  }
  public class ExecPlanModel {
    public string ExecPlanXml { get; set; }
    public string ExecPlanHtml { get; private set; }
    public void TransformXmlToHtml(string stylesheetPath) {
      if (string.IsNullOrEmpty(ExecPlanXml)) return;

      var proc = new XslCompiledTransform();
      proc.Load(stylesheetPath);
      using (var writer = new StringWriter())
      using (var reader = XmlReader.Create(new StringReader(ExecPlanXml))) {
        proc.Transform(reader, null, writer);
        ExecPlanHtml = writer.ToString();
      }
    }
  }
  public class RowModel {
    public IEnumerable<CellModel> Value { get; set; }
    public void RenameColumn() {
      var nameCounts = new Dictionary<string, int>();
      foreach (var cell in Value) {
        if (nameCounts.Keys.Contains(cell.ColumnName)) {
          nameCounts[cell.ColumnName] = nameCounts[cell.ColumnName] + 1;
          cell.RenameColumnName = $"{cell.ColumnName}_{nameCounts[cell.ColumnName]}";
        } else {
          nameCounts.Add(cell.ColumnName, 0);
        }
      }
    }
    public IEnumerable<string> ColumnNames {
      get { return Value == null ? new string[] { } : Value.Select(cell => cell.DispColumnName); }
    }
  }
  public class CellModel {
    public string ColumnName { get; set; }
    public string Value { get; set; }
    public string RenameColumnName { get; set; }

    public string DispColumnName {
      get { return RenameColumnName ?? ColumnName; }
    }

  }
  static class Formatter {
    public static string ToFormatString(this object target) {
      if (target == null) { return null; }
      if (target is DateTime) { return ((DateTime)target).ToString("yyyy/MM/dd"); }
      return target.ToString();
    }
    public static string ToExprString(this string target) {
      if (string.IsNullOrEmpty(target) || target.Trim().Length == 0) return "Expr";
      return target;
    }
  }
}