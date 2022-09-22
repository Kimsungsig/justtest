using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Threading;


namespace NUI
{
    public class Login
    {
        public string ID;
        public string PW;
    }
    public class User
    {
        public string ID;
        public int TYPE;
        public string PW;
        public int ROBOT_ID;
        public int CAMERA_ID;
        public string NAME;
        public int SITE_ID;
        public DateTime CREATE_DATE;
        public string CREATE_USER_ID;
        public DateTime UPDATE_DATE;
        public string UPDATE_USER_ID;
        public DateTime DELETE_DATE;
        public string DELETE_USER_ID;
    }

    public class DigestAuthFixer : MonoBehaviour
    {
        private static string _host;
        private static string _user;
        private static string _password;
        private static string _realm;
        private static string _nonce;
        private static string _qop;
        private static string _cnonce;
        private static DateTime _cnonceDate;
        private static int _nc;
        public string responseText = "";

        public DigestAuthFixer(string host, string user, string password)
        {
            // TODO: Complete member initialization
            _host = host;
            _user = user;
            _password = password;
        }

        public DigestAuthFixer(string host)
        {
            // TODO: Complete member initialization
            _host = host;
        }

        private string CalculateMd5Hash(
            string input)
        {
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = MD5.Create().ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private string GrabHeaderVar(
            string varName,
            string header)
        {
            var regHeader = new Regex(string.Format(@"{0}=""([^""]*)""", varName));
            var matchHeader = regHeader.Match(header);
            if (matchHeader.Success)
                return matchHeader.Groups[1].Value;
            throw new ApplicationException(string.Format("Header {0} not found", varName));
        }

        private string GetDigestHeader(
            string dir)
        {
            _nc = _nc + 1;

            var ha1 = CalculateMd5Hash(string.Format("{0}:{1}:{2}", _user, _realm, _password));
            var ha2 = CalculateMd5Hash(string.Format("{0}:{1}", "PUT", dir));
            var digestResponse =
                CalculateMd5Hash(string.Format("{0}:{1}:{2:00000000}:{3}:{4}:{5}", ha1, _nonce, _nc, _cnonce, _qop, ha2));

            return string.Format("Digest username=\"{0}\", realm=\"{1}\", nonce=\"{2}\", uri=\"{3}\", " +
                "algorithm=MD5, response=\"{4}\", qop={5}, nc={6:00000000}, cnonce=\"{7}\"",
                _user, _realm, _nonce, dir, digestResponse, _qop, _nc, _cnonce);
        }

        public string GrabResponseJson2(string dir, string strjson)
        {
            var url = _host + dir;
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            byte[] byteArray = Encoding.UTF8.GetBytes(strjson);
            string responseText = string.Empty;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                //if (ex.Response == null || ((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.Unauthorized)
                //    throw;
                var request2 = (HttpWebRequest)WebRequest.Create(uri);
                request2.Method = "POST";
                request2.ContentType = "application/json; charset=utf-8";
                Stream dataStream = request2.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                using (HttpWebResponse resp = (HttpWebResponse)request2.GetResponse())
                {
                    HttpStatusCode status = resp.StatusCode;
                    Console.WriteLine(status);      // status 가 정상일경우 OK가 입력된다.

                    // 응답과 관련된 stream을 가져온다.
                    Stream respStream = resp.GetResponseStream();
                    using (StreamReader streamReader = new StreamReader(respStream))
                    {
                        responseText = streamReader.ReadToEnd();
                    }
                }
                response = (HttpWebResponse)request2.GetResponse();

            }
            Debug.Log(responseText);
            return responseText;
        }

