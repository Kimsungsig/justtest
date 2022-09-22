using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;
using TMPro;


public class controller : MonoBehaviour
{
    public TextMeshPro jointText;
    public GameObject joint1;
    public GameObject joint2;
    public GameObject joint3;
    public GameObject joint4;
    public GameObject joint5;
    public GameObject joint6;
    Thread jointMove = null;

    public class Joint
    {
        public DateTime dateTime;
        public string joint_1;
        public string joint_2;
        public string joint_3;
        public string joint_4;
        public string joint_5;
        public string joint_6;
    }

    string move_timeStr;

    List<Joint> jointList = new List<Joint>();
    public static AmazonDynamoDBClient client = null;
    // Start is called before the first frame update
    void Start()
    {
        ClientConfig();
        DateTime move_timeDate = DateTime.UtcNow - TimeSpan.FromDays(15);
        move_timeStr = move_timeDate.ToString(AWSSDKUtils.ISO8601DateFormat);
        jointMove = new Thread(GetJoint);
        jointMove.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //joint1.gameObject.transform.localEulerAngles = new Vector3(0, 0, -float.Parse(joint_1));
        //joint2.gameObject.transform.localEulerAngles = new Vector3(0, float.Parse(joint_2), 0);
        //joint3.gameObject.transform.localEulerAngles = new Vector3(-180, -180 + float.Parse(joint_3), 180);
        //joint4.gameObject.transform.localEulerAngles = new Vector3(0, 0, -float.Parse(joint_4));
        //joint5.gameObject.transform.localEulerAngles = new Vector3(-180, -180 + float.Parse(joint_5), 180);
        //joint6.gameObject.transform.localEulerAngles = new Vector3(0, 0, -float.Parse(joint_6));

        //joint1.gameObject.transform.localEulerAngles = Vector3.Slerp(joint1.gameObject.transform.localEulerAngles, new Vector3(0, 0, -float.Parse(joint_1)), Time.deltaTime);
        //joint2.gameObject.transform.localEulerAngles = Vector3.Slerp(joint2.gameObject.transform.localEulerAngles, new Vector3(0, float.Parse(joint_2), 0), Time.deltaTime);
        //joint3.gameObject.transform.localEulerAngles = Vector3.Slerp(joint3.gameObject.transform.localEulerAngles, new Vector3(-180, -180 + float.Parse(joint_3), 180), Time.deltaTime);
        //joint4.gameObject.transform.localEulerAngles = Vector3.Slerp(joint4.gameObject.transform.localEulerAngles, new Vector3(0, 0, -float.Parse(joint_4)), Time.deltaTime);
        //joint5.gameObject.transform.localEulerAngles = Vector3.Slerp(joint5.gameObject.transform.localEulerAngles, new Vector3(-180, -180 + float.Parse(joint_5), 180), Time.deltaTime);
        //joint6.gameObject.transform.localEulerAngles = Vector3.Slerp(joint6.gameObject.transform.localEulerAngles, new Vector3(0, 0, -float.Parse(joint_6)), Time.deltaTime);

        joint1.gameObject.transform.localRotation = Quaternion.Slerp(joint1.gameObject.transform.localRotation, Quaternion.Euler(0, 0, -float.Parse(joint_1)), Time.deltaTime);
        joint2.gameObject.transform.localRotation = Quaternion.Slerp(joint2.gameObject.transform.localRotation, Quaternion.Euler(0, float.Parse(joint_2), 0), Time.deltaTime);
        joint3.gameObject.transform.localRotation = Quaternion.Slerp(joint3.gameObject.transform.localRotation, Quaternion.Euler(-180, -180 + float.Parse(joint_3), 180), Time.deltaTime);
        joint4.gameObject.transform.localRotation = Quaternion.Slerp(joint4.gameObject.transform.localRotation, Quaternion.Euler(0, 0, -float.Parse(joint_4)), Time.deltaTime);
        joint5.gameObject.transform.localRotation = Quaternion.Slerp(joint5.gameObject.transform.localRotation, Quaternion.Euler(-180, -180 + float.Parse(joint_5), 180), Time.deltaTime);
        joint6.gameObject.transform.localRotation = Quaternion.Slerp(joint6.gameObject.transform.localRotation, Quaternion.Euler(0, 0, -float.Parse(joint_6)), Time.deltaTime);

        jointText.text = "Joint 1 : " + joint_1 + "\nJoint 2 : " + joint_2 + "\nJoint 3 : " + joint_3 + "\nJoint 4 : " + joint_4 + "\nJoint 5 : " + joint_5 + "\nJoint 6 : " + joint_6;
    }


    private void ClientConfig()
    {
        try
        {
            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
            clientConfig.RegionEndpoint = Amazon.RegionEndpoint.APNortheast2;
            client = new AmazonDynamoDBClient("AKIAYY7EIIJNOBKFHI7H", "rXQihfl2+3WgtR4XUc43ipZZ9kqHIuMA1KOT1Z53", clientConfig);

        }
        catch (Exception e)
        {
            jointText.text = "fail Client Config : " + e.Message;
        }
    }

