using Microsoft.SqlServer.TransactSql.ScriptDom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TSQLRunnerWeb {
  public class ScriptDomService {
    public Version Type { get; private set; }

    public ScriptDomService() : this(Version.V2016) { }
    public ScriptDomService(Version v) {
      Type = v;
    }

    public enum Version {
      V2008, V2012, V2014, V2016
    }
    public Tuple<TSqlScript, IEnumerable<ParseError>> ParseSql(string query) {
      var parser = CreateParser(Type);
      IList<ParseError> errors;
      TSqlFragment flagment;
      using (var sr = new StringReader(query)) {
        flagment = parser.Parse(sr, out errors);
      }

      return new Tuple<TSqlScript, IEnumerable<ParseError>>(flagment as TSqlScript, errors);
    }
    public string ToQueryString(TSqlFragment fragment) {
      var generator = CreateGenerator(Type);
      string query;
      generator.GenerateScript(fragment, out query);
      return query;
    }
    private static TSqlParser CreateParser(Version v) {
      switch (v) {
        case Version.V2008:
          return new TSql100Parser(true);
        case Version.V2012:
          return new TSql110Parser(true);
        case Version.V2014:
          return new TSql120Parser(true);
        case Version.V2016:
          return new TSql130Parser(true);
        default:
          throw new ArgumentOutOfRangeException($"Unsupport: {nameof(Version)}:{v}");
      }
    }
    private static SqlScriptGenerator CreateGenerator(Version v) {
      switch (v) {
        case Version.V2008:
          return new Sql100ScriptGenerator();
        case Version.V2012:
          return new Sql110ScriptGenerator();
        case Version.V2014:
          return new Sql120ScriptGenerator();
        case Version.V2016:
          return new Sql130ScriptGenerator();
        default:
          throw new ArgumentOutOfRangeException($"Unsupport: {nameof(Version)}:{v}");
      }
    }
  }
}