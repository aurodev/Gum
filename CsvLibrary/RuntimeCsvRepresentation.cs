using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Xml.Serialization;
using System.IO;
using ToolsUtilities;

namespace CsvLibrary
{
    public enum QuoteBehavior
    {
        StandardCsv,
        NoChange
    }

    #region CsvHeader struct

    public struct CsvHeader : IEquatable<CsvHeader>
    {
        public static CsvHeader Empty = new CsvHeader(null);

        public string Name;
        public bool IsRequired;

        public static bool operator ==(CsvHeader c1, CsvHeader c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(CsvHeader c1, CsvHeader c2)
        {
            return !c1.Equals(c2);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        // This gets set in BuildMemberTypeIndexInformation
        public MemberTypes MemberTypes;

        public CsvHeader(string name)
        {
            Name = name;
            IsRequired = false;
            MemberTypes = (MemberTypes)0;
        }



        #region IEquatable<CsvHeader> Members

        public bool Equals(CsvHeader other)
        {
            return Name == other.Name &&
                IsRequired == other.IsRequired &&
                MemberTypes == other.MemberTypes;
        }

        #endregion
    }
    #endregion

    #region XML Docs
    /// <summary>
    /// Represents the raw data loaded from a csv file.  This is
    /// used if the data must be processed or converted by hand to
    /// other object types.
    /// </summary>
    #endregion
    public class RuntimeCsvRepresentation
    {
        #region Fields


        public CsvHeader[] Headers;
        public List<string[]> Records = new List<string[]>();

        #endregion

        #region Properties

        public string GetFirstDuplicateHeader
        {
            get
            {
                for (int i = 0; i < Headers.Length - 1; i++)
                {
                    for (int j = i + 1; j < Headers.Length; j++)
                    {
                        if (Headers[i].Name == Headers[j].Name)
                        {
                            return Headers[i].Name;
                        }
                    }
                }

                return null;
            }
        }

        public string FirstDuplicateRequiredField
        {
            get
            {
                int requiredIndex = -1;

                for (int i = 0; i < Headers.Length; i++)
                {
                    if (Headers[i].IsRequired)
                    {
                        requiredIndex = i;
                        break;
                    }
                }

                if (requiredIndex == -1)
                {
                    // It may be that the headers do specify something is required
                    // but the identified header hasn't been identified yet.
                    for (int i = 0; i < Headers.Length; i++)
                    {
                        if (IsHeaderRequired(Headers[i].Name))
                        {
                            requiredIndex = i;
                            break;
                        }
                    }
                }

                if (requiredIndex != -1)
                {
                    Dictionary<string, object> dictionaryToFill = new Dictionary<string,object>(Records.Count);

                    foreach (string[] row in Records)
                    {
                        string stringAtIndex = row[requiredIndex];

                        if (!string.IsNullOrEmpty(stringAtIndex))
                        {
                            if (dictionaryToFill.ContainsKey(stringAtIndex))
                            {
                                return stringAtIndex;
                            }
                            else
                            {
                                dictionaryToFill.Add(stringAtIndex, null);
                            }
                        }
                    }
                }

                return null;
            }
        }

        [XmlIgnore]
        public string Name
        {
            get;
            set;
        }

        #endregion

        //public void CreateObjectList(Type typeOfElement, IList listToPopulate)
        //{
        //    #region If primitive or string
        //    if (typeOfElement.IsPrimitive || typeOfElement == typeof(string))
        //    {
        //        if (typeOfElement == typeof(string))
        //        {
        //            listToPopulate.Add(this.Headers[0]);

        //            for (int i = 0; i < this.Records.Count; i++)
        //            {
        //                listToPopulate.Add(this.Records[i][0]);
        //            }
        //        }
        //        else
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }
        //    #endregion

        //    else if (typeOfElement == typeof(List<string>))
        //    {
        //        for (int i = 0; i < this.Records.Count; i++)
        //        {
        //            string[] record = Records[i];
        //            List<string> row = new List<string>();
        //            listToPopulate.Add(row);
        //            row.AddRange(record);
        //        }

        //    }
        //    else if (typeOfElement == typeof(string[]))
        //    {
        //        for (int i = 0; i < this.Records.Count; i++)
        //        {
        //            listToPopulate.Add(Records[i]);
        //        }

        //    }

        //    #region Not primitive or string (class/struct)
        //    else
        //    {
        //        CreateNonPrimitiveList(typeOfElement, listToPopulate);
        //    }
        //    #endregion


        //}

        //public void CreateObjectDictionary<KeyType, ValueType>(Dictionary<KeyType, ValueType> dictionaryToPopulate, string contentManagerName)
        //{
        //    Type typeOfElement = typeof(ValueType);

        //    if (typeOfElement.IsPrimitive || typeOfElement == typeof(string))
        //    {
        //        throw new InvalidOperationException("Can't create dictionaries of primitives or strings because they don't have a key");
        //    }
        //    else
        //    {
        //        MemberTypeIndexPair[] memberTypeIndexPairs;
        //        PropertyInfo[] propertyInfos;
        //        FieldInfo[] fieldInfos;
        //        GetReflectionInformation(typeOfElement, out memberTypeIndexPairs, out propertyInfos, out fieldInfos);

        //        #region Get the required header which we'll use for the key

        //        CsvHeader csvHeaderForKey = CsvHeader.Empty;

        //        int headerIndex = 0;

        //        foreach (CsvHeader header in Headers)
        //        {
        //            if (header.IsRequired)
        //            {
        //                csvHeaderForKey = header;
        //                break;
        //            }
        //            headerIndex++;
        //        }

        //        if (csvHeaderForKey == CsvHeader.Empty)
        //        {
        //            throw new InvalidOperationException("Could not find a property to use as the key.  You need to put (required) after one of the headers to identify it as required.");
        //        }

        //        #endregion

        //        int numberOfColumns = Headers.Length;

        //        object lastElement = null;
        //        bool wasRequiredMissing = false;


        //        for (int row = 0; row < Records.Count; row++)
        //        {
        //            object newElement;
        //            bool newElementFailed;

        //            wasRequiredMissing = TryCreateNewObjectFromRow(
        //                typeOfElement,

        //                contentManagerName,
        //                memberTypeIndexPairs,
        //                propertyInfos,
        //                fieldInfos,
        //                numberOfColumns,
        //                lastElement,
        //                wasRequiredMissing,
        //                row,
        //                out newElement,
        //                out newElementFailed);

        //            if (!newElementFailed && !wasRequiredMissing)
        //            {
        //                KeyType keyToUse = default(KeyType);
                        
        //                if (typeOfElement == typeof(string[]))
        //                {
        //                    keyToUse = (KeyType)(((string[])newElement)[headerIndex] as object);
        //                }
        //                else
        //                {

        //                    if (csvHeaderForKey.MemberTypes == MemberTypes.Property)
        //                    {
        //                        keyToUse = LateBinder<ValueType>.Instance.GetProperty<KeyType>((ValueType)newElement, csvHeaderForKey.Name);
        //                    }
        //                    else
        //                    {
        //                        keyToUse = LateBinder<ValueType>.Instance.GetField<KeyType>((ValueType)newElement, csvHeaderForKey.Name);
        //                    }
        //                }
        //                if (dictionaryToPopulate.ContainsKey(keyToUse))
        //                {
        //                    throw new InvalidOperationException("The key " + keyToUse +
        //                        " is already part of the dictionary.");
        //                }
        //                else
        //                {
        //                    dictionaryToPopulate.Add(keyToUse, (ValueType)newElement);
        //                }

        //                lastElement = newElement;
        //            }
        //        }


        //    }


        //}

        //private void CreateNonPrimitiveList(Type typeOfElement, IList listToPopulate)
        //{
        //    MemberTypeIndexPair[] memberTypeIndexPairs;
        //    PropertyInfo[] propertyInfos;
        //    FieldInfo[] fieldInfos;
        //    GetReflectionInformation(typeOfElement, out memberTypeIndexPairs, out propertyInfos, out fieldInfos);


        //    int numberOfColumns = Headers.Length;

        //    object lastElement = null;
        //    bool wasRequiredMissing = false;


        //    for (int row = 0; row < Records.Count; row++)
        //    {
        //        object newElement;
        //        bool newElementFailed;

        //        wasRequiredMissing = TryCreateNewObjectFromRow(
        //            typeOfElement, 
        //            contentManagerName, 
        //            memberTypeIndexPairs, 
        //            propertyInfos, 
        //            fieldInfos, 
        //            numberOfColumns, 
        //            lastElement, 
        //            wasRequiredMissing, 
        //            row, 
        //            out newElement, 
        //            out newElementFailed);

        //        if (!newElementFailed && !wasRequiredMissing)
        //        {
        //            listToPopulate.Add(newElement);

        //            lastElement = newElement;
        //        }
        //    }

        //}

        //private bool TryCreateNewObjectFromRow(Type typeOfElement, string contentManagerName, MemberTypeIndexPair[] memberTypeIndexPairs, 
        //    PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos, int numberOfColumns, object lastElement, bool wasRequiredMissing, int row, out object newElement, out bool newElementFailed)
        //{
        //    wasRequiredMissing = false;
        //    newElementFailed = false;

        //    #region Special-case handle string[].  We use these for localization
        //    if (typeOfElement == typeof(string[]))
        //    {
        //        int requiredColumn = -1;

        //        for (int i = 0; i < Headers.Length; i++)
        //        {
        //            if (Headers[i].IsRequired)
        //            {
        //                requiredColumn = i;
        //                break;
        //            }
        //        }

        //        //bool isRequired =;

        //        if (requiredColumn != -1 && string.IsNullOrEmpty(Records[row][requiredColumn]))
        //        {
        //            wasRequiredMissing = true;
        //            newElement = null;
        //        }
        //        else
        //        {
        //            string[] returnObject = new string[numberOfColumns];

        //            for (int column = 0; column < numberOfColumns; column++)
        //            {
        //                returnObject[column] = Records[row][column];
        //            }

        //            newElement = returnObject;
        //        }
        //    }

        //    #endregion

        //    else
        //    {
        //        newElement = Activator.CreateInstance(typeOfElement);

        //        for (int column = 0; column < numberOfColumns; column++)
        //        {
        //            if (memberTypeIndexPairs[column].Index != -1)
        //            {

        //                object objectToSetValueOn = newElement;
        //                if (wasRequiredMissing)
        //                {
        //                    objectToSetValueOn = lastElement;
        //                }
        //                int columnIndex = memberTypeIndexPairs[column].Index;

        //                bool isRequired = Headers[column].IsRequired;

        //                if (isRequired && string.IsNullOrEmpty(Records[row][column]))
        //                {
        //                    wasRequiredMissing = true;
        //                    continue;
        //                }

        //                #region If the member is a Property, so set the value obtained from converting the string.
        //                if (memberTypeIndexPairs[column].MemberType == MemberTypes.Property)
        //                {
        //                    // Currently requirements for lists are not working on properties.  Maybe we need to fix this up sometime
        //                    object propertyValueToSet = PropertyValuePair.ConvertStringToType(
        //                            Records[row][column],
        //                            propertyInfos[memberTypeIndexPairs[column].Index].PropertyType, contentManagerName);

        //                    propertyInfos[memberTypeIndexPairs[column].Index].SetValue(
        //                        objectToSetValueOn,
        //                        propertyValueToSet,
        //                        null);
        //                }
        //                #endregion

        //                #region Else, it's a Field, so set the value obtained from converting the string.
        //                else if (memberTypeIndexPairs[column].MemberType == MemberTypes.Field)
        //                {
        //                    //try
        //                    {
        //                        FieldInfo fieldInfo;
        //                        bool isList;
        //                        object valueToSet;
        //                        GetFieldValueToSet(contentManagerName, fieldInfos, row, column, columnIndex, out fieldInfo, out isList, out valueToSet);

        //                        if (isList)
        //                        {
        //                            // Check to see if the list is null.
        //                            // If so, create it. We want to make the
        //                            // list even if we're not going to add anything
        //                            // to it.  Maybe we'll change this in the future
        //                            // to improve memory usage?
        //                            object objectToCallOn = fieldInfo.GetValue(objectToSetValueOn);
        //                            if (objectToCallOn == null)
        //                            {
        //                                objectToCallOn = Activator.CreateInstance(fieldInfo.FieldType);

        //                                fieldInfo.SetValue(objectToSetValueOn, objectToCallOn);
        //                            }

        //                            if (valueToSet != null)
        //                            {
        //                                MethodInfo methodInfo = fieldInfo.FieldType.GetMethod("Add");

        //                                methodInfo.Invoke(objectToCallOn, new object[] { valueToSet });
        //                            }
        //                        }
        //                        else if (!wasRequiredMissing)
        //                        {
        //                            fieldInfo.SetValue(objectToSetValueOn, valueToSet);
        //                        }
        //                    }
        //                    // May 5, 2011:
        //                    // This code used
        //                    // to try/catch and
        //                    // just throw away failed
        //                    // attempts to instantiate
        //                    // a new object.  This caused
        //                    // debugging problems.  I think
        //                    // we should be stricter with this
        //                    // and let the exception occur so that
        //                    // developers can fix any problems related
        //                    // to CSV deseiralization.  Silent bugs could
        //                    // be difficult/annoying to track down.
        //                    //catch
        //                    //{
        //                    //    // don't worry, just skip for now.  May want to log errors in the future if this
        //                    //    // throw-away of exceptions causes difficult debugging.
        //                    //    newElementFailed = true;
        //                    //    break;
        //                    //}
        //                }
        //                #endregion
        //            }
        //        }
        //    }
        //    return wasRequiredMissing;
        //}

        //private void GetFieldValueToSet(string contentManagerName, FieldInfo[] fieldInfos, int row, int column, int columnIndex, out FieldInfo fieldInfo, out bool isList, out object valueToSet)
        //{
        //    fieldInfo = fieldInfos[columnIndex];

        //    isList = fieldInfo.FieldType.Name == "List`1";

        //    Type typeOfObjectInCell = fieldInfo.FieldType;

        //    if (isList)
        //    {
        //        typeOfObjectInCell = fieldInfo.FieldType.GetGenericArguments()[0];
        //    }

        //    string cellValue = Records[row][column];
        //    if (string.IsNullOrEmpty(cellValue))
        //    {
        //        valueToSet = null;
        //    }
        //    else
        //    {
        //        valueToSet = PropertyValuePair.ConvertStringToType(
        //            Records[row][column],
        //            typeOfObjectInCell,
        //            contentManagerName);
        //    }
        //}

        //private void GetReflectionInformation(Type typeOfElement, out MemberTypeIndexPair[] memberTypeIndexPairs, out PropertyInfo[] propertyInfos, out FieldInfo[] fieldInfos)
        //{
        //    memberTypeIndexPairs = new MemberTypeIndexPair[Headers.Length];

        //    propertyInfos = typeOfElement.GetProperties();
        //    fieldInfos = typeOfElement.GetFields();

        //    #region Remove whitespace and commas, identify if the types are required.
        //    // The headers may be include spaces to be more "human readable".
        //    // Of course members can't have spaces.  Therefore "Max HP" is acceptable
        //    // as a header, but the member might be MaxHP.  Therefore, we need to remove
        //    // whitespace.
        //    for (int i = 0; i < Headers.Length; i++)
        //    {
        //        string modifiedHeader = FlatRedBall.Utilities.StringFunctions.RemoveWhitespace(Headers[i].Name);
        //        bool isRequired = false;


        //        if (modifiedHeader.Contains("("))
        //        {
        //            int openingIndex = modifiedHeader.IndexOf('(');

        //            isRequired = IsHeaderRequired(modifiedHeader);

        //            modifiedHeader = modifiedHeader.Substring(0, openingIndex);
        //        }

        //        Headers[i].Name = modifiedHeader;
        //        Headers[i].IsRequired = isRequired;
        //    }
        //    #endregion


        //    BuildMemberTypeIndexInformation(memberTypeIndexPairs, propertyInfos, fieldInfos);
        //}

        #region Public Methods

        public RuntimeCsvRepresentation Clone()
        {
            RuntimeCsvRepresentation toReturn = new RuntimeCsvRepresentation();

            if (this.Headers != null)
            {
                toReturn.Headers = new CsvHeader[this.Headers.Length];
                this.Headers.CopyTo(toReturn.Headers, 0);
            }

            toReturn.Records = new List<string[]>();

            foreach (string[] record in this.Records)
            {
                string[] newRecord = new string[record.Length];
                record.CopyTo(newRecord, 0);

                toReturn.Records.Add(newRecord);
            }

            return toReturn;
        }

        public void AppendToThis(RuntimeCsvRepresentation whatToAppend)
        {
            // This will not use headers
            if (whatToAppend.Headers != null && whatToAppend.Headers.Length != 0)
            {
                throw new InvalidOperationException("The argument RuntimeCsvRepresentation cannot have headers");
            }

            // Let's make sure there are valid records
            if (Records == null)
            {
                Records = new List<string[]>();
            }

            if (whatToAppend.Records != null)
            {
                this.Records.AddRange(whatToAppend.Records);
            }
        }

        public void Format(params object[] parameters)
        {
            // for now we'll assume all parameters are of the same type
            bool isString = false;
            foreach (var item in parameters)
            {
                if (item is string)
                {
                    isString = true;
                    break;
                }
            }


            bool isRcr = false;
            foreach (var item in parameters)
            {
                if (item is RuntimeCsvRepresentation)
                {
                    isRcr = true;
                    break;
                }
            }

            bool isStringDictionary = false;
            foreach (var item in parameters)
            {
                if (item is Dictionary<string, string>)
                {
                    isStringDictionary = true;
                    break;
                }
            }


            bool isRcrDictionary = false;
            foreach (var item in parameters)
            {
                if (item is Dictionary<string, RuntimeCsvRepresentation>)
                {
                    isRcrDictionary = true;
                    break;
                }
            }            
                        
            
            if (isString)
            {
                FormatString(parameters);
            }
            else if (isRcr)
            {
                FormatRuntimeCsvRepresentation(parameters);
            }
            else if (isStringDictionary)
            {
                FormatStringDictionary((Dictionary<string, string>)parameters[0]);
                //FormatDictionaries(parameters);
            }
            else if (isRcrDictionary)
            {
                Format((Dictionary<string, RuntimeCsvRepresentation>)parameters[0]);
            }
        }

        public void Format<T>(Dictionary<string, T> namedValues)
        {
            if (namedValues is Dictionary<string, string>)
            {
                FormatStringDictionary(((object)namedValues) as Dictionary<string, string>);
            }
            else
            {
                for (int i = 0; i < this.Records.Count; i++)
                {
                    string[] record = this.Records[i];

                    foreach (KeyValuePair<string, T> kvp in namedValues)
                    {
                        string key = kvp.Key;
                        object value = kvp.Value;

                        i = ReplaceRow(i, record, key, value);
                    }
                }
            }
        }

        public void Save(string fileName)
        {
            Save(fileName, ',', Encoding.UTF8, QuoteBehavior.StandardCsv);

        }

        public void Save(string fileName, char delimiter, Encoding encoding, QuoteBehavior quoteBehavior)
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            string delimiterAsString = delimiter.ToString();

            #region Get the max length

            int maxLength = 0;

            if (Headers != null)
            {
                maxLength = Math.Max(Headers.Length, maxLength);
            }

            for (int i = 0; i < Records.Count; i++)
            {
                maxLength = Math.Max(Records[i].Length, maxLength);
            }

            #endregion


            if (Headers != null)
            {
                for (int i = 0; i < Headers.Length; i++)
                {
                    PerformCsvAppend(Headers[i].Name, stringBuilder, delimiterAsString, quoteBehavior);

                    if (i != Headers.Length - 1)
                    {
                        stringBuilder.Append(delimiter);
                    }
                    else
                    {
                        stringBuilder.Append("\r\n");
                    }
                }
            }


            foreach (string[] row in this.Records)
            {
                int i = 0;
                for (i = 0; i < maxLength; i++)
                {
                    if (i < row.Length)
                    {
                        PerformCsvAppend(row[i], stringBuilder, delimiterAsString, quoteBehavior);
                    }
                    else
                    {
                        PerformCsvAppend("", stringBuilder, delimiterAsString, quoteBehavior);
                    }

                    if (i != maxLength - 1)
                    {
                        stringBuilder.Append(delimiter);
                    }
                    else
                    {
                        stringBuilder.Append("\r\n");
                    }

                }
            }

            string whatToSave = stringBuilder.ToString();

            string directory = FileManager.GetDirectory(fileName);
            Directory.CreateDirectory(directory);

            FileManager.SaveText(whatToSave, fileName, encoding);
        }

