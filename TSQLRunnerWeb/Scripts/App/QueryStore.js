var QS = (function () {
  var queries = new Array();
  var currentIndex = -1;
  var canSessionStorage = function () {
    return (window && window.sessionStorage);
  };
  var instance = {
    delimiter: ";@@@;",
    clear: function () {
      queries = new Array();
      currentIndex = -1;

      if (!canSessionStorage()) return;
      window.sessionStorage.clear();
    },
    loadQueries: function () {
      if (!canSessionStorage()) return;

      var item = window.sessionStorage.getItem("queries");
      if (typeof item != "string") {
        return;
      }

      queries = item.split(this.delimiter);
    },
    storeQuery: function (query) {
      if (!canSessionStorage()) return;

      queries.push(query);

      window.sessionStorage.setItem("queries", queries.join(this.delimiter));
    },
    nextQuery: function (text) {
      if (queries.length == 0 || queries.length <= currentIndex) {
        return;
      }

      currentIndex = currentIndex + 1;
      text.val(queries[currentIndex]);
    },
    prevQuery: function (text) {
      if (currentIndex <= 0) {
        return;
      }

      currentIndex = currentIndex - 1;
      text.val(queries[currentIndex]);
    }
  };
  return instance;
})();