        public string GrabResponseJson(string dir, string strjson)
        {
            //StartCoroutine(GrabResponseJson_using(a, b));
            //Thread.Sleep(1000);
            var url = _host + dir;
            using (UnityWebRequest request = UnityWebRequest.Post(url, strjson))
            {
                byte[] jsonToSend = new UTF8Encoding().GetBytes(strjson);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                //request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                //request.SetRequestHeader("Content-Type", "application/json");
                request.useHttpContinue = false;
                request.SetRequestHeader("Content-Type", "application/json");
                //"application/json; charset=utf-8"
                //request.SendWebRequest();
                request.SendWebRequest();
                Thread.Sleep(1000);
                //yield return new WaitForSeconds(3f);
                //request.SendWebRequest();
                Debug.Log("-----------------------------------------------------------------------------------");
                Debug.Log(url);
                Debug.Log(request.result);
                Debug.Log(request.downloadHandler.text);
                if (request.error == null)
                {
                    Debug.Log("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                    Debug.Log(request.downloadHandler.text);
                }
                else
                {
                    Debug.Log("error");
                }
                responseText = request.downloadHandler.text;
            }
            return responseText;
        }

        public string Get_forID(string dir, int Id)
        {

            //StartCoroutine(GrabResponseJson_using(a, b));
            //Thread.Sleep(1000);
            var url = _host + dir + "/" + Id;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                //byte[] jsonToSend = new UTF8Encoding().GetBytes(strjson);
                //request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                //request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                //request.SetRequestHeader("Content-Type", "application/json");
                request.useHttpContinue = false;
                request.SetRequestHeader("Content-Type", "application/json");
                //"application/json; charset=utf-8"
                //request.SendWebRequest();
                request.SendWebRequest();
                Thread.Sleep(1000);
                //yield return new WaitForSeconds(3f);
                //request.SendWebRequest();
                Debug.Log("-----------------------------------------------------------------------------------");
                Debug.Log(url);
                Debug.Log(request.result);
                Debug.Log(request.downloadHandler.text);
                if (request.error == null)
                {
                    Debug.Log("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                    Debug.Log(request.downloadHandler.text);
                }
                else
                {
                    Debug.Log("error");
                }
                responseText = request.downloadHandler.text;
            }
            return responseText;
        }

        public string GrabResponse(string dir, string strxml)
        {
            var url = _host + dir;
            var uri = new Uri(url);

            var request = (HttpWebRequest)WebRequest.Create(uri);

            byte[] xml = System.Text.Encoding.ASCII.GetBytes(strxml);

            // If we've got a recent Auth header, re-use it!
            if (!string.IsNullOrEmpty(_cnonce) &&
                DateTime.Now.Subtract(_cnonceDate).TotalHours < 1.0)
            {
                request.Headers.Add("Authorization", GetDigestHeader(dir));
            }

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                // Try to fix a 401 exception by adding a Authorization header
                if (ex.Response == null || ((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.Unauthorized)
                    throw;

                var wwwAuthenticateHeader = ex.Response.Headers["WWW-Authenticate"];
                _realm = GrabHeaderVar("realm", wwwAuthenticateHeader);
                _nonce = GrabHeaderVar("nonce", wwwAuthenticateHeader);
                _qop = GrabHeaderVar("qop", wwwAuthenticateHeader);

                _nc = 0;
                _cnonce = new System.Random().Next(123400, 9999999).ToString();
                _cnonceDate = DateTime.Now;

                var request2 = (HttpWebRequest)WebRequest.Create(uri);
                //request2.ContentType = "application/xml; charset=UTF-8";
                request2.ContentType = "text/xml; charset=UTF-8";
                request2.ContentLength = xml.Length;
                request2.Headers.Add("Authorization", GetDigestHeader(dir));
                request2.Method = "PUT";

                System.IO.Stream reqStream = request2.GetRequestStream();
                reqStream.Write(xml, 0, xml.Length);
                reqStream.Close();

                response = (HttpWebResponse)request2.GetResponse();
            }
            var reader = new StreamReader(response.GetResponseStream());
            var apiResult = reader.ReadToEnd();
            return reader.ReadToEnd();
        }
    }
}

public class ConnectLogin : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        /*NUI.DigestAuthFixer digest = new NUI.DigestAuthFixer("http://192.168.2.249:80", "admin", "s1234567");

        string xml = "<PTZData><AbsoluteHigh><elevation> 45 </elevation><azimuth> 1750 </azimuth><absoluteZoom>2</absoluteZoom></AbsoluteHigh></PTZData>";

        //여기서 xml 값 설정해줘서 실제 값으로 처리될 수 있도록 할 것.

        try { 
            string strReturn = digest.GrabResponse("/ISAPI/PTZCtrl/channels/1/absolute", xml);
        }
        catch (Exception e) 
        {
        }
        //string strReturn = digest.GrabResponse("/ISAPI/PTZCtrl/channels/1/absolute");
        digest.ToString();*/
        /*
        string url = "http://192.168.2.2:4000/user/login";
        NUI.Login stars = new NUI.Login();
        stars.ID = "admin";
        stars.PW = "1q2w3e4r";
        string starts = JsonUtility.ToJson(stars);
        //string strReturn2 = tesese(url, starts);
        if (strReturn2 == "[]")
        {
            //EventHandler.Execute("OnFailedToRecoverPassword");
            Debug.Log("[RecoverPassword] Password resetted!");
        }
        else if (strReturn2 == "")
        {
            //EventHandler.Execute("OnPasswordRecovered");
            Debug.Log("SERVER is DOWN!!");
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }*/

        /// ----------- 아래쪽내용 안드로이드에서 안먹히는 함수임
        //NUI.DigestAuthFixer test_cli = new NUI.DigestAuthFixer("http://127.0.0.1:4000", "root", "root");
        /*NUI.DigestAuthFixer test_cli = new NUI.DigestAuthFixer("http://127.0.0.1:4000");
        NUI.User TEST = new NUI.User();
        TEST.ID = "admin";
        TEST.PW = "1q2w3e4r";
        string TESTJSON = JsonUtility.ToJson(TEST);
        string strReturn2 = "";

        NUI.Login stars = new NUI.Login();
        stars.ID = "admin";
        stars.PW = "1q2w3e4r";
        string starts = JsonUtility.ToJson(stars);


        Debug.Log("0번");

        try
        {
            strReturn2 = test_cli.GrabResponseJson("/user/login", starts);

        }
        catch (Exception e)
        {

        }
        Debug.Log("1번");
        Debug.Log(strReturn2);
        if (strReturn2 == "[]")
        {
            //EventHandler.Execute("OnFailedToRecoverPassword");
            Debug.Log("[RecoverPassword] Password resetted!");
        }
        else if (strReturn2 == "")
        {
            //EventHandler.Execute("OnPasswordRecovered");
            Debug.Log("SERVER is DOWN!!");
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }*/


    }

    // Update is called once per frame
    void Update()
    {

    }

}