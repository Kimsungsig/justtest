using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;
using MRTKExtensions.Gesture;
using UnityEngine;
using TMPro;

namespace DyiPinchGrab
{
    public class DyiHandManipulation : MonoBehaviour
    {
        public TextMeshPro coordiText;
        public GameObject indexObject;

        private static AmazonDynamoDBClient client = null;

        MixedRealityPose pose;
        [SerializeField]
        private TrackedHandJoint trackedHandJoint = TrackedHandJoint.IndexTip;

        [SerializeField]
        private float grabDistance = 0.1f;

        [SerializeField]
        private Handedness trackedHand = Handedness.Right;

        [SerializeField]
        private bool trackPinch = true;

        [SerializeField]
        private bool trackGrab = false;

        private IMixedRealityHandJointService handJointService;

        private IMixedRealityHandJointService HandJointService =>
            handJointService ??
            (handJointService = CoreServices.GetInputSystemDataProvider<IMixedRealityHandJointService>());

        private MixedRealityPose? previousLeftHandPose;

        private MixedRealityPose? previousRightHandPose;

        public Boolean send = false;

        void Start()
        {
            ClientConfig();
            //indexObject = Instantiate(sphereMarker, this.transform);
        }

        private void Update()
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out pose))
            {
                //indexObject.GetComponent<Renderer>().enabled = true;
                //if (pose == MixedRealityPose.)
                indexObject.transform.position = pose.Position;
            }

            var rightHandPose = GetHandPose(Handedness.Right, previousRightHandPose != null);

            {
                var jointTransform = HandJointService.RequestJointTransform(trackedHandJoint, trackedHand);
                if (rightHandPose != null && previousRightHandPose != null)
                {
                    if(send == false)
                    {
                        ProcessPoseChange(previousRightHandPose, rightHandPose);
                        send = true;
                    }
                    
                }
                else
                {
                    send = false;
                }
            }
            previousRightHandPose = rightHandPose;
        }

        private MixedRealityPose? GetHandPose(Handedness hand, bool hasBeenGrabbed)
        {
            if ((trackedHand & hand) == hand)
            {
                if (HandJointService.IsHandTracked(hand) &&
                    ((GestureUtils.IsPinching(hand) && trackPinch) ||
                     (GestureUtils.IsGrabbing(hand) && trackGrab)))
                {
                    var jointTransform = HandJointService.RequestJointTransform(trackedHandJoint, hand);
                    var palmTransForm = HandJointService.RequestJointTransform(TrackedHandJoint.Palm, hand);
                    
                    if(hasBeenGrabbed || 
                       Vector3.Distance(gameObject.transform.position, jointTransform.position) <= grabDistance)
                    {
                        return new MixedRealityPose(jointTransform.position, palmTransForm.rotation);
                    }
                }
            }

            return null;
        }
        
        private void ProcessPoseChange(MixedRealityPose? previousPose, MixedRealityPose? currentPose)
        {
            var delta = currentPose.Value.Position - previousPose.Value.Position;
            var deltaRotation = Quaternion.FromToRotation(previousPose.Value.Forward, currentPose.Value.Forward);
            gameObject.transform.position += delta;
            gameObject.transform.rotation *= deltaRotation;

            float x = - 180f - (indexObject.gameObject.transform.position.x * 1000f * 0.75f);
            float y = 521f + (indexObject.gameObject.transform.position.y * 1000f * 0.75f);
            float z = indexObject.gameObject.transform.position.z * 1000f * 0.85f;
            float u = indexObject.gameObject.transform.rotation.x;
            float v = indexObject.gameObject.transform.rotation.y;
            float w = indexObject.gameObject.transform.rotation.z;

            coordiText.text = "X : " + z + "\nY : " + x + "\nZ : " + y + "\nU : " + w + "\nV : " + u + "\nW : " + v;
            InsertCommand((z / 1000f).ToString(), (x / 1000f).ToString(), (y / 1000f).ToString(), "0", "-180", "0");
            
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
                coordiText.text = "fail Client Config : " + e.Message;
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
                coordiText.text = "fail Insert : " + e.Message;
            }
        }
    }
}