        #endregion

        #region Private Methods

        private static bool IsHeaderRequired(string header)
        {
            bool isRequired = false;
            int openingIndex = header.IndexOf('(');

            string qualifiers = header.Substring(openingIndex + 1, header.Length - (openingIndex + 1) - 1);

            if (qualifiers.Contains(","))
            {
                string[] brokenUp = qualifiers.Split(',');

                foreach (string s in brokenUp)
                {
                    if (s == "required")
                    {
                        isRequired = true;
                    }
                }
            }
            else
            {
                if (qualifiers == "required")
                {
                    isRequired = true;
                }
            }
            return isRequired;
        }

        private void BuildMemberTypeIndexInformation(MemberTypeIndexPair[] memberTypeIndexPairs, PropertyInfo[] propertyInfos, FieldInfo[] fieldInfos)
        {
            for (int i = 0; i < Headers.Length; i++)
            {
                memberTypeIndexPairs[i].Index = -1;

                #region See if the header at index i is a field
                for (int j = 0; j < fieldInfos.Length; j++)
                {
                    if (fieldInfos[j].Name == Headers[i].Name)
                    {
                        Headers[i].MemberTypes = MemberTypes.Field;

                        memberTypeIndexPairs[i] = new MemberTypeIndexPair();
                        memberTypeIndexPairs[i].Index = j;
                        memberTypeIndexPairs[i].MemberType = MemberTypes.Field;
                        break;
                    }
                }

                if (memberTypeIndexPairs[i].Index != -1 && memberTypeIndexPairs[i].MemberType == MemberTypes.Field)
                {
                    continue;
                }
                #endregion

                #region If we got this far, then it's not a field, so check if it's a property

                for (int j = 0; j < propertyInfos.Length; j++)
                {
                    if (propertyInfos[j].Name == Headers[i].Name)
                    {
                        Headers[i].MemberTypes = MemberTypes.Property;

                        memberTypeIndexPairs[i] = new MemberTypeIndexPair();
                        memberTypeIndexPairs[i].Index = j;
                        memberTypeIndexPairs[i].MemberType = MemberTypes.Property;
                        break;
                    }
                }

                if (memberTypeIndexPairs[i].Index != -1 && memberTypeIndexPairs[i].MemberType == MemberTypes.Property)
                {
                    continue;
                }
                #endregion

                // Is this needed:
                memberTypeIndexPairs[i].Index = -1;
            }
        }


