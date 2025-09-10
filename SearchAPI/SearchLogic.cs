using System;
using System.Collections.Generic;
using Shared.Model;

namespace SearchAPI
{
    public class SearchLogic
    {
        IDatabase mDatabase;

        public SearchLogic(IDatabase database)
        {
            mDatabase = database;
        }

        /* Perform search of documents containing words from query. The result will
         * contain details about amost maxAmount of documents.
         */
        public SearchResult Search(String[] query, int maxAmount, bool caseSensitive = false)
        {
            List<string> ignored;

            DateTime start = DateTime.Now;

            // Convert words to wordids
            var wordIds = mDatabase.GetWordIds(query, out ignored, caseSensitive);

            // perform the search - get all docIds
            var docIds =  mDatabase.GetDocuments(wordIds);

            // get ids for the first maxAmount             
            var top = new List<int>();
            foreach (var p in docIds.GetRange(0, Math.Min(maxAmount, docIds.Count)))
                top.Add(p.Key);

            // compose the result.
            // all the documentHit
            List<Shared.Model.DocumentHit> docresult = new List<Shared.Model.DocumentHit>();
            int idx = 0;
            foreach (var doc in mDatabase.GetDocDetails(top))
            {
                var missing = mDatabase.WordsFromIds(mDatabase.getMissing(doc.mId, wordIds));
                  
                docresult.Add(new Shared.Model.DocumentHit(doc, docIds[idx++].Value, missing));
            }

            return new Shared.Model.SearchResult(query, docIds.Count, docresult, ignored, DateTime.Now - start);
        }
    }
}
