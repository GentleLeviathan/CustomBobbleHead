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
        }

        private void OnLevelLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name == "CustomMapBase" || arg0.name == "Akutan")
            {
                StartCoroutine(InitWaiter());
            }
        }
        private IEnumerator InitWaiter()
        {
            yield return new WaitForSeconds(2f);
                SetupNewBobbleHead();
            yield break;
        }

        private void SetupNewBobbleHead()
        {
            //Make sure we were able to load the prefab
            if(newBobblePrefab != null)
            {
                GameObject defaultBobbleHead = GameObject.Find("bobblePilot");
                defaultBobbleValues = defaultBobbleHead.GetComponent<BobbleHead>();

                newBobbleHead = GameObject.Instantiate(newBobblePrefab);
                bobbleScale = newBobbleHead.transform.localScale;
                newBobbleHead.transform.parent = defaultBobbleHead.transform.parent;
                newBobbleHead.transform.localPosition = defaultBobbleHead.transform.localPosition;
                newBobbleHead.transform.localRotation = defaultBobbleHead.transform.localRotation;

                Transform Head = null;
                Transform Root = null;
                try { Root = newBobbleHead.transform.Find("CustomBobbleHead/Armature/Root"); try { Head = newBobbleHead.transform.Find("CustomBobbleHead/Armature/Root/Head"); }
                    catch(NullReferenceException e) { base.LogError("CustomBobbleHead: Head was not found under 'CustomBobbleHead/Armature/Root/'. BobbleHead physics will be disabled."); }
                } catch(NullReferenceException e) { base.LogError("CustomBobbleHead: Root was not found under 'CustomBobbleHead/Armature/'. BobbleHead physics will be disabled."); }

                if (Head != null && Root != null)
                {
                    BobbleHead newBobbleValues = newBobbleHead.AddComponent<BobbleHead>();
                    //Keeping just in case
                    //newBobbleHead.transform.InverseTransformPoint((newBobbleHead.transform.position - Head.transform.position)) * -newBobbleHead.transform.localScale.y;
                    //Unique values
                    newBobbleValues.headObject = Head.gameObject;
                    newBobbleValues.headAnchor = Vector3.zero;
                    newBobbleValues.positionTarget = Vector3.zero;
                    //Default values
                    newBobbleValues.angularLimit = defaultBobbleValues.angularLimit;
                    newBobbleValues.headMass = defaultBobbleValues.headMass;
                    newBobbleValues.linearLimit = defaultBobbleValues.linearLimit;
                    newBobbleValues.positionDamper = defaultBobbleValues.positionDamper;
                    newBobbleValues.positionSpring = defaultBobbleValues.positionSpring;
                    newBobbleValues.rotationSpring = defaultBobbleValues.rotationSpring;
                    newBobbleValues.shakeIntensity = defaultBobbleValues.shakeIntensity;
                    newBobbleValues.shakeRate = defaultBobbleValues.shakeRate;
                    newBobbleValues.testShaking = defaultBobbleValues.testShaking;
                }
                Destroy(defaultBobbleHead);
            }
        }
    }
}
