using System;

namespace System.Configuration.DataEntities {

    internal sealed class MetadataFullName {
        internal MetadataFullName(MetadataName currentName,MetadataFullName previous) {
            this._currentName = currentName;
            this._previous = previous;
        }

        //ex:System.Configuration.DataEntities

        //_currentWord = 'DataEntities'
        private readonly MetadataName _currentName;

        //_previous = 'Configuration'
        private readonly MetadataFullName _previous;

        //No need to override the Equals and GetHashCode, Pointer to the object we want to use the default implementation of judgments
        /// <summary>
        /// Return namespace string.
        /// </summary>
        /// <returns>namespace string.</returns>
        public override string ToString() {
            int size = 0;

            var current = this;
            while (current != null) {
                size += current._currentName.ToString().Length;

                current = current._previous;
                if (current != null) {
                    size++; // dot
                }
            }

            char[] chars = new char[size];
            current = this;
            var index = size;

            while (current != null) {
                var word = current._currentName.ToString();
                for (int j = word.Length - 1; j >= 0; j--) {
                    index--;
                    chars[index] = word[j];
                }

                current = current._previous;
                if (current != null) {
                    index--;
                    chars[index] = '.';
                }
            }

            return new string(chars);
        }
    }
}