    string joint_1 = "0";
    string joint_2 = "0";
    string joint_3 = "0";
    string joint_4 = "0";
    string joint_5 = "0";
    string joint_6 = "0";

    public async void GetJoint()
    {
        try
        {
            while (true)
            {
                Thread.Sleep(300);

                string jointId = "joint";

                if (jointList.Count > 0)
                {
                    move_timeStr = jointList[jointList.Count - 1].dateTime.ToString(AWSSDKUtils.ISO8601DateFormat);
                }

                var request = new QueryRequest
                {
                    TableName = "robotJoint_date",
                    ReturnConsumedCapacity = "TOTAL",
                    KeyConditionExpression = "joint = :v_replyId and move_time > :v_interval",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                        {":v_replyId", new AttributeValue {
                             S = jointId
                         }},
                        {":v_interval", new AttributeValue {
                             S = move_timeStr
                         }}
                    },

                    // Optional parameter.
                    ProjectionExpression = "joint, move_time, joint_data",
                    // Optional parameter.
                    ConsistentRead = true
                };

                var response = await client.QueryAsync(request);

                foreach (Dictionary<string, AttributeValue> item in response.Items)
                {
                    var doc = Document.FromAttributeMap(item);
                    JObject joint = JObject.Parse(doc.ToJsonPretty());
                    DateTime date = (DateTime)joint["move_time"];
                    JObject header = (JObject)joint["joint_data"];

                    Joint jointData = new Joint();
                    jointData.dateTime = date;
                    jointData.joint_1 = header["1"].ToString();
                    jointData.joint_2 = header["2"].ToString();
                    jointData.joint_3 = header["3"].ToString();
                    jointData.joint_4 = header["4"].ToString();
                    jointData.joint_5 = header["5"].ToString();
                    jointData.joint_6 = header["6"].ToString();

                    jointList.Add(jointData);
                }

                //Debug.Log("list count : " + jointList.Count);20220922
                //Table table = Table.LoadTable(client, "robotJoint_date");

                //ScanOperationConfig scanOps = new ScanOperationConfig();
                //string paginationToken = "";
                //if (!string.IsNullOrEmpty(paginationToken))
                //{
                //    scanOps.PaginationToken = paginationToken;
                //}


                //var results = table.Scan(scanOps);

                //List<Document> data = await results.GetNextSetAsync();

                //if (data.Count > 0)
                //{
                //    DateTime curDate = DateTime.Now;
                //    for (int i = 0; i > data.Count; i++)
                //    {
                //        JObject joint = JObject.Parse(data[0].ToJsonPretty());
                //        DateTime date = (DateTime)joint["date"];
                //        if (date < curDate)
                //        {
                //            JObject header = (JObject)joint["data"];
                //            //joint_1 = header["1"].ToString();
                //            //joint_2 = header["2"].ToString();
                //            //joint_3 = header["3"].ToString();
                //            //joint_4 = header["4"].ToString();
                //            //joint_5 = header["5"].ToString();
                //            //joint_6 = header["6"].ToString();

                //            Joint jointData = new Joint();
                //            jointData.dateTime = date;
                //            jointData.joint_1 = header["1"].ToString();
                //            jointData.joint_2 = header["2"].ToString();
                //            jointData.joint_3 = header["3"].ToString();
                //            jointData.joint_4 = header["4"].ToString();
                //            jointData.joint_5 = header["5"].ToString();
                //            jointData.joint_6 = header["6"].ToString();

                //            jointList.Add(jointData);
                //            Debug.Log("Size : " + jointList.Count);
                //        }
                //    }
                //}

                //joint1.gameObject.transform.rotation = Quaternion.Euler(-90, 90, -float.Parse(joint_1));
                //joint2.gameObject.transform.rotation = Quaternion.Euler(-90, 90 + float.Parse(joint_2), 0);
                //joint3.gameObject.transform.rotation = Quaternion.Euler(-270, float.Parse(joint_3), 270);
                //joint4.gameObject.transform.rotation = Quaternion.Euler(-90, 90, -float.Parse(joint_4));
                //joint5.gameObject.transform.rotation = Quaternion.Euler(-270, -90.0f + float.Parse(joint_5), 180);
                //joint6.gameObject.transform.rotation = Quaternion.Euler(-90, 90, -float.Parse(joint_6));

                //joint1.gameObject.transform.Rotate(0, 0, -float.Parse(joint_1));
                //joint2.gameObject.transform.Rotate(0, float.Parse(joint_2), 0);
                //joint3.gameObject.transform.Rotate(-180, -180 + float.Parse(joint_3), 180);
                //joint4.gameObject.transform.Rotate(0, 0, -float.Parse(joint_4));
                //joint5.gameObject.transform.Rotate(-180, -180 + float.Parse(joint_5), 180);
                //joint6.gameObject.transform.Rotate(0, 0, -float.Parse(joint_6));

            }

        }
        catch (Exception e)
        {
            jointText.text = "fail Get Joint : " + e.Message;
        }
        finally
        {
            jointMove.Abort();
        }
    }
}