using System;
using System.Threading;
using UnityEngine;

namespace Jambuddy.Junsu
{
    public class GameController : MonoBehaviour
    {
        private GameObject _target;

        public void Update()
        {
            if (_target == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    _target = hit.collider.gameObject;
                    SubscribeEvent(EventType.Add, _target);
                }
            }
        }

        // 외부에서 호출하여 해당 오브젝트를 등록 가능
        public void SubscribeEvent(EventType evtType, GameObject go)
        {
            switch (evtType)
            {
                case EventType.Add:
                    EffectTargetManager.onAddBlock += go.GetComponent<EffectTarget>().HandleBlockApplication;
                    EffectTargetManager.onApplyEffect += go.GetComponent<EffectTarget>().ApplyEffect;
                    break;
                default:
                    break;
            }
        }

        public void ApplyBlock()
        {
            EffectTargetManager.InvokeApplyBlock();
            EffectTargetManager.onAddBlock -= _target.GetComponent<EffectTarget>().HandleBlockApplication;
            EffectTargetManager.onApplyEffect -= _target.GetComponent<EffectTarget>().ApplyEffect;
            _target = null;
        }

        public void AddOppositeCallback()
        {
            EffectTargetManager.onAddBlock("OppositeMoving");
        }

        public void AddRotationCallback()
        {
            EffectTargetManager.onAddBlock("Rotation");
        }

        public void AddGravityCallback()
        {
            EffectTargetManager.onAddBlock("Gravity");
        }
    }
}