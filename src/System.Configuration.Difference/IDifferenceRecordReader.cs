using System;

namespace System.Configuration.Difference {
    
    public interface IDifferenceRecordReader {

        /// <summary>
        /// Read next node.
        /// </summary>
        /// <returns>return next node, if end of stream then return null.</returns>
        IDifferenceRecord Read();
    }
}
