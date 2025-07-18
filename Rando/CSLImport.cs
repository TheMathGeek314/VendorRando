using System;
using System.Collections.Generic;
using MonoMod.ModInterop;
using RandomizerMod.Logging;

namespace VendorRando {
    internal static class CondensedSpoilerLogger {
        [ModImportName("CondensedSpoilerLogger")]
        private static class CSLImport {
            public static Action<string, Func<LogArguments, bool>, List<string>> AddCategory = null;
        }
        static CondensedSpoilerLogger() {
            typeof(CSLImport).ModInterop();
        }
        public static void AddCategory(string categoryName, Func<LogArguments, bool> test, List<string> entries)
            => CSLImport.AddCategory?.Invoke(categoryName, test, entries);
    }
}
