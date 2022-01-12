using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Xml;

namespace TTNC
{

    class TTNCApi
    {
        #region properties
        public XmlDocument Doc { get; set; }
        public XmlElement Root { get; set; }
        #endregion

        #region attributes
        private String username = "";
        private String password = "";
        private String vKey = "";
        private ArrayList results = new ArrayList();
        public Dictionary<string, string> dict = new Dictionary<string, string>();
        public ArrayList requests;
        public TTNCResponse response;
        #endregion

        #region constructors

        public TTNCApi(String username, String password, String VKey)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            this.requests = new ArrayList();
            this.username = username;
            this.password = password;
            this.vKey = VKey;
            this.Doc = new XmlDocument();
            this.Root = this.Doc.CreateElement("", "NoveroRequest", "");

            this.Doc.AppendChild(Root);
            this.sessionRequest();
        }

        public TTNCApi()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            this.requests = new ArrayList();
            this.Doc = new XmlDocument();
            this.Root = this.Doc.CreateElement("", "NoveroRequest", "");
            this.Doc.AppendChild(Root);
        }

        #endregion

        #region Methods

        private void sessionRequest()
        {
            TTNCRequest request = new TTNCRequest(this, "Auth", "SessionLogin", "SessionRequest");
            request.setData("Username", this.username);
            request.setData("Password", this.password);
            request.setData("VKey", this.vKey);
            request.UserAgent = "TTNC_Client_CSharp";

            Request_tree t = new Request_tree();
            t.request = request;
            t.id = request.RequestId;
            this.requests.Add(t);
        }

        public void usesession(String sessionId)
        {
            XmlElement SessionId_element = this.Doc.CreateElement("", "SessionId", "");
            XmlText sessionId_string = this.Doc.CreateTextNode(sessionId);
            SessionId_element.AppendChild(sessionId_string);
            this.Root.AppendChild(SessionId_element);
        }

        public TTNCRequest NewRequest(String target, String name, String id)
        {
            TTNCRequest NewRequest = new TTNCRequest(this, target, name, id);
            Request_tree t = new Request_tree();
            t.request = NewRequest;
            t.id = NewRequest.RequestId;
            this.requests.Add(t);
            return NewRequest;
        }

        public TTNCRequest NewRequest(String target, String name)
        {
            TTNCRequest NewRequest = new TTNCRequest(this, target, name, "");
            Request_tree t = new Request_tree();
            t.request = NewRequest;
            t.id = NewRequest.RequestId;
            this.requests.Add(t);
            return NewRequest;
        }

        public void MakeRequests()
        {
            foreach (Request_tree r_tree in this.requests)
            {
                this.Root.AppendChild(r_tree.request.Root);
            }

            Console.WriteLine(this.Doc.InnerXml);

            byte[] bytes = Encoding.UTF8.GetBytes(this.Doc.InnerXml);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://xml.ttnc.co.uk/api/");
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "text/xml";
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                string message = String.Format("POST failed. Received HTTP {0}", response.StatusCode);
                throw new ApplicationException(message);
            }
            else
            {
                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    String responseString = reader.ReadToEnd();
                    XmlDocument tmp_doc = new XmlDocument();
                    tmp_doc.LoadXml(responseString);
                    this.response = new TTNCResponse(tmp_doc);
                    Console.WriteLine(responseString);

                }
            }

        }

        public TTNCParser GetResponseFromId(string id)
        {

            foreach (XmlElement el_p in this.response.Doc.GetElementsByTagName("NoveroResponse"))
            {
                foreach (XmlElement el in el_p.GetElementsByTagName("Response"))
                {
                    if (el.GetAttribute("RequestId") == id)
                    {
                        return new TTNCParser(el, true);
                    }
                }
            }
            return new TTNCParser();
        }

        #endregion
    }

    class TTNCRequest
    {
        #region properties
        public TTNCApi Api { get; set; }
        public String RequestId { get; set; }
        public XmlDocument Doc { get; set; }
        public XmlElement Root { get; set; }
        #endregion

        #region constructors

        public TTNCRequest(TTNCApi api, String target, String name, String id)
        {
            this.Api = api;
            if (id != "")
                this.RequestId = id;
            else
                this.RequestId = Guid.NewGuid().ToString("N");

            this.Doc = this.Api.Doc;
            this.Root = this.Api.Doc.CreateElement("", "Request", "");
            this.Root.SetAttribute("id", this.RequestId);
            this.Root.SetAttribute("name", name);
            this.Root.SetAttribute("target", target);
        }

        #endregion

        #region Methods

        public void setData(string key, string value)
        {
            XmlElement key_element = this.Doc.CreateElement("", key, "");
            XmlText value_string = this.Doc.CreateTextNode(value);
            key_element.AppendChild(value_string);
            this.Root.AppendChild(key_element);
        }

        public TTNCParser GetResponse()
        {
            if (this.Api == null) return null;
            else
            {
                return this.Api.GetResponseFromId(this.RequestId);
            }

        }

        #endregion
    }

    class TTNCResponse
    {
        #region properties

        public XmlDocument Doc { get; set; }

        #endregion

        #region constructors

        public TTNCResponse(String response)
        {
            this.Doc.LoadXml(response);
        }
        public TTNCResponse(XmlDocument response)
        {
            this.Doc = response;
        }

        #endregion

    }
    class TTNCParser
    {
        #region attributes
        public Dictionary<string, List<TTNCParser>> value_dic_List=new Dictionary<string,List<TTNCParser>>();
        public String value_string;
        #endregion

        #region constructors
        public TTNCParser(XmlElement document)
        {

            this.value_string = "";

            if (document.HasChildNodes)
            {
                try
                {
                    foreach (Object x in document)
                    {
                        if (x is XmlElement)
                        {
                            XmlElement e = (XmlElement)x;
                            TTNCParser P = new TTNCParser(e);
                            if (!this.value_dic_List.ContainsKey(e.Name.ToString()))
                            {
                                this.value_dic_List[e.Name.ToString()] = new List<TTNCParser>();
                            }
                            this.value_dic_List[e.Name.ToString()].Add(P);
                        }
                        else if (x is XmlText)
                        {
                            XmlText t = (XmlText)x;
                            this.value_string = t.Value;
                        }
                    }
                }
                catch (InvalidCastException e)
                {
                    throw e;
                }
            }
            else
            {
                this.value_string = document.Value;
            }
        }

        public TTNCParser(XmlElement document, bool attrib)
        {
            TTNCParser P ;
            this.value_string = "";
            if (document.HasAttributes)
            {
                 P = new TTNCParser();

                foreach (XmlAttribute attr in document.Attributes)
                {
                    if (!this.value_dic_List.ContainsKey("@attribute"))
                    {
                        this.value_dic_List["@attribute"] = new List<TTNCParser>();
                    }
                    if (!P.value_dic_List.ContainsKey(attr.Name + ""))
                    {
                        P.value_dic_List[attr.Name + ""] = new List<TTNCParser>();
                    }
                    P.value_dic_List[attr.Name + ""].Add( new TTNCParser(attr.Value + ""));
                    this.value_dic_List["@attribute"].Add(P);
                }
            }

            if (document.HasChildNodes)
            {

                foreach (XmlElement x in document)
                {
                    if (!this.value_dic_List.ContainsKey(x.Name.ToString()))
                    {
                        this.value_dic_List[x.Name.ToString()] = new List<TTNCParser>();
                    }
                    this.value_dic_List[x.Name.ToString()].Add(new TTNCParser(x));
                }

            }
            else
            {
                this.value_string = document.Value;
            }
        }

        public TTNCParser()
        {
            this.value_string = "";
        }
        #endregion

        #region Methods
        public TTNCParser(String value)
        {
            this.value_string = value;
        }

        public void display()
        {

            if (this.value_dic_List != null && this.value_dic_List.Count > 0)
            {
                foreach (KeyValuePair<string, List<TTNCParser>> d in this.value_dic_List)
                {
                    Console.Write(d.Key + " =>");

                    for(int j=0; j < d.Value.Count ;j++){
                        d.Value[j].display();
                    }
                }
            }
            else
            {
                Console.WriteLine(this.value_string);
            }
        }
        #endregion

    }


    #region structs

    struct Request_tree
    {
        public String id;
        public TTNCRequest request;
    }

    #endregion
}
