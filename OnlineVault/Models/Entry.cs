using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace OnlineVault.Models
{
    public class Entry
    {
        List<string> prevPasswords = new List<string>();
        string username;
        string password;

        public Entry()
        {

        }

        public string GetUsername()
        {
            return username;
        }

        public string GetPassword()
        {
            return password;
        }

        public Error SetUsername(string username)
        {
            Regex r = new Regex("^[a-zA-Z0-9.!@?#$%&:';()*,^=_+-]+$");
            if (r.IsMatch(username))
            {
                this.username = username;
                return Error.None;
            }
            else
            {
                return Error.InvalidUsername;
            }
        }

        public Error SetPassword(string password)
        {
            if (prevPasswords.Contains(password))
            {
                return Error.ExistingPassword;
            }
            else
            {
                if (prevPasswords.Count >= 10)
                {
                    prevPasswords.RemoveAt(0);
                }
                if (this.password != null)
                {
                    prevPasswords.Add(this.password);
                }
                this.password = password;
                return Error.None;
            }
        }

        public enum Error
        {
            InvalidUsername,
            InvalidPassword,
            ExistingPassword,
            None
        }

        public XmlElement Serialize(XmlElement domainElement, XmlDocument xmlDocument)
        {
            XmlElement entryElement = (XmlElement)domainElement.AppendChild(xmlDocument.CreateElement("Entry"));
            entryElement.AppendChild(xmlDocument.CreateElement("Username")).InnerText = username;
            entryElement.AppendChild(xmlDocument.CreateElement("Password")).InnerText = password;
            XmlElement previousPasswordsElement = (XmlElement)entryElement.AppendChild(xmlDocument.CreateElement("PreviousPasswords"));
            foreach (string prevPassword in prevPasswords)
            {
                previousPasswordsElement.AppendChild(xmlDocument.CreateElement("PreviousPassword")).InnerText = prevPassword;
            }
            return entryElement;
        }
    }
}
