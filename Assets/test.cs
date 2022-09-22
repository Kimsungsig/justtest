using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using Microsoft.MixedReality.Toolkit.Utilities;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Sphere;
    public GameObject Cube;
    public TextMeshPro meshPro;
    public AmazonDynamoDBClient client = null;
    void Start()
    {
        ClientConfig();
        meshPro.text = "Start";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        Vector3 vector = collision.contacts[0].point;
        string text = "X : " + vector.x.ToString() + "Y : " + vector.y.ToString() + "Z : " + vector.z.ToString();

        meshPro.text = text;
    }
    public void ReturnSphereVector()
    { 
        float x = Sphere.gameObject.transform.position.x;
        float y = Sphere.gameObject.transform.position.y;
        float z = Sphere.gameObject.transform.position.z;
        float u = Sphere.gameObject.transform.rotation.x;
        float v = Sphere.gameObject.transform.rotation.y;
        float w = Sphere.gameObject.transform.rotation.z;

        string text = "X : " + x.ToString() + "\nY : " + y.ToString() + "\nZ : " + z.ToString();

        meshPro.text = text;

        InsertCommand(x.ToString(), y.ToString(), z.ToString(), u.ToString(), v.ToString(), w.ToString());
    }

    public void ReturnCubeVector()
    {
        float x = Cube.gameObject.transform.position.x;
        float y = Cube.gameObject.transform.position.y;
        float z = Cube.gameObject.transform.position.z;
        float u = Cube.gameObject.transform.rotation.x;
        float v = Cube.gameObject.transform.rotation.y;
        float w = Cube.gameObject.transform.rotation.z;

        string text = "X : " + x.ToString() + "\nY : " + y.ToString() + "\nZ : " + z.ToString();
        
        meshPro.text = text;

        InsertCommand(x.ToString(), y.ToString(), z.ToString(), u.ToString(), v.ToString(), w.ToString());
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
            meshPro.text = "fail12 : " + e.Message;
        }
    }

    private void InsertCommand(string x, string y, string z, string u, string v, string w)
    {
        try
        {
            Table table = Table.LoadTable(client, "hololensCommand");
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            JObject coordinate = new JObject(
                new JProperty("Command", "Move"),
                new JProperty("data", new JObject(
                new JProperty("x", x),
                new JProperty("y", y),
                new JProperty("z", z),
                new JProperty("u", u),
                new JProperty("v", v),
                new JProperty("w", w))));
            string tmpCoordinate = coordinate.ToString();
            Document doc = Document.FromJson(tmpCoordinate);
            Task put = table.PutItemAsync(doc);
            put.Wait();
        }
        catch (Exception e)
        {
            meshPro.text = "fail12 : " + e.Message;
        }
    }
}