        private int ReplaceRow(int i, string[] record, string key, object value)
        {
            string keyWithBrackets = "{{" + key + "}}";
            if (ContainsStringContain(record, keyWithBrackets))
            {
                if (value is RuntimeCsvRepresentation)
                {
                    this.Records.RemoveAt(i);


                    RuntimeCsvRepresentation whatToInsert = (RuntimeCsvRepresentation)value;

                    if (whatToInsert.Records != null)
                    {
                        this.Records.InsertRange(i, whatToInsert.Records);
                        i += whatToInsert.Records.Count - 1; // subtract 1 becaue the i++ will add the last one
                    }
                    else
                    {
                        i--;
                    }
                }
                else
                {
                    for (int recordIndex = 0; recordIndex < record.Length; recordIndex++)
                    {
                        if (record[recordIndex].Contains(keyWithBrackets))
                        {
                            // The user may pass a null
                            // argument.  If so, we just
                            // want to remove the {{X}} and
                            // replace it with an empty string.
                            string whatToReplaceWith = "";
                            if (value != null)
                            {
                                whatToReplaceWith = value.ToString();
                            }
                            record[recordIndex] = record[recordIndex].Replace(keyWithBrackets, whatToReplaceWith);
                        }
                    }
                }
            }
            return i;
        }

