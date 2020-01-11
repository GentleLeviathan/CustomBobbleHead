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
        private BobbleHead defaultBobbleValues;
        private Vector3 bobbleScale;
        private VTOLVehicles vehicleType;

        private string currentSceneName;
        private bool subbedToOnExitScene = false;

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
            SceneManager.sceneLoaded += OnLevelLoaded;
            DontDestroyOnLoad(this.gameObject);
        }

        private void OnLevelLoaded(Scene arg0, LoadSceneMode arg1)
        {
            currentSceneName = arg0.name;
            if (currentSceneName == "CustomMapBase" || currentSceneName == "Akutan")
            {
                StartCoroutine(InitWaiter());
            }
        }

        //Waits 2 seconds for the scenario to initialize and the player vehicle to spawn in.
        //Probably doesn't need to be anywhere near this long, but it's just for those guys with old slow hard drives
        private IEnumerator InitWaiter()
        {
            yield return new WaitForSeconds(2f);
                //Subscribing to OnExitScene because it seems to be the only public event when RestartMission is called.
                if (!subbedToOnExitScene) { FlightSceneManager.instance.OnExitScene += OnMissionRestart; subbedToOnExitScene = true; }
                SetupNewBobbleHead();
            yield break;
        }

        private void OnMissionRestart()
        {
            //If we are in a flyable scene (OnExitScene obviously gets invoked regardless)
            if (currentSceneName == "CustomMapBase" || currentSceneName == "Akutan")
            {
                //Additional cleanup in case the plane has crashed/pilot has ejected, and not despawned during the restart.
                if (newBobbleHead != null)
                {
                    Destroy(newBobbleHead);
                    StartCoroutine(InitWaiter());
                }
                else
                {
                    StartCoroutine(InitWaiter());
                }
            }
            else
            {
                //Unsubscribe
                FlightSceneManager.instance.OnExitScene -= OnMissionRestart; subbedToOnExitScene = false;
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
                VTOLVehicles cv = VTOLAPI.GetPlayersVehicleEnum();
                switch (cv)
                {
                    case VTOLVehicles.FA26B:
                        localOffset = new Vector3(0.035f, 1.4e-05f, -0.0075f);
                        bobbleEnabled = true;
                        break;
                    case VTOLVehicles.F45A:
                        //Might find a place to stick a smaller bobblehead later, not sure if it's worth the time though
                        break;
                    case VTOLVehicles.AV42C:
                        localOffset = new Vector3(-0.05f, 1.4e-05f, -0.1f);
                        bobbleEnabled = true;
                        break;
                    default:
                        break;
                }

                if (bobbleEnabled)
                {
                    GameObject defaultBobbleHead = GameObject.Find("bobblePilot");
                    defaultBobbleValues = defaultBobbleHead.GetComponent<BobbleHead>();

                    newBobbleHead = GameObject.Instantiate(newBobblePrefab);
                    bobbleScale = newBobbleHead.transform.localScale;
                    newBobbleHead.transform.parent = defaultBobbleHead.transform.parent;
                    newBobbleHead.transform.localRotation = defaultBobbleHead.transform.localRotation;

                    //Adding localOffset to offset the bobblehead on a per vehicle basis to a location that looks better in my opinion.
                    //(especially in the AV-42C, the default location is terrible, i guess just to get out of the way of the old quicksave buttons)
                    newBobbleHead.transform.localPosition = defaultBobbleHead.transform.localPosition + localOffset;

                    Transform Head = null;
                    Transform Root = null;
                    try
                    {
                        Root = newBobbleHead.transform.Find("CustomBobbleHead/Armature/Root"); try { Head = newBobbleHead.transform.Find("CustomBobbleHead/Armature/Root/Head"); }
                        catch (NullReferenceException e) { base.LogError("CustomBobbleHead: Head was not found under 'CustomBobbleHead/Armature/Root/'. BobbleHead physics will be disabled."); }
                    }
                    catch (NullReferenceException e) { base.LogError("CustomBobbleHead: Root was not found under 'CustomBobbleHead/Armature/'. BobbleHead physics will be disabled."); }

                    if (Head != null && Root != null)
                    {
                        BobbleHead newBobbleValues = newBobbleHead.AddComponent<BobbleHead>();
                        //Keeping just in case, this was cool math that would work but was unnecessary :*(
                        //newBobbleHead.transform.InverseTransformPoint((newBobbleHead.transform.position - Head.transform.position)) * -newBobbleHead.transform.localScale.y;

                        //Unique values
                        newBobbleValues.headObject = Head.gameObject;
                        newBobbleValues.headAnchor = Vector3.zero;
                        newBobbleValues.positionTarget = Vector3.zero;

                        //Default values
                        newBobbleValues.angularLimit = defaultBobbleValues.angularLimit;
                        newBobbleValues.headMass = defaultBobbleValues.headMass * 0.75f;
                        newBobbleValues.linearLimit = defaultBobbleValues.linearLimit;
                        newBobbleValues.positionDamper = defaultBobbleValues.positionDamper;
                        newBobbleValues.positionSpring = defaultBobbleValues.positionSpring;
                        newBobbleValues.rotationSpring = defaultBobbleValues.rotationSpring * 1.25f;
                        newBobbleValues.shakeIntensity = defaultBobbleValues.shakeIntensity;
                        newBobbleValues.shakeRate = defaultBobbleValues.shakeRate;
                        newBobbleValues.testShaking = defaultBobbleValues.testShaking;
                    }
                    Destroy(defaultBobbleHead);
                }
            }
        }
    }
}
