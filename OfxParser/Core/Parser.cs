using System.Globalization;
using System.Text;
using System.Xml;
using OfxParser.Exceptions;
using OfxParser.Models;

namespace OfxParser.Core
{
    public static class Parser
    {
        public static OfxData LoadFromFile(this OfxData ofxData, string path)
        {
            return LoadFromFile(path);
        }
        public static OfxData LoadFromFile(string path)
        {
            StringBuilder ofxTranslated = TranslateOfxToXml(path);
            SaveXmlToDisk(path + ".xml", ofxTranslated);
            return ReadOfxAsXml(path + ".xml");
        }
        private static StringBuilder TranslateOfxToXml(string ofxSourceFile)
        {
            StringBuilder result = new StringBuilder();
            string line;

            if (!File.Exists(ofxSourceFile))
                throw new FileNotFoundException("OFX source file not found: " + ofxSourceFile);

            StreamReader streamReader = File.OpenText(ofxSourceFile);
            while ((line = streamReader.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("</") && line.EndsWith(">"))
                {
                    result.Append(line);
                }
                else if (line.StartsWith("<") && line.EndsWith(">"))
                {
                    //ADJUST FOR POSSIBLE (BUT NOT ALLOWED) EMPTY OFX TAGS
                    if (line == "<BALAMT>" || line == "<PRINYTD>" || line == "<PRINLTD>")
                    {
                        result.Append(line);
                        result.Append(ReturnFinalTag(line));
                    }
                    else
                    {
                        result.Append(line);
                    }
                }
                else if (line.StartsWith("<") && !line.EndsWith(">"))
                {
                    result.Append(line);
                    result.Append(ReturnFinalTag(line));
                }
            }
            streamReader.Close();

            return result;
        }
        private static string ReturnFinalTag(string content)
        {
            string finalTagReturn = string.Empty;

            if ((content.IndexOf("<") != -1) && (content.IndexOf(">") != -1))
            {
                int position1 = content.IndexOf("<");
                int position2 = content.IndexOf(">");
                if ((position2 - position1) > 2)
                {
                    finalTagReturn = content.Substring(position1, (position2 - position1) + 1);
                    finalTagReturn = finalTagReturn.Replace("<", "</");
                }
            }
            return finalTagReturn;
        }
        private static void SaveXmlToDisk(string xmlNewFile, StringBuilder ofxTranslated)
        {
            if (System.IO.File.Exists(xmlNewFile))
            {
                System.IO.File.Delete(xmlNewFile);
            }

            StreamWriter sw = File.CreateText(xmlNewFile);
            sw.WriteLine(@"<?xml version=""1.0""?>");
            sw.WriteLine(ofxTranslated.ToString());
            sw.Close();
        }
        private static OfxData ReadOfxAsXml(string ofxPath)
        {
            XmlTextReader xmlTextReader = new(ofxPath);
            string currentElement = string.Empty;
            OfxTransaction? currentTransaction = null;
            OfxData ofxData = new();
            Bank? bank = null;
            BankAccount? bankAccount = null;
            try
            {
                while (xmlTextReader.Read())
                {
                    if (xmlTextReader.NodeType == XmlNodeType.EndElement)
                    {
                        switch (xmlTextReader.Name)
                        {
                            case "STMTTRN":
                                if (currentTransaction != null)
                                {
                                    ofxData.AddTransaction(currentTransaction);
                                    currentTransaction = null;
                                }
                                break;
                        }
                    }
                    if (xmlTextReader.NodeType == XmlNodeType.Element)
                    {
                        currentElement = xmlTextReader.Name;

                        switch (currentElement)
                        {
                            case "STMTTRN":
                                currentTransaction = new OfxTransaction();
                                break;
                        }
                    }
                    if (xmlTextReader.NodeType == XmlNodeType.Text)
                    {
                        switch (currentElement)
                        {
                            case "BALAMT":
                                ofxData.BALAMT = ConvertOfxCurrencyToDouble(xmlTextReader.Value, ofxData);
                                break;
                            case "DTASOF":
                                ofxData.DTASOF = ConvertOfxDateToDateTime(xmlTextReader.Value, ofxData);
                                break;
                            case "ORG":
                                bank ??= new Bank();
                                bank.ORG = xmlTextReader.Value;
                                break;
                            case "FID":
                                bank ??= new Bank();
                                bank.FID = int.Parse(xmlTextReader.Value);
                                break;
                            case "DTSTART":
                                ofxData.DTSTART = ConvertOfxDateToDateTime(xmlTextReader.Value, ofxData);
                                break;
                            case "DTEND":
                                ofxData.DTEND = ConvertOfxDateToDateTime(xmlTextReader.Value, ofxData);
                                break;
                            case "BANKID":
                                bankAccount ??= new BankAccount();
                                bankAccount.BANKID = int.Parse(xmlTextReader.Value);
                                break;
                            case "BRANCHID":
                                bankAccount ??= new BankAccount();
                                bankAccount.BRANCHID = xmlTextReader.Value;
                                break;
                            case "ACCTID":
                                bankAccount ??= new BankAccount();
                                bankAccount.ACCTID = xmlTextReader.Value;
                                break;
                            case "ACCTTYPE":
                                bankAccount ??= new BankAccount();
                                bankAccount.ACCTTYPE = xmlTextReader.Value;
                                break;
                            case "TRNTYPE":
                                if (currentTransaction != null)
                                {
                                    currentTransaction.TRNTYPE = xmlTextReader.Value;
                                }
                                break;
                            case "DTPOSTED":
                                if (currentTransaction != null)
                                {
                                    currentTransaction.DTPOSTED = ConvertOfxDateToDateTime(xmlTextReader.Value, ofxData);
                                }
                                break;
                            case "TRNAMT":
                                if (currentTransaction != null)
                                {
                                    currentTransaction.TRNAMT = ConvertOfxCurrencyToDouble(xmlTextReader.Value, ofxData);
                                }
                                break;
                            case "FITID":
                                if (currentTransaction != null)
                                {
                                    currentTransaction.FITID = xmlTextReader.Value;
                                }
                                break;
                            case "CHECKNUM":
                                if (currentTransaction != null)
                                {
                                    currentTransaction.CHECKNUM = Convert.ToInt64(xmlTextReader.Value);
                                }
                                break;
                            case "MEMO":
                                if (currentTransaction != null)
                                {
                                    currentTransaction.MEMO = string.IsNullOrEmpty(xmlTextReader.Value) ? string.Empty : xmlTextReader.Value.Trim().Replace("  ", " ");
                                }
                                break;
                        }
                    }
                }
            }
            catch (XmlException xe)
            {
                throw new OFXParserException($"Invalid OFX file! Internal message: {xe.Message}");
            }
            finally
            {
                xmlTextReader.Close();
            }
            ofxData.BANKACCTFROM = bankAccount ?? new BankAccount();
            ofxData.FI = bank ?? new Bank();

            return ofxData;
        }
        private static double ConvertOfxCurrencyToDouble(string ofxCurrency, OfxData ofxData)
        {
            try
            {
                NumberFormatInfo provider = new NumberFormatInfo();
                provider.NumberDecimalSeparator = ".";
                var result= Convert.ToDouble(ofxCurrency, provider);
                return result;
            }
            catch (Exception)
            {
                ofxData.AddWarning($"Invalid transaction value/amount: {ofxCurrency}");
            }
            return 0;
        }

        private static DateTime ConvertOfxDateToDateTime(string ofxDate, OfxData ofxData)
        {
            DateTime extractedDateTime = DateTime.MinValue;
            try
            {
                if (ofxDate.Length >= 14)
                    extractedDateTime = new DateTime(int.Parse(ofxDate.Substring(0, 4)), int.Parse(ofxDate.Substring(4, 2)), int.Parse(ofxDate.Substring(6, 2)), int.Parse(ofxDate.Substring(8, 2)), int.Parse(ofxDate.Substring(10, 2)), int.Parse(ofxDate.Substring(12, 2)));
                else
                    extractedDateTime = new DateTime(int.Parse(ofxDate.Substring(0, 4)), int.Parse(ofxDate.Substring(4, 2)), int.Parse(ofxDate.Substring(6, 2)));

            }
            catch (Exception)
            {
                ofxData.AddWarning($"Invalid datetime: {ofxDate}");
            }
            return extractedDateTime;
        }
    }
}
