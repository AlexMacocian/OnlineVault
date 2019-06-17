using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace OnlineVault.Models
{
    public class Vault
    {
        public string HashedPinCode;
        public string Filename;
        public string Filepath;
        public SortedList<string, Domain> Domains = new SortedList<string, Domain>();

        public Vault()
        {

        }

        public Vault(string serializedObject)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(serializedObject);

            XmlNodeList l = xmlDoc.GetElementsByTagName("Domain");
            foreach (XmlNode node in l)
            {
                string name = node.SelectSingleNode("Name").InnerText;
                Domain domain = new Domain(name);
                XmlNodeList ln = node.SelectNodes("Entry");
                foreach (XmlNode lnNode in ln)
                {
                    Entry entry = new Entry();
                    string username = lnNode.SelectSingleNode("Username").InnerText;
                    entry.SetUsername(username);

                    XmlNodeList lnp = lnNode.SelectSingleNode("PreviousPasswords").SelectNodes("PreviousPassword");
                    foreach (XmlNode lnpNode in lnp)
                    {
                        entry.SetPassword(lnpNode.InnerText);
                    }

                    string encryptedPassword = lnNode.SelectSingleNode("Password").InnerText;
                    entry.SetPassword(encryptedPassword);
                    domain.Entries.Add(entry);
                }
                Domains.Add(domain.Name, domain);
            }
        }

        public Domain AddDomain(string Name)
        {
            if (Domains.ContainsKey(Name))
            {
                return Domains[Name];
            }
            else
            {
                Domain dom = new Domain(Name);
                Domains.Add(Name, dom);
                return dom;
            }
        }

        public Domain FindDomain(string Name)
        {
            if (Domains.ContainsKey(Name))
            {
                return Domains[Name];
            }
            return null;
        }

        public bool ContainsDomain(Domain dom)
        {
            if (Domains.ContainsKey(dom.Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ContainsDomain(String Name)
        {
            if (Domains.ContainsKey(Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string Serialize()
        {
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement vaultElement = (XmlElement)xmlDocument.AppendChild(xmlDocument.CreateElement("Vault"));

            foreach (KeyValuePair<string, Domain> p in Domains)
            {
                Domain domain = p.Value;
                XmlElement domainElement = (XmlElement)vaultElement.AppendChild(xmlDocument.CreateElement("Domain"));
                domainElement.AppendChild(xmlDocument.CreateElement("Name")).InnerText = domain.Name;
                foreach (Entry entry in domain.Entries)
                {
                    domainElement.AppendChild(entry.Serialize(domainElement, xmlDocument));
                }
            }
            return xmlDocument.OuterXml;
        }
    }
}
