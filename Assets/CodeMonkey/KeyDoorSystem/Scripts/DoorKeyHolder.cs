/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeMonkey.KeyDoorSystemCM {

    /// <summary>
    /// Added to Player to Hold keys
    /// </summary>
    public class DoorKeyHolder : MonoBehaviour {

        public event EventHandler OnDoorKeyAdded;
        public event EventHandler OnDoorKeyUsed;

        [Header("Key Holder")]
        [Tooltip("List of Keys currently being held")]
        public List<Key> doorKeyHoldingList = new List<Key>();


        void OnTriggerEnter2D(Collider2D collider) {
            TryInteract(collider.gameObject);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            TryInteract(collision.gameObject);
        }

        private void TryInteract(GameObject obj) {
            DoorKey doorKey = obj.GetComponent<DoorKey>();
            if (doorKey == null) doorKey = obj.GetComponentInParent<DoorKey>();
            if (doorKey != null) {
                doorKeyHoldingList.Add(doorKey.key);
                doorKey.DestroySelf();
                OnDoorKeyAdded?.Invoke(this, EventArgs.Empty);
            }

            DoorLock doorLock = obj.GetComponent<DoorLock>();
            if (doorLock == null) doorLock = obj.GetComponentInParent<DoorLock>();
            if (doorLock != null) {
                if (doorKeyHoldingList.Contains(doorLock.key)) {
                    doorLock.OpenDoor();
                    StartCoroutine(DestroyAfterAnimation(doorLock.gameObject));
                    if (doorLock.removeKeyOnUse) {
                        doorKeyHoldingList.Remove(doorLock.key);
                    }
                    OnDoorKeyUsed?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private IEnumerator DestroyAfterAnimation(GameObject door) {
            Animator animator = door.GetComponent<Animator>();
            if (animator != null) {
                // Wait one frame for the trigger to transition into the Open state
                yield return null;
                // Now wait until the Open animation finishes
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                while (stateInfo.normalizedTime < 1f) {
                    yield return null;
                    if (door == null) yield break;
                    stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                }
            }
            if (door != null) Destroy(door);
        }

    }

}