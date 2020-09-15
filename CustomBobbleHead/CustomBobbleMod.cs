using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CustomBobbleHead
{
    public class CustomBobbleMod : VTOLMOD
    {
        private bool AssetLoaded = false;
        private string PathToBundle;
        private GameObject newBobblePrefab;
        private GameObject newBobbleHead;
        private InteractiveBobbleHead bobbleInteractive;
        private Vector3 bobbleScale;
        private VTOLVehicles vehicleType;

        public override void ModLoaded() { base.ModLoaded(); }
        private void Awake()
        {
            //Asset bundle file name needs to be "custombobblehead" (not case sensitive)
            PathToBundle = Directory.GetCurrentDirectory() + @"\VTOLVR_ModLoader\mods\CustomBobbleHead\custombobblehead";
            vehicleType = VTOLAPI.GetPlayersVehicleEnum();

            //If we haven't already loaded the Asset Bundle
            if (!AssetLoaded)
            {
                //--------------------------------------------Prefab inside the asset bundle needs to be named "CustomBobbleHead" (case sensitive)
                newBobblePrefab = FileLoader.GetAssetBundleAsGameObject(PathToBundle, "CustomBobbleHead.prefab");
                AssetLoaded = true;
            }
            VTOLAPI.SceneLoaded += SceneLoaded;
            VTOLAPI.MissionReloaded += OnMissionRestart;
            DontDestroyOnLoad(this.gameObject);
        }

        private void SceneLoaded(VTOLScenes arg0)
        {
            switch (arg0)
            {
                case VTOLScenes.Akutan:
                    StartCoroutine(InitWaiter());
                    break;
                case VTOLScenes.OpenWater:
                    StartCoroutine(InitWaiter());
                    break;
                case VTOLScenes.CustomMapBase:
                    StartCoroutine(InitWaiter());
                    break;
            }
        }

        //Waits 2 seconds for the scenario to initialize and the player vehicle to spawn in.
        //Probably doesn't need to be anywhere near this long, but it's just for those guys with old slow hard drives
        private IEnumerator InitWaiter()
        {
            yield return new WaitForSeconds(0.75f);
            SetupNewBobbleHead();
            yield break;
        }

        private void OnMissionRestart()
        {

            //Additional cleanup in case the plane has crashed/pilot has ejected, and not despawned during the restart.
            if (newBobbleHead != null)
            {
                Destroy(newBobbleHead);
                newBobbleHead = null;
                StartCoroutine(InitWaiter());
            }
            else
            {
                StartCoroutine(InitWaiter());
            }
        }

        private void SetupNewBobbleHead()
        {
            //Make sure we were able to load the prefab - more null checks == better :*)
            if(newBobblePrefab != null)
            {
                //Vehicle specific default local offsets and actions
                Vector3 localOffset = Vector3.zero;
                bool bobbleEnabled = false;

                //Adding localOffset to offset the bobblehead on a per vehicle basis to a location that looks better in my opinion.
                //(especially in the AV-42C, the default location is terrible, i guess just to get out of the way of the old quicksave buttons)
                VTOLVehicles cv = VTOLAPI.GetPlayersVehicleEnum();
                switch (cv)
                {
                    case VTOLVehicles.FA26B:
                        localOffset = new Vector3(-0.4512793f, 1.0411f, 5.863763f);
                        bobbleEnabled = true;
                        break;
                    case VTOLVehicles.F45A:
                        //Might find a better place to stick it later
                        localOffset = new Vector3(-0.4056f, 0.4991f, 5.4594f);
                        bobbleEnabled = true;
                        break;
                    case VTOLVehicles.AV42C:
                        localOffset = new Vector3(-0.5683f, 0.7659f, 0.5186f);
                        bobbleEnabled = true;
                        break;
                    default:
                        bobbleEnabled = false;
                        break;
                }

                if (bobbleEnabled)
                {
                    GameObject defaultBobbleHead = GameObject.Find("BobblePilot");

                    newBobbleHead = GameObject.Instantiate(newBobblePrefab);

                    GameObject bobbleRoot = GameObject.Instantiate(defaultBobbleHead);
                    bobbleRoot.transform.SetParent(defaultBobbleHead.transform.parent, false);
                    bobbleRoot.transform.localPosition = localOffset;
                    bobbleRoot.transform.localRotation = defaultBobbleHead.transform.localRotation;
                    
                    for(int i = 0; i < bobbleRoot.transform.childCount; i++)
                    {
                        Destroy(bobbleRoot.transform.GetChild(i).gameObject);
                    }

                    bobbleInteractive = bobbleRoot.GetComponent<InteractiveBobbleHead>();

                    bobbleScale = newBobbleHead.transform.localScale;
                    newBobbleHead.transform.parent = bobbleRoot.transform;
                    newBobbleHead.transform.localPosition = Vector3.zero;
                    newBobbleHead.transform.localRotation = Quaternion.identity;

                    Transform HeadTF = null;
                    Transform colliderCenter = newBobbleHead.transform.Find("CustomBobbleHead/ColliderCenter");
                    try
                    {
                       HeadTF = newBobbleHead.transform.Find("CustomBobbleHead/HeadTF");
                    }
                    catch (NullReferenceException e)
                    {
                        base.LogError("CustomBobbleHead: 'HeadTF' was not found under 'CustomBobbleHead/CustomBobbleHead/'. BobbleHead interaction will based on bobble position.");
                    }

                    if (HeadTF != null)
                    {
                        bobbleInteractive.headTransform = HeadTF;
                        bobbleInteractive.headLocalTarget = HeadTF.localPosition;
                        if (colliderCenter) { bobbleInteractive.headColliderCenter = colliderCenter.localPosition; }
                    }
                    else
                    {
                        bobbleInteractive.headTransform = newBobbleHead.transform;
                        bobbleInteractive.headLocalTarget = Vector3.zero;
                        bobbleInteractive.headColliderCenter = Vector3.zero;
                    }
                    Destroy(defaultBobbleHead);
                }
            }
        }
    }
}
