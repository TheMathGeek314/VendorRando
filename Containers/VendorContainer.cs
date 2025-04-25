using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HutongGames.PlayMaker;
using ItemChanger;
using Satchel;

namespace VendorRando {
    public abstract class VendorContainer<T>: Container where T : VendorContainer<T>{
        protected static GameObject npcObject;
        protected static Vector3 npcOffset;
        protected static GameObject menuObject;
        protected static List<GameObject> otherObjects;
        protected static List<Vector3> objectOffset;
        protected List<GameObject> myObjects;
        protected ContainerInfo myInfo;

        public override bool SupportsInstantiate => true;

        public override GameObject GetNewContainer(ContainerInfo info) {
            GameObject npc = GameObject.Instantiate(npcObject);
            doIgnoreProximity(npc);
            GameObject.Instantiate(menuObject, new Vector3(8.53f, 0.54f, -1.8609f), Quaternion.identity).SetActive(true);
            myObjects ??= new();
            foreach(GameObject obj in otherObjects) {
                myObjects.Add(GameObject.Instantiate(obj));
            }
            return npc;
        }

        public override void ApplyTargetContext(GameObject obj, float x, float y, float elevation) {
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
        }

        // idk if region is auto-detected, and idk if I can override that to allow multiple shops to coexist
        /*private void IgnoreProxAndSetRegion(GameObject npc, GameObject region) {
            PlayMakerFSM[] fsms = npc.GetComponents<PlayMakerFSM>();
            for(int i = 0; i < fsms.Length; i++) {
                if(fsms[i].FsmName == "npc_control") {
                    fsms[i].GetValidState("Move Hero Left").InsertAction(new IgnoreProximity(), 0);
                    fsms[i].GetValidState("Move Hero Right").InsertAction(new IgnoreProximity(), 0);
                }
                else if(fsms[i].FsmName == "Conversation Control") {
                    fsms[i].FsmVariables.GetFsmGameObject("Shop Region").Value = region;
                }
            }
        }*/
    }

    public class IgnoreProximity: FsmStateAction {
        public override void OnEnter() {
            Fsm.Event(FsmEvent.GetFsmEvent("FINISHED"));
            Finish();//Is this needed? ¯\_(ツ)_/¯
        }
    }
}