        bool ContainsStringContain(string[] array, string value)
        {
            foreach (string s in array)
            {
                if (s.Contains(value))
                {
                    return true;
                }
            }
            return false;
        }

        private void FormatRuntimeCsvRepresentation(object[] parameters)
        {
            
            // For now this code
            // is going to see if
            // any rows contain {X}
            // and if so, that row will
            // get replaced with the RCR
            // in the parameters argument.

            for(int i = 0; i < this.Records.Count; i++)
            {
                string[] record = this.Records[i];

                for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                {
                    i = ReplaceRow(i, record, parameterIndex.ToString(), parameters[parameterIndex]);
                }
            }
        }

        private void FormatString(object[] parameters)
        {
            if (Headers != null)
            {
                for (int headerIndex = 0; headerIndex < Headers.Length; headerIndex++)
                {

                }
            }
            foreach (string[] record in Records)
            {
                for (int cellIndex = 0; cellIndex < record.Length; cellIndex++)
                {
                    
                    ReplaceCell(parameters, ref record[cellIndex], GetCellContentsAfter(record, cellIndex));
                }
            }
        }

        private void FormatStringDictionary(Dictionary<string, string> values)
        {
            foreach (string[] record in Records)
            {
                for (int cellIndex = 0; cellIndex < record.Length; cellIndex++)
                {
                    ReplaceCell(values, ref record[cellIndex], GetCellContentsAfter(record, cellIndex));
                }
            }
        }

