using System.Collections.Generic;
using UnityEngine;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public class SlyContainer: VendorContainer<SlyContainer> {
        /*private static GameObject npcObject;
        private static Vector3 npcOffset;
        private static GameObject menuObject;
        private static List<GameObject> otherObjects;
        private static List<Vector3> objectOffset;
        private List<GameObject> myObjects;
        private ContainerInfo myInfo;*/

        public override string Name => "Sly";
        //public override bool SupportsInstantiate => true;

        public static void definePrefabs(Dictionary<string, GameObject> preObjs) {
            npcObject = preObjs["Basement Closed"];
            npcOffset = new Vector3(-17.2f, -6.4081f, 0.029f);
            menuObject = preObjs["Shop Menu"];
            addObject(preObjs, "_Scenery/Shop Counter", -0.03f, -0.7281f, 0.023f);
        }

        public override GameObject GetNewContainer(ContainerInfo info) {
            GameObject sly = GameObject.Instantiate(npcObject);
            foreach(PlayMakerFSM fsm in sly.GetComponents<PlayMakerFSM>()) {
                if(fsm.gameObject.name.StartsWith("Basement Closed") && fsm.FsmName == "Control") {
                    sly.GetComponent<PlayMakerFSM>().GetValidState("Check").RemoveAction(0);
                    doIgnoreProximity(sly.FindGameObjectInChildren("Sly Shop"));
                }
            }

            GameObject.Instantiate(menuObject, new Vector3(8.53f, 0.54f, -1.8609f), Quaternion.identity).SetActive(true);
            myObjects ??= new();
            foreach(GameObject obj in otherObjects) {
                myObjects.Add(GameObject.Instantiate(obj));
            }
            return sly;
        }

        /*public override void ApplyTargetContext(GameObject obj, float x, float y, float elevation) {
            obj.transform.position = new Vector3(x, y - elevation, 0) + npcOffset;
            obj.SetActive(true);
            for(int i = 0; i < myObjects.Count; i++) {
                myObjects[i].transform.position = new Vector3(x, y - elevation, 0) + objectOffset[i];
                myObjects[i].SetActive(true);
            }
        }

        public override void ApplyTargetContext(GameObject obj, GameObject target, float elevation) {
            ApplyTargetContext(obj, target.transform.position.x, target.transform.position.y, elevation);
        }

        protected static void addObject(Dictionary<string, GameObject> po, string name, float x, float y, float z) {
            otherObjects ??= new();
            objectOffset ??= new();
            otherObjects.Add(po[name]);
            objectOffset.Add(new Vector3(x, y, z));
        }

        protected void doIgnoreProximity(GameObject npcObj) {
            PlayMakerFSM[] fsms = npcObj.GetComponents<PlayMakerFSM>();
            for(int i = 0; i < fsms.Length; i++) {
                if(fsms[i].FsmName == "npc_control") {
                    fsms[i].GetValidState("Move Hero Left").InsertAction(new IgnoreProximity(), 0);
                    fsms[i].GetValidState("Move Hero Right").InsertAction(new IgnoreProximity(), 0);
                    break;
                }
            }
        }*/
    }
}
