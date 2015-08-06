using System.Collections.Concurrent;

namespace System.Configuration.DataEntities {
    
    internal sealed class MetadataFullNameContainer {
        //all fullname
        private readonly ConcurrentDictionary<SplitNames, MetadataFullName> _fullNames;
        private readonly MetadataNameContainer _nameContainer;
        private readonly static MetadataFullName _emptyFullName = new MetadataFullName(null,null);

        public MetadataFullNameContainer(MetadataNameContainer nameContainer) {
            this._nameContainer = nameContainer;
            this._fullNames = new ConcurrentDictionary<SplitNames, MetadataFullName>();
        }

        public MetadataFullName GetFullName(string fullName) {
            if (string.IsNullOrEmpty(fullName)) {
                return _emptyFullName;
            }

            return this.GetFullName(GetSplitNames(fullName));
        }

        private MetadataFullName GetFullName(SplitNames splitNames) {
            return this._fullNames.GetOrAdd(splitNames,
                (key) => {
                    if (key.Size == 1) {
                        return new MetadataFullName(key.Names[key.StartIndex], null);
                    }
                    else {
                        var pre = new SplitNames(key.Names, key.StartIndex, key.Size - 1);
                        var preFullName = this.GetFullName(pre);
                        return new MetadataFullName(key.Names[key.StartIndex + key.Size - 1], preFullName);
                    }
                });
        }

        private SplitNames GetSplitNames(string fullName) {
            string[] splitStrings = fullName.Split('.');
            var size = splitStrings.Length;
            MetadataName[] names = new MetadataName[size];

            for (int i = 0; i < size; i++) {
                names[i] = this._nameContainer.GetVerifyName(splitStrings[i]);
            }

            return new SplitNames(names);
        }

        #region SplitNames
        private struct SplitNames {
            public readonly MetadataName[] Names;
            public readonly int StartIndex;
            public readonly int Size;

            public SplitNames(MetadataName[] names) {
                this.Names = names;
                this.StartIndex = 0;
                this.Size = names.Length;
            }

            public SplitNames(MetadataName[] names, int startIndex, int size) {
                this.Names = names;
                this.StartIndex = startIndex;
                this.Size = size;
            }

            public override bool Equals(object obj) {
                if (obj is SplitNames) {
                    var other = (SplitNames)obj;
                    if (this.Size != other.Size) {
                        return false;
                    }

                    var thisIndex = this.StartIndex;
                    var otherIndex = other.StartIndex;

                    for (int i = 0; i < this.Size; i++) {
                        if (this.Names[thisIndex] != other.Names[otherIndex]) {
                            return false;
                        }
                        thisIndex++;
                        otherIndex++;
                    }

                    return true;
                }

                return false;
            }

            public override int GetHashCode() {
                var hashcode = this.Names[StartIndex].GetHashCode();
                for (int i = StartIndex + 1; i < this.Size; i++) {
                    hashcode ^= this.Names[i].GetHashCode();
                }
                return hashcode;
            }
        } 
        #endregion

    }
}