        private string GetCellContentsAfter(string[] record, int cellIndex)
        {
            if (cellIndex < record.Length - 1)
            {
                return record[cellIndex + 1];
            }
            else
            {
                return null;
            }
        }


        private void ReplaceCell(object[] parameters, ref string p, string cellContentsAfter)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                string whatToSearchFor = "{{" + i + "}}";

                if (!p.StartsWith("//") && p.Contains(whatToSearchFor))
                {
                    p = p.Replace(whatToSearchFor, (string)parameters[i]);
                }
                else if (cellContentsAfter != null && cellContentsAfter.StartsWith("//") && cellContentsAfter.Contains("Replace" + whatToSearchFor))
                {
                    p = (string)parameters[i];
                }
            }
        }

        private void ReplaceCell(Dictionary<string, string> values, ref string p, string cellContentsAfter)
        {
            foreach (KeyValuePair<string, string> kvp in values)
            {
                string whatToSearchFor = "{{" + kvp.Key + "}}";

                if (p != null && !p.StartsWith("//") && p.Contains(whatToSearchFor))
                {
                    p = p.Replace(whatToSearchFor, kvp.Value);
                }
                else if (cellContentsAfter != null && cellContentsAfter.StartsWith("//") && cellContentsAfter.Contains("Replace" + whatToSearchFor))
                {
                    p = kvp.Value;
                }
            }
        }


        void PerformCsvAppend(string whatToAppend, StringBuilder stringBuilder, string delimiter, QuoteBehavior quoteBehavior)
        {
            if (!string.IsNullOrEmpty(whatToAppend))
            {
                if (quoteBehavior == QuoteBehavior.StandardCsv)
                {
                    whatToAppend = whatToAppend.Replace("\"", "\"\"\"");

                    if (whatToAppend.Contains(delimiter))
                    {
                        // If it's got
                        // a comma in it
                        // then we need to 
                        // surround the text
                        // with quotes
                        whatToAppend = "\"" + whatToAppend + "\"";
                    }
                }
            }
            stringBuilder.Append(whatToAppend);

        }


        #endregion
    }


    static class ExtensionMethods
    {
        public static bool Contains(this string[] values, string value)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

    }
}
