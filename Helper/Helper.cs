using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Helpers;
using System.Web.Script.Serialization;

namespace HRIS.Helper
{
    public class AgilityHelper
    {
        internal static bool InList<T>(T value, params T[] valuesToCompare)
        {
            if (valuesToCompare.Length == 0)
                return false;
            foreach (T pv in valuesToCompare)
            {
                if (pv.Equals(value))
                    return true;
            }
            return false;
        }

        internal static string SuffixRF(string _fieldName)
        {
            //" => " + 
            return _fieldName + " is a required field";
        }

        /// <summary>
        /// Suffix field name with standard validation text i.e. 'xxx' must be +ve and non Zero
        /// </summary>
        /// <param name="_fieldName">The field name/pro[erty to be prefixed for fixed validation text</param>
        /// <returns></returns>
        internal static string SuffixgtZero(string _fieldName)
        {
            return _fieldName +" must be +ve and greater than Zero";
        }

        /// <summary>
        /// Suffix field name with standard validation text i.e. 'xxx' must be Zero or +ve value
        /// </summary>
        /// <param name="_fieldName">The field name/property to be prefixed for fixed validation text</param>
        /// <returns></returns>
        internal static string SuffixZero(string _fieldName)
        {
            return " => "+ _fieldName + " must be Zero or +ve value";
        }

        /// <summary>
        /// Suffix field name with standard validation text i.e. 'xxx' must not be zero
        /// </summary>
        /// <param name="_fieldName">The field name/property to be prefixed for fixed validation text</param>
        /// <returns></returns>
        internal static string SuffixNZ(string _fieldName)
        {
            return _fieldName + " must be non-Zero";
        }

        /// <summary>
        /// Suffix field name with standard validation text i.e. 'xxx' must be newer than current date
        /// </summary>
        /// <param name="_fieldName">The field name/property to be prefixed for fixed validation text</param>
        /// <returns></returns>
        internal static string SuffixgtCurrent(string _fieldName)
        {
            return _fieldName + " must be newer than current Date";
        }

        /// <summary>
        /// Suffix field name with standard validation text i.e. 'xxx' must be older than current date
        /// </summary>
        /// <param name="_fieldName">The field name/property to be prefixed for fixed validation text</param>
        /// <returns></returns>
        internal static string SuffixltCurrent(string _fieldName)
        {
            return _fieldName + " must be older than current Date";
        }

        /// <summary>
        /// Suffix field name with standard validation text i.e. 'yyy' must be greater than 'xxx'
        /// </summary>
        /// <param name="_fieldName1">The field to be lesser</param>
        /// <param name="_fieldName2">The field to be greater</param>
        /// <returns></returns>
        internal static string Suffixgt(string _fieldName1, string _fieldName2)
        {
            return _fieldName2 + " must be greater than " + _fieldName1;
        }

        /// <summary>
        /// Suffix field name with standard validation text i.e. 'xxx' must be older than current date
        /// </summary>
        /// <param name="_fieldName">The field name/property to be prefixed for fixed validation text</param>
        /// <returns></returns>
        internal static string SuffixDifferent(string _fieldName1, string _fieldName2)
        {
            return _fieldName1 + " & " + _fieldName2 + " must not be same";
        }

        /// <summary>
        /// Suffix field name with proper email format
        /// </summary>
        /// <param name="_fieldName">The field name/proerty to be prefix for fixed validation text</param>
        /// <returns></returns>
        internal static string EmailFormat(string _fieldName)
        {
            return "Please enter " + _fieldName + " in valid format like someone@somewhere.com";
        }

        /// <summary>
        /// Suffix field name with proper phone / mobile number
        /// </summary>
        /// <param name="_fieldName">The field name/proerty to be prefix for fixed validation text</param>
        /// <returns></returns>
        internal static string CallFormat(string _fieldName)
        {
            return "Please enter" +_fieldName+"in valid format like xxxx-xxxxxxx";
        }

        internal static string MaskPassword(string pValue, bool pEncrypt)
        {
            string strPwd = "";
            int intCntr = 1;
            if (pEncrypt)
            {
                foreach (char c in pValue)
                {
                    strPwd += Convert.ToByte(c) - 25 + intCntr++;
                    strPwd += ",";
                }
            }
            else
            {
                for (int i = 0; i < pValue.Trim().Length; )
                {
                    strPwd += Convert.ToChar(Convert.ToByte(pValue.Substring(i, pValue.IndexOf(",", i) - i)) + 25 - intCntr++);
                    i = pValue.IndexOf(",", i) + 1;
                }
            }
            return strPwd;
        }

        internal static bool isComplex(string pPassword)
        {
            bool hasInt = false;
            bool hasSC = false;
            bool hasAlphaSmall = false;
            bool hasAlphaCapital = false;
            char[] cPassword = pPassword.ToCharArray();
            foreach (char c in cPassword)
            {
                if (Convert.ToByte(c) >= 48 && Convert.ToByte(c) <= 57)
                    hasInt = true;
                if (Convert.ToByte(c) >= 65 && Convert.ToByte(c) <= 90)
                    hasAlphaCapital = true;
                if (Convert.ToByte(c) >= 97 && Convert.ToByte(c) <= 122)
                    hasAlphaSmall = true;
                if ((Convert.ToByte(c) < 48 || Convert.ToByte(c) > 122) || (Convert.ToByte(c) > 57 && Convert.ToByte(c) < 65)
                    || (Convert.ToByte(c) > 90 && Convert.ToByte(c) < 97))
                    hasSC = true;
            }
            if (!hasInt || !hasSC || !hasAlphaCapital || !hasAlphaSmall)
            {
                string msg = "Password must contain combination of Capital & Small Alphabet, Number and Special Character " +
                   "e.g. 20-Dec-79 or AqT-607 Description of Failure: Your Password doesn't contain" + (hasInt ? "" : "Number (Valid values are 0-9)") +
                   (hasAlphaCapital ? "" : "Capital Alphabet (Valid values are A-Z)") + (hasAlphaSmall ? "" : "Small Alphabet (Valid values are a-z)") +
                   (hasSC ? "" : "Special characters (Valid values are any character other than Alphabet and Number)") + "Please try new password using above mentioned criteria" + "," + "Weak Password";
                throw new Exception(msg);
            }
            return (hasInt && hasSC && hasAlphaCapital && hasAlphaSmall);
        }

        //internal static DataTable ListToDataTable<T>(IList<T> varlist)
        //{
        //    DataTable dt = new DataTable();

        //    //special handling for value types and string
        //    //In value type, the DataTable is expected to contain the values of all the variables (items) present in List.
        //    //Hence only one column is created in the DataTable named “Values”,
        //    //Though String is a reference type, due to its behavior it is treated as a special case and handle it as value type only.
        //    if (typeof(T).IsValueType || typeof(T).Equals(typeof(string)))
        //    {
        //        DataColumn dc = new DataColumn("Values");

        //        dt.Columns.Add(dc);

        //        foreach (T item in varlist)
        //        {
        //            DataRow dr = dt.NewRow();
        //            dr[0] = item;
        //            dt.Rows.Add(dr);
        //        }
        //    }
        //    //for reference types other than  string
        //    // Used PropertyInfo class of System.Reflection
        //    else
        //    {
        //        //find all the public properties of this Type using reflection excluding properties marked for not to browsed
        //        PropertyInfo[] propT = typeof(T).GetProperties().Where(
        //            p => p.GetCustomAttributes(typeof(BrowsableAttribute), false).Count() == 0 ||
        //                ((BrowsableAttribute)p.GetCustomAttributes(typeof(BrowsableAttribute), false)[0]).Browsable).ToArray();

        //        foreach (PropertyInfo pi in propT)
        //        {
        //            DataColumn dc = new DataColumn(pi.Name, Nullable.GetUnderlyingType(
        //                    pi.PropertyType) ?? pi.PropertyType);
        //            if (pi.GetCustomAttributes(typeof(DisplayAttribute), false).Any())
        //                dc.Caption = pi.GetCustomAttributes(typeof(DisplayAttribute), false).Cast<DisplayAttribute>().FirstOrDefault().Name;
        //            dt.Columns.Add(dc);
        //        }

        //        //now we iterate through all the items , take the corresponding values and add a new row in dt
        //        for (int item = 0; item < varlist.Count(); item++)
        //        {
        //            DataRow dr = dt.NewRow();

        //            for (int property = 0; property < propT.Length; property++)
        //            {
        //                try
        //                {
        //                    dr[property] = propT[property].GetValue(varlist[item], null);
        //                }
        //                catch (Exception)
        //                {
        //                    dr[property] = DBNull.Value;
        //                }
        //            }

        //            dt.Rows.Add(dr);
        //        }
        //    }
        //    dt.AcceptChanges();
        //    return dt;
        //}

        /// <summary>
        /// To chopoff unnecessary database info and technical details in the error msg to make it readable and secure
        /// </summary>
        /// <param name="_message">Complete Message Contents</param>
        /// <returns></returns>
        internal static string extractDBMsg(string _message)
        {
            try
            {
                if (_message.IndexOf("\"") > 0)
                    return _message.Substring(_message.IndexOf("\"") + 1, _message.LastIndexOf("\"", _message.IndexOf("\"")) - 2);
                else
                    return _message;
            }
            catch (Exception)
            {
                return _message;
            }
        }

        /// <summary>
        /// returns list of objects where Edit = true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="details"></param>
        /// <returns></returns>
        internal static IEnumerable<T> GetEdits<T>(List<T> details)
        {
            return details.Where(x => ((dynamic)x).Edit);
        }

        /// <summary>
        /// returns list of objects where Add = true and Delete = false - ***This function will not check Edit property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_details"></param>
        /// <returns></returns>
        internal static IEnumerable<T> GetAdds<T>(List<T> _details)
        {
            return _details.Where(x => ((dynamic)x).Add && !((dynamic)x).Delete);
        }

        /// <summary>
        /// returns list of objects where atleast one of Add, Edit or Delete = true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_details"></param>
        /// <returns></returns>
        internal static IEnumerable<T> GetChanges<T>(List<T> _details)
        {
            return _details.Where(x => ((dynamic)x).Add || ((dynamic)x).Edit || ((dynamic)x).Delete);
        }

        /// <summary>
        /// returns list of objects where atleast one of Add or Delete = true - ***This function will not check Edit property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_details"></param>
        /// <returns></returns>
        internal static IEnumerable<T> GetADChanges<T>(List<T> _details)
        {
            return _details.Where(x => ((dynamic)x).Add || ((dynamic)x).Delete);
        }
    }
}